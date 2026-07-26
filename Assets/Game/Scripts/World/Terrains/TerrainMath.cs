using UnityEngine;

public static class TerrainMath
{
    public static float GetHeight(float x, float z, float scale, float heightMultiplier, float xOffset, float zOffset)
    {
        float xCoord = (x * scale) + xOffset;
        float zCoord = (z * scale) + zOffset;
        return Mathf.PerlinNoise(xCoord, zCoord) * heightMultiplier;
    }
}