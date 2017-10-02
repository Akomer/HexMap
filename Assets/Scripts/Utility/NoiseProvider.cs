using UnityEngine;

public class NoiseProvider
{

    private static Texture2D noiseSource;
    private const float noiseScale = 0.003f;

    public static Texture2D NoiseTexture
    {
        get { return noiseSource; }
    }

    public static void Init(Texture2D tex)
    {
        noiseSource = tex;
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(
            position.x * noiseScale, 
            position.z * noiseScale);
    }

}
