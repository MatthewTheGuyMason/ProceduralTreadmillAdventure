//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               WaveFunctionCollapseTreadmill.cs
//  Author:             Matthew Mason
//  Date Created:       06/10/2021
//  Date Last Modified  15/12/2021
//  Brief:              Script controlling the generation of tiles into a grid using wave function collapse
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script controlling the generation of tiles into a grid using wave function collapse
/// </summary>
public class WaveFunctionCollapseTreadmill : MonoBehaviour
{
    #region Public Constants
    /// <summary>
    /// The number of sections the grid is split into (one for visible tiles, one for planned tiles and one for the tiles being planned)
    /// </summary>
    public const int numberOfSections = 3;

    public const int ProabilitySpaceLoopBreakerMultiplier = 100;
    #endregion
    
    #region Private Serialized Field
    [SerializeField] [Tooltip("If the tiles representing the possibility space are instantiated as the generation goes on")]
    private bool showPossibillitySpace = true;

    [SerializeField] [Tooltip("The data one how the biomes should transition based what tile it is on")]
    private BiomeTransitionaryData biomeTransitionaryData;

    [SerializeField] [Tooltip("The amount of time before it should place down a new tile ")]
    private float timeBetweenPlacements;

    [SerializeField] [Tooltip("The number of possibility spaces to check per frame from the stack, will make each frame slower but more likely to finish plan before reaching next section")]
    private int numberOfPossiblitySpacesToCheckPerFrame = 200;
    [SerializeField] [Tooltip("The amount the x offset of the treadmill effect is multiplied by, used to transition biomes quicker")]
    private int xOffsetMultiplier = 1;

    [SerializeField] [Tooltip("The tile set generated from an example grid")]
    private TileSet exampleGridData;

    [SerializeField] [Tooltip("The slider used to show how far along the grid generation is")]
    private UnityEngine.UI.Slider progressSlider;

    [SerializeField] [Tooltip("The dimensions of the grid that will be generated")]
    private Vector3Int gridDimensions;
    #endregion

    #region Private Variables
    /// <summary>
    /// Used to mark out which grid spaces are in the stack possibility stack
    /// </summary>
    private bool[][][] possibilityUpdateExistsInStack;

    /// <summary>
    /// The game objects used to visual
    /// </summary>
    private GameObject[][][][] possibilitySpaceVisualiserObjects;

    /// <summary>
    /// The start of the tiles that have been planned
    /// </summary>
    private int plannedTilesStartX;
    /// <summary>
    /// The start of the tiles that are currently being planed
    /// </summary>
    private int tilesBeingPlannedStartX;
    /// <summary>
    /// The end of the tiles that are currently being planned
    /// </summary>
    private int tilesBeingPlannedEndX;
    /// <summary>
    /// How many steps forward have been taken, moving the generation along the xAxis
    /// </summary>
    private int xOffset;

    /// <summary>
    /// All the tiles that could possibility exist in each grid coordinate
    /// </summary>
    private List<TileComponent>[][][] possibilitySpace;

    /// <summary>
    /// A stack of all the coordinates in the possibility space that need to be updated
    /// </summary>
    private Stack<Vector3Int> possibilitySpaceLocationsToUpdate;

    /// <summary>
    /// The tiles that have been placed down in the grid
    /// </summary>
    private TileComponent[][][] tileGrid;
    #endregion

    #region Unity Methods
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(plannedTilesStartX, 0, -2f), 0.5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(new Vector3(tilesBeingPlannedStartX, 0, -2f), 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(tilesBeingPlannedEndX, 0, -2f), 0.5f);

        for (int x = 0; x < gridDimensions.x; ++x)
        {
            BiomeTransitionaryData.BiomeWeights biomeWeights = biomeTransitionaryData.GetBiomeWeightsAtTile(x + xOffset * xOffsetMultiplier);

            if (biomeWeights.grasslandUnitInterval < biomeWeights.desertUnitInterval)
            {
                Gizmos.color = Color.yellow;
            }
            else if (biomeWeights.grasslandUnitInterval < biomeWeights.tundraUnitInterval)
            {
                Gizmos.color = Color.white;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(new Vector3(x, 0, -3f), 0.5f);
        }
    }
    #endif

