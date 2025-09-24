//basis from https://github.com/dilmerv/ARCloudAnchors/blob/master/Assets/Scripts/Managers/ARDebugManager.cs
using Assets.Scripts.Core;
using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.Rendering.CoreEditorDrawer<TData>;

public class ARDebugManager : Assets.Scripts.Core.Singleton<ARDebugManager>
{
    [SerializeField]
    private TextMeshProUGUI debugAreaText = null;

    [SerializeField]
    private bool enableDebug = false;

    [SerializeField]
    private int maxLines = 8;

    void OnEnable()
    {
        debugAreaText.enabled = enableDebug;
        enabled = enableDebug;
    }

    public void LogInfo(string message)
    {
        ClearLines();
        debugAreaText.text += $"{DateTime.Now.ToString("yyyy-dd-M HH:mm:ss")}: <color=\"white\">{message}</color>\n";
    }

    public void LogError(string message)
    {
        ClearLines();
        debugAreaText.text += $"{DateTime.Now.ToString("yyyy-dd-M HH:mm:ss")}: <color=\"red\">{message}</color>\n";
    }

    public void LogWarning(string message)
    {
        ClearLines();
        debugAreaText.text += $"{DateTime.Now.ToString("yyyy-dd-M HH:mm:ss")}: <color=\"yellow\">{message}</color>\n";
    }

    private void ClearLines()
    {
        if (debugAreaText.text.Split('\n').Count() >= maxLines)
        {
            debugAreaText.text = string.Empty;
        }
    }
}