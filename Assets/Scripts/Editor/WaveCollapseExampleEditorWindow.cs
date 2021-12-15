//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               WaveCollapseExampleEditorWindow.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              An editor window that is used to help build a example grid and for turning that example grid into a tile-set
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// An editor window that is used to help build a example grid and for turning that example grid into a tile-set
/// </summary>
public class WaveCollapseExampleEditorWindow : EditorWindow
{
    #region Private Variables
    /// <summary>
    /// The tiles grid being modified and used
    /// </summary>
    private TileComponent[][][] tileGrid;
    /// <summary>
    /// The grid dimensions the grid will be set to and is expected to be
    /// </summary>
    private Vector3Int gridDimensions;
    /// <summary>
    /// The controller for the grid being modified and used 
    /// </summary>
    private ExampleGridController gridController;
    /// <summary>
    /// The tile that will be added to new grid cells that don't already have a tile component
    /// </summary>
    private GameObject defaultTilePrefab;
    /// <summary>
    /// The bottom back left cell of the area that will be filled in with brush tile on area fill
    /// </summary>
    private Vector3Int brushBottomBackLeft;
    /// <summary>
    /// The top front right tile of the area that will be filled in with brush tile on area fill
    /// </summary>
    private Vector3Int brushTopFrontRight;
    /// <summary>
    /// The tile that the area will be filled with on area filled
    /// </summary>
    private TileComponent brushTile;
    /// <summary>
    /// The ID to update the biomes type of
    /// </summary>
    private int idToUpdateBiomesOf = -1;
    /// <summary>
    /// The biome type to change the tiles of given ID to
    /// </summary>
    private TileData.BiomeType biomeTypeToChangeTo;
    #endregion

