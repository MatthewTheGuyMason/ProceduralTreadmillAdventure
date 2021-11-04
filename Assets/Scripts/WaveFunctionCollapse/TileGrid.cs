using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public class TileComparer : IComparer<TileData>
    {
        // Call CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer<TileData>.Compare(TileData x, TileData y)
        {
            return (x.ID - y.ID);
        }

        //
    }

    [SerializeField]
    private TileComponent[][][] tileGrid;

    [SerializeField]
    private List<TileData>[][][] proabailitySpace;

    [SerializeField]
    private Vector3Int gridDimensions;

    [SerializeField]
    private TileData[] tileSet;

#if UNITY_EDITOR
    [SerializeField]
    private bool showGrid;
#endif


    private void Start()
    {
        tileGrid = new TileComponent[gridDimensions.x][][];
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            tileGrid[i] = new TileComponent[gridDimensions.y][];
            for (int k = 0; k < tileGrid[i].Length; ++k)
            {
                tileGrid[i][k] = new TileComponent[gridDimensions.z];
            }
        }

        proabailitySpace = new List<TileData>[gridDimensions.x][][];
        for (int i = 0; i < proabailitySpace.Length; ++i)
        {
            proabailitySpace[i] = new List<TileData>[gridDimensions.y][];
            for (int j = 0; j < proabailitySpace[i].Length; ++j)
            {
                proabailitySpace[i][j] = new List<TileData>[gridDimensions.z];
                //for (int k = 0; k < proabailitySpace[i][j].Length; ++k)
                //{
                //    proabailitySpace[i][j][k] = new List<Tile>();
                //}
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

    private float GetShannonEntropy(List<TileData> possibilitySpace)
    {
        float weightSum = 0.0f;
        float weightTimesLogWeight = 0.0f;
        for (int i = 0; i < possibilitySpace.Count; ++i)
        {
            weightSum += possibilitySpace[i].BaseTileWeight;
            weightTimesLogWeight += possibilitySpace[i].BaseTileWeight * Mathf.Log(possibilitySpace[0].BaseTileWeight);
        }

        return Mathf.Log(weightSum) - weightTimesLogWeight / weightSum;
    }

    private void WaveFunctionCollapse()
    {
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

    private List<TileData> GetPossibilitySpaceForSingleTile(int xPosition, int yPosition, int zPosition)
    {
        List<TileData> validTiles = new List<TileData>(tileSet);
        // Check the position to see what tiles are valid
        // Floor tile can't be placed off the floor
        if (yPosition > 0)
        {
            validTiles.RemoveAll(tile => tile.TileType == TileData.TileTypes.Floor);
        }
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

        // Check the above sockets
        if (gridDimensions.y > yPosition + 1)
        {
            if (tileGrid[xPosition][yPosition + 1][zPosition] != null)
            {
                // Remove all tiles that does not match up with the above socket
                validTiles.RemoveAll(tile => !tile.TileSocketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition + 1][zPosition].TileData.TileSocketData, SocketData.Sockets.Below));
            }
        }
        else
        {
            validTiles.RemoveAll(tile => !tile.TileSocketData.validNeighbours.AboveNeighbours.Contains(-1));
        }
        // Check below sockets
        if (yPosition > 0)
        {
            if (tileGrid[xPosition][yPosition - 1][zPosition] != null)
            {
                // Remove all tiles that does match up with the below socket
                validTiles.RemoveAll(tile => !tile.TileSocketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition - 1][zPosition].TileData.TileSocketData, SocketData.Sockets.Above));
            }
        }
        else
        {
            validTiles.RemoveAll(tile => !tile.TileSocketData.validNeighbours.BelowNeighbours.Contains(-1));
        }

        if (validTiles.Count == 0)
        {
            int i = 0;
        }

        return validTiles;
    }

    private TileData GetRandomTileFromProababilitySpace(int xPosition, int yPosition, int zPosition)
    {
        // Add together the weightings
        float maxWeighting = 0.0f;
        for (int i = 0; i < proabailitySpace[xPosition][yPosition][zPosition].Count; ++i)
        {
            maxWeighting += proabailitySpace[xPosition][yPosition][zPosition][i].BaseTileWeight;
        }
        // Roll a random number between 0 and the max value
        float randomNumber = Random.Range(0.0f, maxWeighting);

        // Iterate over the possibility until there is a number between the current total weight and the current total weight
        float currentWeight = 0.0f;
        for (int i = 0; i < proabailitySpace[xPosition][yPosition][zPosition].Count; ++i)
        {
            if (randomNumber < currentWeight + proabailitySpace[xPosition][yPosition][zPosition][i].BaseTileWeight)
            {
                return proabailitySpace[xPosition][yPosition][zPosition][i];
            }
            currentWeight += proabailitySpace[xPosition][yPosition][zPosition][i].BaseTileWeight;
        }
        Debug.LogError("No tile able to be selected in probability space");
        return null;
        //return proabailitySpace[xPosition][yPosition][zPosition][Random.Range(0, proabailitySpace[xPosition][yPosition][zPosition].Count)];
    }

    private void PropergateChangeAcrossTileGrid(int xPosition, int yPosition, int zPosition)
    {
        for (int x = xPosition - 1; x < xPosition + 1; ++x)
        {
            for (int y = yPosition - 1; y < yPosition + 1; ++y)
            {
                for (int z = zPosition - 1; z < zPosition + 1; ++z)
                {
                    if (x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y && z >= 0 && z < gridDimensions.z)
                    {
                        List<TileData> currentPossiblitySpace = proabailitySpace[xPosition][yPosition][zPosition];
                        List<TileData> newProabilitySpace = GetPossibilitySpaceForSingleTile(xPosition, yPosition, zPosition);
                        currentPossiblitySpace.Sort(new TileComparer());
                        newProabilitySpace.Sort(new TileComparer());
                        if (newProabilitySpace.Count != currentPossiblitySpace.Count)
                        {
                            proabailitySpace[x][y][z] = newProabilitySpace;
                            PropergateChangeAcrossTileGrid(x, y, z);
                            break;
                        }
                        for (int i = 0; i < currentPossiblitySpace.Count; ++i)
                        {
                            // if the space has changed, propagate over these new tiles
                            if (currentPossiblitySpace[i].ID != newProabilitySpace[i].ID)
                            {

                                proabailitySpace[x][y][z] = newProabilitySpace;
                                PropergateChangeAcrossTileGrid(x, y, z);
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateEntireProbalitySpace()
    {
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    proabailitySpace[x][y][z] = GetPossibilitySpaceForSingleTile(x, y, z);
                }
            }
        }
    }

    private IEnumerator PlacementInteration()
    {
        // Build the initial possibility space
        for (int x = 0; x < gridDimensions.x; ++x)
        {
            for (int y = 0; y < gridDimensions.y; ++y)
            {
                for (int z = 0; z < gridDimensions.z; ++z)
                {
                    proabailitySpace[x][y][z] = GetPossibilitySpaceForSingleTile(x, y, z);
                }
            }
        }

        for (int i = 0; i < gridDimensions.x * gridDimensions.y * gridDimensions.z; ++i)
        {
            // Find the lowest entropy value
            float lowestEntropyValue = float.MaxValue;
            Vector3Int lowestEntropyTilesCoords = Vector3Int.zero;
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        if (proabailitySpace[x][y][z].Count < lowestEntropyValue)
                        {
                            if (tileGrid[x][y][z] == null)
                            {
                                lowestEntropyValue = GetShannonEntropy(proabailitySpace[x][y][z]);
                                lowestEntropyTilesCoords = new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }

            if (lowestEntropyValue == 0)
            {
                int fish = 0;
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
            TileData newTileData = GetRandomTileFromProababilitySpace(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z);
            tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z] = GameObject.Instantiate(tileGrid[lowestEntropyTilesCoords.x][lowestEntropyTilesCoords.y][lowestEntropyTilesCoords.z], new Vector3(lowestEntropyTilesCoords.x, lowestEntropyTilesCoords.y, lowestEntropyTilesCoords.z), transform.rotation, transform);


            // Propagate across to the grid
            UpdateEntireProbalitySpace();
            //PropergateChangeAcrossTileGrid(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);
            yield return new WaitForSeconds(0.0f);
        }
    }

}

