using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileExampleGrid : MonoBehaviour
{
    [SerializeField]
    private TileData[][][] tileGrid;

    [SerializeField]
    private Vector3Int gridDimensions;

    [SerializeField]
    private GameObject canvasTilePrefabs;

    //public Dictionary<int, TileAdjacencyRules> createdRules;

    private void Start()
    {
        PlaceCanvasTiles();
    }

    private void OnDrawGizmos()
    {
        
    }

    /// <summary>
    /// Place all the tiles that the user can use to place down more tiles
    /// </summary>
    private void PlaceCanvasTiles()
    {
        tileGrid = new TileData[gridDimensions.x][][];
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            tileGrid[i] = new TileData[gridDimensions.y][];
            for (int k = 0; k < tileGrid.Length; ++k)
            {
                tileGrid[i][k] = new TileData[gridDimensions.z];
            }
        }

        for (int i = 0; i < gridDimensions.x; ++i)
        {
            for (int j = 0; j < gridDimensions.z; ++j)
            {
                tileGrid[i][0][j] = GameObject.Instantiate(canvasTilePrefabs, new Vector3(i - (float)gridDimensions.x * 0.5f, 0f, j - (float)gridDimensions.z * 0.5f), transform.rotation).GetComponent<TileData>();
                tileGrid[i][0][j].GridCoordinates = new Vector3Int(i, 0, j);
            }
        }
    }

    public bool TryAddTile(TileData tileToAdd, Vector3Int gridCoordinates)
    {
        if (CheckIfGridCoordinatesValid(gridCoordinates))
        {
            tileGrid[gridCoordinates.x][gridCoordinates.y][gridCoordinates.z] = tileToAdd;
            tileToAdd.GridCoordinates = gridCoordinates;
            return true;
        }
        return false;
    }

    public bool CheckIfGridCoordinatesValid(Vector3Int gridCoordinates)
    {
        if (gridCoordinates.x > -1 && gridCoordinates.x < gridDimensions.x)
        {
            if (gridCoordinates.y > -1 && gridCoordinates.y < gridDimensions.y)
            {
                if (gridCoordinates.z > -1 && gridCoordinates.z < gridDimensions.z)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
