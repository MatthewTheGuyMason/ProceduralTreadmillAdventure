//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               TileSet.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              A scriptable object containing set of tile component attached to prefabs 
//====================================================================================================================================================================================================================================================================================================================================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object containing set of tile component attached to prefabs 
/// </summary>
public class TileSet : ScriptableObject
{
    [SerializeField] [Tooltip("The tile component attached to different prefabs")]
    private List<TileComponent> tilePrefabs;

    /// <summary>
    /// The tile component attached to different prefabs
    /// </summary>
    public List<TileComponent> TilePrefabs
    {
        get
        {
            return tilePrefabs;
        }
        set
        {
            tilePrefabs = value;
        }
    }
}
