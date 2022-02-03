using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisualiseField))]
[CanEditMultipleObjects]
public class VisualiseFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VisualiseField myScript = (VisualiseField)target;
        if (GUILayout.Button("Visualise"))
        {
            myScript.Visualise();
        }
    }
}
