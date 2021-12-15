//======================================================================================================================================================================================================================================================================================================================================================
//  Name:               TileData.cs
//  Author:             Matthew Mason
//  Date Created:       29/10/2021
//  Date Last Modified: 15/12/2021
//  Brief:              Class for storing data about a tile both inside and outside of the grid
//======================================================================================================================================================================================================================================================================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for storing data about a tile both inside and outside of the grid
/// </summary>
[System.Serializable]
public class TileData
{
    #region Public Enumerations
    /// <summary>
    /// The type of tiles that can exist
    /// </summary>
    public enum TileTypes
    {
        Undecided   = 0b00000000,
        Empty       = 0b00000001,
        Floor       = 0b00000010,
        Cliff       = 0b00000100,
        Plant       = 0b00001000
    }

    public enum BiomeType
    {
        Desert = 2,
        DesertToGrassland = 1,
        Grassland = 0,
        GrasslandToSnow = -1,
        Tundra = -2,

        NA = 3
    }
    #endregion

    #region Protected Serialized Fields
    [SerializeField] 
    [Tooltip("The identifier number for the tile")]
    protected int id = -1;

    [SerializeField]
    [Tooltip("The weighting for how likely this tile is to be picked")]
    protected float[] baseTileWeights;

    [SerializeField]
    [Tooltip("The socket data used for the tile")]
    protected SocketData tileSocketData;

    [SerializeField]
    [Tooltip("The type of tile this is")]
    protected TileTypes tileType;

    [SerializeField]
    [Tooltip("The type of biomes that this type of tile supposed to go into")]
    protected BiomeType biomeType;
    #endregion

    #region Public Properties
    /// <summary>
    /// The identifier number for the tile
    /// </summary>
    public int ID
    {
        get
        {
            return id;
        }
    }

    /// <summary>
    /// The weighting for how likely this tile is to be picked
    /// </summary>
    public float[] Weights
    {
        get
        {
            return baseTileWeights;
        }
        set
        {
            baseTileWeights = value;
        }
    }

    /// <summary>
    /// The type of tile this is
    /// </summary>
    public TileTypes TileType 
    { 
        get
        {
            return tileType;
        }
    }

    /// <summary>
    /// The socket data used for the tile 
    /// </summary>
    public SocketData TileSocketData
    {
        get
        {
            return tileSocketData;
        }
        set
        {
            tileSocketData = value;
        }
    }

    /// <summary>
    /// The type of tile this is
    /// </summary>
    public BiomeType TileBiomeType
    {
        get
        {
            return biomeType;
        }
        set
        {
            biomeType = value;
        }
    }

    /// <summary>
    /// The 3D grid coordinates this tile is located at
    /// </summary>
    public Vector3Int GridCoordinates { get; set; }
    #endregion

    #region Public Methods
    /// <summary>
    /// Constructor
    /// </summary>
    public TileData()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="tileData"></param>
    public TileData(TileData tileData)
    {
        id = tileData.id;
        baseTileWeights = new float[tileData.baseTileWeights.Length];
        tileData.Weights.CopyTo(baseTileWeights, 0);
        tileSocketData = tileData.tileSocketData;
        tileType = tileData.tileType;
        GridCoordinates = tileData.GridCoordinates;
        biomeType = tileData.biomeType;
    }

    /// <summary>
    /// Returns the weight of the tile based on it y position relative to grid 
    /// </summary>
    /// <returns>The weight of the tile based on it y position relative to grid t</returns>
    public virtual float GetWeight(int yGridCoords, int gridHeight)
    {
        float gridCoordDistance = (float)baseTileWeights.Length / (float)gridHeight;   // Get how much each index is work in relation to the grid height
        float gridPosition = yGridCoords * gridCoordDistance;           // Move that far up the weight indexes
        int lowerIndex = Mathf.FloorToInt(gridPosition);                // Get the lower index
        int upperIndex = Mathf.CeilToInt(gridPosition);                 // Get the upper index
        float leftOverDecimal = gridPosition - lowerIndex;              // Get the decimal that is left over between the index
        if (upperIndex >= baseTileWeights.Length)
        {
            return baseTileWeights[baseTileWeights.Length - 1];
        }
        return Mathf.Lerp(baseTileWeights[lowerIndex], baseTileWeights[upperIndex], leftOverDecimal); // return the weight lerp between the 2 closet values using the demical
    }
    #endregion
}
