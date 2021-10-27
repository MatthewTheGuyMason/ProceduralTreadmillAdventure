

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{


    public enum TileType
    {
        Undecided   = 0b00000000,
        Empty       = 0b00000001,
        Floor       = 0b00000010,
        Cliff       = 0b00000100,
        Plant       = 0b00001000
    }

    [SerializeField]
    private int id = -1;

    public TileType tileType;

    public Vector3Int gridCoordinates;

    public int slotID;

    public SocketData socketData;

    public int ID
    {
        get
        {
            return id;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //validSidesForTiles = new List<int>[(int)Sockets.Count];
        //for (int i = 0; i < validSidesForTiles.Length; ++i)
        //{
        //    validSidesForTiles[i] = new List<int>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
