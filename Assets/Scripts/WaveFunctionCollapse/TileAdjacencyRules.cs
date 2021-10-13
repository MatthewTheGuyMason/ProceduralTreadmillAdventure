using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAdjacencyRules", menuName = "ScriptableObject/TileAdjacencyRules")]
public class TileAdjacencyRules : ScriptableObject
{
    public enum Sockets
    {
        Above = 0,
        Below = 1,
        Front = 2,
        Back = 3,
        Left = 4,
        Right = 5,

        Count = 6
    }

    [System.Serializable]
    public struct SideValidTiles
    {
        [SerializeField]
        public List<int> validTileForSide;
    }

    // All the tiles that are valid for its sides
    [SerializeField]
    private List<SideValidTiles> validTilesForSides;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
