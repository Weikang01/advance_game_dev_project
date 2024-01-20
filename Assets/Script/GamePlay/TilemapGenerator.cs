using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Tilemap))]
public class TilemapGenerator : MonoBehaviour
{
    public enum ReferenceTile
    {
        TileGrid,
        RuleTile
    }

    public ReferenceTile referenceTile;

    [HideInInspector] public Grid reference;
    [HideInInspector] public RuleTile ruleTile;

    public int width = 100;
    public int height = 100;

    private Tilemap tilemap;
    private Tile[] availableTiles; // An array of tiles to choose from

    public enum GenerationAlgorithm
    {
        SimpleRandom,
        TextureBaseGeneration,
        WaveFunctionCollapse
    }

    public GenerationAlgorithm generationType;

    [HideInInspector] public Texture2D referenceTexture;
    public enum Interpolation
    {
        NearestNeighbour,
        Bilinear,
    }
    [HideInInspector] public Interpolation interpolation;


    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        
        // get available tiles from the tilemap
        if (referenceTile == ReferenceTile.TileGrid)
        {
            Tilemap tm = reference.GetComponentInChildren<Tilemap>();
            availableTiles = GetNonEmptyTiles(tm);
        }

        switch (generationType)
        {
            case GenerationAlgorithm.SimpleRandom:
                RandomGenerateTilemap();
                break;
            case GenerationAlgorithm.TextureBaseGeneration:
                GenerateFromTexture();
                break;
            default:
                RandomGenerateTilemap();
                break;
        }
    }

    private Tile[] GetNonEmptyTiles(Tilemap tm)
    {
        List<Tile> nonEmptyTiles = new List<Tile>();
        BoundsInt bounds = tm.cellBounds;
        foreach (var position in bounds.allPositionsWithin)
        {
            Tile tile = tm.GetTile<Tile>(position);
            if (tile != null)
            {
                nonEmptyTiles.Add(tile);
            }
        }

        return nonEmptyTiles.ToArray();
    }

    void RandomGenerateTilemap()
    {
        tilemap.ClearAllTiles(); // Clear any existing tiles in the Tilemap

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (referenceTile == ReferenceTile.TileGrid)
                {
                    tilemap.SetTile(tilePosition, availableTiles[UnityEngine.Random.Range(0, availableTiles.Length)]); // Set the random tile at the current position
                }
                else if (referenceTile == ReferenceTile.RuleTile)
                {
                    tilemap.SetTile(tilePosition, ruleTile);
                }
                TileBase randomTile = ruleTile; // Get a random tile from the array
                tilemap.SetTile(tilePosition, randomTile); // Set the random tile at the current position
            }
        }
    }

    Color NearestNeighbourInterpolation(Texture2D texture, float x, float y)
    {
        return texture.GetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    Color BilinearInterpolation(Texture2D texture, float x, float y)
    {
        float x1 = Mathf.Floor(x);
        float x2 = Mathf.Ceil(x);
        float y1 = Mathf.Floor(y);
        float y2 = Mathf.Ceil(y);

        // Get the four neighboring pixel colors
        Color c1 = texture.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x1), 0, texture.width - 1), Mathf.Clamp(Mathf.RoundToInt(y1), 0, texture.height - 1));
        Color c2 = texture.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x2), 0, texture.width - 1), Mathf.Clamp(Mathf.RoundToInt(y1), 0, texture.height - 1));
        Color c3 = texture.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x1), 0, texture.width - 1), Mathf.Clamp(Mathf.RoundToInt(y2), 0, texture.height - 1));
        Color c4 = texture.GetPixel(Mathf.Clamp(Mathf.RoundToInt(x2), 0, texture.width - 1), Mathf.Clamp(Mathf.RoundToInt(y2), 0, texture.height - 1));

        // Calculate fractional parts of x and y
        float fracX = x - x1;
        float fracY = y - y1;

        // Perform bilinear interpolation
        Color topInterpolation = Color.Lerp(c1, c2, fracX);
        Color bottomInterpolation = Color.Lerp(c3, c4, fracX);
        Color finalInterpolation = Color.Lerp(topInterpolation, bottomInterpolation, fracY);

        return finalInterpolation;
    }

    void GenerateFromTexture()
    {
        tilemap.ClearAllTiles(); // Clear any existing tiles in the Tilemap

        Func<Texture2D, float, float, Color> interpolationFunc;
        switch (interpolation)
        {
            case Interpolation.NearestNeighbour:
                interpolationFunc = NearestNeighbourInterpolation;
                break;
            case Interpolation.Bilinear:
                interpolationFunc = BilinearInterpolation;
                break;
            default:
                interpolationFunc = NearestNeighbourInterpolation;
                break;
        }

        // calculate stride (the width of the texture in pixels)
        float hstride = referenceTexture.width / (float)(width - 1);
        float vstride = referenceTexture.height / (float)(height - 1);

        // Loop through all the pixels in the reference texture
        float u, v;

        for (int x = 0; x < width; x++)
        {
            u = x * hstride;

            for (int y = 0; y < height; y++)
            {
                v = y * vstride;

                // Get the color of the pixel at the current position
                Color pixelColor = interpolationFunc(referenceTexture, u, v);

                TileBase closestTile = null;
                if (referenceTile == ReferenceTile.TileGrid)
                {
                    closestTile = FindClosestTile(pixelColor);
                }
                else if (referenceTile == ReferenceTile.RuleTile)
                {
                    closestTile = pixelColor.a > 0.1f ? ruleTile : null;
                }

                // Set the tile in the tilemap at the current position
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePosition, closestTile);
            }
        }
    }

    TileBase FindClosestTile(Color targetColor)
    {
        // Check if targetColor's alpha is too low
        if (targetColor.a < 0.1f) // Adjust the threshold value as needed
        {
            return null; // Return null or an empty tile when alpha is low
        }

        Tile closestTile = availableTiles[0];

        float closestDistance = ColorDistance(targetColor, closestTile.color);

        foreach (var tile in availableTiles)
        {
            float distance = ColorDistance(targetColor, tile.color);
            if (distance < closestDistance)
            {
                closestTile = tile;
                closestDistance = distance;
            }
        }

        return closestTile;
    }


    float ColorDistance(Color a, Color b)
    {
        // You can use any distance metric here. Euclidean distance is a common choice.
        return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2));
    }
}
