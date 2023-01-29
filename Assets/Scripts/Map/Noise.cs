/* Credit goes to Sebastian Lague, for its work on procedural landmass
 * generation. */

using System;
using UnityEngine;

public class NoiseGenerator
{
    public float scale;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int octaves;
    public Vector2[] octaveOffsets;
    public float maxHeight = 0;
    public float minHeight = float.MaxValue;

    [SerializeField]
    int _seed;

    public NoiseGenerator(int numberOfIterations, float scale = 25, int octaves = 5, float persistance = 0.5f, float lacunarity = 2f)
    {
        this.scale = scale;
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
        octaveOffsets = new Vector2[octaves];

        ValidateValues();

        System.Random pRng = new System.Random();
        _seed = pRng.Next();
        System.Random seededRng = new System.Random(_seed);

        for (int oct = 0; oct < octaves; oct++)
        {
            float octOffsetX = (float)seededRng.NextDouble() * seededRng.Next(0, 100000);
            float octOffsetY = (float)seededRng.NextDouble() * seededRng.Next(0, 100000);
            octaveOffsets[oct] = new Vector2(octOffsetX, octOffsetY);
        }

        for (int i = 0; i < numberOfIterations; i++)
        {
            float x = pRng.Next(-100000, 100000);
            float y = pRng.Next(-100000, 100000);

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int oct = 0; oct < octaves; oct++)
            {
                float sampleX = (x + octaveOffsets[oct].x) / scale * frequency;
                float sampleY = (y + octaveOffsets[oct].y) / scale * frequency;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;

                // FIXME: Avoid extremely high sampleX and sampleY values not 
                // well processed by PerlinNoise
                if (frequency > 10000)
                {
                    Debug.LogWarning("Lacunarity or octaves is too high, which leads to perlin noise incorrect inputs.");
                    break;
                }
            }

            if (noiseHeight > maxHeight)
            {
                maxHeight = noiseHeight;
            }
            if (noiseHeight < minHeight)
            {
                minHeight = noiseHeight;
            }
        }

    }

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }

    public float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float offsetX, float offsetY)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float amplitude;
        float frequency;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x + offsetX) / scale * frequency;
                    float sampleY = (y + octaveOffsets[i].y + offsetY) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                    
                    // FIXME: Avoid extremely high sampleX and sampleY values not 
                    // well processed by PerlinNoise
                    if (frequency > 10000)
                    {
                        break;
                    }
                }

                noiseMap[x, y] = noiseHeight;
                noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

}