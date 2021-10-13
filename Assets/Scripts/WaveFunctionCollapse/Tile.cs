using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private int id;

    public Vector3Int gridCoordinates;

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
