//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               TileGrid.cs
//  Author:             Matthew Mason
//  Date Created:       06/10/2021
//  Date Last Modified  07/12/2021
//  Brief:              Script controlling the generation of tiles into a grid using wave function collapse
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script controlling the generation of tiles into a grid using wave function collapse
/// </summary>
public class TileGrid : MonoBehaviour
{
    #region Private Serialized Field
#if UNITY_EDITOR
    [SerializeField]
    [Tooltip("If the grid layout should be shown")]
    private bool showGrid;
#endif
    [SerializeField] [Tooltip("If the tiles representing the possibility space are instantiated as the generation goes on")]
    private bool showPossibillitySpace = true;

    [SerializeField] [Tooltip("The data one how the biomes should transition based what tile it is on")]
    private BiomeTransitionaryData biomeTransitionaryData;

    [SerializeField] [Tooltip("The amount of time before it should place down a new tile ")]
    private float timeBetweenPlacements;

    [SerializeField] [Tooltip("The tile set generated from an example grid")]
    private TileSet exampleGridData;

    [SerializeField] [Tooltip("The slider used to show how far along the grid generation is")]
    private UnityEngine.UI.Slider progressSlider;

    [SerializeField] [Tooltip("The dimensions of the grid that will be generated")]
    private Vector3Int gridDimensions;
    #endregion

    #region Private Variables
    /// <summary>
    /// The game objects used to visual
    /// </summary>
    private GameObject[][][][] possibilitySpaceVisualiserObjects;

    private int xOffset;
    private int expectedPlannedTiles;
    private int numberOfTilesPlanned;

    /// <summary>
    /// A stack of all the coordinates in the possibility space that need to be updated
    /// </summary>
    private Stack<Vector3Int> possibilitySpaceLocationsToUpdate;

    /// <summary>
    /// All the tiles that could possibility exist in each grid coordinate
    /// </summary>
    private List<TileComponent>[][][] possibilitySpace;

    /// <summary>
    /// The start of the tiles that have been planned
    /// </summary>
    private int plannedTilesStartX;

    /// <summary>
    /// The start of the tiles that are currently being planed
    /// </summary>
    private int tilesBeingPlannedStartX;

    private int tilesBeingPlannedEndX;

    /// <summary>
    /// The tiles that have been placed down in the grid
    /// </summary>
    private TileComponent[][][] tileGrid;
    #endregion

    #region Unity Method

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGrid)
        {
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        Gizmos.DrawWireCube(new Vector3(x, y, z), Vector3.one);
                    }
                }
            }
        }
    }
