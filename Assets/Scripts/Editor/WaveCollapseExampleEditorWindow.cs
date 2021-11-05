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
        gridParent = (GameObject)EditorGUILayout.ObjectField("Grid Parent GameObject", gridParent, typeof(GameObject), true);
        defaultTilePrefab = (GameObject)EditorGUILayout.ObjectField("Default Tile Prefab", defaultTilePrefab, typeof(GameObject), false);

        int tilePrefabsInUseArraySize = EditorGUILayout.IntField("Tile Prefabs In Use Size", tilePrefabsInUse.Length);
        if (tilePrefabsInUseArraySize != tilePrefabsInUse.Length)
        {
            GameObject[] newArray = new GameObject[tilePrefabsInUseArraySize];
            int smallestArrayLength = tilePrefabsInUse.Length < tilePrefabsInUseArraySize ? tilePrefabsInUse.Length : tilePrefabsInUseArraySize;
            System.Array.Copy(tilePrefabsInUse, newArray, smallestArrayLength);
            tilePrefabsInUse = newArray;
        }
        if (showPrefabsInUseArray = EditorGUILayout.Foldout(showPrefabsInUseArray, "Show Prefabs Array"))
        {
            for (int i = 0; i < tilePrefabsInUseArraySize; ++i)
            {
                tilePrefabsInUse[i] = (GameObject)EditorGUILayout.ObjectField(tilePrefabsInUse[i], typeof(GameObject), false);
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
            List<TileComponent> tileComponents = BuildExampleGridData();

            // Create Folder To Store Prefabs
            AssetDatabase.CreateFolder("Assets", "NewExampleGridDataPrefabs");
            AssetDatabase.SaveAssets();
            AssetDatabase.CreateFolder("Assets/NewExampleGridDataPrefabs", "SocketData");
            AssetDatabase.SaveAssets();
            List<GameObject> newPrefabs = new List<GameObject>(tileComponents.Count);
            for (int i = 0; i < tileComponents.Count; ++i)
            {
                AssetDatabase.CreateAsset(tileComponents[i].TileData.TileSocketData, "Assets/NewExampleGridDataPrefabs/SocketData/" + tileComponents[i].gameObject.name + "SocketData.Asset");
                newPrefabs.Add(PrefabUtility.SaveAsPrefabAsset(tileComponents[i].gameObject, "Assets/NewExampleGridDataPrefabs/" + tileComponents[i].gameObject.name + ".prefab"));
            }

            ExampleGridData exampleGridData = ExampleGridData.CreateInstance<ExampleGridData>();
            exampleGridData.tilePrefabs = newPrefabs;
            AssetDatabase.CreateAsset(exampleGridData, "Assets/NewExampleGridData.asset");
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

    private List<TileComponent> BuildExampleGridData()
    {
        List<TileComponent> currentTileTypes = new List<TileComponent>();
        List<int> currentTileFequency = new List<int>();

        int tileCount = gridDimensions.x * gridDimensions.y * gridDimensions.z;

        // Build the jagged array of tiles
        tileGrid = new TileComponent[gridDimensions.x][][];
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            tileGrid[i] = new TileComponent[gridDimensions.y][];
            for (int k = 0; k < tileGrid[i].Length; ++k)
            {
                tileGrid[i][k] = new TileComponent[gridDimensions.z];
            }
        }
        int childIndex = 0;
        if (gridParent.transform.childCount == tileCount)
        {
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        tileGrid[x][y][z] = gridParent.transform.GetChild(childIndex).GetComponentInChildren<TileComponent>();
                        ++childIndex;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Grid Parent did not contain enough children for the number of tiles");
            return null;
        }

        // Iterate over all the tiles
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    if (gridParent != null)
                    {
                        TileComponent currentTile = tileGrid[x][y][z];
                        if (TryGetTileWithMatchingID(currentTileTypes, currentTile.TileData.ID, out int index))
                        {
                            ++currentTileFequency[index];
                            AddAllNeighbouringSocketsToSocketData(currentTileTypes[index].TileData.TileSocketData, new Vector3Int(x, y, z));
                        }
                        else
                        {
                            currentTileTypes.Add(currentTile);
                            currentTileFequency.Add(1);
                            SocketData newSocketData = SocketData.CreateInstance<SocketData>();
                            newSocketData.CopySocketIDs(currentTile.TileData.TileSocketData);
                            currentTile.TileData.TileSocketData = newSocketData;
                            AddAllNeighbouringSocketsToSocketData(currentTile.TileData.TileSocketData, new Vector3Int(x, y, z));
                        }
                        // Add all the nearby sockets to its lists

                    }
                }
            }
        }

        // Assign the weights for each tile type

        float weightPerFequency = 1.0f / tileCount;

        for (int i = 0; i < currentTileTypes.Count; ++i)
        {
            currentTileTypes[i].TileData.BaseTileWeight = weightPerFequency * currentTileFequency[i];
        }

        return currentTileTypes;
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

    private void AddAllNeighbouringSocketsToSocketData(SocketData socketData, Vector3Int gridCoords)
    {
        // Right Face
        if (gridCoords.x + 1 < gridDimensions.x)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Right, socketData, tileGrid[gridCoords.x + 1][gridCoords.y][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Right, socketData, null);
        }

        // Left Face
        if (gridCoords.x - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Left, socketData, tileGrid[gridCoords.x - 1][gridCoords.y][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Left, socketData, null);
        }

        // Above Face
        if (gridCoords.y + 1 < gridDimensions.y)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Above, socketData, tileGrid[gridCoords.x][gridCoords.y + 1][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Above, socketData, null);
        }

        // Below Face
        if (gridCoords.y - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Below, socketData, tileGrid[gridCoords.x][gridCoords.y - 1][gridCoords.z]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Below, socketData, null);
        }

        // Front Face
        if (gridCoords.z + 1 < gridDimensions.z)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Front, socketData, tileGrid[gridCoords.x][gridCoords.y][gridCoords.z + 1]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Front, socketData, null);
        }

        // Back Face
        if (gridCoords.z - 1 > -1)
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Back, socketData, tileGrid[gridCoords.x][gridCoords.y][gridCoords.z - 1]);
        }
        else
        {
            AddOpposingSocketToValidSockets(SocketData.Sides.Back, socketData, null);
        }
    }

    private void AddOpposingSocketToValidSockets(SocketData.Sides socketDirection, SocketData socketDataToAddTo, TileComponent opposingTileComponent)
    {
        int opposingID;
        if (opposingTileComponent != null)
        {
            opposingID = opposingTileComponent.TileData.TileSocketData.GetIdOfSide(SocketData.GetOpposingSocket(socketDirection));
        }
        else
        {
            opposingID = -1;
        }

        List<int> validNeighbourList = socketDataToAddTo.validNeighbours.GetValidNeighbourListForSide(socketDirection);

        if (!validNeighbourList.Contains(opposingID))
        {
            validNeighbourList.Add(opposingID);
        }
    }
    // Todo 
    private bool TryGetTileComponentWithMatchingIdFromPrefabList(GameObject[] gameObjects, int IdToMatch, out TileComponent tileComponent)
    {
        for (int i = 0; i < gameObjects.Length; ++i)
        {
            if (gameObjects[i].TryGetComponent<TileComponent>(out tileComponent))
            {
                if (tileComponent.TileData.ID == IdToMatch)
                {
                    return true;
                }
            }
        }
        tileComponent = null;
        return false;
    }

    // TODO: Make sure the window is copying prefab reference rather than scene object reference when creating the editor example data
}
