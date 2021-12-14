using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeTransitionaryData", menuName = "ScriptableObject/BiomeTransitionaryData")]
public class BiomeTransitionaryData : ScriptableObject
{
    [System.Serializable]
    public struct BiomeWeights
    {
        public float grasslandUnitInterval;
        public float desertUnitInterval;
        public float tundraUnitInterval;

        public BiomeWeights(float grasslandUnitInterval, float desertUnitInterval, float tundraUnitInterval)
        {
            this.grasslandUnitInterval = grasslandUnitInterval;
            this.desertUnitInterval = desertUnitInterval;
            this.tundraUnitInterval = tundraUnitInterval;
        }

        public static BiomeWeights Lerp(BiomeWeights a, BiomeWeights b, float t)
        {
            return new BiomeWeights(Mathf.Lerp(a.grasslandUnitInterval, b.grasslandUnitInterval, t),
                Mathf.Lerp(a.desertUnitInterval, b.desertUnitInterval, t),
                Mathf.Lerp(a.tundraUnitInterval, b.tundraUnitInterval, t));
        }
    }

    [System.Serializable]
    public struct TranstionKeyframe
    {
        public BiomeWeights biomeWeights;

        [Min(1)]
        public int numberOfTilesTileNextKeyframe;
    }

    public TranstionKeyframe[] transtionKeyframes;

    public BiomeWeights GetValuesAtTile(int tileNumber)
    {
        // Validation Checks
        if (transtionKeyframes == null)
        {
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (transtionKeyframes.Length == 0)
        {
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (transtionKeyframes.Length == 1)
        {
            return transtionKeyframes[0].biomeWeights;
        }
        int totalTilesCovered = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            totalTilesCovered += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        int tilePosition = tileNumber % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + transtionKeyframes[i].numberOfTilesTileNextKeyframe)
            {
                int nextIndex = i + 1;
                // Loop back around if next tile doesn't exist
                if (nextIndex == transtionKeyframes.Length)
                {
                    nextIndex = 0;
                }
                return BiomeWeights.Lerp(transtionKeyframes[i].biomeWeights, transtionKeyframes[nextIndex].biomeWeights, (1f / transtionKeyframes[i].numberOfTilesTileNextKeyframe) * (tilePosition - currentTilePosition));
            }
            currentTilePosition += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }

    public BiomeWeights LastKeyBiomeWeightsFromTileNumber(int tileNumber)
    {
        // Validation Checks
        if (transtionKeyframes == null)
        {
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (transtionKeyframes.Length == 0)
        {
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (transtionKeyframes.Length == 1)
        {
            return transtionKeyframes[0].biomeWeights;
        }

        int totalTilesCovered = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            totalTilesCovered += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        int tilePosition = tileNumber % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + transtionKeyframes[i].numberOfTilesTileNextKeyframe)
            {
                return transtionKeyframes[i].biomeWeights;
            }
            currentTilePosition += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }

    public BiomeWeights GetKeyFrameBeforeLast(int tileNumber)
    {
        // Validation Checks
        if (transtionKeyframes == null)
        {
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (transtionKeyframes.Length == 0)
        {
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (transtionKeyframes.Length == 1)
        {
            return transtionKeyframes[0].biomeWeights;
        }

        int totalTilesCovered = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            totalTilesCovered += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        int tilePosition = tileNumber % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + transtionKeyframes[i].numberOfTilesTileNextKeyframe)
            {
                if (i == 0)
                {
                    return transtionKeyframes[transtionKeyframes.Length - 1].biomeWeights;
                }
                return transtionKeyframes[i - 1].biomeWeights;
            }
            currentTilePosition += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }

    public BiomeWeights GetNextKey(int tileNumber)
    {
        // Validation Checks
        if (transtionKeyframes == null)
        {
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (transtionKeyframes.Length == 0)
        {
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (transtionKeyframes.Length == 1)
        {
            return transtionKeyframes[0].biomeWeights;
        }
        int totalTilesCovered = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            totalTilesCovered += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        int tilePosition = tileNumber % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < transtionKeyframes.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + transtionKeyframes[i].numberOfTilesTileNextKeyframe)
            {
                int nextIndex = i + 1;
                // Loop back around if next tile doesn't exist
                if (nextIndex == transtionKeyframes.Length)
                {
                    nextIndex = 0;
                }
                return transtionKeyframes[nextIndex].biomeWeights;
            }
            currentTilePosition += transtionKeyframes[i].numberOfTilesTileNextKeyframe;
        }
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }
}