#endif

    private void Start()
    {
        // Make the grid x a multiple of 3
        gridDimensions.x += 3 - gridDimensions.x % 3;


        int thirdOfXDimension = (gridDimensions.x / 3);
        // One third of the grid is being planned space
        expectedPlannedTiles = thirdOfXDimension * gridDimensions.y * gridDimensions.z;
        tilesBeingPlannedStartX = gridDimensions.x - thirdOfXDimension;
        tilesBeingPlannedEndX = gridDimensions.x;
        // The next third of the grid is planned space (the last third is visible space but no assigning is necessary)
        plannedTilesStartX = tilesBeingPlannedStartX - thirdOfXDimension;

        // Create propagation stack
        possibilitySpaceLocationsToUpdate = new Stack<Vector3Int>();

        // Create tile grid
        tileGrid = new TileComponent[gridDimensions.x][][];
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            tileGrid[x] = new TileComponent[gridDimensions.y][];
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                tileGrid[x][y] = new TileComponent[gridDimensions.z];
            }
        }

        // Create possibility space
        possibilitySpace = new List<TileComponent>[gridDimensions.x][][];
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            possibilitySpace[x] = new List<TileComponent>[gridDimensions.y][];
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                possibilitySpace[x][y] = new List<TileComponent>[gridDimensions.z];
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.tilePrefabs);
                    possibilitySpace[x][y][z].AddRange(exampleGridData.tilePrefabs);
                }
            }
        }

        // Create possibility space visualization if applicable
        if (showPossibillitySpace)
        {
            possibilitySpaceVisualiserObjects = new GameObject[gridDimensions.x][][][];
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                possibilitySpaceVisualiserObjects[x] = new GameObject[gridDimensions.y][][];
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    possibilitySpaceVisualiserObjects[x][y] = new GameObject[gridDimensions.z][];
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        possibilitySpaceVisualiserObjects[x][y][z] = new GameObject[0];
                    }
                }
            }
        }



        // Start generation
        progressSlider.maxValue = gridDimensions.x * gridDimensions.y * gridDimensions.z;
        progressSlider.value = 0f;
        // Set up visible space
        PlacementInterations(Vector3Int.zero, new Vector3Int(plannedTilesStartX, gridDimensions.y, gridDimensions.z));
        // Set up planned tiles (And the to be planned tiles will just be possibility space anyway which is what we want)
        SetUpPlannedTiles();
        StartCoroutine(ForwardsStep());
    }

    private void Update()
    {
        // Always build up the planned tiles if not completed 
        if (numberOfTilesPlanned != expectedPlannedTiles)
        {
            PlanTile();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Returns true if a given coordinate is within a grid, false otherwise
    /// </summary>
    /// <param name="x">The x of the coordinates to check if they are inside the grid</param>
    /// <param name="y">The y of the coordinates to check if they are inside the grid</param>
    /// <param name="z">The z of the coordinates to check if they are inside the grid</param>
    /// <returns>True if a given coordinate is within a grid, false otherwise</returns>
    private bool CheckIfCoordIsInGrid(int x, int y, int z)
    {
        if (x > -1)
        {
            if (x < gridDimensions.x)
            {
                if (y > -1)
                {
                    if (y < gridDimensions.y)
                    {
                        if (z > -1)
                        {
                            if (z < gridDimensions.z)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Returns true if a given coordinate is within a grid, false otherwise
    /// </summary>
    /// <param name="coordinate">The coordinates to check if they are inside the grid</param>
    /// <returns>True if a given coordinate is within a grid, false otherwise</returns>
    private bool CheckIfCoordIsInGrid(Vector3Int coordinate)
    {
        return CheckIfCoordIsInGrid(coordinate.x, coordinate.y, coordinate.z);
    }

    /// <summary>
    /// Returns the weight of a given tile adjusted for its position in the grid
    /// </summary>
    /// <param name="tileComponent">The tile component to get the weight of</param>
    /// <param name="xPosition">The x position of the tile when the weight is gotten</param>
    /// <param name="yPosition">The y position of the tile when the weight is gotten</param>
    /// <returns>the weight of a given tile adjusted for its position in the grid</returns>
    private float GetWeightOfTileAdjustedForPosition(TileComponent tileComponent, int xPosition, int yPosition)
    {
        return tileComponent.TileData.GetWeight(yPosition, gridDimensions.y);
        float baseWeight = tileComponent.TileData.GetWeight(yPosition, gridDimensions.y);
        float transitionWeight;
        // Get the current weights from the transitional data
        BiomeTransitionaryData.BiomeWeights weights = biomeTransitionaryData.GetValuesAtTile(xPosition + xOffset);
        switch (tileComponent.TileData.TileBiomeType)
        {
            case TileData.BiomeType.Desert:
                return baseWeight * weights.desertUnitInterval;
            case TileData.BiomeType.DesertToGrassland:
                // If its a transition base the weight multiplier off how close the weightings
                if (weights.grasslandUnitInterval > 0 && weights.desertUnitInterval > 0)
                {
                    if (weights.grasslandUnitInterval > weights.desertUnitInterval)
                    {
                        transitionWeight = weights.grasslandUnitInterval - weights.desertUnitInterval;
                    }
                    else
                    {
                        transitionWeight = weights.desertUnitInterval - weights.grasslandUnitInterval;
                    }
                    transitionWeight = 1 - transitionWeight;
                    return baseWeight * transitionWeight;
                }
                else
                {
                    return 0;
                }
            case TileData.BiomeType.Grassland:
                return baseWeight * weights.grasslandUnitInterval;
            case TileData.BiomeType.GrasslandToSnow:
                // If its a transition base the weight multiplier off how close the weightings
                if (weights.grasslandUnitInterval > 0 && weights.tundraUnitInterval > 0)
                {
                    if (weights.grasslandUnitInterval > weights.tundraUnitInterval)
                    {
                        transitionWeight = weights.grasslandUnitInterval - weights.tundraUnitInterval;
                    }
                    else
                    {
                        transitionWeight = weights.tundraUnitInterval - weights.grasslandUnitInterval;
                    }
                    transitionWeight = 1 - transitionWeight;
                    return baseWeight * transitionWeight;
                }
                else
                {
                    return 0;
                }
            case TileData.BiomeType.Tundra:
                return baseWeight * weights.tundraUnitInterval;
            case TileData.BiomeType.NA:
                return baseWeight;
        }

        return baseWeight;
    }
    /// <summary>
    /// Returns the entropy of a set of tile possibilities
    /// </summary>
    /// <param name="possibilities">The possible tile components to get the entropy of </param>
    /// <param name="yPosition">The position in the y axis that the possibilities are gotten from</param>
    /// <param name="xPosition">The position in the x axis that the possibilities are gotten from</param>
    /// <returns>The entropy of a set of tile possibilities</returns>
    private float GetShannonEntropyOfPossibilitySpace(List<TileComponent> possibilities, int yPosition, int xPosition)
    {
        List<TileComponent> uniqueIdTiles = GetUniqueIDTilesFromList(possibilities);

        float weightSum = 0.0f;
        float weightTimesLogWeight = 0.0f;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            float tileWeight = GetWeightOfTileAdjustedForPosition(uniqueIdTiles[i], xPosition, yPosition);
            weightSum += tileWeight;
            weightTimesLogWeight += tileWeight * Mathf.Log(tileWeight);
        }

        return (Mathf.Log(weightSum) - weightTimesLogWeight) / weightSum;
    }

    /// <summary>
    /// Instantiates the game object to visualize the possibility space
    /// </summary>
    /// <param name="possibilitySpace"></param>
    /// <param name="gridPosition"></param>
    /// <returns>all the object instantiated in the visualization</returns>
    private GameObject[] AddProabilitySpaceObjects(List<TileComponent> possibilitySpace, Vector3Int gridPosition)
    {
        GameObject[] newGameObjects = new GameObject[possibilitySpace.Count + 1];
        newGameObjects[0] = new GameObject(gridPosition.ToString());
        newGameObjects[0].transform.position = gridPosition;
        for (int i = 1; i < newGameObjects.Length; ++i)
        {
            // Get where it should be placed
            Vector3 position = gridPosition;
            position -= Vector3.one * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * i;
            newGameObjects[i] = GameObject.Instantiate(possibilitySpace[i - 1].gameObject, position, possibilitySpace[i - 1].transform.rotation, newGameObjects[0].transform);
            newGameObjects[i].transform.localScale = Vector3.one / (possibilitySpace.Count + 1);
        }
        return newGameObjects;
    }

    /// <summary>
    /// Loops over the entire grid and places all the tiles using wave function collapse
    /// </summary>
    /// <returns></returns>
    private void PlacementInterations(Vector3Int areaStartPosition, Vector3Int areaEndPosition)
    {
        // Add every tiles to be updated
        for (int x = areaStartPosition.x; x < areaEndPosition.x; ++x)
        {
            for (int y = areaStartPosition.y; y < areaEndPosition.y; ++y)
            {
                for (int z = areaStartPosition.z; z < areaEndPosition.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.tilePrefabs);
                    possibilitySpaceLocationsToUpdate.Push(new Vector3Int(x, y, z));
                }
            }
        }

        int numberOfTiles = (areaEndPosition.x - areaStartPosition.x) * (areaEndPosition.y - areaStartPosition.y) * (areaEndPosition.z - areaStartPosition.z);

        for (int i = 0; i < numberOfTiles; ++i)
        {
            // Update all the tiles necessary
            UpdatePossibilitySpaceFromPropergation();

            // Find the lowest entropy value tile
            float lowestEntropyValue = float.MaxValue;
            Vector3Int lowestEntropyTilesCoords = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            int count = 0;
            for (int x = areaStartPosition.x; x < areaEndPosition.x; ++x)
            {
                for (int y = areaStartPosition.y; y < areaEndPosition.y; ++y)
                {
                    for (int z = areaStartPosition.z; z < areaEndPosition.z; ++z)
                    {
                        if (tileGrid[x][y][z] == null)
                        {
                            float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z], y, x);
                            ++count;
                            if (float.IsNaN(entropy))
                            {
                                Debug.LogError("Entropy gave NaN result");
                            }
                            if (entropy < lowestEntropyValue)
                            {
                                lowestEntropyValue = entropy;
                                lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                Debug.Log("Zero entropies calculated for iteration: " + i);
            }

            // Select tile from its possibilities as the tile to be placed
            TileComponent newTileComponent = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            ++progressSlider.value;
            if (newTileComponent != null)
            {
                // Instantiate the tile
                tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] =
                GameObject.Instantiate(newTileComponent.gameObject, new Vector3(lowestEntropyTilesCoords.x,
                lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z), newTileComponent.gameObject.transform.rotation, transform).GetComponent<TileComponent>();
                // Add its neighbors to be updated
                AddAllNeighboursToPropergationStack(lowestEntropyTilesCoords);
            }
            else
            {
                Debug.LogError("Generation failed");
                ResetGenAttempt();
            }
            // Wait the required time
            //yield return null;
        }
        //xOffset += 2;
        //StartCoroutine(GenerateMoreTiles(2));

        //yield break;
    }
    /// <summary>
    /// Loops over the entire grid and places all the tiles using wave function collapse
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlacementInterations()
    {
        // Add every tiles to be updated
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilitySpaceLocationsToUpdate.Push(new Vector3Int(x, y, z));
                }
            }
        }

        for (int i = 0; i < gridDimensions.x * gridDimensions.y * gridDimensions.z; ++i)
        {
            // Update all the tiles necessary
            UpdatePossibilitySpaceFromPropergation();

            // Find the lowest entropy value tile
            float lowestEntropyValue = float.MaxValue;
            Vector3Int lowestEntropyTilesCoords = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            int count = 0;
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        if (tileGrid[x][y][z] == null)
                        {
                            float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z], y, x);
                            ++count;
                            if (float.IsNaN(entropy))
                            {
                                Debug.LogError("Entropy gave NaN result");
                            }
                            if (entropy < lowestEntropyValue)
                            {
                                lowestEntropyValue = entropy;
                                lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                Debug.Log("Zero entropies calculated for iteration: " + i);
            }

            // Select tile from its possibilities as the tile to be placed
            TileComponent newTileComponent = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            ++progressSlider.value;
            if (newTileComponent != null)
            {
                // Instantiate the tile
                tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] =
                GameObject.Instantiate(newTileComponent.gameObject, new Vector3(lowestEntropyTilesCoords.x,
                lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z), newTileComponent.gameObject.transform.rotation, transform).GetComponent<TileComponent>();
                // Add its neighbors to be updated
                AddAllNeighboursToPropergationStack(lowestEntropyTilesCoords);
            }
            else
            {
                Debug.LogError("Generation failed");
                ResetGenAttempt();
            }
            // Wait the required time
            yield return new WaitForSeconds(timeBetweenPlacements);
        }
    }

    /// <summary>
    /// Returns all possible tiles for a single grid location
    /// </summary>
    /// <param name="xPosition">The x coordinate of the grid location checked</param>
    /// <param name="yPosition">The y coordinate of the grid location checked</param>
    /// <param name="zPosition">The z coordinate of the grid location checked</param>
    /// <returns>All possible tiles for a single grid location</returns>
    private List<TileComponent> GetPossibilitySpaceForSingleTile(int xPosition, int yPosition, int zPosition)
    {
        // If the tile was already set then the only possibility is that tile
        if (tileGrid[xPosition][yPosition][zPosition] != null)
        {
            return new List<TileComponent>(1) { tileGrid[xPosition][yPosition][zPosition] };
        }
        List<TileComponent> validTiles = new List<TileComponent>(exampleGridData.tilePrefabs);

        // Strip out any and tiles that don't have a chance of spawn at this position
        for (int i = 0; i < validTiles.Count; ++i)
        {
            if (GetWeightOfTileAdjustedForPosition(validTiles[i], xPosition, yPosition) <= 0f)
            {
                validTiles.RemoveAt(i);
                --i;
            }
        }

        // remove all tiles that could not spawn based on neighboring sockets
        Vector3Int gridPosition = new Vector3Int(xPosition, yPosition, zPosition);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.up, 1, SocketData.Sides.Above);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.down, 1, SocketData.Sides.Below);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.forward, 2, SocketData.Sides.Front);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.right, 0, SocketData.Sides.Right);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.back, 2, SocketData.Sides.Back);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.left, 0, SocketData.Sides.Left);

        return validTiles;
    }
    /// <summary>
    /// Returns all possible tiles for a single grid location
    /// </summary>
    /// <param name="gridCoordinates">The coordinate of the grid location checked</param>
    /// <returns>All possible tiles for a single grid location</returns>
    private List<TileComponent> GetPossibilitySpaceForSingleTile(Vector3Int gridCoordinates)
    {
        return GetPossibilitySpaceForSingleTile(gridCoordinates.x, gridCoordinates.y, gridCoordinates.z);
    }
    /// <summary>
    /// Returns the first instance of each tile that has the same ID value
    /// </summary>
    /// <param name="searchedList">The list of tile components to search through</param>
    /// <returns>The first instance of each tile that has the same ID value</returns>
    private List<TileComponent> GetUniqueIDTilesFromList(List<TileComponent> searchedList)
    {
        List<TileComponent> uniqueIdTiles = new List<TileComponent>();
        for (int i = 0; i < searchedList.Count; ++i)
        {
            // Check if the uniqueIdTiles doesn't contain the a tile with same ID
            bool containsID = false;
            for (int j = 0; j < uniqueIdTiles.Count; ++j)
            {
                if (uniqueIdTiles[j].TileData.ID == searchedList[i].TileData.ID)
                {
                    containsID = true;
                    break;
                }
            }
            if (!containsID)
            {
                uniqueIdTiles.Add(searchedList[i]);
            }
        }
        return uniqueIdTiles;
    }
    /// <summary>
    /// Removes and tiles from a list that do not match up the sockets of tile both placed and in the possibility space at a given side
    /// </summary>
    /// <param name="currentTileList">The list of tiles to remove from</param>
    /// <param name="gridPosition">The position in the grid that the list of tiles is linked to</param>
    /// <param name="tileCheckOffset">The grid offset that needs to have its sockets checked against the list</param>
    /// <param name="offSetIndexChecked">The index of the vector3 that the position with have its value changed by the offset</param>
    /// <param name="sideChecked">The side to check the socket of</param>
    /// <returns>The currentTileList with tiles that don't match up the sockets of tile both placed and in the possibility space next to it removed</returns>
    private List<TileComponent> RemoveInvalidPossibleTilesBasedOnSocket(List<TileComponent> currentTileList, Vector3Int gridPosition, Vector3Int tileCheckOffset, int offSetIndexChecked, SocketData.Sides sideChecked)
    {
        List<TileComponent> returnedTiles = currentTileList;
        Vector3Int offestGridPosition = gridPosition + tileCheckOffset;
        // Check the above sockets
        // If the offset grid position is within the bounds of the grid
        if (tileCheckOffset[offSetIndexChecked] > 0 ? gridDimensions[offSetIndexChecked] > offestGridPosition[offSetIndexChecked] : offestGridPosition[offSetIndexChecked] > -1)
        {
            if (tileGrid[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z] != null)
            {
                // Remove all tiles that does not match up with the above socket
                returnedTiles.RemoveAll(tile => !tile.TileData.TileSocketData.CheckValidSocketConnection(tileGrid[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z].TileData.TileSocketData, SocketData.GetOpposingSocket(sideChecked)));
            }
            // Remove all tiles that don't match up with the sockets of the possibilities spaces
            else
            {
                if (possibilitySpace[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z] != null)
                {
                    for (int i = 0; i < returnedTiles.Count; ++i)
                    {
                        // Check if the current return tile can match with any of the socket of given type in the possibility space
                        bool canMatchWithAnyTile = false;
                        for (int j = 0; j < possibilitySpace[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z].Count; ++j)
                        {
                            if (returnedTiles[i].TileData.TileSocketData.CheckValidSocketConnection(possibilitySpace[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z][j].TileData.TileSocketData, SocketData.GetOpposingSocket(sideChecked)))
                            {
                                if (possibilitySpace[offestGridPosition.x][offestGridPosition.y][offestGridPosition.z][j].TileData.TileSocketData.CheckValidSocketConnection(returnedTiles[i].TileData.TileSocketData, sideChecked))
                                {
                                    canMatchWithAnyTile = true;
                                    break;
                                }

                            }
                        }
                        if (!canMatchWithAnyTile)
                        {
                            if (sideChecked == SocketData.Sides.Below)
                            {
                                i = i;
                            }
                            returnedTiles.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }
        else
        {
            // X side should not matter to allow for treadmill effect
            if (offSetIndexChecked != 0)
            {
                returnedTiles.RemoveAll(tile => !tile.TileData.TileSocketData.validNeighbours.GetValidNeighbourListForSide(sideChecked).Contains(-1));
            }

        }

        return returnedTiles;
    }
    private List<TileComponent> ShiftTileAroundGrid(Vector3Int shiftVector)
    {
        List<TileComponent> tilesOutOfGrid = new List<TileComponent>();
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    if (CheckIfCoordIsInGrid(new Vector3Int(x, y, z) + shiftVector))
                    {
                        if (tileGrid[x][y][z] != null)
                        {
                            tileGrid[x][y][z].transform.position += shiftVector;
                        }
                        tileGrid[x + shiftVector.x][y + shiftVector.y][z + shiftVector.z] = tileGrid[x][y][z];
                        tileGrid[x][y][z] = null;
                    }
                    else
                    {
                        tilesOutOfGrid.Add(tileGrid[x][y][z]);
                    }
                }
            }
        }
        return tilesOutOfGrid;
    }

    /// <summary>
    /// Returns a random tile from coordinate in the possibility space 
    /// </summary>
    /// <param name="xPosition">The x coordinate of the possibility space checked</param>
    /// <param name="yPosition">The y coordinate of the possibility space checked</param>
    /// <param name="zPosition">The z coordinate of the possibility space checked</param>
    /// <returns>A random tile from coordinate in the possibility space</returns>
    private TileComponent GetRandomTileFromProababilitySpace(int xPosition, int yPosition, int zPosition)
    {
        // Group the tiles but weights
        List<TileComponent> uniqueIdTiles = GetUniqueIDTilesFromList(possibilitySpace[xPosition][yPosition][zPosition]);

        // Add together the weightings
        float maxWeighting = 0.0f;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            maxWeighting += GetWeightOfTileAdjustedForPosition(uniqueIdTiles[i], xPosition, yPosition);
            //maxWeighting += uniqueIdTiles[i].TileData.GetWeight(yPosition, gridDimensions.y);
        }
        // Roll a random number between 0 and the max value
        float randomNumber = Random.Range(0.0f, maxWeighting);



        // Iterate over the possibility until there is a number between the current total weight and the current total weight
        float currentWeight = 0.0f;
        int selectedTileId = -1;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            if (randomNumber < currentWeight + GetWeightOfTileAdjustedForPosition(uniqueIdTiles[i], xPosition, yPosition))
            {
                selectedTileId = i;
                break;
            }
            currentWeight += GetWeightOfTileAdjustedForPosition(uniqueIdTiles[i], xPosition, yPosition);
        }

        if (selectedTileId > -1)
        {
            // Get the id of the selected tile
            int selectedID = uniqueIdTiles[selectedTileId].TileData.ID;
            // Randomly pick from the possible tiles with the same ID
            List<TileComponent> idMatchingTiles = new List<TileComponent>(4);
            for (int i = 0; i < possibilitySpace[xPosition][yPosition][zPosition].Count; ++i)
            {
                if (possibilitySpace[xPosition][yPosition][zPosition][i].TileData.ID == selectedID)
                {
                    idMatchingTiles.Add(possibilitySpace[xPosition][yPosition][zPosition][i]);
                }
            }
            return idMatchingTiles[Random.Range(0, idMatchingTiles.Count)];
        }

        Debug.LogError("No tile able to be selected in probability space");
        return null;
    }
    /// <summary>
    /// Returns a random tile from coordinate in the possibility space 
    /// </summary>
    /// <param name="gridCoordinates">The coordinate of the possibility space checked</param>
    /// <returns>A random tile from coordinate in the possibility space </returns>
    private TileComponent GetRandomTileFromProababilitySpace(Vector3Int gridCoordinates)
    {
        return GetRandomTileFromProababilitySpace(gridCoordinates.x, gridCoordinates.y, gridCoordinates.z);
    }

    /// <summary>
    /// Adds all the tiles around a given tile to the propagation stack
    /// </summary>
    /// <param name="coordinate"></param>
    private void AddAllNeighboursToPropergationStack(Vector3Int coordinate)
    {
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x, coordinate.y, coordinate.z + 1));
        possibilitySpaceLocationsToUpdate.Push(new Vector3Int(coordinate.x, coordinate.y, coordinate.z - 1));
    }
    /// <summary>
    /// Drop the tiles off the end of the grid
    /// </summary>
    private void DropEndTiles()
    {
        for (int y = 0; y < gridDimensions.y; ++y)
        {
            for (int z = 0; z < gridDimensions.z; ++z)
            {
                if (tileGrid[0][y][z] != null)
                {
                    tileGrid[0][y][z].gameObject.AddComponent<Rigidbody>();
                    tileGrid[0][y][z] = null;
                }
            }
        }
    }
    private void DropTiles(List<TileComponent> tilesToDrop)
    {
        for (int i = 0; i < tilesToDrop.Count; ++i)
        {
            tilesToDrop[i].gameObject.AddComponent<Rigidbody>();
        }
    }
    //private IEnumerator GenerateMoreTiles(int numberOfColumnsToGenerate)
    //{
    //    List<TileComponent> droppedTiles = ShiftTileAroundGrid(Vector3Int.left * numberOfColumnsToGenerate);
    //    DropTiles(droppedTiles);
    //    StartCoroutine(PlacementInterations(new Vector3Int(gridDimensions.x - numberOfColumnsToGenerate, 0, 0), gridDimensions));
    //    yield break;
    //}


    /// <summary>
    /// Resets the generation attempt so that the generation can start over
    /// </summary>
    private void ResetGenAttempt()
    {
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            for (int j = 0; j < tileGrid[i].Length; ++j)
            {
                for (int k = 0; k < tileGrid[i][j].Length; ++k)
                {
                    if (tileGrid[i][j][k] != null)
                    {
                        Destroy(tileGrid[i][j][k].gameObject);
                    }
                }
            }
        }

        tileGrid = new TileComponent[gridDimensions.x][][];
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            tileGrid[i] = new TileComponent[gridDimensions.y][];
            for (int k = 0; k < tileGrid[i].Length; ++k)
            {

                tileGrid[i][k] = new TileComponent[gridDimensions.z];
            }
        }

        possibilitySpace = new List<TileComponent>[gridDimensions.x][][];
        for (int i = 0; i < possibilitySpace.Length; ++i)
        {
            possibilitySpace[i] = new List<TileComponent>[gridDimensions.y][];
            for (int j = 0; j < possibilitySpace[i].Length; ++j)
            {
                possibilitySpace[i][j] = new List<TileComponent>[gridDimensions.z];
            }
        }
        progressSlider.value = 0f;
    }
    /// <summary>
    /// Updates every tile in the possibility space
    /// </summary>
    [System.Obsolete]
    private void UpdateEntireProbalitySpace()
    {
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    List<TileComponent> previousPossibilites = new List<TileComponent>(possibilitySpace[x][y][z]);
                    possibilitySpace[x][y][z] = GetPossibilitySpaceForSingleTile(x, y, z);

                    if (possibilitySpace[x][y][z].Count == 0)
                    {
                        Debug.LogError("Possibility space was empty for tile at grid position:" + new Vector3Int(x, y, z).ToString());
                    }
                    bool samePossibilitesForThisSpace = true;
                    for (int i = 0; i < previousPossibilites.Count; ++i)
                    {
                        if (!possibilitySpace[x][y][z].Contains(previousPossibilites[i]))
                        {
                            samePossibilitesForThisSpace = false;
                            break;
                        }
                    }
                    if (!samePossibilitesForThisSpace)
                    {
                        AddAllNeighboursToPropergationStack(new Vector3Int(x, y, z));
                        if (showPossibillitySpace)
                        {
                            for (int j = 0; j < possibilitySpaceVisualiserObjects[x][y][z].Length; ++j)
                            {
                                GameObject.Destroy(possibilitySpaceVisualiserObjects[x][y][z][j]);
                            }
                            possibilitySpaceVisualiserObjects[x][y][z] = AddProabilitySpaceObjects(possibilitySpace[x][y][z], new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Updates all the possibility spaces added to the stack for updating, will add more tiles to the stack if updating the possibility space changes the stack
    /// </summary>
    private void UpdatePossibilitySpaceFromPropergation()
    {
        int tilesAssesedCount = 0; // This counted is just to prevent an endless loop
        while (possibilitySpaceLocationsToUpdate.Count > 0)
        {
            if (tilesAssesedCount > gridDimensions.x * gridDimensions.y * gridDimensions.z * 100)
            {
                Debug.Log("prorogation looped too many times! Breaking...");
                break;
            }

            // pop the next location to update
            Vector3Int coordAssesed = possibilitySpaceLocationsToUpdate.Pop();
            if (CheckIfCoordIsInGrid(coordAssesed))
            {
                // Update the location
                List<TileComponent> previousPossibilites = new List<TileComponent>(possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z]);
                possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z] = GetPossibilitySpaceForSingleTile(coordAssesed.x, coordAssesed.y, coordAssesed.z);

                if (possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z].Count == 0)
                {
                    Debug.LogError("Possibility space was empty for tile at grid position:" + new Vector3Int(coordAssesed.x, coordAssesed.y, coordAssesed.z).ToString());
                }

                // Check if it changed add its neighbors to the stack if it has
                bool samePossibilitesForThisSpace = true;
                if (previousPossibilites.Count != possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z].Count)
                {
                    samePossibilitesForThisSpace = false;
                }
                else
                {
                    for (int i = 0; i < previousPossibilites.Count; ++i)
                    {
                        if (!possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z].Contains(previousPossibilites[i]))
                        {
                            samePossibilitesForThisSpace = false;
                            break;
                        }
                    }
                }

                if (!samePossibilitesForThisSpace)
                {
                    AddAllNeighboursToPropergationStack(coordAssesed);
                    if (showPossibillitySpace)
                    {
                        for (int j = 0; j < possibilitySpaceVisualiserObjects[coordAssesed.x][coordAssesed.y][coordAssesed.z].Length; ++j)
                        {
                            GameObject.Destroy(possibilitySpaceVisualiserObjects[coordAssesed.x][coordAssesed.y][coordAssesed.z][j]);
                        }
                        possibilitySpaceVisualiserObjects[coordAssesed.x][coordAssesed.y][coordAssesed.z] = AddProabilitySpaceObjects(possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z], new Vector3Int(coordAssesed.x, coordAssesed.y, coordAssesed.z));
                    }
                }
            }

            ++tilesAssesedCount;
        }
    }

    private void PlanTile()
    {
        //// Add every tiles to be updated
        //for (int x = areaStartPosition.x; x < areaEndPosition.x; ++x)
        //{
        //    for (int y = areaStartPosition.y; y < areaEndPosition.y; ++y)
        //    {
        //        for (int z = areaStartPosition.z; z < areaEndPosition.z; ++z)
        //        {
        //            possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.tilePrefabs);
        //            possibilitySpaceLocationsToUpdate.Push(new Vector3Int(x, y, z));
        //        }
        //    }
        //}

        //int numberOfTiles = (areaEndPosition.x - areaStartPosition.x) * (areaEndPosition.y - areaStartPosition.y) * (areaEndPosition.z - areaStartPosition.z);

        //for (int i = 0; i < numberOfTiles; ++i)
        //{
            // Update all the tiles necessary
            UpdatePossibilitySpaceFromPropergation();

            // Find the lowest entropy value tile
            float lowestEntropyValue = float.MaxValue;
            Vector3Int lowestEntropyTilesCoords = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            int count = 0;
            for (int x = tilesBeingPlannedStartX; x < tilesBeingPlannedEndX; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        if (tileGrid[x][y][z] == null)
                        {
                            float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z], y, x);
                            ++count;
                            if (float.IsNaN(entropy))
                            {
                                Debug.LogError("Entropy gave NaN result");
                            }
                            if (entropy < lowestEntropyValue)
                            {
                                lowestEntropyValue = entropy;
                                lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                Debug.Log("Zero entropies calculated for iteration: ");
            }

            // Select tile from its possibilities as the tile to be placed
            TileComponent newTileComponent = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            ++progressSlider.value;
            if (newTileComponent != null)
            {
            // Set the tile ready for instantiation later
            tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] = newTileComponent;
                // Instantiate the tile
                //tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] =
                //GameObject.Instantiate(newTileComponent.gameObject, new Vector3(lowestEntropyTilesCoords.x,
                //lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z), newTileComponent.gameObject.transform.rotation, transform).GetComponent<TileComponent>();
                // Add its neighbors to be updated
                AddAllNeighboursToPropergationStack(lowestEntropyTilesCoords);
            }
            else
            {
                Debug.LogError("Generation failed");
                ResetGenAttempt();
            }
        // Wait the required time
        //yield return new WaitForSeconds(timeBetweenPlacements);
        //}
        ++numberOfTilesPlanned; 
        //xOffset += 2;
        //StartCoroutine(GenerateMoreTiles(2));
    }

    private void SetUpPlannedTiles()
    {
        //// Add every tiles to be updated
        for (int x = plannedTilesStartX; x < tilesBeingPlannedStartX; ++x)
        {
            for (int y = gridDimensions.y; y < gridDimensions.y; ++y)
            {
                for (int z = gridDimensions.z; z < gridDimensions.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.tilePrefabs);
                    possibilitySpaceLocationsToUpdate.Push(new Vector3Int(x, y, z));
                }
            }
        }

        int numberOfTiles = (tilesBeingPlannedStartX - plannedTilesStartX) * gridDimensions.y * gridDimensions.z;

        for (int i = 0; i < numberOfTiles; ++i)
        {
            // Update all the tiles necessary
            UpdatePossibilitySpaceFromPropergation();

            // Find the lowest entropy value tile
            float lowestEntropyValue = float.MaxValue;
            Vector3Int lowestEntropyTilesCoords = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            int count = 0;
            for (int x = plannedTilesStartX; x < tilesBeingPlannedStartX; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        if (tileGrid[x][y][z] == null)
                        {
                            float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z], y, x);
                            ++count;
                            if (float.IsNaN(entropy))
                            {
                                Debug.LogError("Entropy gave NaN result");
                            }
                            if (entropy < lowestEntropyValue)
                            {
                                lowestEntropyValue = entropy;
                                lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                Debug.Log("Zero entropies calculated for iteration: ");
            }

            // Select tile from its possibilities as the tile to be placed
            TileComponent newTileComponent = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            ++progressSlider.value;
            if (newTileComponent != null)
            {
                // Set the tile ready for instantiation later
                tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] = newTileComponent;
                // Add its neighbors to be updated
                AddAllNeighboursToPropergationStack(lowestEntropyTilesCoords);
            }
            else
            {
                Debug.LogError("Generation failed");
                ResetGenAttempt();
            }
        }
    }

    private IEnumerator ForwardsStep()
    {
        // Instantiate the new line of tiles
        for (int y = 0; y < gridDimensions.y; ++y)
        {
            for (int z = 0; z < gridDimensions.z; ++z)
            {
                tileGrid[plannedTilesStartX][y][z] =
                GameObject.Instantiate(tileGrid[plannedTilesStartX][y][z].gameObject, new Vector3(plannedTilesStartX,
                y, z), tileGrid[plannedTilesStartX][y][z].gameObject.transform.rotation, transform).GetComponent<TileComponent>();
            }
        }
        // Push all tiles back;
        List<TileComponent> droppedTiles = ShiftTileAroundGrid(Vector3Int.left);
        DropTiles(droppedTiles);

        ++xOffset;
        // push the start and end of tiles to plan back
        --tilesBeingPlannedStartX;
        --tilesBeingPlannedEndX;

        // Check if there are no more planned tiles
        if (tilesBeingPlannedStartX == plannedTilesStartX)
        {
            if (expectedPlannedTiles != numberOfTilesPlanned)
            {
                yield return new WaitUntil(CheckPlanningComplete);
            }
            tilesBeingPlannedStartX = gridDimensions.x - gridDimensions.x / 3;
            tilesBeingPlannedEndX = gridDimensions.x;
            numberOfTilesPlanned = 0;
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ForwardsStep());
    }

    private bool CheckPlanningComplete()
    {
        return expectedPlannedTiles == numberOfTilesPlanned;
    }
    #endregion
}

