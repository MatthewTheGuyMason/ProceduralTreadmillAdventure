using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SocketData", menuName = "ScriptableObject/SocketData")]
public class SocketData : ScriptableObject
{
    // TODO: Make a custom inspector for this


    private void Awake()
    {
        OnValidate();
    }

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
        //public List<int> aboveNeighbours;
        //public List<int> belowNeighbours;
        //public List<int> frontNeighbours;
        //public List<int> rightNeighbours;
        //public List<int> backNeighbours;
        //public List<int> lefteNeighbours;

        public List<int>[] sideNeighbours;

        public List<int> AboveNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Above];
            }
            set
            {
                sideNeighbours[(int)Sockets.Above] = value;
            }
        }
        public List<int> BelowNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Below];
            }
            set
            {
                sideNeighbours[(int)Sockets.Below] = value;
            }
        }
        public List<int> FrontNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Front];
            }
            set
            {
                sideNeighbours[(int)Sockets.Front] = value;
            }
        }
        public List<int> RightNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Right];
            }
            set
            {
                sideNeighbours[(int)Sockets.Right] = value;
            }
        }
        public List<int> BackNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Back];
            }
            set
            {
                sideNeighbours[(int)Sockets.Back] = value;
            }
        }
        public List<int> LeftNeighbours
        {
            get
            {
                return sideNeighbours[(int)Sockets.Left];
            }
            set
            {
                sideNeighbours[(int)Sockets.Left] = value;
            }
        }
    }

    public int[] sideSocketIds;

    public int AboveSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Above];
        }
        set
        {
            sideSocketIds[(int)Sockets.Above] = value;
        }
    }
    public int BelowSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Below];
        }
        set
        {
            sideSocketIds[(int)Sockets.Below] = value;
        }
    }
    public int FrontSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Front];
        }
        set
        {
            sideSocketIds[(int)Sockets.Front] = value;
        }
    }
    public int RightSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Right];
        }
        set
        {
            sideSocketIds[(int)Sockets.Right] = value;
        }
    }
    public int BackSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Back];
        }
        set
        {
            sideSocketIds[(int)Sockets.Back] = value;
        }
    }
    public int LeftSocket
    {
        get
        {
            return sideSocketIds[(int)Sockets.Left];
        }
        set
        {
            sideSocketIds[(int)Sockets.Left] = value;
        }
    }

    public Neighbours validNeighbours;

    public bool CheckValidSocketConnection(SocketData otherSocket, Sockets otherSocketToCheck)
    {
        switch (otherSocketToCheck)
        {
            case Sockets.Above:
                return otherSocket.validNeighbours.AboveNeighbours.Contains(BelowSocket) && validNeighbours.BelowNeighbours.Contains(otherSocket.AboveSocket);
            case Sockets.Below:
                return otherSocket.validNeighbours.BelowNeighbours.Contains(AboveSocket) && validNeighbours.AboveNeighbours.Contains(otherSocket.BelowSocket);
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

    public void CopyData(SocketData socketDataToCopy)
    {
        validNeighbours.AboveNeighbours = new List<int>(socketDataToCopy.validNeighbours.AboveNeighbours);
        validNeighbours.BelowNeighbours = new List<int>(socketDataToCopy.validNeighbours.BelowNeighbours);
        validNeighbours.FrontNeighbours = new List<int>(socketDataToCopy.validNeighbours.FrontNeighbours);
        validNeighbours.RightNeighbours = new List<int>(socketDataToCopy.validNeighbours.RightNeighbours);
        validNeighbours.BackNeighbours  = new List<int>(socketDataToCopy.validNeighbours.BackNeighbours);
        validNeighbours.LeftNeighbours = new List<int>(socketDataToCopy.validNeighbours.LeftNeighbours);

        AboveSocket = socketDataToCopy.AboveSocket;
        BelowSocket = socketDataToCopy.BelowSocket;
        FrontSocket = socketDataToCopy.FrontSocket;
        RightSocket = socketDataToCopy.RightSocket;
        BackSocket = socketDataToCopy.BackSocket;
        LeftSocket = socketDataToCopy.LeftSocket;
    }

    private void OnValidate()
    {
        if (validNeighbours.sideNeighbours == null)
        {
            validNeighbours.sideNeighbours = new List<int>[(int)Sockets.Count];

        }
        for (int i = 0; i < (int)Sockets.Count; ++i)
        {

            if (validNeighbours.sideNeighbours[i] == null)
            {
                validNeighbours.sideNeighbours[i] = new List<int>();
            }
        }

        if (sideSocketIds == null)
        {
            sideSocketIds = new int[(int)Sockets.Count];
        }
    }
}
