using System.Collections.Generic;
using UnityEngine;

public static class DebugLogger
{
    public static bool IsEnabled = true;

    public static void Log(object message) { if (IsEnabled) Debug.Log(message); }
    public static void LogWarning(object message) { if (IsEnabled) Debug.LogWarning(message); }
    public static void LogError(object message) { if (IsEnabled) Debug.LogError(message); }
}
