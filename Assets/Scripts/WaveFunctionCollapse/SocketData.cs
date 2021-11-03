using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SocketData", menuName = "ScriptableObject/SocketData")]
public class SocketData : ScriptableObject
{
    public enum Sockets
    {
        Above   = 0,
        Below   = 1,

        Front   = 2,
        Right   = 3,
        Back    = 4,
        Left    = 5,

        Count   = 6
    }

    [System.Serializable]
    public struct Neighbours
    {
        public List<int> aboveNeighbours;
        public List<int> belowNeighbours;
        public List<int> frontNeighbours;
        public List<int> rightNeighbours;
        public List<int> backNeighbours;
        public List<int> lefteNeighbours;

    }

    public int aboveSocket;
    public int belowSocket;
    public int frontSocket;
    public int rightSocket;
    public int backSocket;
    public int leftSocket;

    public Neighbours validNeighbours;

    public bool CheckValidSocketConnection(SocketData otherSocket, Sockets otherSocketToCheck)
    {
        switch (otherSocketToCheck)
        {
            case Sockets.Above:
                return otherSocket.validNeighbours.aboveNeighbours.Contains(belowSocket) && validNeighbours.belowNeighbours.Contains(otherSocket.aboveSocket);
            case Sockets.Below:
                return otherSocket.validNeighbours.belowNeighbours.Contains(aboveSocket) && validNeighbours.aboveNeighbours.Contains(otherSocket.belowSocket);
            case Sockets.Front:
                break;
            case Sockets.Right:
                break;
            case Sockets.Back:
                break;
            case Sockets.Left:
                break;
            case Sockets.Count:
                break;
        }

        return false;
    }
}
