//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               ExampleGridController.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              A component used to mark an object as the parent of an example grid and control is functions
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component used to mark an object as the parent of an example grid and control is functions
/// </summary>
public class ExampleGridController : MonoBehaviour
{
    #region Private Serialized Fields
    [SerializeField] [Tooltip("The dimensions the grid is expected to be")]
    private Vector3Int gridDimensions;

    [SerializeField] [Tooltip("The objects in the grid that act as parent for the cells in the grid")]
    private List<GameObject> objectsParentsInGrid;
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the index that for an item of a element of 1D array from a 3D Grid 
    /// </summary>
    /// <param name="coordinate">The coordinate of the grid cell to get the index of</param>
    /// <param name="gridDimensions">The sizes of the grid that would contain the elements</param>
    /// <returns>The index that for an item of a element of 1D array from a 3D Grid</returns>
    public int GetIndexOfCoordinate(Vector3Int coordinate, Vector3Int gridDimensions)
    {
        return gridDimensions.y * gridDimensions.z * coordinate.x + gridDimensions.z * coordinate.y + coordinate.z;
    }

    /// <summary>
    /// Adjusts the grid so that it has the new dimensions whilst retaining the tiles it already contained, provided they are still within the new dimensions
    /// </summary>
    /// <param name="newDimensions">The new dimensions to adjust the grid to</param>
    /// <param name="defaultTile">The tile that will be added as child to all the new parent cell that did not contain any tiles</param>
    public void AdjustGridDimensions(Vector3Int newDimensions, GameObject defaultTile = null)
    {
        // Create a jagged array to store the new objects temporarily
        GameObject[] newTileParents = new GameObject[newDimensions.x * newDimensions.y * newDimensions.z];
        // Create a new grid of new tile parents
        for (int x = 0; x < newDimensions.x; ++x)
        {
            for (int y = 0; y < newDimensions.y; ++y)
            {
                for (int z = 0; z < newDimensions.z; ++z)
                {
                    int coordinatesIndex = GetIndexOfCoordinate(new Vector3Int(x, y, z), newDimensions);
                    newTileParents[coordinatesIndex] = (new GameObject(x.ToString() + ", " + y.ToString() + ", " + z.ToString()));
                    newTileParents[coordinatesIndex].transform.SetParent(transform);
                    newTileParents[coordinatesIndex].transform.localPosition = new Vector3(x, y, z);
                    if (defaultTile != null)
                    {
                        GameObject.Instantiate(defaultTile, newTileParents[coordinatesIndex].transform.position, newTileParents[coordinatesIndex].transform.rotation, newTileParents[coordinatesIndex].transform);
                    }
                }
            }
        }

        // Copy over the next the tiles from the last grid
        for (int x = 0; x < gridDimensions.x && x < newDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y && y < newDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z && z < newDimensions.z; ++z)
                {
                    int oldCoordinatesIndex = GetIndexOfCoordinate(new Vector3Int(x, y, z), gridDimensions);
                    int newCoordinatesIndex = GetIndexOfCoordinate(new Vector3Int(x, y, z), newDimensions);
                    // Destroy the default tiles if ones are being copied;
                    if (objectsParentsInGrid[oldCoordinatesIndex].transform.childCount > 0)
                    {
                        for (int i = 0; i < newTileParents[newCoordinatesIndex].transform.childCount; ++i)
                        {
                            Debug.Log(newTileParents[newCoordinatesIndex].transform.childCount);
                            DestroyImmediate(newTileParents[newCoordinatesIndex].transform.GetChild(i).gameObject);
                            //--i;
                        }

                    }
                    // Add the tiles from the previous grid
                    for (int i = 0; i < objectsParentsInGrid[oldCoordinatesIndex].transform.childCount; ++i)
                    {
                        objectsParentsInGrid[oldCoordinatesIndex].transform.GetChild(i).SetParent(newTileParents[newCoordinatesIndex].transform);
                        --i;
                    }
                }
            }
        }

        // Delete the old grid
        // Create a new grid of new tile parents
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    DestroyImmediate(objectsParentsInGrid[GetIndexOfCoordinate(new Vector3Int(x, y, z), gridDimensions)]);
                }
            }
        }

        // Make this new grid the current grid
        objectsParentsInGrid = new List<GameObject>(newTileParents);
        gridDimensions = newDimensions;
    }

    /// <summary>
    /// Builds all the parent objects for the grid cells
    /// </summary>
    /// <param name="newDimensions">The dimensions the grid will be after the object are made</param>
    /// <param name="defaultTile">The tile that will be added as child to all the new parent cells</param>
    public void BuildGridObjectParents(Vector3Int newDimensions, GameObject defaultTile = null)
    {
        // Create a jagged array to store the new objects temporarily
        objectsParentsInGrid = new List<GameObject>(newDimensions.x * newDimensions.y * newDimensions.z);

        for (int x = 0; x < newDimensions.x; ++x)
        {
            for (int y = 0; y < newDimensions.y; ++y)
            {
                for (int z = 0; z < newDimensions.z; ++z)
                {
                    GameObject newEmpty = new GameObject(x.ToString() + ", " + y.ToString() + ", " + z.ToString());
                    newEmpty.transform.SetParent(transform);
                    newEmpty.transform.localPosition = new Vector3(x, y, z);
                    if (defaultTile != null)
                    {
                        GameObject.Instantiate(defaultTile, newEmpty.transform.position, newEmpty.transform.rotation, newEmpty.transform);
                    }
                    objectsParentsInGrid.Add(newEmpty);
                }
            }
        }
        gridDimensions = newDimensions;
    }

    /// <summary>
    /// Chances to the all the biomes type of tiles within a given ID within the grid
    /// </summary>
    /// <param name="idToUpdate">The ID of the tiles to chance the biomes type of</param>
    /// <param name="typeToChangeTo">The biomes type to chance the tiles too</param>
    public void ChanceBiomeTypeOfTilesWithID(int idToUpdate, TileData.BiomeType typeToChangeTo)
    {
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    int coordinatesIndex = GetIndexOfCoordinate(new Vector3Int(x, y, z), gridDimensions);
                    TileComponent tileComponent = objectsParentsInGrid[coordinatesIndex].GetComponentInChildren<TileComponent>();
                    if (tileComponent.TileData.ID == idToUpdate)
                    {
                        tileComponent.TileData.TileBiomeType = typeToChangeTo;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fills the cells inside a given area 
    /// </summary>
    /// <param name="bottemLeft">The grid cell in the smallest x,y,z of the area</param>
    /// <param name="topRight">The grid cell in the smallest x,y,z of the area</param>
    /// <param name="tile">The tile to replace all the tiles in the area with</param>
    public void FillInAreaWithTile(Vector3Int bottemLeft, Vector3Int topRight, GameObject tile)
    {
        topRight = Vector3Int.Min(topRight, gridDimensions - Vector3Int.one);
        bottemLeft = Vector3Int.Max(bottemLeft, Vector3Int.zero);
        for (int x = bottemLeft.x; x <= topRight.x; ++x)
        {
            for (int y = bottemLeft.y; y <= topRight.y; ++y)
            {
                for (int z = bottemLeft.z; z <= topRight.z; ++z)
                {
                    int coordinatesIndex = GetIndexOfCoordinate(new Vector3Int(x, y, z), gridDimensions);
                    if (coordinatesIndex > 0 && coordinatesIndex < objectsParentsInGrid.Count)
                    {
                        // Destroy the old tiles
                        if (objectsParentsInGrid[coordinatesIndex].transform.childCount > 0)
                        {
                            for (int i = 0; i < objectsParentsInGrid[coordinatesIndex].transform.childCount; ++i)
                            {
                                DestroyImmediate(objectsParentsInGrid[coordinatesIndex].transform.GetChild(i).gameObject);
                            }
                        }
                        // Instantiate the new ones
                        GameObject.Instantiate(tile, objectsParentsInGrid[coordinatesIndex].transform.position, objectsParentsInGrid[coordinatesIndex].transform.rotation, objectsParentsInGrid[coordinatesIndex].transform);
                    }
                }
            }
        }
    }
    #endregion
}
