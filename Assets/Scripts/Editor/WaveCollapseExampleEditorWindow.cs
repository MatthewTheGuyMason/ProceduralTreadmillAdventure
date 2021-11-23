using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveCollapseExampleEditorWindow : EditorWindow
{
    private TileComponent[][][] tileGrid;

    private Vector3Int gridDimensions;

    private ExampleGridController gridController;

    private GameObject defaultTilePrefab;

    private GameObject defaultFloorTilePrefab;

    [SerializeField]
    private GameObject[] tilePrefabsInUse;

    private SerializedProperty tilePrefabsInUseProperty;

    private bool showPrefabsInUseArray;

    private Vector3Int brushBottomLeft;

    private Vector3Int brushTopRight;

    private TileComponent brushTile;

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
        gridController = (ExampleGridController)EditorGUILayout.ObjectField("Grid Parent ExampleGridController", gridController, typeof(ExampleGridController), true);
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

        if (GUILayout.Button("Build Grid Template"))
        {
            BuildGridObjectParents();
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Generate Build Data"))
        {
            if (BuildExampleGridData(out List<GameObject> prefabBasis, out List<TileData> tileDatas))
            {
                // Create Folder To Store Prefabs
                AssetDatabase.CreateFolder("Assets", "NewExampleGridDataPrefabs");
                AssetDatabase.SaveAssets();
                AssetDatabase.CreateFolder("Assets/NewExampleGridDataPrefabs", "SocketData");
                AssetDatabase.SaveAssets();
                List<GameObject> newPrefabs = new List<GameObject>(tileDatas.Count * 4);
                for (int i = 0; i < tileDatas.Count; ++i)
                {
                    AssetDatabase.CreateAsset(tileDatas[i].TileSocketData, "Assets/NewExampleGridDataPrefabs/SocketData/" + prefabBasis[i].name + "SocketData.Asset");
                    newPrefabs.Add(PrefabUtility.SaveAsPrefabAsset(prefabBasis[i], "Assets/NewExampleGridDataPrefabs/" + prefabBasis[i].name + ".prefab"));
                    if (newPrefabs[newPrefabs.Count - 1].TryGetComponent<TileComponent>(out TileComponent newTileComponent))
                    {
                        newTileComponent.TileData = tileDatas[i];
                        PrefabUtility.SavePrefabAsset(newPrefabs[i]);
                        // Create 3 additional prototypes
                        TileComponent[] newPrefabComponents = CreatePrototypes(newTileComponent);
                        for (int j = 0; j < newPrefabComponents.Length; ++j)
                        {
                            newPrefabs.Add(newPrefabComponents[j].gameObject);
                        }
                    }
                }

                ExampleGridData exampleGridData = ExampleGridData.CreateInstance<ExampleGridData>();
                exampleGridData.tilePrefabs = new List<TileComponent>();
                for (int i = 0; i < newPrefabs.Count; ++i)
                {
                    exampleGridData.tilePrefabs.Add(newPrefabs[i].GetComponent<TileComponent>());
                }
                AssetDatabase.CreateAsset(exampleGridData, "Assets/NewExampleGridData.asset");
            }
            else
            {
                Debug.LogError("Generate Example Build Data Failed");
            }
        }

        if (GUILayout.Button("Adjust Grid Dimensions"))
        {
            if (gridController != null)
            {
                gridController.AdjustGridDimensions(gridDimensions, defaultTilePrefab);
            }
            AssetDatabase.SaveAssets();
        }

        brushBottomLeft = EditorGUILayout.Vector3IntField("Brush Bottom Left", brushBottomLeft);
        brushTopRight = EditorGUILayout.Vector3IntField("Brush Top Right", brushTopRight);
        brushTile = (TileComponent)EditorGUILayout.ObjectField("Brush Tile", brushTile, typeof(TileComponent), false);

        if (GUILayout.Button("Fill Area"))
        {
            gridController.FillInAreaWithTile(brushBottomLeft, brushTopRight, brushTile.gameObject);
            AssetDatabase.SaveAssets();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void BuildGridObjectParents()
    {
        gridController = new GameObject("ExampleGrid").AddComponent<ExampleGridController>();
        gridController.BuildGridObjectParents(gridDimensions, defaultTilePrefab);
    }

    // TODO: Add some code to detectRotation of a tile and account for it when building the data set

    private bool BuildExampleGridData(out List<GameObject> prefabBasisObjects, out List<TileData> tileDatas)
    {
        tileDatas = new List<TileData>();
        prefabBasisObjects = new List<GameObject>();
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
        if (gridController.transform.childCount == tileCount)
        {
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        tileGrid[x][y][z] = gridController.transform.GetChild(childIndex).GetComponentInChildren<TileComponent>();
                        ++childIndex;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Grid Parent did not contain enough children for the number of tiles");
            return false;
        }

        // Iterate over all the tiles
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    if (gridController != null)
                    {
                        TileComponent currentTile = tileGrid[x][y][z];
                        if (TryGetTileWithMatchingID(tileDatas, currentTile.TileData.ID, out int index))
                        {
                            ++currentTileFequency[index];
                            AddAllNeighbouringSocketsToSocketData(tileDatas[index].TileSocketData, new Vector3Int(x, y, z));
                        }
                        else
                        {
                            tileDatas.Add(new TileData(currentTile.TileData));
                            prefabBasisObjects.Add(currentTile.gameObject);
                            currentTileFequency.Add(1);
                            SocketData newSocketData = SocketData.CreateInstance<SocketData>();
                            newSocketData.CopySocketIDs(currentTile.TileData.TileSocketData);
                            tileDatas[tileDatas.Count - 1].TileSocketData = newSocketData;
                            AddAllNeighbouringSocketsToSocketData(tileDatas[tileDatas.Count - 1].TileSocketData, new Vector3Int(x, y, z));
                        }
                        // Add all the nearby sockets to its lists

                    }
                }
            }
        }

        // Assign the weights for each tile type

        float weightPerFequency = 1.0f / tileCount;

        for (int i = 0; i < tileDatas.Count; ++i)
        {
            tileDatas[i].Weight = weightPerFequency * currentTileFequency[i];
        }

        return true;
    }

    private bool TryGetTileWithMatchingID(List<TileData> listToCheck, int IDtoCheckFor, out int index)
    {
        index = -1;
        for (int i = 0; i < listToCheck.Count; ++i)
        {
            if (listToCheck[i].ID == IDtoCheckFor)
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

    private TileComponent[] CreatePrototypes(TileComponent tileComponent)
    {
        TileComponent[] newTileComponents = new TileComponent[3];
        for (int i = 1; i < 4; ++i)
        {
            string prototypeName = "";
            switch (i)
            {
                case 1:
                    prototypeName = "PrototypeRight";
                    break;
                case 2:
                    prototypeName = "PrototypeBackwards";
                    break;
                case 3:
                    prototypeName = "PrototypeLeft";
                    break;
            }

            // Create a game object prefab copy
            GameObject newPrototypeInstance = GameObject.Instantiate(tileComponent.gameObject);
            GameObject newPrototype = PrefabUtility.SaveAsPrefabAsset(newPrototypeInstance, "Assets/NewExampleGridDataPrefabs/" + tileComponent.gameObject.name + prototypeName + ".prefab");
            DestroyImmediate(newPrototypeInstance);
            newPrototype.transform.Rotate(Vector3.up, -i * 90);

            newTileComponents[i - 1] = newPrototype.GetComponent<TileComponent>();

            // Create a new scriptable object for the sockets
            SocketData newSocketData = ScriptableObject.CreateInstance<SocketData>();
            newSocketData.CopyData(tileComponent.TileData.TileSocketData);
            newSocketData.RotateAroundY(i);
            AssetDatabase.CreateAsset(newSocketData, "Assets/NewExampleGridDataPrefabs/SocketData/" + tileComponent.gameObject.name + prototypeName + "SocketData.Asset");

            newTileComponents[i - 1].TileData.TileSocketData = newSocketData;

            PrefabUtility.SavePrefabAsset(newPrototype);
        }

        return newTileComponents;
    }

    //private void FillInAreaWithTile(Vector3Int bottemLeft, Vector3Int topRight, TileComponent tileToPlace)
    //{
    //    int 
    //}

    //private int ConvertGridCordinateToIndex(Vector3Int GridIndex)
    //{
    //    int xMultiplier = gridDimensions.
    //}
}
