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

    public enum Sides
    {
        Above   = 0,
        Below   = 1,

        Front   = 2,
        Right   = 3,
        Back    = 4,
        Left    = 5,

        Count   = 6,
        Undecided = 7
    }

    [System.Serializable]
    public class Neighbours
    {
        [SerializeField]
        private List<int> aboveNeighbours;
        [SerializeField]
        private List<int> belowNeighbours;
        [SerializeField]
        private List<int> frontNeighbours;
        [SerializeField]
        private List<int> rightNeighbours;
        [SerializeField]
        private List<int> backNeighbours;
        [SerializeField]
        private List<int> leftNeighbours;

        [SerializeField]
        private List<int>[] allNeighbours; 

        public List<int> AboveNeighbours
        {
            get
            {
                return aboveNeighbours;
            }
            set
            {
                aboveNeighbours = value;
                allNeighbours[(int)Sides.Above] = aboveNeighbours;
            }
        }
        public List<int> BelowNeighbours
        {
            get
            {
                return belowNeighbours;
            }
            set
            {
                belowNeighbours = value;
                allNeighbours[(int)Sides.Below] = belowNeighbours;
            }
        }
        public List<int> FrontNeighbours
        {
            get
            {
                return frontNeighbours;
            }
            set
            {
                frontNeighbours = value;
                allNeighbours[(int)Sides.Front] = frontNeighbours;
            }
        }
        public List<int> RightNeighbours
        {
            get
            {
                return rightNeighbours;
            }
            set
            {
                rightNeighbours = value;
                allNeighbours[(int)Sides.Right] = rightNeighbours;
            }
        }
        public List<int> BackNeighbours
        {
            get
            {
                return backNeighbours;
            }
            set
            {
                backNeighbours = value;
                allNeighbours[(int)Sides.Back] = backNeighbours;
            }
        }
        public List<int> LeftNeighbours
        {
            get
            {
                return leftNeighbours;
            }
            set
            {
                leftNeighbours = value;
                allNeighbours[(int)Sides.Left] = leftNeighbours;
            }
        }

        public Neighbours()
        {
            aboveNeighbours = new List<int>();
            belowNeighbours = new List<int>();
            frontNeighbours = new List<int>();
            rightNeighbours = new List<int>();
            backNeighbours  = new List<int>();
            leftNeighbours  = new List<int>();

            allNeighbours = new List<int>[(int)Sides.Count];
            allNeighbours[(int)Sides.Above] = aboveNeighbours;
            allNeighbours[(int)Sides.Below] = belowNeighbours;
            allNeighbours[(int)Sides.Front] = frontNeighbours;
            allNeighbours[(int)Sides.Right] = rightNeighbours;
            allNeighbours[(int)Sides.Back] = backNeighbours;
            allNeighbours[(int)Sides.Left] = leftNeighbours;
        }




        public List<int> GetValidNeighbourListForSide(Sides side)
        {
            return allNeighbours[(int)side];
        }

        public void RotateAroundY(int numberOf90DegreeClockwiseTurns)
        {
            int[][] allOldNeighbours = new int[4][];
            for (int i = 0; i < allOldNeighbours.Length; ++i)
            {
                allOldNeighbours[i] = new int[allNeighbours[i + (int)Sides.Front].Count];
                for (int j = 0; j < allOldNeighbours[i].Length; ++j)
                {
                    allOldNeighbours[i][j] = allNeighbours[i + (int)Sides.Front][j];
                }
            }

            for (int i = 0; i < 4; ++i)
            {
                int rotatedFaceIndex = i + numberOf90DegreeClockwiseTurns;
                while (rotatedFaceIndex >= allOldNeighbours.Length)
                {
                    rotatedFaceIndex -= allOldNeighbours.Length;
                }

                allNeighbours[i + (int)Sides.Front].Clear();
                allNeighbours[i + (int)Sides.Front].AddRange(allOldNeighbours[rotatedFaceIndex]);
            }
        }
    }

    public static Sides RotateSidesDirection(Sides startingSideDirection, int numberOf90Turns)
    {
        if (startingSideDirection >= Sides.Count || startingSideDirection < Sides.Front)
        {
            return startingSideDirection;
        }
        else
        {
            numberOf90Turns %= 4;
            startingSideDirection += numberOf90Turns;
            if (startingSideDirection > Sides.Left)
            {
                startingSideDirection -= 4;
            }
            return startingSideDirection;
        }
    }

    /// <summary>
    /// Returns a socket on the opposite side to the opposing socket
    /// </summary>
    /// <param name="sideType">The side to get the side at the other side of</param>
    /// <returns>a socket on the opposite side to the opposing socket</returns>
    public static Sides GetOpposingSocket(Sides sideType)
    {
        switch (sideType)
        {
            case Sides.Above:
                return Sides.Below;
            case Sides.Below:
                return Sides.Above;
            case Sides.Front:
                return Sides.Back;
            case Sides.Right:
                return Sides.Left;
            case Sides.Back:
                return Sides.Front;
            case Sides.Left:
                return Sides.Right;
        }
        return Sides.Undecided;
    }

    public int[] sideSocketIds;

    public int AboveSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Above];
        }
        set
        {
            sideSocketIds[(int)Sides.Above] = value;
        }
    }
    public int BelowSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Below];
        }
        set
        {
            sideSocketIds[(int)Sides.Below] = value;
        }
    }
    public int FrontSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Front];
        }
        set
        {
            sideSocketIds[(int)Sides.Front] = value;
        }
    }
    public int RightSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Right];
        }
        set
        {
            sideSocketIds[(int)Sides.Right] = value;
        }
    }
    public int BackSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Back];
        }
        set
        {
            sideSocketIds[(int)Sides.Back] = value;
        }
    }
    public int LeftSocket
    {
        get
        {
            return sideSocketIds[(int)Sides.Left];
        }
        set
        {
            sideSocketIds[(int)Sides.Left] = value;
        }
    }

    public Neighbours validNeighbours;

    public int GetIdOfSide(Sides side)
    {
        return sideSocketIds[(int)side];
    }

    public bool CheckValidSocketConnection(SocketData otherSocket, Sides otherSocketToCheck)
    {
        return otherSocket.validNeighbours.GetValidNeighbourListForSide(otherSocketToCheck).Contains(GetIdOfSide(GetOpposingSocket(otherSocketToCheck)));
    }

    public void CopyData(SocketData socketDataToCopy)
    {
        validNeighbours.AboveNeighbours = new List<int>(socketDataToCopy.validNeighbours.AboveNeighbours);
        validNeighbours.BelowNeighbours = new List<int>(socketDataToCopy.validNeighbours.BelowNeighbours);
        validNeighbours.FrontNeighbours = new List<int>(socketDataToCopy.validNeighbours.FrontNeighbours);
        validNeighbours.RightNeighbours = new List<int>(socketDataToCopy.validNeighbours.RightNeighbours);
        validNeighbours.BackNeighbours  = new List<int>(socketDataToCopy.validNeighbours.BackNeighbours);
        validNeighbours.LeftNeighbours = new List<int>(socketDataToCopy.validNeighbours.LeftNeighbours);

        CopySocketIDs(socketDataToCopy);
    }

    public void CopySocketIDs(SocketData socketDataToCopy)
    {
        AboveSocket = socketDataToCopy.AboveSocket;
        BelowSocket = socketDataToCopy.BelowSocket;
        FrontSocket = socketDataToCopy.FrontSocket;
        RightSocket = socketDataToCopy.RightSocket;
        BackSocket = socketDataToCopy.BackSocket;
        LeftSocket = socketDataToCopy.LeftSocket;
    }

    public static Vector3Int GetCooridnateOffSetForSide(Sides socketsSide)
    {
        switch (socketsSide)
        {
            case Sides.Above:
                return Vector3Int.up;
            case Sides.Below:
                return Vector3Int.down;
            case Sides.Front:
                return Vector3Int.forward;
            case Sides.Right:
                return Vector3Int.right;
            case Sides.Back:
                return Vector3Int.back;
            case Sides.Left:
                return Vector3Int.left;
        }
        return Vector3Int.zero;
    }

    public static Sides GetSideFromCooridnateOff(Vector3Int CooridnateOff)
    {
        if (CooridnateOff == Vector3Int.up)
        {
            return Sides.Above;
        }
        else if (CooridnateOff == Vector3Int.down)
        {
            return Sides.Below;
        }
        else if (CooridnateOff == Vector3Int.forward)
        {
            return Sides.Front;
        }
        else if (CooridnateOff == Vector3Int.right)
        {
            return Sides.Right;
        }
        else if (CooridnateOff == Vector3Int.back)
        {
            return Sides.Back;
        }
        else if (CooridnateOff == Vector3Int.left)
        {
            return Sides.Left;
        }

        return Sides.Undecided;
    }

    public void RotateAroundY(int numberOf90DegreeClockwiseTurns)
    {
        int[] allOldSideID = new int[4];

        for (int i = 0; i < allOldSideID.Length; ++i)
        {
            allOldSideID[i] = sideSocketIds[i + (int)Sides.Front];
        }

        for (int i = 0; i < 4; ++i)
        {
            int rotatedFaceIndex = i + numberOf90DegreeClockwiseTurns;
            while (rotatedFaceIndex >= allOldSideID.Length)
            {
                rotatedFaceIndex -= allOldSideID.Length;
            }

            Debug.Log(((Sides)(i + (int)Sides.Front)).ToString() + " To " + ((Sides)(rotatedFaceIndex + 2)).ToString());

            sideSocketIds[i + (int)Sides.Front] = allOldSideID[rotatedFaceIndex];
        }
        validNeighbours.RotateAroundY(numberOf90DegreeClockwiseTurns);

    }


    private void OnValidate()
    {
        if (validNeighbours == null)
        {
            validNeighbours = new Neighbours();
        }
        if (sideSocketIds == null)
        {
            sideSocketIds = new int[(int)Sides.Count];
        }
    }

    //private bool RotationHasEffect()
    //{
    //    for (int i = (int)Sides.Front; i <= (int)Sides.Left; ++i)
    //    {
    //        if (sideSocketIds[i] != sideSocketIds[(int)Sides.Front])
    //        {

    //        }
    //    }
    //}
}