    private void Start()
    {
        // Make the grid x size is a multiple of 3
        gridDimensions.x += numberOfSections - gridDimensions.x % numberOfSections;

        int thirdOfXDimension = (gridDimensions.x / 3);
        // One third of the grid is being planned space
        tilesBeingPlannedStartX = gridDimensions.x - thirdOfXDimension;
        tilesBeingPlannedEndX = gridDimensions.x;
        // The next third of the grid is planned space (the last third is visible space but no variables are necessary)
        plannedTilesStartX = tilesBeingPlannedStartX - thirdOfXDimension;

        // Create propagation stack
        possibilitySpaceLocationsToUpdate = new Stack<Vector3Int>();

        // Create all the jagged arrays
        tileGrid = new TileComponent[gridDimensions.x][][];
        possibilityUpdateExistsInStack = new bool[gridDimensions.x][][];
        possibilitySpace = new List<TileComponent>[gridDimensions.x][][];
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            tileGrid[x] = new TileComponent[gridDimensions.y][];
            possibilityUpdateExistsInStack[x] = new bool[gridDimensions.y][];
            possibilitySpace[x] = new List<TileComponent>[gridDimensions.y][];
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                tileGrid[x][y] = new TileComponent[gridDimensions.z];
                possibilityUpdateExistsInStack[x][y] = new bool[gridDimensions.z];
                possibilitySpace[x][y] = new List<TileComponent>[gridDimensions.z];
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.TilePrefabs);
                    possibilitySpace[x][y][z].AddRange(exampleGridData.TilePrefabs);
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
        progressSlider.minValue = 0f;
        progressSlider.maxValue = (tilesBeingPlannedEndX - tilesBeingPlannedStartX) * gridDimensions.y * gridDimensions.z;
        progressSlider.value = 0f;
        // Set up visible space
        PlaceTilesInGridArea(Vector3Int.zero, new Vector3Int(plannedTilesStartX, gridDimensions.y, gridDimensions.z));
        // Set up planned tiles (And the to be planned tiles will just be possibility space anyway which is what we want)
        SetUpPlannedTiles();
        // Start the loop
        StartCoroutine(ForwardsStep());
    }

    private void Update()
    {
        if (possibilitySpaceLocationsToUpdate.Count > 0)
        {
            // Update the possibility space until it is either fully finished or the limit of times it can be update per frame has been reached
            for (int i = 0; i < numberOfPossiblitySpacesToCheckPerFrame; ++i)
            {
                if (possibilitySpaceLocationsToUpdate.Count > 0)
                {
                    if (!UpdateSinglePossibilitySpaceLocation())
                    {

                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogWarning("Update single possibility space has failed in update");
                        #endif
                        ResetGenAttempt();
                        return;
                    }

                }
                else
                {
                    // Always build up the planned tiles if not completed 
                    if (!CheckPlanningComplete())
                    {
                        if (tilesBeingPlannedStartX != tilesBeingPlannedEndX)
                        {
                            PlanTile();
                        }

                        break;
                    }
                }
            }
        }
        else
        {
            // Always build up the planned tiles if not completed 
            if (!CheckPlanningComplete())
            {
                if (tilesBeingPlannedStartX != tilesBeingPlannedEndX)
                {
                    PlanTile();
                }
            }
        }

    }
    #endregion

    #region Private Methods
    #region Boolean Returning
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
    /// Check if the number of planned tiles is equal to the expected
    /// </summary>
    /// <returns>True if the number of planned tiles is equal to the expected</returns>
    private bool CheckPlanningComplete()
    {
        for (int x = tilesBeingPlannedStartX; x < tilesBeingPlannedEndX; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    if (tileGrid[x][y][z] == null)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    /// <summary>
    /// Attempts to find a the tile with lowest entropy in a given area 
    /// </summary>
    /// <param name="areaInclusiveBottemBackLeft">The minimal grid position values of the area of grid to search for the lowest entropy tile, including the cells with these values</param>
    /// <param name="areaExclusiveTopFrontRight">The maximum grid position values of the area of grid to search for the lowest entropy tile, excluding the cells with these values</param>
    /// <param name="lowestEntropyTilesCoords">The tile found to have the lowest entropy</param>
    /// <returns>True if ant tile with a valid entropy value was found, false otherwise</returns>
    private bool TryGetLowestEntropyTile(Vector3Int areaInclusiveBottemBackLeft, Vector3Int areaExclusiveTopFrontRight, out Vector3Int lowestEntropyTilesCoords)
    {
        // Find the lowest entropy value tile
        float lowestEntropyValue = float.MaxValue;
        int count = 0;
        lowestEntropyTilesCoords = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        for (int x = areaInclusiveBottemBackLeft.x; x < areaExclusiveTopFrontRight.x; ++x)
        {
            for (int y = areaInclusiveBottemBackLeft.y; y < areaExclusiveTopFrontRight.y; ++y)
            {
                for (int z = areaInclusiveBottemBackLeft.z; z < areaExclusiveTopFrontRight.z; ++z)
                {
                    if (tileGrid[x][y][z] == null)
                    {
                        // If there is one possibility the result is certain and entropy can't get lower than this
                        if (possibilitySpace[x][y][z].Count == 1)
                        {
                            lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            return true;
                        }
                        float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z], y, x);
                        ++count;
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if (float.IsNaN(entropy))
                        {
                            Debug.LogError("Entropy gave NaN result at possibility space: " + new Vector3(x, y, z));
                            for (int i = 0; i < possibilitySpace[x][y][z].Count; ++i)
                            {
                                Debug.Log(possibilitySpace[x][y][z][i].gameObject.name + " currently had a weight of: " + GetWeightOfTileAdjustedForPosition(possibilitySpace[x][y][z][i], x, y));
                            }
                        }
                        #endif
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
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Zero entropies calculated for Grid");
            #endif
            return false;
        }
        return true;
    }
    /// <summary>
    /// Updates all the possibility spaces added to the stack for updating, will add more tiles to the stack if updating the possibility space changes the stack
    /// </summary>
    private bool UpdatePossibilitySpaceFromPropergation()
    {
        int tilesAssesedCount = 0; // This counted is just to prevent an endless loop
        while (possibilitySpaceLocationsToUpdate.Count > 0)
        {
            if (tilesAssesedCount > gridDimensions.x * gridDimensions.y * gridDimensions.z * ProabilitySpaceLoopBreakerMultiplier)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("prorogation looped too many times! Breaking...");
                #endif
                break;
            }

            if (!UpdateSinglePossibilitySpaceLocation())
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("UpdateSinglePossibilitySpaceLocation failed in UpdatePossibilitySpaceFromPropergation");
                #endif
                return false;
            }

            ++tilesAssesedCount;
        }
        return true;
    }
    /// <summary>
    /// Pops and updates a single possibility space location from the needs updating stack
    /// </summary>
    private bool UpdateSinglePossibilitySpaceLocation()
    {
        // pop the next location to update
        Vector3Int coordAssesed = possibilitySpaceLocationsToUpdate.Pop();
        possibilityUpdateExistsInStack[coordAssesed.x][coordAssesed.y][coordAssesed.z] = false;
        if (CheckIfCoordIsInGrid(coordAssesed))
        {
            // Update the location
            List<TileComponent> previousPossibilites = new List<TileComponent>(possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z]);
            possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z] = GetPossibilitySpaceForSingleTile(coordAssesed.x, coordAssesed.y, coordAssesed.z);

            if (possibilitySpace[coordAssesed.x][coordAssesed.y][coordAssesed.z].Count == 0)
            {
                return false;
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
        return true;
    }
    #endregion

    #region Float Returning
    /// <summary>
    /// Returns the weight of a given tile adjusted for its position in the grid
    /// </summary>
    /// <param name="tileComponent">The tile component to get the weight of</param>
    /// <param name="xPosition">The x position of the tile when the weight is gotten</param>
    /// <param name="yPosition">The y position of the tile when the weight is gotten</param>
    /// <returns>the weight of a given tile adjusted for its position in the grid</returns>
    private float GetWeightOfTileAdjustedForPosition(TileComponent tileComponent, int xPosition, int yPosition)
    {
        float baseWeight = tileComponent.TileData.GetWeight(yPosition, gridDimensions.y);
        float transitionWeight;
        // Get the current weights from the transitional data
        BiomeTransitionaryData.BiomeWeights weights = biomeTransitionaryData.GetBiomeWeightsAtTile(xPosition + (xOffset * xOffsetMultiplier));
        switch (tileComponent.TileData.TileBiomeType)
        {
            case TileData.BiomeType.Desert:
                return baseWeight * weights.desertUnitInterval;
            case TileData.BiomeType.DesertToGrassland:
                // If its a transition base the weight multiplier off how close the weightings
                if (weights.grasslandUnitInterval > 0f && weights.desertUnitInterval > 0f)
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
                    return Mathf.Max(float.Epsilon, baseWeight * transitionWeight); // Transition tiles are sometimes essential to the grid so better not lock them off completely
                }
                else
                {
                    return 0f;
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
                    return Mathf.Max(float.Epsilon, baseWeight * transitionWeight); // Transition tiles are sometimes essential to the grid so better not lock them off completely
                }
                else
                {
                    return 0f;
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
            // Discount any tiles that have a weight of zero
            if (tileWeight == 0f)
            {
                tileWeight = float.Epsilon;
            }
            weightSum += tileWeight;
            weightTimesLogWeight += tileWeight * Mathf.Log(tileWeight);
        }

        return (Mathf.Log(weightSum) - weightTimesLogWeight) / weightSum;
    }
    #endregion

    #region GameObject Returning
    /// <summary>
    /// Instantiates the game object to visualize the possibility space
    /// </summary>
    /// <param name="possibilitySpace"></param>
    /// <param name="gridPosition"></param>
    /// <returns>all the object instantiated in the visualization</returns>
    private GameObject[] AddProabilitySpaceObjects(List<TileComponent> possibilitySpace, Vector3Int gridPosition)
    {
        GameObject[] newGameObjects = new GameObject[possibilitySpace.Count + 1];
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (possibilitySpace.Count == 0)
        {

            // Creating a error sphere for debugging purposes
            GameObject errorSphere = GameObject.Instantiate(exampleGridData.TilePrefabs[32].gameObject, gridPosition, transform.rotation);
            errorSphere.transform.position = gridPosition;
            errorSphere.name = "ERROR!";
            errorSphere.tag = "Finish";
            newGameObjects[0] = errorSphere;
            return newGameObjects;
        }
        #endif
        newGameObjects[0] = new GameObject(gridPosition.ToString());
        newGameObjects[0].transform.position = gridPosition;
        for (int i = 1; i < newGameObjects.Length; ++i)
        {
            // Get where it should be placed
            Vector3 position = gridPosition;
            position -= Vector3.one * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * i;
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (possibilitySpace[i - 1] == null)
            {
                Debug.LogError("possibilitySpace.game at index " + (i - 1) + " was null");
            }
            if (possibilitySpace[i - 1].gameObject == null)
            {
                Debug.LogError("possibilitySpace.game at index " + (i - 1) + " was null");
            }
            if (newGameObjects[0].gameObject == null)
            {
                Debug.LogError("newGameObjects at index 0 was null");
            }
            #endif
            newGameObjects[i] = GameObject.Instantiate(possibilitySpace[i - 1].gameObject, position, possibilitySpace[i - 1].transform.rotation, newGameObjects[0].transform);
            newGameObjects[i].transform.localScale = Vector3.one / (possibilitySpace.Count + 1);
        }
        return newGameObjects;
    }
    #endregion

    #region IEnumerator Returning
    /// <summary>
    /// Coroutine that make the entire grid step forwards, waiting a set amount of time before moving forwards and for the planning to finished before running itself again
    /// </summary>
    private IEnumerator ForwardsStep()
    {
        // Push all tiles back;
        ShiftTileAroundGrid(Vector3Int.left, out List<TileComponent> droppedTiles);
        DropTiles(droppedTiles);

        yield return new WaitForSeconds(timeBetweenPlacements);
        // Instantiate the new line of tiles
        int xPositionToSpawn = plannedTilesStartX - 1;
        for (int y = 0; y < gridDimensions.y; ++y)
        {
            for (int z = 0; z < gridDimensions.z; ++z)
            {
                tileGrid[xPositionToSpawn][y][z] =
                GameObject.Instantiate(tileGrid[xPositionToSpawn][y][z].gameObject, new Vector3(xPositionToSpawn,
                y, z), tileGrid[xPositionToSpawn][y][z].gameObject.transform.rotation, transform).GetComponent<TileComponent>();
            }
        }
        MoveTilesToGridPositions();

        ++xOffset;
        // push the start and end of tiles to plan back
        --tilesBeingPlannedStartX;
        --tilesBeingPlannedEndX;

        // Check if there are no more planned tiles
        if (tilesBeingPlannedStartX == plannedTilesStartX)
        {
            if (!CheckPlanningComplete())
            {
                yield return new WaitUntil(CheckPlanningComplete);
            }
            progressSlider.value = 0;

            tilesBeingPlannedStartX = gridDimensions.x - gridDimensions.x / 3;
            tilesBeingPlannedEndX = gridDimensions.x;
        }
        StartCoroutine(ForwardsStep());
    }
    #endregion

    #region List Of Tile Component Returning
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
        List<TileComponent> validTiles = new List<TileComponent>(exampleGridData.TilePrefabs);

        Dictionary<TileComponent, float> tileWeights = new Dictionary<TileComponent, float>();

        // Strip out any and tiles that don't have a chance of spawn at this position
        for (int i = 0; i < validTiles.Count; ++i)
        {
            float weight = GetWeightOfTileAdjustedForPosition(validTiles[i], xPosition, yPosition);
            if (weight <= 0f)
            {
                validTiles.RemoveAt(i);
                --i;
            }
            else
            {
                tileWeights.Add(validTiles[i], weight);
            }
        }

        // Remove all tiles that could not spawn based on neighboring sockets
        Vector3Int gridPosition = new Vector3Int(xPosition, yPosition, zPosition);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.up, 1, SocketData.Sides.Above);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.down, 1, SocketData.Sides.Below);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.forward, 2, SocketData.Sides.Front);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.right, 0, SocketData.Sides.Right);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.back, 2, SocketData.Sides.Back);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.left, 0, SocketData.Sides.Left);

        // If all tiles left have epsilon weighting
        // This is code that could mostly likely have be done better with more time, however it fixes an issue with incorrect biomes related transition tile placement
        bool allTilesEpislon = true;
        for (int i = 0; i < validTiles.Count; ++i)
        {
            if (tileWeights.TryGetValue(validTiles[i], out float weight))
            {
                if (weight != float.Epsilon)
                {
                    allTilesEpislon = false;
                    break;
                }
            }
        }
        if (allTilesEpislon)
        {

            List<TileComponent> newValidList = new List<TileComponent>(validTiles);
            // Get the next and previous biomes to make sure they are valid
            BiomeTransitionaryData.BiomeWeights nextBiomeType = biomeTransitionaryData.GetNextKey(xPosition + xOffset);
            BiomeTransitionaryData.BiomeWeights lastBiomeType = biomeTransitionaryData.GetKeyFrameBeforeLast(xPosition + xOffset);

            bool removeDesert = true;
            if (lastBiomeType.desertUnitInterval > 0 || nextBiomeType.desertUnitInterval > 0)
            {
                removeDesert = false;
            }
            bool removeTundra = true;
            if (lastBiomeType.tundraUnitInterval > 0 || nextBiomeType.tundraUnitInterval > 0)
            {
                removeTundra = false;
            }

            // Validate each one is in the correct biomes
            for (int i = 0; i < newValidList.Count; ++i)
            {
                if (removeDesert)
                {
                    if (newValidList[i].TileData.TileBiomeType == TileData.BiomeType.Desert || validTiles[i].TileData.TileBiomeType == TileData.BiomeType.DesertToGrassland)
                    {
                        newValidList.RemoveAt(i);
                        --i;
                    }

                }
                if (removeTundra)
                {
                    if (newValidList[i].TileData.TileBiomeType == TileData.BiomeType.Tundra || validTiles[i].TileData.TileBiomeType == TileData.BiomeType.GrasslandToSnow)
                    {
                        newValidList.RemoveAt(i);
                        --i;
                    }
                }

            }
        }
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
                // Remove all tiles that does not match up with the socket
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
                returnedTiles.RemoveAll(tile => !tile.TileData.TileSocketData.ValidNeighbours.GetValidNeighbourListForSide(sideChecked).Contains(-1));
            }
        }
        return returnedTiles;
    }
    /// <summary>
    /// Moves each tile's set tile and possibility space along the grid in the given direction
    /// </summary>
    /// <param name="shiftVector">The vector for to shift the tiles in the grid</param>
    /// <param name="tilesShiftedOutOfGrid">The that are no longer in the grid as a result of being shifted</param>
    private void ShiftTileAroundGrid(Vector3Int shiftVector, out List<TileComponent> tilesShiftedOutOfGrid)
    {
        tilesShiftedOutOfGrid = new List<TileComponent>();
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    Vector3Int shiftedPosition = new Vector3Int(x, y, z) + shiftVector;
                    if (CheckIfCoordIsInGrid(shiftedPosition))
                    {
                        tileGrid[shiftedPosition.x][shiftedPosition.y][shiftedPosition.z] = tileGrid[x][y][z];
                        tileGrid[x][y][z] = null;

                        // Move possibility space as well
                        possibilitySpace[shiftedPosition.x][shiftedPosition.y][shiftedPosition.z] = possibilitySpace[x][y][z];
                        possibilitySpace[x][y][z] = new List<TileComponent>();
                        if (showPossibillitySpace)
                        {
                            ClearPossibilitySpaceVisulisation(shiftedPosition);
                            possibilitySpaceVisualiserObjects[shiftedPosition.x][shiftedPosition.y][shiftedPosition.z] = AddProabilitySpaceObjects(possibilitySpace[shiftedPosition.x][shiftedPosition.y][shiftedPosition.z], shiftedPosition);
                        }
                    }
                    else
                    {
                        tilesShiftedOutOfGrid.Add(tileGrid[x][y][z]);
                    }
                }
            }
        }
        // Empty possibility spaces will need fixing
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    if (possibilitySpace[x][y][z].Count == 0)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.TilePrefabs);
                        TryAddCoordinateToNeedsUpdatingStack(position);
                        if (showPossibillitySpace)
                        {
                            ClearPossibilitySpaceVisulisation(position);
                            possibilitySpaceVisualiserObjects[x][y][z] = AddProabilitySpaceObjects(possibilitySpace[position.x][position.y][position.z], position);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Moves all tiles in the grid to the world positions that should be based on their grid positions
    /// </summary>
    private void MoveTilesToGridPositions()
    {
        for (int x = 0; x<  gridDimensions.x; ++x)
        {
            for (int y = 0; y<  gridDimensions.y; ++y)
            {
                for (int z = 0; z<  gridDimensions.z; ++z)
                {
                    if (tileGrid[x][y][z] != null)
                    {
                        tileGrid[x][y][z].transform.position  = new Vector3(x,y,z);
                    }
                }
            }
        }
    }
    #endregion

    #region Tile Component Returning
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
            if (randomNumber <= currentWeight + GetWeightOfTileAdjustedForPosition(uniqueIdTiles[i], xPosition, yPosition))
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
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError("No tile able to be selected in probability space");
        #endif
        if (possibilitySpace[xPosition][yPosition][zPosition].Count != 0)
        {
            // Select the first one so it can still return somthing
            return possibilitySpace[xPosition][yPosition][zPosition][0];
        }
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            Debug.LogError("... Because there was no tile to select");
        }
        #endif
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
    #endregion

    #region Void Returning
    /// <summary>
    /// Adds all the tiles around a given tile to the propagation stack
    /// </summary>
    /// <param name="coordinate">The coordinate to get the neighboring tiles of</param>
    private void AddAllNeighboursToPropergationStack(Vector3Int coordinate)
    {
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x + 1, coordinate.y, coordinate.z));
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x - 1, coordinate.y, coordinate.z));
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x, coordinate.y + 1, coordinate.z));
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x, coordinate.y - 1, coordinate.z));
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x, coordinate.y, coordinate.z + 1));
        TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(coordinate.x, coordinate.y, coordinate.z - 1));
    }
    /// <summary>
    /// Clears all the tiles from 
    /// </summary>
    /// <param name="coordinateToClear"></param>
    private void ClearPossibilitySpaceVisulisation(Vector3Int coordinateToClear)
    {
        if (possibilitySpaceVisualiserObjects != null)
        {
            for (int j = 0; j < possibilitySpaceVisualiserObjects[coordinateToClear.x][coordinateToClear.y][coordinateToClear.z].Length; ++j)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (!possibilitySpaceVisualiserObjects[coordinateToClear.x][coordinateToClear.y][coordinateToClear.z][j].CompareTag("Finish") || possibilitySpaceVisualiserObjects[coordinateToClear.x][coordinateToClear.y][coordinateToClear.z].Length > 0)
                {
                    GameObject.Destroy(possibilitySpaceVisualiserObjects[coordinateToClear.x][coordinateToClear.y][coordinateToClear.z][j]);
                }
                #else
                GameObject.Destroy(possibilitySpaceVisualiserObjects[coordinateToClear.x][coordinateToClear.y][coordinateToClear.z][j]);
                #endif
            }
        }
    }
    /// <summary>
    /// Performs a bit of validation before adding a coordinate to the needs updating stack
    /// </summary>
    /// <param name="coordinate">The coordinate to try to add to the stack</param>
    private void TryAddCoordinateToNeedsUpdatingStack(Vector3Int coordinate)
    {
        if (CheckIfCoordIsInGrid(coordinate))
        {
            if (!possibilityUpdateExistsInStack[coordinate.x][coordinate.y][coordinate.z])
            {
                possibilitySpaceLocationsToUpdate.Push(coordinate);
                possibilityUpdateExistsInStack[coordinate.x][coordinate.y][coordinate.z] = true;
            }
        }
    }
    /// <summary>
    /// Drop the tiles off the end of the grid
    /// </summary>
    private void DropTiles(List<TileComponent> tilesToDrop)
    {
        for (int i = 0; i < tilesToDrop.Count; ++i)
        {
            // Invisible tiles won't be seen so just destroy them
            if (tilesToDrop[i].TileData.TileType == TileData.TileTypes.Empty)
            {
                Destroy(tilesToDrop[i].gameObject);
            }
            else
            {
                tilesToDrop[i].gameObject.AddComponent<Rigidbody>().drag = 1f;
                tilesToDrop[i].gameObject.layer = 6; // add the falling block layer
            }
        }
    }
    /// <summary>
    /// Loops and area of the grid and places all the tiles using wave function collapse
    /// </summary>
    private void PlaceTilesInGridArea(Vector3Int areaStartPosition, Vector3Int areaEndPosition)
    {
        // Add every tiles to be updated
        for (int x = areaStartPosition.x; x < areaEndPosition.x; ++x)
        {
            for (int y = areaStartPosition.y; y < areaEndPosition.y; ++y)
            {
                for (int z = areaStartPosition.z; z < areaEndPosition.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.TilePrefabs);
                    TryAddCoordinateToNeedsUpdatingStack(new Vector3Int(x, y, z));
                }
            }
        }

        int numberOfTiles = (areaEndPosition.x - areaStartPosition.x) * (areaEndPosition.y - areaStartPosition.y) * (areaEndPosition.z - areaStartPosition.z);

        for (int i = 0; i < numberOfTiles; ++i)
        {
            // Update all the tiles necessary
            if (!UpdatePossibilitySpaceFromPropergation())
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("UpdatePossibilitySpaceFromPropergation has failed in PlaceTilesInGridArea");
                #endif
                return;
            }

            // Find the lowest entropy value tile
            if (TryGetLowestEntropyTile(areaStartPosition, areaEndPosition, out Vector3Int lowestEntropyTilesCoords))
            {
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
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    Debug.LogError("Generation failed");
                }
                #endif
            }
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError("TryGetLowestEntropyTile returned false in place tiles in grid area");
            }
            #endif
        }
    }

    /// <summary>
    /// Pick a single tile to be spawn at the lowest entropy cell in the planning grid
    /// </summary>
    private void PlanTile()
    {
        // Update all the tiles necessary
        if (!UpdatePossibilitySpaceFromPropergation())
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("UpdatePossibilitySpaceFromPropergation has failed in PlanTile");
            #endif
            ResetGenAttempt();
            return;
        }

        // Find the lowest entropy value tile
        if (TryGetLowestEntropyTile(new Vector3Int(tilesBeingPlannedStartX,0,0), new Vector3Int(tilesBeingPlannedEndX, gridDimensions.y, gridDimensions.z), out Vector3Int lowestEntropyTilesCoords))
        {
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
        }
    }
    /// <summary>
    /// Resets the generation attempt so that the generation can start over
    /// </summary>
    private void ResetGenAttempt()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning("Resetting tiles being planned");
        #endif

        // Flush the possibility space update stack
        possibilitySpaceLocationsToUpdate.Clear();
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilityUpdateExistsInStack[x][y][z] = false;
                }
            }
        }

        // Reset Planning values
        progressSlider.value = 0;
        // Reset the planning possibility space
        for (int x = tilesBeingPlannedStartX; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    // Reset the previous choice
                    tileGrid[x][y][z] = null;
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.TilePrefabs);
                    if (showPossibillitySpace)
                    {
                        ClearPossibilitySpaceVisulisation(new Vector3Int(x, y, z));
                        possibilitySpaceVisualiserObjects[x][y][z] = AddProabilitySpaceObjects(possibilitySpace[x][y][z], new Vector3Int(x, y, z));
                    }
                    TryAddCoordinateToNeedsUpdatingStack((new Vector3Int(x, y, z)));
                }
            }
        }
    }

    /// <summary>
    /// Sets up all the tiles in the section of the grid for tiles that have already been planned ready to be spawn as visible objects
    /// </summary>
    private void SetUpPlannedTiles()
    {
        //// Add every tiles to be updated
        for (int x = plannedTilesStartX; x < tilesBeingPlannedStartX; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilitySpace[x][y][z] = new List<TileComponent>(exampleGridData.TilePrefabs);
                    TryAddCoordinateToNeedsUpdatingStack((new Vector3Int(x, y, z)));
                }
            }
        }

        // Get how many tiles should be planned
        int numberOfTiles = (tilesBeingPlannedStartX - plannedTilesStartX) * gridDimensions.y * gridDimensions.z;

        for (int i = 0; i < numberOfTiles; ++i)
        {
            // Update all the tiles necessary
            if (!UpdatePossibilitySpaceFromPropergation())
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("UpdatePossibilitySpaceFromPropergation has failed in SetUpPlannedTiles");
                #endif
                return;
            }

            // Find the lowest entropy value tile
            if (TryGetLowestEntropyTile(new Vector3Int(plannedTilesStartX, 0, 0), new Vector3Int(tilesBeingPlannedStartX, gridDimensions.y, gridDimensions.z), out Vector3Int lowestEntropyTilesCoords))
            {
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
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    Debug.LogError("Generation failed On setup");
                }
                #endif
            }
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError("TryGetLowestEntropyTile returned false in SetUpPlannedTiles");
            }
            #endif
        }
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
                            ClearPossibilitySpaceVisulisation(new Vector3Int(x, y, z));
                            possibilitySpaceVisualiserObjects[x][y][z] = AddProabilitySpaceObjects(possibilitySpace[x][y][z], new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }
    }

#endregion
#endregion
}