    #region Unity Methods
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Example Grid Starting", EditorStyles.boldLabel);
        // Grid Dimensions
        gridDimensions = EditorGUILayout.Vector3IntField("Grid Dimensions", gridDimensions);
        // Controlled grid
        gridController = (ExampleGridController)EditorGUILayout.ObjectField("Grid Parent ExampleGridController", gridController, typeof(ExampleGridController), true);
        // Default tile
        defaultTilePrefab = (GameObject)EditorGUILayout.ObjectField("Default Tile Prefab", defaultTilePrefab, typeof(GameObject), false);
        // Initial Grid Building
        if (GUILayout.Button("Build Grid Template"))
        {
            BuildGridObjectParents();
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Example Grid Editing", EditorStyles.boldLabel);
        // ID Based Biomes type updating
        idToUpdateBiomesOf = EditorGUILayout.IntField("Id To Update", idToUpdateBiomesOf);
        biomeTypeToChangeTo = (TileData.BiomeType)EditorGUILayout.EnumPopup("Id To Update", biomeTypeToChangeTo);
        if (GUILayout.Button("Update biomes type of tiles with ID"))
        {
            gridController.ChanceBiomeTypeOfTilesWithID(idToUpdateBiomesOf, biomeTypeToChangeTo);
        }
        // Area Brush
        brushBottomBackLeft = EditorGUILayout.Vector3IntField("Brush Bottom Back Left (Inclusive to placement)", brushBottomBackLeft);
        brushTopFrontRight = EditorGUILayout.Vector3IntField("Brush Top Front Right (Inclusive to placement)", brushTopFrontRight);
        brushTile = (TileComponent)EditorGUILayout.ObjectField("Brush Tile", brushTile, typeof(TileComponent), false);
        if (GUILayout.Button("Fill Area"))
        {
            gridController.FillInAreaWithTile(brushBottomBackLeft, brushTopFrontRight, brushTile.gameObject);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Adjust Grid Dimensions"))
        {
            if (gridController != null)
            {
                gridController.AdjustGridDimensions(gridDimensions, defaultTilePrefab);
            }
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tile Set Generation", EditorStyles.boldLabel);
        // Generation
        if (GUILayout.Button("Generate Build Data"))
        {
            if (BuildTileSet(out List<GameObject> prefabBasis, out List<TileData> tileDatas))
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
                    if (PrefabUtility.IsPartOfAnyPrefab(prefabBasis[i]))
                    {
                        PrefabUtility.UnpackPrefabInstance(prefabBasis[i], PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                    newPrefabs.Add(PrefabUtility.SaveAsPrefabAsset(prefabBasis[i], "Assets/NewExampleGridDataPrefabs/" + prefabBasis[i].name + ".prefab"));
                    if (newPrefabs[newPrefabs.Count - 1].TryGetComponent<TileComponent>(out TileComponent newTileComponent))
                    {
                        newTileComponent.TileData = tileDatas[i];
                        newPrefabs[i * 4].transform.rotation = Quaternion.Euler(0, 0, 0);
                        PrefabUtility.SavePrefabAsset(newPrefabs[i * 4]);

                        // Create 3 additional prototypes
                        TileComponent[] newPrefabComponents = CreatePrototypes(newTileComponent);
                        for (int j = 0; j < newPrefabComponents.Length; ++j)
                        {
                            newPrefabs.Add(newPrefabComponents[j].gameObject);
                        }
                    }
                }

                TileSet exampleGridData = TileSet.CreateInstance<TileSet>();
                exampleGridData.TilePrefabs = new List<TileComponent>();
                for (int i = 0; i < newPrefabs.Count; ++i)
                {
                    exampleGridData.TilePrefabs.Add(newPrefabs[i].GetComponent<TileComponent>());
                }
                AssetDatabase.CreateAsset(exampleGridData, "Assets/NewExampleGridData.asset");
            }
            else
            {
                Debug.LogError("Generate Example Build Data Failed");
            }
        }
    }
    #endregion

    #region Public Static Methods
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Wave Collapse Example Editor Window")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        WaveCollapseExampleEditorWindow window = (WaveCollapseExampleEditorWindow)EditorWindow.GetWindow(typeof(WaveCollapseExampleEditorWindow));
        window.Show();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Gets a set of game objects to be used a the basis for the new tile set prefabs as well as the tile data's they will contain
    /// </summary>
    /// <param name="prefabBasisObjects">Set of game objects to be used a the basis for the new tile set prefabs</param>
    /// <param name="tileDatas">tile data for each of the new tile types</param>
    /// <returns>True if the building was successful, false otherwise</returns>
    private bool BuildTileSet(out List<GameObject> prefabBasisObjects, out List<TileData> tileDatas)
    {
        tileDatas = new List<TileData>();
        prefabBasisObjects = new List<GameObject>();
        List<int[]> currentTileFequencyForEachYPosition = new List<int[]>();

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
                        // If the current tile ID is already found add the frequency of that tile at the current y position and its neighbor sockets to the valid neighbors
                        if (TryGetTileWithMatchingID(tileDatas, currentTile.TileData.ID, out int index))
                        {
                            ++currentTileFequencyForEachYPosition[index][y]; // Adding to 
                            AddAllNeighbouringSocketsToSocketData(tileDatas[index].TileSocketData, new Vector3Int(x, y, z), GetNumberOf90DegreeTurnsFromRotationAngle(currentTile.transform.rotation.eulerAngles.y));
                        }
                        // Otherwise make a prefab basis and tile data from it, then add its frequency and neighbors
                        else
                        {

                            tileDatas.Add(new TileData(currentTile.TileData));
                            prefabBasisObjects.Add(currentTile.gameObject);
                            // Create the new list of arrays for frequency for each y position and add one to the current y position
                            currentTileFequencyForEachYPosition.Add(new int[gridDimensions.y]);
                            ++currentTileFequencyForEachYPosition[currentTileFequencyForEachYPosition.Count - 1][y];

                            SocketData newSocketData = SocketData.CreateInstance<SocketData>();
                            newSocketData.CopySocketIDs(currentTile.TileData.TileSocketData);
                            tileDatas[tileDatas.Count - 1].TileSocketData = newSocketData;
                            AddAllNeighbouringSocketsToSocketData(tileDatas[tileDatas.Count - 1].TileSocketData, new Vector3Int(x, y, z), GetNumberOf90DegreeTurnsFromRotationAngle(currentTile.transform.rotation.eulerAngles.y));
                        }
                    }
                }
            }
        }

        // Assign the weights for each tile type and each y value
        float weightPerFequency = 1.0f / (gridDimensions.x * gridDimensions.z);

        for (int i = 0; i < tileDatas.Count; ++i)
        {
            tileDatas[i].Weights = new float[gridDimensions.y];
            for (int j = 0; j < tileDatas[i].Weights.Length; ++j)
            {
                tileDatas[i].Weights[j] = weightPerFequency * currentTileFequencyForEachYPosition[i][j];
            }

        }

        return true;
    }
    /// <summary>
    /// Attempt to find a tile in a given list with an ID matching the one given
    /// </summary>
    /// <param name="listToCheck">The list to search through for a tile with given ID</param>
    /// <param name="IDtoCheckFor">The ID to search the tiles in the list for</param>
    /// <param name="index">The index in the list of tiles with an ID matching the one given</param>
    /// <returns>True if the tile with a matching ID is found, false otherwise</returns>
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

    /// <summary>
    /// Return the number of times a tile would have been rotated around its y axis 90 degrees based on it eular angles
    /// </summary>
    /// <param name="rotation">The Y euler rotation value</param>
    /// <returns>The number of times a tile would have been rotated around its y axis 90 degrees based on it eular angles</returns>
    private int GetNumberOf90DegreeTurnsFromRotationAngle(float rotation)
    {
        // Check rotation of the opposing tile component 
        float clockwiseRotation = rotation;
        if (rotation < 0)
        {
            clockwiseRotation = 360 + rotation;
        }

        // If it is rotated then adjust the side that must be gotten accordingly
        if (Mathf.Round(clockwiseRotation) == 90f)
        {
            return 1;
        }
        else if (Mathf.Round(clockwiseRotation) == 180f)
        {
            return 2;
        }
        else if (Mathf.Round(clockwiseRotation) == 270f)
        {
            return 3;
        }
        return 0;
    }

    /// <summary>
    /// Creates a set of prefab Game Object for the other 3 90 degree rotations of a tile and returns their tile components
    /// </summary>
    /// <param name="tileComponent">The original tile component to basis the new prefabs and their tile component off</param>
    /// <returns>A set of prefab Game Object for the other 3 90 degree rotations of a tile and returns their tile components</returns>
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

    /// <summary>
    /// Add all the Neighbors of a cell at the given coordinates to the list of valid neighbors in a given socket data 
    /// </summary>
    /// <param name="socketData">The Socket data toad</param>
    /// <param name="gridCoords">The coordinate the socket data would be located at</param>
    /// <param name="numberOf90TurnsFromRotation">The number of tiles to rotate the sides socket data 90 degrees around an invisible y Axis (To allow for tile rotation in example)</param>
    private void AddAllNeighbouringSocketsToSocketData(SocketData socketData, Vector3Int gridCoords, int numberOf90TurnsFromRotation)
    {
        // Iterate over every vector3Int Axis
        for (int i = 0; i < 3; ++i)
        {
            // Get the offset
            Vector3Int offset = Vector3Int.zero;
            offset[i] = 1;
            // Adjust the direction to add to based on the object's rotation
            SocketData.Sides sideDirection = SocketData.RotateSidesDirection(SocketData.GetSideFromCooridnateOff(offset), 4 - numberOf90TurnsFromRotation);

            // Check if the offset gird coordinate is within the grid to assign the tile component that is at the offset position
            TileComponent tileComponent = null;
            if (gridCoords[i] + 1 < gridDimensions[i])
            {
                Vector3Int offestPosition = gridCoords + offset;
                tileComponent = tileGrid[offestPosition.x][offestPosition.y][offestPosition.z];
            }

            // Add the opposing sockets to this
            AddOpposingSocketToValidSockets(sideDirection, SocketData.GetSideFromCooridnateOff(offset), socketData, tileComponent);

            // Do the same to the opposite side
            offset[i] = -1;
            sideDirection = SocketData.GetOpposingSocket(sideDirection);
            tileComponent = null;
            if (gridCoords[i] - 1 > -1)
            {
                Vector3Int offestPosition = gridCoords + offset;
                tileComponent = tileGrid[offestPosition.x][offestPosition.y][offestPosition.z];
            }
            AddOpposingSocketToValidSockets(sideDirection, SocketData.GetSideFromCooridnateOff(offset), socketData, tileComponent);
        }
    }
    /// <summary>
    /// Add the socket ID of an opposing tile component to a valid neighbor list of a socket data based on the side given
    /// </summary>
    /// <param name="SideBeingAddedToo">The side of the socket data that the valid ID will be added too</param>
    /// <param name="socketDirection">The direction the side added too is currently facing</param>
    /// <param name="socketDataToAddTo">The socket data to add to the valid neighbors of</param>
    /// <param name="opposingTileComponent">The tile component that the socket id is being gotten</param>
    private void AddOpposingSocketToValidSockets(SocketData.Sides SideBeingAddedToo, SocketData.Sides socketDirection, SocketData socketDataToAddTo, TileComponent opposingTileComponent)
    {
        // Check rotation of the opposing tile component 
        int opposingID;
        SocketData.Sides otherSocketDirection = SocketData.Sides.Undecided;
        if (opposingTileComponent != null)
        {
            otherSocketDirection = SocketData.GetOpposingSocket(socketDirection);
            otherSocketDirection = SocketData.RotateSidesDirection(otherSocketDirection, 4 - GetNumberOf90DegreeTurnsFromRotationAngle(opposingTileComponent.transform.rotation.eulerAngles.y));
            opposingID = opposingTileComponent.TileData.TileSocketData.GetIdOfSide(otherSocketDirection);
        }
        else
        {
            opposingID = -1;
        }

        List<int> validNeighbourList = socketDataToAddTo.ValidNeighbours.GetValidNeighbourListForSide(SideBeingAddedToo);

        if (!validNeighbourList.Contains(opposingID))
        {
            validNeighbourList.Add(opposingID);
        }
    }
    /// <summary>
    /// Build up the grid of object that act as parent for tiles in each cell of the grid, adding the default prefabs as appropriate
    /// </summary>
    private void BuildGridObjectParents()
    {
        gridController = new GameObject("ExampleGrid").AddComponent<ExampleGridController>();
        gridController.BuildGridObjectParents(gridDimensions, defaultTilePrefab);
    }
    #endregion
}
