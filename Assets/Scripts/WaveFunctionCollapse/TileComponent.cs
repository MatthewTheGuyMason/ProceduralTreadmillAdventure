using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponent : MonoBehaviour
{
    [SerializeField] [Tooltip("The tile data this tile contains")]
    private TileData tileData;

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
