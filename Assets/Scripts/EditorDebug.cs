using System.Diagnostics; // For StackTrace
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EditorDebug : MonoBehaviour
{
    public static EditorDebug Instance { get; private set; }

    [Header("Debug Settings")]
    public bool enableLogging = true;

    [Header("Script Database")]
    public DebugScriptDatabase scriptDatabase;

    [Tooltip("Scripts to ignore from logging")]
    public List<string> ignoreScripts = new List<string>();

    private HashSet<string> activeScripts = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        RefreshActiveScripts();
    }

    // Refresh active scripts based on ignore list
    public void RefreshActiveScripts()
    {
        activeScripts.Clear();
        if (scriptDatabase == null)
            return;

        foreach (var scriptName in scriptDatabase.GetAllScriptNames())
        {
            if (!ignoreScripts.Contains(scriptName))
                activeScripts.Add(scriptName);
        }
    }

    // Automatically detect calling script
    private string GetCallerScriptName()
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(2); // 0=GetCallerScriptName, 1=Log, 2=caller
        var method = frame.GetMethod();
        return method.DeclaringType.Name;
    }

    // Static logging functions
    public static void Log(string message)
    {
        if (Instance == null || !Instance.enableLogging)
            return;

        string scriptName = Instance.GetCallerScriptName();
        if (!Instance.activeScripts.Contains(scriptName))
            return;

        DebugLogger.Log($"[{scriptName}] {message}");
    }

    public static void LogWarning(string message)
    {
        if (Instance == null || !Instance.enableLogging)
            return;

        string scriptName = Instance.GetCallerScriptName();
        if (!Instance.activeScripts.Contains(scriptName))
            return;

        DebugLogger.LogWarning($"[{scriptName}] {message}");
    }

    public static void LogError(string message)
    {
        if (Instance == null || !Instance.enableLogging)
            return;

        string scriptName = Instance.GetCallerScriptName();
        if (!Instance.activeScripts.Contains(scriptName))
            return;

        DebugLogger.LogError($"[{scriptName}] {message}");
    }

    // Editor-only: initially populate ignore list if empty
    private void OnValidate()
    {
        if (scriptDatabase == null)
            return;

        if (ignoreScripts == null)
            ignoreScripts = new List<string>();

        if (ignoreScripts.Count == 0)
            ignoreScripts.AddRange(scriptDatabase.GetAllScriptNames());

        RefreshActiveScripts();
    }

    // Dynamic ignore/unignore functions
    public void IgnoreScript(string scriptName)
    {
        if (!ignoreScripts.Contains(scriptName))
            ignoreScripts.Add(scriptName);

        activeScripts.Remove(scriptName);
    }

    public void UnignoreScript(string scriptName)
    {
        if (ignoreScripts.Contains(scriptName))
            ignoreScripts.Remove(scriptName);

        if (scriptDatabase != null && scriptDatabase.GetAllScriptNames().Contains(scriptName))
            activeScripts.Add(scriptName);
    }
}

// Custom inspector for reset/add buttons
[CustomEditor(typeof(EditorDebug))]
public class EditorDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorDebug gm = (EditorDebug)target;

        if (GUILayout.Button("Reset Ignore List (Empty)"))
        {
            gm.ignoreScripts.Clear();
            gm.RefreshActiveScripts();
        }

        if (GUILayout.Button("Repopulate Ignore List (All Scripts)"))
        {
            if (gm.scriptDatabase != null)
            {
                gm.ignoreScripts = new List<string>(gm.scriptDatabase.GetAllScriptNames());
                gm.RefreshActiveScripts();
            }
        }
    }
}
#endif