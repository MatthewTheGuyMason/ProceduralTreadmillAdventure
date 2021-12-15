//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               BiomeTransitionaryData.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              A Scriptable Object class that stores a set of biomes weight that can be polled to get biomes weights at a x position of tiles
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable Object class that stores a set of biomes weight that can be polled to get biomes weights at a x position of tiles
/// </summary>
[CreateAssetMenu(fileName = "BiomeTransitionaryData", menuName = "ScriptableObject/BiomeTransitionaryData")]
public class BiomeTransitionaryData : ScriptableObject
{
    #region Public Structures 
    /// <summary>
    /// The structure used to define the additional spawn chances of tiles from each biomes
    /// </summary>
    [System.Serializable]
    public struct BiomeWeights
    {
        /// <summary>
        /// The chance of spawning tiles from the grassland biomes
        /// </summary>
        public float grasslandUnitInterval;
        /// <summary>
        /// The chance of spawning tiles from the desert biomes
        /// </summary>
        public float desertUnitInterval;
        /// <summary>
        /// The chance of spawning tiles from the tundra biomes
        /// </summary>
        public float tundraUnitInterval;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="grasslandUnitInterval">The chance of spawning tiles from the grassland biomes</param>
        /// <param name="desertUnitInterval">The chance of spawning tiles from the desert biomes</param>
        /// <param name="tundraUnitInterval">The chance of spawning tiles from the tundra biomes</param>
        public BiomeWeights(float grasslandUnitInterval, float desertUnitInterval, float tundraUnitInterval)
        {
            this.grasslandUnitInterval = grasslandUnitInterval;
            this.desertUnitInterval = desertUnitInterval;
            this.tundraUnitInterval = tundraUnitInterval;
        }

        /// <summary>
        /// Returns a new biomes weight that is an interpolated version
        /// </summary>
        /// <param name="a">The biomes weight that the interpolation starts with at 0 t</param>
        /// <param name="b">The biomes weight that the interpolation ends with at 1</param>
        /// <param name="t">The t value for where the value should be between a and b as a unit interval</param>
        /// <returns>A new biomes weight that is an interpolated version</returns>
        public static BiomeWeights Lerp(BiomeWeights a, BiomeWeights b, float t)
        {
            return new BiomeWeights(Mathf.Lerp(a.grasslandUnitInterval, b.grasslandUnitInterval, t),
                Mathf.Lerp(a.desertUnitInterval, b.desertUnitInterval, t),
                Mathf.Lerp(a.tundraUnitInterval, b.tundraUnitInterval, t));
        }
    }

    /// <summary>
    /// structure of a set of biomes weight and how far they stretch before the next key position
    /// </summary>
    [System.Serializable]
    public struct TranstionKeyPosition
    {
        /// <summary>
        /// The biomes weightings that should be used when
        /// </summary>
        public BiomeWeights biomeWeights;

        /// <summary>
        /// The number of tiles along the x that this biomes weight will effect beyond its 
        /// </summary>
        [Min(1)]
        public int numberOfTilesTileNextKeyPosition;
    }
    #endregion

    #region Public Variables
    [SerializeField] [Tooltip("The keys used to the define the transitions across the biomes weights")]
    public TranstionKeyPosition[] transtionKeyPositions;
    #endregion

    #region Public Properties
    /// <summary>
    /// The keys used to the define the transitions across the biomes weights 
    /// </summary>
    public TranstionKeyPosition[] TranstionKeyPositions
    {
        get
        {
            return transtionKeyPositions;
        }
        set
        {
            transtionKeyPositions = value;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the biomes weights for a tile with the given x position
    /// </summary>
    /// <param name="tileXPosition">The position of a tiles along the x axis</param>
    /// <returns>The biomes weights for a tile with the given x position</returns>
    public BiomeWeights GetBiomeWeightsAtTile(int tileXPosition)
    {
        // Validation Checks
        if (TranstionKeyPositions == null)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (TranstionKeyPositions.Length == 0)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (TranstionKeyPositions.Length == 1)
        {
            return TranstionKeyPositions[0].biomeWeights;
        }
        int totalTilesCovered = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            totalTilesCovered += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        int tilePosition = tileXPosition % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition)
            {
                int nextIndex = i + 1;
                // Loop back around if next tile doesn't exist
                if (nextIndex == TranstionKeyPositions.Length)
                {
                    nextIndex = 0;
                }
                return BiomeWeights.Lerp(TranstionKeyPositions[i].biomeWeights, TranstionKeyPositions[nextIndex].biomeWeights, (1f / TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition) * (tilePosition - currentTilePosition));
            }
            currentTilePosition += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        #endif
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }

    /// <summary>
    /// Returns the biomes weights of the key-frame that was last reach before the given 
    /// </summary>
    /// <param name="tileXPosition">The position of a tiles along the x axis</param>
    /// <returns>the biomes weights of the key-frame that was last reach before the given </returns>
    public BiomeWeights GetKeyFrameBeforeLast(int tileXPosition)
    {
        // Validation Checks
        if (TranstionKeyPositions == null)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (TranstionKeyPositions.Length == 0)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (TranstionKeyPositions.Length == 1)
        {
            return TranstionKeyPositions[0].biomeWeights;
        }

        int totalTilesCovered = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            totalTilesCovered += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        int tilePosition = tileXPosition % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition)
            {
                if (i == 0)
                {
                    return TranstionKeyPositions[TranstionKeyPositions.Length - 1].biomeWeights;
                }
                return TranstionKeyPositions[i - 1].biomeWeights;
            }
            currentTilePosition += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        #endif
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }

    /// <summary>
    /// Returns the biomes weights of the key that would appear next from the tile grid
    /// </summary>
    /// <param name="tileXPosition">The position of a tiles along the x axis</param>
    /// <returns>The biomes weights of the key that would appear next from the tile grid</returns>
    public BiomeWeights GetNextKey(int tileXPosition)
    {
        // Validation Checks
        if (TranstionKeyPositions == null)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was null when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }
        if (TranstionKeyPositions.Length == 0)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("transtionKeyframes was had no keyframes when referenced in ValuesAtTile of BiomeTransitionaryData", this);
            #endif
            return new BiomeWeights(0.0f, 0.0f, 0.0f);
        }

        if (TranstionKeyPositions.Length == 1)
        {
            return TranstionKeyPositions[0].biomeWeights;
        }
        int totalTilesCovered = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            totalTilesCovered += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        int tilePosition = tileXPosition % totalTilesCovered;
        int currentTilePosition = 0;
        for (int i = 0; i < TranstionKeyPositions.Length; ++i)
        {
            // If the tile is within these key-frames
            if (tilePosition < currentTilePosition + TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition)
            {
                int nextIndex = i + 1;
                // Loop back around if next tile doesn't exist
                if (nextIndex == TranstionKeyPositions.Length)
                {
                    nextIndex = 0;
                }
                return TranstionKeyPositions[nextIndex].biomeWeights;
            }
            currentTilePosition += TranstionKeyPositions[i].numberOfTilesTileNextKeyPosition;
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("Somehow the biome weights could not be found during ValuesAtTile in BiomeTransitionaryData");
        #endif
        return new BiomeWeights(0.0f, 0.0f, 0.0f);
    }
    #endregion
}
