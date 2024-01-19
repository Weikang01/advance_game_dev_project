using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilemapGenerator))]
public class TilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TilemapGenerator tilemapGenerator = (TilemapGenerator)target;

        switch (tilemapGenerator.generationType)
        {
            case TilemapGenerator.GenerationType.SimpleRandom:
                break;
            case TilemapGenerator.GenerationType.TextureBaseGeneration:
                tilemapGenerator.referenceTexture = (Texture2D)EditorGUILayout.ObjectField("Reference Texture", tilemapGenerator.referenceTexture, typeof(Texture2D), false);
                break;
        }
    }
}
