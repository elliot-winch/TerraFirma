using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshGenerator))]
[CanEditMultipleObjects]
public class MeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshGenerator myScript = (MeshGenerator)target;
        if (GUILayout.Button("Regenerate"))
        {
            myScript.UpdateChunks();
        }
    }
}
