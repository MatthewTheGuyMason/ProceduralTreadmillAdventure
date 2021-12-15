//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               SocketData.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              Scriptable object used to store the sockets ID and the valid ID that can connect to them
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object used to store the sockets ID and the valid ID that can connect to them
/// </summary>
[CreateAssetMenu(fileName = "SocketData", menuName = "ScriptableObject/SocketData")]
public class SocketData : ScriptableObject
{
    #region Public Enumerations
    public enum Sides
    {
        Above = 0,
        Below = 1,

        Front = 2,
        Right = 3,
        Back = 4,
        Left = 5,

        Count = 6,
        Undecided = 7
    }
    #endregion

    #region Public Structures 
    /// <summary>
    /// Stores a list of IDs for each side of a tile
    /// </summary>
    [System.Serializable]
    public class Neighbours
    {
        #region Private Serialized Fields
        [SerializeField] 
        [Tooltip("The neighbor ids for the above side")]
        private List<int> aboveNeighbours;
        [SerializeField]
        [Tooltip("The neighbor ids for the below side")]
        private List<int> belowNeighbours;
        [SerializeField]
        [Tooltip("The neighbor ids for the front side")]
        private List<int> frontNeighbours;
        [SerializeField]
        [Tooltip("The neighbor ids for the right side")]
        private List<int> rightNeighbours;
        [SerializeField]
        [Tooltip("The neighbor ids for the back side")]
        private List<int> backNeighbours;
        [SerializeField]
        [Tooltip("The neighbor ids for the left side")]
        private List<int> leftNeighbours;

        [SerializeField] [Tooltip("Array of all the list of side neighbors")]
        private List<int>[] allNeighbours;
        #endregion

        #region Public Properties
        /// <summary>
        /// Property for accessing the neighbor ids for the above side  
        /// </summary>
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
        /// <summary>
        /// Property for accessing the neighbor ids for the below side  
        /// </summary>
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
        /// <summary>
        /// Property for accessing the neighbor ids for the front side  
        /// </summary>
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
        /// <summary>
        /// Property for accessing the neighbor ids for the right side  
        /// </summary>
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
        /// <summary>
        /// Property for accessing the neighbor ids for the back side  
        /// </summary>
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
        /// <summary>
        /// Property for accessing the neighbor ids for the left side  
        /// </summary>
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor
        /// </summary>
        public Neighbours()
        {
            aboveNeighbours = new List<int>();
            belowNeighbours = new List<int>();
            frontNeighbours = new List<int>();
            rightNeighbours = new List<int>();
            backNeighbours = new List<int>();
            leftNeighbours = new List<int>();

            allNeighbours = new List<int>[(int)Sides.Count];
            allNeighbours[(int)Sides.Above] = aboveNeighbours;
            allNeighbours[(int)Sides.Below] = belowNeighbours;
            allNeighbours[(int)Sides.Front] = frontNeighbours;
            allNeighbours[(int)Sides.Right] = rightNeighbours;
            allNeighbours[(int)Sides.Back] = backNeighbours;
            allNeighbours[(int)Sides.Left] = leftNeighbours;
        }

        /// <summary>
        /// Returns the valid neighbor list for a given side
        /// </summary>
        /// <param name="side">The side to get the valid neighbors of</param>
        /// <returns>The valid neighbor list for a given side</returns>
        public List<int> GetValidNeighbourListForSide(Sides side)
        {
            return allNeighbours[(int)side];
        }

        /// <summary>
        /// Rotates the neighbor valid list values around imaginary Y axis (so only front, right, back and left rotate) by 90 degrees a give number of times
        /// </summary>
        /// <param name="numberOf90DegreeClockwiseTurns">Number of times to rotate around an imaginary y axis by 90 degrees</param>
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
        #endregion
    }
    #endregion

    #region Public Serialized Fields
    [SerializeField] [Tooltip("The IDs of the sockets of all sides")]
    private int[] sideSocketIds;

    [SerializeField] [Tooltip("The valid neighbors for the sockets")]
    private Neighbours validNeighbours;
    #endregion

    #region Public Properties
    /// <summary>
    /// Property for getting the ID of the above socket
    /// </summary>
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
    /// <summary>
    /// Property for getting the ID of the below socket
    /// </summary>
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
    /// <summary>
    /// Property for getting the ID of the front socket
    /// </summary>
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
    /// <summary>
    /// Property for getting the ID of the right socket
    /// </summary>
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
    /// <summary>
    /// Property for getting the ID of the back socket
    /// </summary>
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
    /// <summary>
    /// Property for getting the ID of the left socket
    /// </summary>
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

