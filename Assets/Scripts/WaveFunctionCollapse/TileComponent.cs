//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               TileComponent.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              Script used to store tile data so it can be attached to an object
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used to store tile data so it can be attached to an object
/// </summary>
public class TileComponent : MonoBehaviour
{
    [SerializeField] [Tooltip("The tile data this tile contains")]
    private TileData tileData;

    /// <summary>
    /// Property assessor for the contained tile data
    /// </summary>
    public TileData TileData
    {
        get
        {
            return tileData;
        }
        set
        {
            tileData = value;
        }
    }
}
