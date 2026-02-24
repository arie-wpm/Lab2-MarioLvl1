#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "DebugScriptDatabase", menuName = "Debug/ScriptDatabase")]
public class DebugScriptDatabase : ScriptableObject {
    public List<MonoScript> allScripts = new List<MonoScript>();

    public void Refresh() {
        allScripts.Clear();

        // Only search inside "Assets/Scripts/"
        string[] guids = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets/Scripts" });

        foreach (var guid in guids) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() != null &&
                script.GetClass().IsSubclassOf(typeof(MonoBehaviour))) {
                allScripts.Add(script);
            }
        }
    }

    public List<string> GetAllScriptNames() {
        return allScripts.Select(s => s.GetClass().Name).ToList();
    }
}
#endif