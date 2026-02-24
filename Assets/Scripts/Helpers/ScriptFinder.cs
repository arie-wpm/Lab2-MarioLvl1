#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class ScriptFinder
{
    public static List<string> GetAllMonoBehaviourNames()
    {
        string[] guids = AssetDatabase.FindAssets("t:MonoScript");
        List<string> names = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() != null &&
                script.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
            {
                names.Add(script.GetClass().Name);
            }
        }

        return names;
    }
}
#endif