using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainManager))]
public class TerrainManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainManager t = (TerrainManager)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Chunks"))
        {
            t.GenerateChunks();
        }

        if (GUILayout.Button("Delete Chunks"))
        {
            t.DeleteChunks();
        }
    }
}
