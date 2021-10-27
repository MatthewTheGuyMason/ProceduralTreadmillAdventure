using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    // Todo: Make the tiles generate over time

    public class TileComparer : IComparer<Tile>
    {
        // Call CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer<Tile>.Compare(Tile x, Tile y)
        {
            return (x.ID - y.ID);
        }

        //
    }

    [SerializeField]
    private Tile[][][] tileGrid;

    [SerializeField]
    private List<Tile>[][][] proabailitySpace;

    [SerializeField]
    private Vector3Int gridDimensions;

    [SerializeField]
    private Tile[] tileSet;


    private void Start()
    {
        tileGrid = new Tile[gridDimensions.x][][];
        for (int i = 0; i < tileGrid.Length; ++i)
        {
            tileGrid[i] = new Tile[gridDimensions.y][];
            for (int k = 0; k < tileGrid[i].Length; ++k)
            {
                tileGrid[i][k] = new Tile[gridDimensions.z];
            }
        }

        proabailitySpace = new List<Tile>[gridDimensions.x][][];
        for (int i = 0; i < proabailitySpace.Length; ++i)
        {
            proabailitySpace[i] = new List<Tile>[gridDimensions.y][];
            for (int j = 0; j < proabailitySpace[i].Length; ++j)
            {
                proabailitySpace[i][j] = new List<Tile>[gridDimensions.z];
                //for (int k = 0; k < proabailitySpace[i][j].Length; ++k)
                //{
                //    proabailitySpace[i][j][k] = new List<Tile>();
                //}
            }
        }

        WaveFunctionCollapse();
    }

    private void OnDrawGizmos()
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

    private List<Tile> GetPossibilitySpaceForSingleTile(int xPosition, int yPosition, int zPosition)
    {
        List<Tile> validTiles = new List<Tile>(tileSet);
        // Check the position to see what tiles are valid
        // Floor tile can't be placed off the floor
        if (yPosition > 0)
        {
            validTiles.RemoveAll(tile => tile.tileType == Tile.TileType.Floor);
        }
        // Only floor can be placed at floor level
        else
        {
            validTiles.RemoveAll(tile => tile.tileType != Tile.TileType.Floor);
        }
        // Plant tiles can't be placed next to each other
        for (int x = xPosition - 1; x < xPosition + 1; ++x)
        {
            for (int y = yPosition - 1; y < yPosition + 1; ++y)
            {
                for (int z = zPosition - 1; z < zPosition + 1; ++z)
                {
                    if (x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y && z >= 0 && z < gridDimensions.z)
                    {
                        if (tileGrid[x][y][z] != null)
                        {
                            if (tileGrid[x][y][z].tileType == Tile.TileType.Plant)
                            {
                                validTiles.RemoveAll(tile => tile.tileType == Tile.TileType.Plant);
                            }
                        }

                    }
                }
            }
        }

        // Check the above sockets
        if (gridDimensions.y > yPosition + 1)
        {
            if (tileGrid[xPosition][yPosition + 1][zPosition] != null)
            {
                // Remove all tiles that does not match up with the above socket
                validTiles.RemoveAll(tile => !tile.socketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition + 1][zPosition].socketData, SocketData.Sockets.Below));
            }
        }
        // Check below sockets
        if (yPosition > 0)
        {
            if (tileGrid[xPosition][yPosition - 1][zPosition] != null)
            {
                // Remove all tiles that does match up with the below socket
                validTiles.RemoveAll(tile => !tile.socketData.CheckValidSocketConnection(tileGrid[xPosition][yPosition - 1][zPosition].socketData, SocketData.Sockets.Above));
            }
        }

        if (validTiles.Count == 0)
        {
            int i = 0;
        }

        return validTiles;
    }

    private Tile GetRandomTileFromProababilitySpace(int xPosition, int yPosition, int zPosition)
    {
        return proabailitySpace[xPosition][yPosition][zPosition][Random.Range(0, proabailitySpace[xPosition][yPosition][zPosition].Count)];
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
                        List<Tile> currentPossiblitySpace = proabailitySpace[xPosition][yPosition][zPosition];
                        List<Tile> newProabilitySpace = GetPossibilitySpaceForSingleTile(xPosition, yPosition, zPosition);
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
            int lowestEntropyValue = int.MaxValue;
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
                                lowestEntropyValue = proabailitySpace[x][y][z].Count;
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
            List<Vector3Int> lowestEntropyTilesCoords = new List<Vector3Int>();
            for (int x = 0; x < gridDimensions.x; ++x)
            {
                for (int y = 0; y < gridDimensions.y; ++y)
                {
                    for (int z = 0; z < gridDimensions.z; ++z)
                    {
                        if (proabailitySpace[x][y][z].Count == lowestEntropyValue)
                        {
                            if (tileGrid[x][y][z] == null)
                            {
                                lowestEntropyTilesCoords.Add(new Vector3Int(x, y, z));
                            }
                        }
                    }
                }
            }

            // Pick a random tile with the lowest entropy
            Vector3Int randomTileCoords = lowestEntropyTilesCoords[Random.Range(0, lowestEntropyTilesCoords.Count)];
            // Select tile from its possibilities as the tile to be placed
            tileGrid[randomTileCoords.x][randomTileCoords.y][randomTileCoords.z] = GetRandomTileFromProababilitySpace(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);
            GameObject.Instantiate(tileGrid[randomTileCoords.x][randomTileCoords.y][randomTileCoords.z].gameObject, new Vector3(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z), transform.rotation, transform);

            // Propagate across to the grid
            UpdateEntireProbalitySpace();
            //PropergateChangeAcrossTileGrid(randomTileCoords.x, randomTileCoords.y, randomTileCoords.z);
            yield return new WaitForSeconds(0.0f);
        }
    }
}

    ///// <summary>
    ///// Place all the tiles that the user can use to place down more tiles
    ///// </summary>
    //private void PlaceCanvasTiles()
    //{
    //    tileGrid = new Tile[gridDimensions.x][][];
    //    for (int i = 0; i < tileGrid.Length; ++i)
    //    {
    //        tileGrid[i] = new Tile[gridDimensions.y][];
    //        for (int k = 0; k < tileGrid.Length; ++k)
    //        {
    //            tileGrid[i][k] = new Tile[gridDimensions.z];
    //        }
    //    }

    //    for (int i = 0; i < gridDimensions.x; ++i)
    //    {
    //        for (int j = 0; j < gridDimensions.z; ++j)
    //        {
    //            tileGrid[i][0][j] = GameObject.Instantiate(canvasTilePrefabs, new Vector3(i - (float)gridDimensions.x * 0.5f, 0f, j - (float)gridDimensions.z * 0.5f), transform.rotation).GetComponent<Tile>();
    //            tileGrid[i][0][j].gridCoordinates = new Vector3Int(i, 0, j);
    //        }
    //    }
    //}

    //private Tile GetFloorTileFromTileSet()
    //{
    //    List<Tile> ApplicableTiles;
    //    for (int i = 0; i < tileSet.Length; ++i)
    //    {

    //    }
    //}

