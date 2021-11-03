using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveCollapseExampleEditorWindow : EditorWindow
{
    private TileComponent[][][] tileGrid;

    private Vector3Int gridDimensions;

    private GameObject gridParent;

    private GameObject defaultTilePrefab;

    private GameObject defaultFloorTilePrefab;

    [SerializeField]
    private GameObject[] tilePrefabsInUse;

    private SerializedProperty tilePrefabsInUseProperty;

    private bool showPrefabsInUseArray;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Wave Collapse Example Editor Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        WaveCollapseExampleEditorWindow window = (WaveCollapseExampleEditorWindow)EditorWindow.GetWindow(typeof(WaveCollapseExampleEditorWindow));
        window.Show();
    }

    private void OnGUI()
    {
        gridDimensions = EditorGUILayout.Vector3IntField("Grid Dimensions", gridDimensions);
        gridParent = (GameObject)EditorGUILayout.ObjectField(gridParent, typeof(GameObject), true);
        defaultTilePrefab = (GameObject)EditorGUILayout.ObjectField(defaultTilePrefab, typeof(GameObject), false);

        int tilePrefabsInUseArraySize = EditorGUILayout.IntField("Tile Prefabs In Use Size", tilePrefabsInUse.Length);
        if (tilePrefabsInUseArraySize != tilePrefabsInUse.Length)
        {
            GameObject[] newArray = new GameObject[tilePrefabsInUseArraySize];
            tilePrefabsInUse.CopyTo(newArray, 0);
            tilePrefabsInUse = newArray;
        }
        if (EditorGUILayout.DropdownButton(new GUIContent("Show Tile Prefabs In Use Array"), FocusType.Passive))
        {
            for (int i = 0; i < tilePrefabsInUseArraySize; ++i)
            {
                tilePrefabsInUse[i] = (GameObject)EditorGUILayout.ObjectField(gridParent, typeof(GameObject), false);
            }
        }

        if (gridParent != null)
        {
            if (GUILayout.Button("Build Grid Template"))
            {
                BuildGridObjectParents();
            }
        }
        if (GUILayout.Button("Generate Build Data"))
        {
            ExampleGridData exampleGridData = BuildExampleGridData();

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void BuildGridObjectParents()
    {
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.x; ++z)
                {
                    GameObject newEmpty = new GameObject(x.ToString() + ", " + y.ToString() + ", " + z.ToString());
                    newEmpty.transform.SetParent(gridParent.transform);
                    newEmpty.transform.localPosition = new Vector3(x, y, z);
                    if (defaultTilePrefab != null)
                    {
                        GameObject.Instantiate(defaultTilePrefab, newEmpty.transform.position, newEmpty.transform.rotation, newEmpty.transform);
                    }
                }
            }
        }
    }

    private ExampleGridData BuildExampleGridData()
    {
        ExampleGridData exampleGridData = new ExampleGridData();
        exampleGridData.tilePrefabs = new List<GameObject>();

        List<TileComponent> currentTileTypes = new List<TileComponent>();
        List<int> currentTileFequency = new List<int>();

        // Iterate over all the tiles
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.x; ++z)
                {
                    if (gridParent != null)
                    {
                        TileComponent currentTile = tileGrid[x][y][z];
                        if (TryGetTileWithMatchingID(currentTileTypes, currentTile.TileData.ID, out int index))
                        {
                            ++currentTileFequency[index];
                        }
                        else
                        {
                            currentTileTypes.Add(currentTile);
                            currentTileFequency.Add(1);
                        }
                        // Add all the nearby sockets to its lists
                        AddAllNeighbouringSocketsToTileSocketData(currentTile, gridDimensions);
                    }
                }
            }
        }

        // Assign the weights for each tile type
        int tileCount = gridDimensions.x * gridDimensions.y * gridDimensions.z;
        float weightPerFequency = 1.0f / tileCount;
        for (int i = 0; i < currentTileTypes.Count; ++i)
        {
            currentTileTypes[i].TileData.BaseTileWeight = weightPerFequency * currentTileFequency[i];
        }



        return exampleGridData;
    }

    private bool TryGetTileWithMatchingID(List<TileComponent> listToCheck, int IDtoCheckFor, out int index)
    {
        index = -1;
        for (int i = 0; i < listToCheck.Count; ++i)
        {
            if (listToCheck[i].TileData.ID == IDtoCheckFor)
            {
                index = i;
                return true;
            }

        }
        return false;
    }

    private void AddAllNeighbouringSocketsToTileSocketData(TileComponent tile, Vector3Int gridCoords)
    {
        // Right Face
        if (gridCoords.x + 1 < gridDimensions.x)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Right, tile, tileGrid[gridCoords.x + 1][gridCoords.y][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Right, tile, null);
        }

        // Left Face
        if (gridCoords.x - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Left, tile, tileGrid[gridCoords.x - 1][gridCoords.y][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Left, tile, null);
        }

        // Above Face
        if (gridCoords.y + 1 < gridDimensions.y)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Above, tile, tileGrid[gridCoords.x][gridCoords.y + 1][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Above, tile, null);
        }

        // Below Face
        if (gridCoords.y - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Right, tile, tileGrid[gridCoords.x][gridCoords.y - 1][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Right, tile, null);
        }

        // Front Face
        if (gridCoords.z + 1 < gridDimensions.z)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Front, tile, tileGrid[gridCoords.x][gridCoords.y][gridCoords.z + 1]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Front, tile, null);
        }

        // Back Face
        if (gridCoords.z - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Back, tile, tileGrid[gridCoords.x][gridCoords.y][gridCoords.z - 1]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sockets.Back, tile, null);
        }
    }

    private void AddOpposingSocketToValidSockets(SocketData.Sockets socketDirection, TileComponent tileComponentToAddTo, TileComponent opposingTileComponent)
    {
        int opposingID;
        switch (socketDirection)
        {
            case SocketData.Sockets.Above:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.belowSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.aboveNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.aboveNeighbours.Add(opposingID);
                }
                break;
            case SocketData.Sockets.Below:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.aboveSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.belowNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.belowNeighbours.Add(opposingID);
                }
                break;
            case SocketData.Sockets.Front:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.backSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.frontNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.frontNeighbours.Add(opposingID);
                }
                break;
            case SocketData.Sockets.Right:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.leftSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.rightNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.rightNeighbours.Add(opposingID);
                }
                break;
            case SocketData.Sockets.Back:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.frontSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.backNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.backNeighbours.Add(opposingID);
                }
                break;
            case SocketData.Sockets.Left:
                if (opposingTileComponent != null)
                {
                    opposingID = opposingTileComponent.TileData.TileSocketData.leftSocket;
                }
                else
                {
                    opposingID = -1;
                }

                if (!tileComponentToAddTo.TileData.TileSocketData.validNeighbours.lefteNeighbours.Contains(opposingID))
                {
                    tileComponentToAddTo.TileData.TileSocketData.validNeighbours.lefteNeighbours.Add(opposingID);
                }
                break;
        }
    }
    // Todo 
    //private bool TryGetTileComponentWithMatchingIdFromPrefabList(List<GameObject> gameObjects, int IdToMatch)
    //{

    //}

    // TODO: Make sure the window is copying prefab reference rather than scene object reference when creating the editor example data
}
