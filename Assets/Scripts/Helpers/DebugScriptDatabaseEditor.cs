#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugScriptDatabase))]
public class DebugScriptDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DebugScriptDatabase db = (DebugScriptDatabase)target;

        if (GUILayout.Button("Refresh Scripts"))
        {
            db.Refresh();
            EditorUtility.SetDirty(db);
            Debug.Log("DebugScriptDatabase refreshed!");
        }
    }
}
#endif