//======================================================================================================================================================================================================================================================================================================================================================
//  Name:               Tile.cs
//  Author:             Matthew Mason
//  Date Created:       29/10/2021
//  Date Last Modified: 29/10/2021
//  Brief:              
//======================================================================================================================================================================================================================================================================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    #region Enumerations
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
    #endregion

    #region Protected Serialized Fields
    [SerializeField] 
    [Tooltip("The identifier number for the tile")]
    protected int id = -1;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("The weighting for how likely this tile is to be picked")]
    protected float baseTileWeight;

    [SerializeField]
    [Tooltip("The socket data used for the tile")]
    protected SocketData tileSocketData;

    [SerializeField]
    [Tooltip("The type of tile this is")]
    protected TileTypes tileType;
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
    public float BaseTileWeight
    {
        get
        {
            return baseTileWeight;
        }
        set
        {
            baseTileWeight = value;
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
    /// The 3D grid coordinates this tile is located at
    /// </summary>
    public Vector3Int GridCoordinates { get; set; }

    /// <summary>
    /// Returns the weight of the 
    /// </summary>
    /// <returns></returns>
    public virtual float GetWeight(TileGrid tileGrid)
    {
        return baseTileWeight;
    }
    #endregion

    #region Public Methods
    public TileData()
    {
    }

    public TileData(TileData tileData)
    {
        id = tileData.id;
        baseTileWeight = tileData.baseTileWeight;
        tileSocketData = tileData.tileSocketData;
        tileType = tileData.tileType;
        GridCoordinates = tileData.GridCoordinates;

    }
    #endregion
}
