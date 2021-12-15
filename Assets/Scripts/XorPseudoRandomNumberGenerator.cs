//====================================================================================================================================================================================================================================================================================================================================================
//  Name:               XorPseudoRandomNumberGenerator.cs
//  Author:             Matthew Mason
//  Date Created:       15/12/2021
//  Date Last Modified  15/12/2021
//  Brief:              Static class for generating random numbers using the XorShift
//====================================================================================================================================================================================================================================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class for generating random numbers using the XorShift
/// </summary>
public static class XorPseudoRandomNumberGenerator
{
    #region Public Variables
    /// <summary>
    /// The current seed value used in the generator
    /// </summary>
    public static ulong seed;
    #endregion

    #region Public Methods
    public static uint XorShiftStarInt()
    {
        /* Algorithm "xor" from p. 4 of Marsaglia, "Xorshift RNGs" */
        ulong x = seed;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        seed = x;
        return (uint)((x * 0x2545F4914F6CDD1DUL) >> 32);
    }

    public static ulong XorShiftStarUlong()
    {
        /* Algorithm "xor" from p. 4 of Marsaglia, "Xorshift RNGs" */
        ulong x = seed;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        seed = x;
        return x * 0x2545F4914F6CDD1DUL;
    }
    #endregion
}
