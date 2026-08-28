using System;
using System.Collections.Generic;
using System.Reflection;
using Den.Tools;
using MapMagic.Core;
using MapMagic.Products;
using MapMagic.Terrains;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class MapMagicRuntimeNavMeshBaker : MonoBehaviour
{
    [SerializeField] private MapMagicObject mapMagic;
    [SerializeField] private NavMeshSurface[] surfaceTemplates;
    [SerializeField] private bool bakeMainTiles = true;
    [SerializeField] private bool bakeDraftTiles;
    [SerializeField] private bool rebuildExistingTilesOnEnable = true;
    [SerializeField] private bool removeTileNavMeshWhenTileMoves = true;
    [SerializeField] private int maxConcurrentBuilds = 1;

    [Header("Tile Bake Bounds")]
    [Tooltip("Extra horizontal bake area around each tile. Keep this at 0 when seam links are enabled to avoid overlapping navmesh islands.")]
    [SerializeField] private float borderOverlap;
    [Tooltip("Extra Y size in the bake bounds, useful for steep terrain, bridges, caves, and tall objects.")]
    [SerializeField] private float verticalBoundsPadding = 40f;

    [Header("Template Link Collection")]
    [Tooltip("Fallback value used only if this Unity version does not expose the NavMeshSurface Generate Links setting internally.")]
    [SerializeField] private bool generateLinksByDefault = true;

    [Header("Tile Seam Links")]
    [Tooltip("Creates explicit runtime links between neighboring tile navmeshes. This is what allows agents to cross from one MapMagic tile navmesh to another.")]
    [SerializeField] private bool createSeamLinks = true;
    [Tooltip("Distance between generated crossing links along a tile border.")]
    [SerializeField] private float seamLinkSpacing = 8f;
    [Tooltip("Width of each generated crossing link.")]
    [SerializeField] private float seamLinkWidth = 6f;
    [Tooltip("How far each link endpoint is placed inside its own tile from the shared border. Lower values reduce apparent acceleration at tile seams.")]
    [SerializeField] private float seamLinkEndpointInset = 1.5f;
    [Tooltip("Height above terrain used when sampling for nearby navmesh at link endpoints.")]
    [SerializeField] private float seamLinkSampleHeight = 3f;
    [Tooltip("Maximum distance from the sampled endpoint to the nearest navmesh.")]
    [SerializeField] private float seamLinkSampleDistance = 8f;
    [Tooltip("NavMesh area assigned to generated seam links.")]
    [SerializeField] private int seamLinkArea;
    [Tooltip("Negative means use the area's normal cost. Zero or positive overrides the traversal cost.")]
    [SerializeField] private float seamLinkCostModifier = -1f;

    [Header("Debug")]
    [SerializeField] private bool logBuilds;

    private static readonly FieldInfo GenerateLinksField = typeof(NavMeshSurface).GetField(
        "m_GenerateLinks",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly Dictionary<TerrainTile, TileNavMesh> tileNavMeshes = new Dictionary<TerrainTile, TileNavMesh>();
    private readonly Queue<BuildRequest> pendingBuilds = new Queue<BuildRequest>();
    private readonly List<BuildRequest> activeBuilds = new List<BuildRequest>();
    private readonly Dictionary<SeamKey, List<NavMeshLinkInstance>> seamLinks = new Dictionary<SeamKey, List<NavMeshLinkInstance>>();
    private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
    private readonly List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

    private void Reset()
    {
        mapMagic = GetComponent<MapMagicObject>();
        surfaceTemplates = GetComponents<NavMeshSurface>();
    }

    private void OnEnable()
    {
        if (mapMagic == null)
            mapMagic = GetComponent<MapMagicObject>();

        TerrainTile.OnTileApplied += HandleTileApplied;
        TerrainTile.OnTileMoved += HandleTileMoved;

        if (rebuildExistingTilesOnEnable && mapMagic != null)
        {
            foreach (TerrainTile tile in mapMagic.tiles.All())
            {
                if (tile != null)
                    QueueTile(tile);
            }
        }
    }

    private void OnDisable()
    {
        TerrainTile.OnTileApplied -= HandleTileApplied;
        TerrainTile.OnTileMoved -= HandleTileMoved;

        foreach (TileNavMesh tileNavMesh in tileNavMeshes.Values)
            tileNavMesh.Remove();

        RemoveAllSeamLinks();
        tileNavMeshes.Clear();
        pendingBuilds.Clear();
        activeBuilds.Clear();
    }

    private void Update()
    {
        RemoveDestroyedTiles();
        StartQueuedBuilds();
    }

    public void RebuildAll()
    {
        if (mapMagic == null)
            return;

        foreach (TerrainTile tile in mapMagic.tiles.All())
        {
            if (tile != null)
                QueueTile(tile);
        }
    }

    private void HandleTileApplied(TerrainTile tile, TileData data, StopToken stop)
    {
        if (stop != null && stop.stop)
            return;

        if (!ShouldBake(tile, data))
            return;

        QueueTile(tile);
    }

    private void HandleTileMoved(TerrainTile tile)
    {
        if (mapMagic != null && tile.mapMagic != mapMagic)
            return;

        if (removeTileNavMeshWhenTileMoves && tileNavMeshes.TryGetValue(tile, out TileNavMesh tileNavMesh))
        {
            RemoveSeamLinks(tileNavMesh);
            tileNavMesh.Remove();
        }
    }

    private bool ShouldBake(TerrainTile tile, TileData data)
    {
        if (tile == null)
            return false;

        if (mapMagic != null && tile.mapMagic != mapMagic)
            return false;

        if (data != null)
        {
            if (data.isDraft && !bakeDraftTiles)
                return false;

            if (!data.isDraft && !bakeMainTiles)
                return false;
        }

        return tile.ActiveTerrain != null && SurfaceCount > 0;
    }

    private int SurfaceCount => surfaceTemplates == null ? 0 : surfaceTemplates.Length;

    private void QueueTile(TerrainTile tile)
    {
        if (tile == null || tile.ActiveTerrain == null || SurfaceCount == 0)
            return;

        TileNavMesh tileNavMesh = GetOrCreateTileNavMesh(tile);
        tileNavMesh.Generation++;

        for (int i = 0; i < surfaceTemplates.Length; i++)
        {
            if (surfaceTemplates[i] == null)
                continue;

            SurfaceNavMesh surfaceNavMesh = tileNavMesh.GetOrCreateSurface(i, surfaceTemplates[i].agentTypeID);
            surfaceNavMesh.Queued = true;
        }

        pendingBuilds.Enqueue(new BuildRequest(tile, tileNavMesh.Generation));
    }

    private TileNavMesh GetOrCreateTileNavMesh(TerrainTile tile)
    {
        if (!tileNavMeshes.TryGetValue(tile, out TileNavMesh tileNavMesh))
        {
            tileNavMesh = new TileNavMesh();
            tileNavMeshes.Add(tile, tileNavMesh);
        }

        return tileNavMesh;
    }

    private void StartQueuedBuilds()
    {
        int allowedBuilds = Mathf.Max(1, maxConcurrentBuilds);

        while (activeBuilds.Count < allowedBuilds && pendingBuilds.Count > 0)
        {
            BuildRequest request = pendingBuilds.Dequeue();

            if (!tileNavMeshes.TryGetValue(request.Tile, out TileNavMesh tileNavMesh))
                continue;

            if (request.Generation != tileNavMesh.Generation || request.Tile == null || request.Tile.ActiveTerrain == null)
                continue;

            for (int surfaceIndex = 0; surfaceIndex < surfaceTemplates.Length; surfaceIndex++)
            {
                if (activeBuilds.Count >= allowedBuilds)
                    return;

                NavMeshSurface surface = surfaceTemplates[surfaceIndex];
                if (surface == null)
                    continue;

                SurfaceNavMesh surfaceNavMesh = tileNavMesh.GetOrCreateSurface(surfaceIndex, surface.agentTypeID);
                if (!surfaceNavMesh.Queued || surfaceNavMesh.Building)
                    continue;

                StartBuild(request.Tile, tileNavMesh, surfaceNavMesh, surface, surfaceIndex);
            }
        }
    }

    private void StartBuild(TerrainTile tile, TileNavMesh tileNavMesh, SurfaceNavMesh surfaceNavMesh, NavMeshSurface surface, int surfaceIndex)
    {
        surfaceNavMesh.Queued = false;
        surfaceNavMesh.Building = true;
        surfaceNavMesh.Ready = false;

        Bounds buildBounds = GetTileBounds(tile);
        List<NavMeshBuildSource> buildSources = CollectSources(tile, surface, buildBounds);

        if (!surfaceNavMesh.Instance.valid)
            surfaceNavMesh.Instance = NavMesh.AddNavMeshData(surfaceNavMesh.Data);

        BuildRequest request = new BuildRequest(tile, tileNavMesh.Generation, surfaceIndex, surfaceNavMesh.Data);
        activeBuilds.Add(request);

        AsyncOperation operation = NavMeshBuilder.UpdateNavMeshDataAsync(surfaceNavMesh.Data, surface.GetBuildSettings(), buildSources, buildBounds);
        operation.completed += _ => HandleBuildCompleted(request);

        if (logBuilds)
            Debug.Log($"Queued runtime NavMesh build for tile {tile.coord}, surface {surfaceIndex}, sources {buildSources.Count}.", this);
    }

    private Bounds GetTileBounds(TerrainTile tile)
    {
        Terrain terrain = tile.ActiveTerrain;
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        float horizontalOverlap = Mathf.Max(0f, borderOverlap);
        Vector3 size = terrainSize + new Vector3(horizontalOverlap * 2f, Mathf.Max(0f, verticalBoundsPadding), horizontalOverlap * 2f);
        Vector3 center = terrainPosition + terrainSize * 0.5f;
        center.y = terrainPosition.y + terrainSize.y * 0.5f;

        return new Bounds(center, size);
    }

    private List<NavMeshBuildSource> CollectSources(TerrainTile tile, NavMeshSurface surface, Bounds buildBounds)
    {
        sources.Clear();
        markups.Clear();
        CollectMarkups(tile, surface);
        bool generateLinks = ShouldGenerateLinks(surface);

        if (surface.collectObjects == CollectObjects.Children)
            NavMeshBuilder.CollectSources(tile.transform, surface.layerMask, surface.useGeometry, surface.defaultArea,
                generateLinks, markups, false, sources);
        else
            NavMeshBuilder.CollectSources(buildBounds, surface.layerMask, surface.useGeometry, surface.defaultArea,
                generateLinks, markups, surface.collectObjects == CollectObjects.MarkedWithModifier, sources);

        if (surface.ignoreNavMeshAgent)
            sources.RemoveAll(source => source.component != null && source.component.GetComponent<NavMeshAgent>() != null);

        if (surface.ignoreNavMeshObstacle)
            sources.RemoveAll(source => source.component != null && source.component.GetComponent<NavMeshObstacle>() != null);

        AppendModifierVolumes(tile, surface, buildBounds);

        return new List<NavMeshBuildSource>(sources);
    }

    private bool ShouldGenerateLinks(NavMeshSurface surface)
    {
        if (GenerateLinksField == null)
            return generateLinksByDefault;

        return (bool)GenerateLinksField.GetValue(surface);
    }

    private void CollectMarkups(TerrainTile tile, NavMeshSurface surface)
    {
        List<NavMeshModifier> modifiers = surface.collectObjects == CollectObjects.Children
            ? new List<NavMeshModifier>(tile.GetComponentsInChildren<NavMeshModifier>())
            : NavMeshModifier.activeModifiers;

        for (int i = 0; i < modifiers.Count; i++)
        {
            NavMeshModifier modifier = modifiers[i];
            if (modifier == null || !modifier.isActiveAndEnabled)
                continue;

            if ((surface.layerMask & (1 << modifier.gameObject.layer)) == 0)
                continue;

            if (!modifier.AffectsAgentType(surface.agentTypeID))
                continue;

            markups.Add(new NavMeshBuildMarkup
            {
                root = modifier.transform,
                overrideArea = modifier.overrideArea,
                area = modifier.area,
                ignoreFromBuild = modifier.ignoreFromBuild,
                applyToChildren = modifier.applyToChildren,
                overrideGenerateLinks = modifier.overrideGenerateLinks,
                generateLinks = modifier.generateLinks
            });
        }
    }

    private void AppendModifierVolumes(TerrainTile tile, NavMeshSurface surface, Bounds buildBounds)
    {
        List<NavMeshModifierVolume> volumes = surface.collectObjects == CollectObjects.Children
            ? new List<NavMeshModifierVolume>(tile.GetComponentsInChildren<NavMeshModifierVolume>())
            : NavMeshModifierVolume.activeModifiers;

        for (int i = 0; i < volumes.Count; i++)
        {
            NavMeshModifierVolume volume = volumes[i];
            if (volume == null || !volume.isActiveAndEnabled)
                continue;

            if ((surface.layerMask & (1 << volume.gameObject.layer)) == 0)
                continue;

            if (!volume.AffectsAgentType(surface.agentTypeID))
                continue;

            Bounds volumeBounds = GetVolumeBounds(volume);
            if (!buildBounds.Intersects(volumeBounds))
                continue;

            sources.Add(new NavMeshBuildSource
            {
                shape = NavMeshBuildSourceShape.ModifierBox,
                transform = Matrix4x4.TRS(volumeBounds.center, volume.transform.rotation, Vector3.one),
                size = volumeBounds.size,
                area = volume.area
            });
        }
    }

    private static Bounds GetVolumeBounds(NavMeshModifierVolume volume)
    {
        Vector3 center = volume.transform.TransformPoint(volume.center);
        Vector3 scale = volume.transform.lossyScale;
        Vector3 size = new Vector3(
            volume.size.x * Mathf.Abs(scale.x),
            volume.size.y * Mathf.Abs(scale.y),
            volume.size.z * Mathf.Abs(scale.z));

        return new Bounds(center, size);
    }

    private void HandleBuildCompleted(BuildRequest request)
    {
        activeBuilds.RemoveAll(active => active.Matches(request));

        if (!tileNavMeshes.TryGetValue(request.Tile, out TileNavMesh tileNavMesh))
            return;

        SurfaceNavMesh surfaceNavMesh = tileNavMesh.GetSurface(request.SurfaceIndex);
        if (surfaceNavMesh == null)
            return;

        surfaceNavMesh.Building = false;

        if (request.Generation != tileNavMesh.Generation)
        {
            surfaceNavMesh.Queued = true;
            pendingBuilds.Enqueue(new BuildRequest(request.Tile, tileNavMesh.Generation));
            return;
        }

        surfaceNavMesh.Ready = true;
        RefreshSeamLinks(request.Tile, request.SurfaceIndex);

        if (logBuilds)
            Debug.Log($"Completed runtime NavMesh build for tile {request.Tile.coord}, surface {request.SurfaceIndex}.", this);
    }

    private void RefreshSeamLinks(TerrainTile tile, int surfaceIndex)
    {
        if (!createSeamLinks || mapMagic == null || tile == null)
            return;

        TryBuildSeamLinks(tile, new Coord(tile.coord.x + 1, tile.coord.z), surfaceIndex);
        TryBuildSeamLinks(tile, new Coord(tile.coord.x - 1, tile.coord.z), surfaceIndex);
        TryBuildSeamLinks(tile, new Coord(tile.coord.x, tile.coord.z + 1), surfaceIndex);
        TryBuildSeamLinks(tile, new Coord(tile.coord.x, tile.coord.z - 1), surfaceIndex);
    }

    private void TryBuildSeamLinks(TerrainTile tile, Coord neighborCoord, int surfaceIndex)
    {
        TerrainTile neighbor = mapMagic.tiles[neighborCoord];
        if (neighbor == null || neighbor.ActiveTerrain == null)
            return;

        if (!tileNavMeshes.TryGetValue(tile, out TileNavMesh tileNavMesh) ||
            !tileNavMeshes.TryGetValue(neighbor, out TileNavMesh neighborNavMesh))
            return;

        SurfaceNavMesh surface = tileNavMesh.GetSurface(surfaceIndex);
        SurfaceNavMesh neighborSurface = neighborNavMesh.GetSurface(surfaceIndex);
        if (surface == null || neighborSurface == null || !surface.Ready || !neighborSurface.Ready)
            return;

        SeamKey key = new SeamKey(tile.coord, neighbor.coord, surfaceIndex);
        RemoveSeamLinks(key, tileNavMesh, neighborNavMesh);

        List<NavMeshLinkInstance> links = CreateSeamLinks(tile, neighbor, surfaceIndex);
        if (links.Count == 0)
            return;

        seamLinks.Add(key, links);
        tileNavMesh.SeamKeys.Add(key);
        neighborNavMesh.SeamKeys.Add(key);

        if (logBuilds)
            Debug.Log($"Created {links.Count} NavMesh seam links between tiles {tile.coord} and {neighbor.coord}, surface {surfaceIndex}.", this);
    }

    private List<NavMeshLinkInstance> CreateSeamLinks(TerrainTile tile, TerrainTile neighbor, int surfaceIndex)
    {
        List<NavMeshLinkInstance> links = new List<NavMeshLinkInstance>();
        NavMeshSurface surface = surfaceTemplates[surfaceIndex];

        int dx = neighbor.coord.x - tile.coord.x;
        int dz = neighbor.coord.z - tile.coord.z;
        if (Mathf.Abs(dx) + Mathf.Abs(dz) != 1)
            return links;

        Rect rect = tile.WorldRect;
        bool xSeam = dx != 0;
        float boundary = xSeam ? (dx > 0 ? rect.xMax : rect.xMin) : (dz > 0 ? rect.yMax : rect.yMin);
        float lineStart = xSeam ? rect.yMin : rect.xMin;
        float lineLength = xSeam ? rect.height : rect.width;

        float spacing = Mathf.Max(1f, seamLinkSpacing);
        float width = Mathf.Max(0.1f, seamLinkWidth > 0f ? seamLinkWidth : spacing * 0.8f);
        float inset = Mathf.Max(borderOverlap + 1f, seamLinkEndpointInset);
        int linkCount = Mathf.Max(1, Mathf.CeilToInt(lineLength / spacing));
        float step = lineLength / linkCount;

        for (int i = 0; i < linkCount; i++)
        {
            float along = lineStart + (i + 0.5f) * step;

            Vector3 start = xSeam
                ? new Vector3(boundary - dx * inset, 0f, along)
                : new Vector3(along, 0f, boundary - dz * inset);

            Vector3 end = xSeam
                ? new Vector3(boundary + dx * inset, 0f, along)
                : new Vector3(along, 0f, boundary + dz * inset);

            if (!TrySampleTileNavMesh(tile, start, out Vector3 sampledStart))
                continue;

            if (!TrySampleTileNavMesh(neighbor, end, out Vector3 sampledEnd))
                continue;

            NavMeshLinkData linkData = new NavMeshLinkData
            {
                startPosition = sampledStart,
                endPosition = sampledEnd,
                width = Mathf.Min(width, step),
                costModifier = seamLinkCostModifier,
                bidirectional = true,
                area = seamLinkArea,
                agentTypeID = surface.agentTypeID
            };

            NavMeshLinkInstance link = NavMesh.AddLink(linkData, Vector3.zero, Quaternion.identity);
            if (NavMesh.IsLinkValid(link))
                links.Add(link);
            else
                NavMesh.RemoveLink(link);
        }

        return links;
    }

    private bool TrySampleTileNavMesh(TerrainTile tile, Vector3 position, out Vector3 sampledPosition)
    {
        Terrain terrain = tile.ActiveTerrain;
        position.y = terrain.SampleHeight(position) + terrain.transform.position.y + seamLinkSampleHeight;

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, seamLinkSampleDistance + seamLinkSampleHeight, NavMesh.AllAreas))
        {
            sampledPosition = hit.position;
            return true;
        }

        sampledPosition = default;
        return false;
    }

    private void RemoveDestroyedTiles()
    {
        List<TerrainTile> removedTiles = null;

        foreach (KeyValuePair<TerrainTile, TileNavMesh> pair in tileNavMeshes)
        {
            if (pair.Key != null)
                continue;

            removedTiles ??= new List<TerrainTile>();
            removedTiles.Add(pair.Key);
            RemoveSeamLinks(pair.Value);
            pair.Value.Remove();
        }

        if (removedTiles == null)
            return;

        for (int i = 0; i < removedTiles.Count; i++)
            tileNavMeshes.Remove(removedTiles[i]);
    }

    private void RemoveSeamLinks(TileNavMesh tileNavMesh)
    {
        foreach (SeamKey key in tileNavMesh.SeamKeys)
            RemoveSeamLinks(key);

        tileNavMesh.SeamKeys.Clear();
    }

    private void RemoveSeamLinks(SeamKey key, params TileNavMesh[] owners)
    {
        if (!seamLinks.TryGetValue(key, out List<NavMeshLinkInstance> links))
            return;

        for (int i = 0; i < links.Count; i++)
            NavMesh.RemoveLink(links[i]);

        seamLinks.Remove(key);

        for (int i = 0; i < owners.Length; i++)
            owners[i]?.SeamKeys.Remove(key);
    }

    private void RemoveAllSeamLinks()
    {
        foreach (List<NavMeshLinkInstance> links in seamLinks.Values)
        {
            for (int i = 0; i < links.Count; i++)
                NavMesh.RemoveLink(links[i]);
        }

        seamLinks.Clear();
    }

    [Serializable]
    private readonly struct BuildRequest
    {
        public readonly TerrainTile Tile;
        public readonly int Generation;
        public readonly int SurfaceIndex;
        public readonly NavMeshData Data;

        public BuildRequest(TerrainTile tile, int generation)
            : this(tile, generation, -1, null)
        {
        }

        public BuildRequest(TerrainTile tile, int generation, int surfaceIndex, NavMeshData data)
        {
            Tile = tile;
            Generation = generation;
            SurfaceIndex = surfaceIndex;
            Data = data;
        }

        public bool Matches(BuildRequest other)
        {
            return Tile == other.Tile && Generation == other.Generation && SurfaceIndex == other.SurfaceIndex && Data == other.Data;
        }
    }

    private sealed class TileNavMesh
    {
        public int Generation;
        public readonly HashSet<SeamKey> SeamKeys = new HashSet<SeamKey>();
        private readonly Dictionary<int, SurfaceNavMesh> surfaces = new Dictionary<int, SurfaceNavMesh>();

        public SurfaceNavMesh GetOrCreateSurface(int index, int agentTypeId)
        {
            if (!surfaces.TryGetValue(index, out SurfaceNavMesh surface))
            {
                surface = new SurfaceNavMesh(agentTypeId);
                surfaces.Add(index, surface);
            }

            return surface;
        }

        public SurfaceNavMesh GetSurface(int index)
        {
            surfaces.TryGetValue(index, out SurfaceNavMesh surface);
            return surface;
        }

        public void Remove()
        {
            foreach (SurfaceNavMesh surface in surfaces.Values)
                surface.Remove();

            SeamKeys.Clear();
        }
    }

    private sealed class SurfaceNavMesh
    {
        public readonly NavMeshData Data;
        public NavMeshDataInstance Instance;
        public bool Queued;
        public bool Building;
        public bool Ready;

        public SurfaceNavMesh(int agentTypeId)
        {
            Data = new NavMeshData(agentTypeId);
        }

        public void Remove()
        {
            if (Instance.valid)
                Instance.Remove();

            Instance = new NavMeshDataInstance();
            Queued = false;
            Building = false;
            Ready = false;
        }
    }

    private readonly struct SeamKey : IEquatable<SeamKey>
    {
        private readonly int ax;
        private readonly int az;
        private readonly int bx;
        private readonly int bz;
        private readonly int surfaceIndex;

        public SeamKey(Coord a, Coord b, int surfaceIndex)
        {
            bool swap = b.x < a.x || (b.x == a.x && b.z < a.z);

            ax = swap ? b.x : a.x;
            az = swap ? b.z : a.z;
            bx = swap ? a.x : b.x;
            bz = swap ? a.z : b.z;
            this.surfaceIndex = surfaceIndex;
        }

        public bool Equals(SeamKey other)
        {
            return ax == other.ax && az == other.az && bx == other.bx && bz == other.bz && surfaceIndex == other.surfaceIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is SeamKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ax;
                hash = (hash * 397) ^ az;
                hash = (hash * 397) ^ bx;
                hash = (hash * 397) ^ bz;
                hash = (hash * 397) ^ surfaceIndex;
                return hash;
            }
        }
    }
}
