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

        switch (tilemapGenerator.referenceTile)
        {
            case TilemapGenerator.ReferenceTile.TileGrid:
                tilemapGenerator.reference = (Grid)EditorGUILayout.ObjectField("Reference Grid", tilemapGenerator.reference, typeof(Grid), true);
                break;
            case TilemapGenerator.ReferenceTile.RuleTile:
                tilemapGenerator.ruleTile = (RuleTile)EditorGUILayout.ObjectField("Rule Tile", tilemapGenerator.ruleTile, typeof(RuleTile), false);
                break;
        }

        switch (tilemapGenerator.generationType)
        {
            case TilemapGenerator.GenerationAlgorithm.SimpleRandom:
                break;
            case TilemapGenerator.GenerationAlgorithm.TextureBaseGeneration:
                tilemapGenerator.referenceTexture = (Texture2D)EditorGUILayout.ObjectField("Reference Texture", tilemapGenerator.referenceTexture, typeof(Texture2D), false);
                break;
        }
    }
}