    /// <summary>
    /// Property accessors for the valid neighbors for the sockets
    /// </summary>
    public Neighbours ValidNeighbours
    {
        get
        {
            return validNeighbours;
        }
        set
        {
            validNeighbours = value;
        }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        OnValidate();
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
    #endregion

    #region Public Static Methods
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
    /// <summary>
    /// Returns a side based on a coordinate offset
    /// </summary>
    /// <param name="CooridnateOff">The offset away from </param>
    /// <returns></returns>
    public static Sides GetSideFromCooridnateOff(Vector3Int CooridnateOff)
    {
        CooridnateOff = Vector3Int.Min(CooridnateOff, Vector3Int.one);
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
    /// <summary>
    /// Returns a side that has been rotated around an imaginary y axis 90 degrees a given number of times
    /// </summary>
    /// <param name="startingSideDirection">The direction of the side before rotating</param>
    /// <param name="numberOf90Turns">The number of time rotate 90 degrees around an imaginary y axis</param>
    /// <returns>A side that has been rotated around an imaginary y axis 90 degrees a given number of times</returns>
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
    /// Returns an offset based on a given side
    /// </summary>
    /// <param name="socketsSide">The side to return an offset based off</param>
    /// <returns>An offset based on a given side</returns>
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
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks if a side of another socket is valid to connect to this socket data
    /// </summary>
    /// <param name="otherSocket">The other socket data to check against</param>
    /// <param name="otherSocketToCheck">The side of the other socket to check against</param>
    /// <returns>A side of another socket is valid to connect to this socket data</returns>
    public bool CheckValidSocketConnection(SocketData otherSocket, Sides otherSocketToCheck)
    {
        return otherSocket.validNeighbours.GetValidNeighbourListForSide(otherSocketToCheck).Contains(GetIdOfSide(GetOpposingSocket(otherSocketToCheck)));
    }

    /// <summary>
    /// Return the ID of a given side
    /// </summary>
    /// <param name="side">The side to get the ID of</param>
    /// <returns>The ID of a given side</returns>
    public int GetIdOfSide(Sides side)
    {
        return sideSocketIds[(int)side];
    }

    /// <summary>
    /// Copies another socket data into this one
    /// </summary>
    /// <param name="socketDataToCopy">The other socket data to copy from</param>
    public void CopyData(SocketData socketDataToCopy)
    {
        validNeighbours.AboveNeighbours = new List<int>(socketDataToCopy.validNeighbours.AboveNeighbours);
        validNeighbours.BelowNeighbours = new List<int>(socketDataToCopy.validNeighbours.BelowNeighbours);
        validNeighbours.FrontNeighbours = new List<int>(socketDataToCopy.validNeighbours.FrontNeighbours);
        validNeighbours.RightNeighbours = new List<int>(socketDataToCopy.validNeighbours.RightNeighbours);
        validNeighbours.BackNeighbours = new List<int>(socketDataToCopy.validNeighbours.BackNeighbours);
        validNeighbours.LeftNeighbours = new List<int>(socketDataToCopy.validNeighbours.LeftNeighbours);

        CopySocketIDs(socketDataToCopy);
    }

    /// <summary>
    /// Copies all the socket ids from another socket data into this one
    /// </summary>
    /// <param name="socketDataToCopy">The other socket data to copy from</param>
    public void CopySocketIDs(SocketData socketDataToCopy)
    {
        AboveSocket = socketDataToCopy.AboveSocket;
        BelowSocket = socketDataToCopy.BelowSocket;
        FrontSocket = socketDataToCopy.FrontSocket;
        RightSocket = socketDataToCopy.RightSocket;
        BackSocket = socketDataToCopy.BackSocket;
        LeftSocket = socketDataToCopy.LeftSocket;
    }
    /// <summary>
    /// Rotates all the valid neighbors and socket ID around an imaginary Y axis by 90 degrees a given number of times
    /// </summary>
    /// <param name="numberOf90DegreeClockwiseTurns">The number of time to rotate around an imaginary Y axis by 90 degrees</param>
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
    #endregion
}
