using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Small runtime console for development commands.
/// Put this component on the same GameObject as GameManager and assign the
/// command input field in the Inspector.
/// </summary>
public class DevConsole : MonoBehaviour
{
    public static DevConsole Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject consoleRoot;
    [SerializeField] private TMP_InputField commandInput;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private int maxOutputLines = 20;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    [SerializeField] private bool startOpen;

    private readonly Queue<string> outputLines = new Queue<string>();
    private bool unlimitedSisu;
    private bool unlimitedHp;
    private bool consoleOpen;
    private bool previousCanRotate = true;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisibility;

    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (commandInput != null)
            commandInput.onSubmit.AddListener(ExecuteCommand);

        SetConsoleOpen(startOpen);
    }

    private void OnDisable()
    {
        if (commandInput != null)
            commandInput.onSubmit.RemoveListener(ExecuteCommand);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetConsoleOpen(!consoleOpen);

        Player player = GetPlayer();
        if (player == null)
            return;

        if (unlimitedHp && !Mathf.Approximately(player.currentHealth, player.maxHealth))
        {
            player.currentHealth = player.maxHealth;
            player.UpdateHealthUI();
        }

        if (unlimitedSisu && !Mathf.Approximately(player.currentSisu, player.maxSisu))
        {
            player.currentSisu = player.maxSisu;
            player.UpdateSisuUI(true);
        }
    }

    public void ExecuteCommand(string rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand))
            return;

        string command = rawCommand.Trim();
        string[] parts = command.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        string verb = parts[0].ToLowerInvariant();
        bool success;

        switch (verb)
        {
            case "close":
                SetConsoleOpen(false);
                success = true;
                break;
            case "+":
            case "-":
                success = ExecuteResourceChange(verb == "+", parts);
                break;
            case "killall":
                success = KillAllEnemies();
                break;
            case "unlimited":
                success = ExecuteUnlimited(parts);
                break;
            case "walk":
                success = ExecuteSpeedCommand(parts, false);
                break;
            case "run":
                success = ExecuteSpeedCommand(parts, true);
                break;
            case "help":
                PrintHelp();
                success = true;
                break;
            default:
                Print("Unknown command. Type 'help' for available commands.");
                success = false;
                break;
        }

        if (success)
            Print("> " + command);

        if (commandInput != null)
        {
            commandInput.text = string.Empty;
            if (consoleOpen)
                commandInput.ActivateInputField();
        }
    }

    public void SetConsoleOpen(bool open)
    {
        consoleOpen = open;
        IsOpen = open;

        if (consoleRoot != null && consoleRoot != gameObject)
            consoleRoot.SetActive(open);
        else
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                canvas.enabled = open;
        }

        PlayerMovement movement = GetPlayerMovement();
        if (movement != null)
            movement.enabled = !open;

        if (MouseLook.instance != null)
        {
            if (open)
            {
                previousCanRotate = MouseLook.instance.canRotate;
                MouseLook.instance.canRotate = false;
            }
            else
            {
                MouseLook.instance.canRotate = previousCanRotate;
            }
        }

        if (open)
        {
            previousCursorLockState = Cursor.lockState;
            previousCursorVisibility = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (commandInput != null)
            {
                commandInput.ActivateInputField();
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(commandInput.gameObject);
            }
        }
        else
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisibility;
        }
    }

    private bool ExecuteResourceChange(bool add, string[] parts)
    {
        if (parts.Length != 3 || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount))
        {
            Print("Usage: + hp 50 | - sisu 27 | + money 100");
            return false;
        }

        amount = Mathf.Abs(amount);
        if (amount == 0)
            return true;

        int signedAmount = add ? amount : -amount;
        string resource = parts[1].ToLowerInvariant();
        Player player = GetPlayer();

        switch (resource)
        {
            case "hp":
            case "health":
                if (player == null) return false;
                player.currentHealth = Mathf.Clamp(player.currentHealth + signedAmount, 0f, player.maxHealth);
                player.UpdateHealthUI();
                return true;

            case "sisu":
            case "stamina":
                if (player == null) return false;
                player.currentSisu = Mathf.Clamp(player.currentSisu + signedAmount, 0f, player.maxSisu);
                player.UpdateSisuUI(true);
                return true;

            case "money":
            case "cash":
                if (GameManager.GM == null) return false;
                GameManager.GM.AdjustMoney(signedAmount);
                return true;

            default:
                Print("Unknown resource. Use hp, sisu or money.");
                return false;
        }
    }

    private bool ExecuteUnlimited(string[] parts)
    {
        if (parts.Length < 2 || parts.Length > 3)
        {
            Print("Usage: unlimited sisu [on|off]");
            return false;
        }

        bool enabled = true;
        if (parts.Length == 3)
        {
            string state = parts[2].ToLowerInvariant();
            if (state == "off" || state == "false")
                enabled = false;
            else if (state != "on" && state != "true")
            {
                Print("Use 'on' or 'off'.");
                return false;
            }
        }

        switch (parts[1].ToLowerInvariant())
        {
            case "sisu":
            case "stamina":
                unlimitedSisu = enabled;
                if (enabled && GetPlayer() != null)
                {
                    GetPlayer().currentSisu = GetPlayer().maxSisu;
                    GetPlayer().UpdateSisuUI(true);
                }
                return true;

            case "hp":
            case "health":
                unlimitedHp = enabled;
                if (enabled && GetPlayer() != null)
                {
                    GetPlayer().currentHealth = GetPlayer().maxHealth;
                    GetPlayer().UpdateHealthUI();
                }
                return true;

            default:
                Print("Unknown unlimited resource. Use hp or sisu.");
                return false;
        }
    }

    private bool ExecuteSpeedCommand(string[] parts, bool running)
    {
        if (parts.Length != 3 || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float speed) || speed < 0f)
        {
            Print(running ? "Usage: run speed 12" : "Usage: walk speed 5");
            return false;
        }

        PlayerMovement movement = GetPlayerMovement();
        if (movement == null)
            return false;

        if (running)
        {
            movement.ogRunningspeed = speed;
            movement.runningSpeed = speed;
        }
        else
        {
            movement.ogSpeed = speed;
            movement.walkingSpeed = speed;
        }

        return true;
    }

    private bool KillAllEnemies()
    {
        int killed = 0;
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.isDead)
            {
                enemy.Die();
                killed++;
            }
        }

        Print("Killed " + killed + " enemies.");
        return true;
    }

    private Player GetPlayer()
    {
        if (GameManager.GM != null && GameManager.GM.playerScript != null)
            return GameManager.GM.playerScript;

        return Player.instance;
    }

    private PlayerMovement GetPlayerMovement()
    {
        Player player = GetPlayer();
        return player != null ? player.GetComponent<PlayerMovement>() : null;
    }

    private void PrintHelp()
    {
        Print("+/- hp|sisu|money amount");
        Print("killall");
        Print("unlimited hp|sisu [on|off]");
        Print("walk speed X / run speed X");
        Print("close");
    }

    private void Print(string message)
    {
        Debug.Log("[DevConsole] " + message);
        if (outputText == null)
            return;

        outputLines.Enqueue(message);
        while (outputLines.Count > Mathf.Max(1, maxOutputLines))
            outputLines.Dequeue();

        StringBuilder builder = new StringBuilder();
        foreach (string line in outputLines)
            builder.AppendLine(line);
        outputText.text = builder.ToString();
    }
}
