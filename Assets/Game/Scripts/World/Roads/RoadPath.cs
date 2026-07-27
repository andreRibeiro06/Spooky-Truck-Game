using UnityEngine;

public static class RoadPath
{
    // Adjust these values in code or expose them if preferred
    public static float pathFrequency = 0.01f;  // Controls how sharp/frequent curves are
    public static float pathAmplitude = 200f;     // Controls how far left/right the road swings
    public static float seed = 123.45f;         // Change seed to generate a completely new world layout

    /// <summary>
    /// Calculates a procedurally generated X coordinate using 1D Perlin Noise.
    /// </summary>
    public static float GetRoadX(float worldZ)
    {
        // Sample Perlin noise along the Z axis (Offset by seed for randomness)
        float noiseSample = Mathf.PerlinNoise(seed, worldZ * pathFrequency);

        // Mathf.PerlinNoise returns a value between 0.0 and 1.0.
        // Subtracting 0.5 centers it between -0.5 and +0.5, allowing the road to curve left and right.
        float centeredNoise = noiseSample - 0.5f;

        return centeredNoise * pathAmplitude;
    }

    /// <summary>
    /// Calculates a smooth, low-frequency elevation profile along the procedural road.
    /// </summary>
    public static float GetSmoothRoadElevation(float worldZ, float baseScale, float heightMultiplier)
    {
        float roadX = GetRoadX(worldZ);

        // Zoom out scale so elevation transitions happen smoothly
        float smoothScale = baseScale * 3f;

        float sampleX = (roadX / smoothScale) + seed;
        float sampleZ = (worldZ / smoothScale) + seed;

        float perlin = Mathf.PerlinNoise(sampleX, sampleZ);

        return perlin * heightMultiplier * 0.6f;
    }

    /// <summary>
    /// Gets forward tangent vector along the procedural curve.
    /// </summary>
    public static Vector3 GetRoadTangent(float worldZ)
    {
        float delta = 0.1f;
        float x1 = GetRoadX(worldZ - delta);
        float x2 = GetRoadX(worldZ + delta);

        Vector3 p1 = new Vector3(x1, 0, worldZ - delta);
        Vector3 p2 = new Vector3(x2, 0, worldZ + delta);

        return (p2 - p1).normalized;
    }
}