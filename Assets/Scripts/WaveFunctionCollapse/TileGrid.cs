using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public class TileComparer : IComparer<TileComponent>
    {
        int IComparer<TileComponent>.Compare(TileComponent x, TileComponent y)
        {
            return (x.TileData.ID - y.TileData.ID);
        }

        //
    }

    [SerializeField]
    private TileComponent[][][] tileGrid;

    [SerializeField]
    private List<TileComponent>[][][] possibilitySpace;

    private GameObject[][][][] possibilitySpaceObjects;

    [SerializeField]
    private Vector3Int gridDimensions;

    //[SerializeField]
    //private TileComponent[] tileSet;

    [SerializeField]
    private int timeBetweenPlacements;

    [SerializeField]
    private ExampleGridData exampleGridData;

    [SerializeField]
    private UnityEngine.UI.Slider progressSlider;

    //[SerializeField]
    //private int seed;

#if UNITY_EDITOR
    [SerializeField]
    private bool showGrid;
#endif


    private void Start()
    {
        //Random.InitState(seed);
        // Strip out any tiles with a weight of 0 for safety
        for (int i = 0; i < exampleGridData.tilePrefabs.Count; ++i)
        {
            if (exampleGridData.tilePrefabs[i].TileData.Weight <= 0.0f)
            {
                Debug.LogWarning("tilePrefabs " + exampleGridData.tilePrefabs[i].gameObject.name + " had zero weighting and so was removed from possible tiles", exampleGridData.tilePrefabs[i].gameObject);
                exampleGridData.tilePrefabs.RemoveAt(i);
                --i;
            }

        }

        Debug.Log(ShannonEntropy(new float[3] { 0.1f, 0.1f, 0.1f }));
        Debug.Log(ShannonEntropy(new float[3] { 0.12f, 0.02f, 0.40f }));
        Debug.Log(ShannonEntropy(new float[1] { 1f }));

        tileGrid = new TileComponent[gridDimensions.x][][];
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            tileGrid[x] = new TileComponent[gridDimensions.y][];
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                tileGrid[x][y] = new TileComponent[gridDimensions.z];
            }
        }

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

        possibilitySpaceObjects = new GameObject[gridDimensions.x][][][];
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            possibilitySpaceObjects[x] = new GameObject[gridDimensions.y][][];
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                possibilitySpaceObjects[x][y] = new GameObject[gridDimensions.z][];
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    possibilitySpaceObjects[x][y][z] = new GameObject[0];
                }
            }
        }

        WaveFunctionCollapse();
    }

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

    private float GetShannonEntropyOfPossibilitySpace(List<TileComponent> possibilitySpace)
    {
        List<TileComponent> uniqueIdTiles = GetUniqueIDTilesFromList(possibilitySpace);

        float weightSum = 0.0f;
        float weightTimesLogWeight = 0.0f;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            weightSum += uniqueIdTiles[i].TileData.Weight;
            weightTimesLogWeight += uniqueIdTiles[i].TileData.Weight * Mathf.Log(uniqueIdTiles[i].TileData.Weight);
        }

        return (Mathf.Log(weightSum) - weightTimesLogWeight) / weightSum;
    }

    private float ShannonEntropy(float[] possibiltyWeights)
    {
        float weightSum = 0.0f;
        float weightTimesLogWeight = 0.0f;
        for (int i = 0; i < possibiltyWeights.Length; ++i)
        {
            weightSum += possibiltyWeights[i];
            weightTimesLogWeight += possibiltyWeights[i] * Mathf.Log(possibiltyWeights[i]);
        }

        return Mathf.Log(weightSum) - weightTimesLogWeight / weightSum;
    }

    private void WaveFunctionCollapse()
    {
        progressSlider.maxValue = gridDimensions.x * gridDimensions.y * gridDimensions.z;
        progressSlider.value = 0f;
        //UpdateEntireProbalitySpace();
        StartCoroutine(PlacementInteration());

        //// Build the initial possibility space
        //for (int x = 0; x < gridDimensions.x; ++x)
        //{
        //    for (int y = 0; y < gridDimensions.y; ++y)
        //    {
        //        for (int z = 0; z < gridDimensions.z; ++z)
        //        {
        //            proabailitySpace[x][y][z] = GetPossibilitySpaceForSingleTile(x,y,z);
        //        }
        //    }
        //}

        //for (int i = 0; i < gridDimensions.x * gridDimensions.y * gridDimensions.z; ++i)
        //{
        //    // Find the lowest entropy value
        //    int lowestEntropyValue = int.MaxValue;
        //    for (int x = 0; x < gridDimensions.x; ++x)
        //    {
        //        for (int y = 0; y < gridDimensions.y; ++y)
        //        {
        //            for (int z = 0; z < gridDimensions.z; ++z)
        //            {
        //                if (proabailitySpace[x][y][z].Count < lowestEntropyValue)
        //                {
        //                    if (tileGrid[x][y][z] == null)
        //                    {
        //                        lowestEntropyValue = proabailitySpace[x][y][z].Count;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Get all the tile with that level of entropy
        //    List<Vector3Int> lowestEntropyTilesCoords = new List<Vector3Int>();
        //    for (int x = 0; x < gridDimensions.x; ++x)
        //    {
        //        for (int y = 0; y < gridDimensions.y; ++y)
        //        {
        //            for (int z = 0; z < gridDimensions.z; ++z)
        //            {
        //                if (proabailitySpace[x][y][z].Count == lowestEntropyValue)
        //                {
        //                    if (tileGrid[x][y][z] == null)
        //                    {
        //                        lowestEntropyTilesCoords.Add(new Vector3Int(x, y, z));
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Pick a random tile with the lowest entropy
        //    Vector3Int randomTileCoords = lowestEntropyTilesCoords[Random.Range(0, lowestEntropyTilesCoords.Count)];
        //    // Select tile from its possibilities as the tile to be placed
        //    tileGrid[randomTileCoords.x][randomTileCoords.y][randomTileCoords.z] = GetRandomTileFromProababilitySpace(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);
        //    GameObject.Instantiate(tileGrid[randomTileCoords.x][randomTileCoords.y][randomTileCoords.z].gameObject, new Vector3(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z), transform.rotation, transform);

        //    // Propagate across to the grid
        //    PropergateChangeAcrossTileGrid(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);
        //}

        ////for (int x = 0; x < gridDimensions.x; ++x)
        ////{
        ////    for (int y = 0; y < gridDimensions.y; ++y)
        ////    {
        ////        for (int z = 0; z < gridDimensions.z; ++z)
        ////        {
        ////            if (tileGrid[x][y][z] != null)
        ////            {
        ////                GameObject.Instantiate(tileGrid[x][y][z].gameObject, new Vector3(x, y, z), transform.rotation, transform);
        ////            }
        ////        }
        ////    }
        ////}
    }

    private List<TileComponent> GetPossibilitySpaceForSingleTile(int xPosition, int yPosition, int zPosition)
    {
        if (tileGrid[xPosition][yPosition][zPosition] != null)
        {
            return new List<TileComponent>(1) { tileGrid[xPosition][yPosition][zPosition] };
        }
        List<TileComponent> validTiles = new List<TileComponent>(exampleGridData.tilePrefabs);
        // Check the position to see what tiles are valid
        // Floor tile can't be placed off the floor
        //if (yPosition > 0)
        //{
        //    validTiles.RemoveAll(tile => tile.TileData.TileType == TileData.TileTypes.Floor);
        //}
        //// Only floor can be placed at floor level
        //else
        //{
        //    validTiles.RemoveAll(tile => tile.TileType != Tile.TileTypes.Floor);
        //}
        //// Plant tiles can't be placed next to each other
        //for (int x = xPosition - 1; x < xPosition + 1; ++x)
        //{
        //    for (int y = yPosition - 1; y < yPosition + 1; ++y)
        //    {
        //        for (int z = zPosition - 1; z < zPosition + 1; ++z)
        //        {
        //            if (x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y && z >= 0 && z < gridDimensions.z)
        //            {
        //                if (tileGrid[x][y][z] != null)
        //                {
        //                    if (tileGrid[x][y][z].TileType == Tile.TileTypes.Plant)
        //                    {
        //                        validTiles.RemoveAll(tile => tile.TileType == Tile.TileTypes.Plant);
        //                    }
        //                }

        //            }
        //        }
        //    }
        //}

        //Vector3Int position = new Vector3Int(xPosition, yPosition, zPosition);

        //Vector3Int checkPosition = position;
        //checkPosition[0] += 1;
        //if (gridDimensions[0] > checkPosition[0])
        //{
        //    if (tileGrid[checkPosition.x][checkPosition.y][checkPosition.z] != null)
        //    {
        //        // Remove all tiles that does not match up with the above socket
        //        validTiles.RemoveAll(tile => !tile.TileSocketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition + 1][zPosition].TileData.TileSocketData, SocketData.Sides.Below));
        //    }
        //}

        //// Check the above sockets
        //if (gridDimensions.y > yPosition + 1)
        //{
        //    if (tileGrid[xPosition][yPosition + 1][zPosition] != null)
        //    {
        //        // Remove all tiles that does not match up with the above socket
        //        validTiles.RemoveAll(tile => !tile.TileSocketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition + 1][zPosition].TileData.TileSocketData, SocketData.Sides.Below));
        //    }
        //}
        //else
        //{
        //    validTiles.RemoveAll(tile => !tile.TileSocketData.validNeighbours.AboveNeighbours.Contains(-1));
        //}
        //// Check below sockets
        //if (yPosition > 0)
        //{
        //    if (tileGrid[xPosition][yPosition - 1][zPosition] != null)
        //    {
        //        // Remove all tiles that does match up with the below socket
        //        validTiles.RemoveAll(tile => !tile.TileSocketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition - 1][zPosition].TileData.TileSocketData, SocketData.Sides.Above));
        //    }
        //}
        //else
        //{
        //    validTiles.RemoveAll(tile => !tile.TileSocketData.validNeighbours.BelowNeighbours.Contains(-1));
        //}

        //if (validTiles.Count == 0)
        //{
        //    int i = 0;
        //}

        Vector3Int gridPosition = new Vector3Int(xPosition, yPosition, zPosition);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.up,       1, SocketData.Sides.Above);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.down,     1, SocketData.Sides.Below);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.forward,  2, SocketData.Sides.Front);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.right,    0, SocketData.Sides.Right);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.back,     2, SocketData.Sides.Back);
        validTiles = RemoveInvalidPossibleTilesBasedOnSocket(validTiles, gridPosition, Vector3Int.left,     0, SocketData.Sides.Left);

        return validTiles;
    }

    private TileComponent GetRandomTileFromProababilitySpace(int xPosition, int yPosition, int zPosition)
    {
        // Group the tiles but weights
        List<TileComponent> uniqueIdTiles = GetUniqueIDTilesFromList(possibilitySpace[xPosition][yPosition][zPosition]);

        // Add together the weightings
        float maxWeighting = 0.0f;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            maxWeighting += uniqueIdTiles[i].TileData.Weight;
        }
        // Roll a random number between 0 and the max value
        float randomNumber = Random.Range(0.0f, maxWeighting);



        // Iterate over the possibility until there is a number between the current total weight and the current total weight
        float currentWeight = 0.0f;
        int selectedTileId = -1;
        for (int i = 0; i < uniqueIdTiles.Count; ++i)
        {
            if (randomNumber < currentWeight + uniqueIdTiles[i].TileData.Weight)
            {
                selectedTileId = i;
                break;
            }
            currentWeight += uniqueIdTiles[i].TileData.Weight;
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
        //return proabailitySpace[xPosition][yPosition][zPosition][Random.Range(0, proabailitySpace[xPosition][yPosition][zPosition].Count)];
    }

    //private void PropergateChangeAcrossTileGrid(int xPosition, int yPosition, int zPosition)
    //{
    //    for (int x = xPosition - 1; x < xPosition + 1; ++x)
    //    {
    //        for (int y = yPosition - 1; y < yPosition + 1; ++y)
    //        {
    //            for (int z = zPosition - 1; z < zPosition + 1; ++z)
    //            {
    //                if (x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y && z >= 0 && z < gridDimensions.z)
    //                {
    //                    List<TileComponent> currentPossiblitySpace = possibilitySpace[xPosition][yPosition][zPosition];
    //                    List<TileComponent> newProabilitySpace = GetPossibilitySpaceForSingleTile(xPosition, yPosition, zPosition);
    //                    currentPossiblitySpace.Sort(new TileComparer());
    //                    newProabilitySpace.Sort(new TileComparer());
    //                    if (newProabilitySpace.Count != currentPossiblitySpace.Count)
    //                    {
    //                        possibilitySpace[x][y][z] = newProabilitySpace;
    //                        PropergateChangeAcrossTileGrid(x, y, z);
    //                        break;
    //                    }
    //                    for (int i = 0; i < currentPossiblitySpace.Count; ++i)
    //                    {
    //                        // if the space has changed, propagate over these new tiles
    //                        if (currentPossiblitySpace[i].TileData.ID != newProabilitySpace[i].TileData.ID)
    //                        {
    //                            possibilitySpace[x][y][z] = newProabilitySpace;
    //                            PropergateChangeAcrossTileGrid(x, y, z);
    //                            for (int j = 0; i < possibilitySpaceObjects[x][y][z].Length; ++i)
    //                            {
    //                                GameObject.Destroy(possibilitySpaceObjects[x][y][z][j]);
    //                            }
    //                            AddProabilitySpaceObjects(newProabilitySpace, new Vector3Int(x, y, z));
    //                            possibilitySpaceObjects[x][y][z] = AddProabilitySpaceObjects(newProabilitySpace, new Vector3Int(x, y, z));
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    private void UpdateEntireProbalitySpace()
    {
        bool samePossibilities = true;
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
                            samePossibilities = false;
                            samePossibilitesForThisSpace = false;
                            break;
                        }
                    }
                    if (!samePossibilitesForThisSpace)
                    {
                        for (int j = 0; j < possibilitySpaceObjects[x][y][z].Length; ++j)
                        {
                            GameObject.Destroy(possibilitySpaceObjects[x][y][z][j]);
                        }
                        possibilitySpaceObjects[x][y][z] = AddProabilitySpaceObjects(possibilitySpace[x][y][z], new Vector3Int(x, y, z));
                    }
                }
            }
        }
        if (!samePossibilities)
        {
            UpdateEntireProbalitySpace();
        }
    }

    private IEnumerator PlacementInteration()
    {
        UpdateEntireProbalitySpace();

        for (int i = 0; i < gridDimensions.x * gridDimensions.y * gridDimensions.z; ++i)
        {
            UpdateEntireProbalitySpace();

            // Find the lowest entropy value
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
                            float entropy = GetShannonEntropyOfPossibilitySpace(possibilitySpace[x][y][z]);
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

            // Get all the tile with that level of entropy

            //for (int x = 0; x < gridDimensions.x; ++x)
            //{
            //    for (int y = 0; y < gridDimensions.y; ++y)
            //    {
            //        for (int z = 0; z < gridDimensions.z; ++z)
            //        {
            //            if (GetShannonEntropy(proabailitySpace[x][y][z]) == lowestEntropyValue)
            //            {
            //                if (tileGrid[x][y][z] == null)
            //                {
            //                    lowestEntropyTilesCoords.Add(new Vector3Int(x, y, z));
            //                }
            //            }
            //        }
            //    }
            //}

            //// Pick a random tile with the lowest entropy
            //Vector3Int randomTileCoords = lowestEntropyTilesCoords[Random.Range(0, lowestEntropyTilesCoords.Count)];
            // Select tile from its possibilities as the tile to be placed
            TileComponent newTileComponent = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            ++progressSlider.value;
            if (newTileComponent != null)
            {
                tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] =
                GameObject.Instantiate(newTileComponent.gameObject, new Vector3(lowestEntropyTilesCoords.x,
                lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z), newTileComponent.gameObject.transform.rotation, transform).GetComponent<TileComponent>();
            }
            else
            {
                Debug.LogError("Generation failed");
                ResetGenAttempt();
            }

            // Propagate across to the grid

            //PropergateChangeAcrossTileGrid(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);

            yield return new WaitForSeconds(timeBetweenPlacements);
        }
    }


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
                // TODO: Make this go through and check if there is a valid connection in any of the possibility space

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
            returnedTiles.RemoveAll(tile => !tile.TileData.TileSocketData.validNeighbours.GetValidNeighbourListForSide(sideChecked).Contains(-1));
        }

        return returnedTiles;
    }

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
                //for (int k = 0; k < proabailitySpace[i][j].Length; ++k)
                //{
                //    proabailitySpace[i][j][k] = new List<Tile>();
                //}
            }
        }
        progressSlider.value = 0f;
    }

    private GameObject[] AddProabilitySpaceObjects(List<TileComponent> possibilitySpace, Vector3Int gridPosition)
    {
        GameObject[] newGameObjects = new GameObject[possibilitySpace.Count + 1];
        newGameObjects[0] = new GameObject(gridPosition.ToString());
        newGameObjects[0].transform.position = gridPosition;
        for (int i = 1; i < newGameObjects.Length; ++i)
        {
            // Get where is should be placed
            Vector3 position = gridPosition;
            position -= Vector3.one * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * 0.5f;
            position += Vector3.one / (possibilitySpace.Count + 1) * i;
            newGameObjects[i] = GameObject.Instantiate(possibilitySpace[i - 1].gameObject, position, possibilitySpace[i - 1].transform.rotation, newGameObjects[0].transform);
            newGameObjects[i].transform.localScale = Vector3.one / (possibilitySpace.Count + 1);
        }
        return newGameObjects;
    }

    private List<TileComponent> GetUniqueIDTilesFromList(List<TileComponent> SearchedList)
    {
        List<TileComponent> uniqueIdTiles = new List<TileComponent>();
        for (int i = 0; i < SearchedList.Count; ++i)
        {
            // Check if the uniqueIdTiles doesn't contain the a tile with same ID
            bool containsID = false;
            for (int j = 0; j < uniqueIdTiles.Count; ++j)
            {
                if (uniqueIdTiles[j].TileData.ID == SearchedList[i].TileData.ID)
                {
                    containsID = true;
                    break;
                }
            }
            if (!containsID)
            {
                uniqueIdTiles.Add(SearchedList[i]);
            }
        }
        return uniqueIdTiles;
    }

    //private void BuildTileSetProtoTypes()
    //{
    //    List<TileComponent> newTileSet = new List<TileComponent>(tileSet.Length * 4);
    //    for (int i = 0; i < tileSet.Length; ++i)
    //    {
    //        for (int j = 0; j < 4; ++i)
    //        {
    //            // TODO: figure out the best way to set up the prototypes 
    //            //newTileSet.Add()
    //        }
    //    }
    //}
}

