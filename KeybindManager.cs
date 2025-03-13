using UnityEngine;

public class KeybindManager : MonoBehaviour
{
	// Singleton instance
	public static KeybindManager Instance { get; private set; }

	// Example keybinds. You can add as many as you need.
	[Header("Basic")]
	public KeyCode pauseKey = KeyCode.Escape;
	public KeyCode shopKey = KeyCode.Tab;

	[Header("Movement")]
	public KeyCode jumpKey = KeyCode.Space;

	[Header("Debugging / Development")]
	public KeyCode spawnWave = KeyCode.I;
	public KeyCode spawnSingleEnemy = KeyCode.O;
	public KeyCode maxSisu = KeyCode.Y;

	[Header("Other")]
	public KeyCode kickKey = KeyCode.F;
	public KeyCode interactKey = KeyCode.E;
	public KeyCode throwGrenadeKey = KeyCode.G;
	public KeyCode selectionMenuKey = KeyCode.Q;
	public KeyCode placeObjectKey = KeyCode.T;

	void Awake()
	{
		// If an instance already exists and it's not this, destroy the new one.
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
