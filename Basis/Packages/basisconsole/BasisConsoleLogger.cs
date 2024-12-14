using Basis.Scripts.UI.UI_Panels;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BasisConsoleLogger : BasisUIBase
{
    public TextMeshProUGUI logText;
    public bool showAllLogsInOrder = true;
    public bool showCollapsedLogs = false;
    public LogType currentLogTypeFilter = LogType.Log;
    public TMP_Dropdown Dropdown;
    public Button ClearButton;
    public Button CollapseButton;
    public Button StopButton;
    public RectTransform Transform;
    public bool IsUpdating = true;
    public TextMeshProUGUI CollapseButtonText;
    public TextMeshProUGUI StopButtonText;
    public float updateInterval = 0.1f; // 100 milliseconds
    public float timeSinceLastUpdate = 0f;
    public BasisButtonHeldCallBack BasisButtonHeldCallBack;
    public Button MouseLock;
    private void Awake()
    {
        BasisLogManager.LoadLogsFromDisk();
        Dropdown.onValueChanged.AddListener(HandlePressed);
        ClearButton.onClick.AddListener(ClearLogs);
        CollapseButton.onClick.AddListener(Collapse);
        StopButton.onClick.AddListener(StopStartLoggingToUI);
        BasisButtonHeldCallBack.OnButtonReleased += OnButtonReleased;
        BasisButtonHeldCallBack.OnButtonPressed += OnButtonPressed;
        MouseLock.onClick.AddListener(ToggleMouse);
    }
    public Canvas Canvas;
    public void ToggleMouse()
    {
        BasisCursorManagement.LockCursor(nameof(BasisConsoleLogger));
    }
    public void OnButtonReleased()
    {

    }
    public void OnButtonPressed()
    {

    }
    public void StopStartLoggingToUI()
    {
        IsUpdating = !IsUpdating;
        if (IsUpdating)
        {
            StopButtonText.text = "Stop";
        }
        else
        {
            StopButtonText.text = "Start";
        }
    }
    public void Collapse()
    {
        showCollapsedLogs = !showCollapsedLogs;
        if (showCollapsedLogs)
        {
            CollapseButtonText.text = "Uncollapse";
        }
        else
        {
            CollapseButtonText.text = "Collapse";
        }
        UpdateLogDisplay();
    }

    public void HandlePressed(int Value)
    {
        showAllLogsInOrder = false;
        switch (Value)
        {
            case 0:
                showAllLogsInOrder = true;
                break;
            case 1:
                currentLogTypeFilter = LogType.Error;
                break;
            case 2:
                currentLogTypeFilter = LogType.Warning;
                break;
            case 3:
                currentLogTypeFilter = LogType.Log;
                break;
        }
        UpdateLogDisplay();
    }
    private void Update()
    {
        if (IsUpdating && BasisLogManager.LogChanged)
        {
            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f; // Reset the timer
                UpdateLogDisplay();
            }
        }
    }

    private void UpdateLogDisplay()
    {
        StringBuilder currentLogDisplay = new StringBuilder();

        if (showCollapsedLogs)
        {
            if (showAllLogsInOrder)
            {
                currentLogDisplay.Append(string.Join("\n", BasisLogManager.GetCombinedCollapsedLogs()));
            }
            else
            {
                currentLogDisplay.Append(string.Join("\n", BasisLogManager.GetCollapsedLogs(currentLogTypeFilter)));
            }
        }
        else if (showAllLogsInOrder)
        {
            currentLogDisplay.Append(string.Join("\n", BasisLogManager.GetAllLogs()));
        }
        else
        {
            currentLogDisplay.Append(string.Join("\n", BasisLogManager.GetLogs(currentLogTypeFilter)));
        }

        logText.text = currentLogDisplay.ToString();
        BasisLogManager.LogChanged = false;
    }

    public void ClearLogs()
    {
        BasisLogManager.ClearLogs();
        logText.text = "";
    }

    public override void DestroyEvent()
    {
        BasisCursorManagement.LockCursor(nameof(BasisConsoleLogger));
    }

    public override void InitalizeEvent()
    {
        BasisCursorManagement.UnlockCursor(nameof(BasisConsoleLogger));
    }
}