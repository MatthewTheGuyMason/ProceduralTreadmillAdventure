using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component used to mark an object as the parent of an example grid and control is functions
/// </summary>
public class ExampleGridController : MonoBehaviour
{
    [SerializeField]
    private Vector3Int gridDimensions;

    [SerializeField]
    private List<GameObject> objectsParentsInGrid;

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
                    // Destroy the old tiles
                    if (objectsParentsInGrid[coordinatesIndex].transform.childCount > 0)
                    {
                        for (int i = 0; i < objectsParentsInGrid[coordinatesIndex].transform.childCount; ++i)
                        {
                            DestroyImmediate(objectsParentsInGrid[coordinatesIndex].transform.GetChild(i).gameObject);
                            //--i;
                        }
                    }
                    // Instantiate the new ones
                    GameObject.Instantiate(tile, objectsParentsInGrid[coordinatesIndex].transform.position, objectsParentsInGrid[coordinatesIndex].transform.rotation, objectsParentsInGrid[coordinatesIndex].transform);
                }
            }
        }
    }

    public int GetIndexOfCoordinate(Vector3Int coordinate, Vector3Int gridDimensions)
    {
        return gridDimensions.y * gridDimensions.z * coordinate.x + gridDimensions.z * coordinate.y + coordinate.z;
    }

    public void UpdateBiomeTypeOfTilesWithID(int idToUpdate, TileData.BiomeType typeToChangeTo)
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
}
