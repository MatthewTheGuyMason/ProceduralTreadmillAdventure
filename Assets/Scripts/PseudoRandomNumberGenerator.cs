using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PseudoRandomNumberGenerator
{
    public static ulong seed;

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
}
