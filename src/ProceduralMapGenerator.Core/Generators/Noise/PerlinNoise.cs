namespace ProceduralMapGenerator.Core.Generators;

public class PerlinNoise
{
    private readonly int[] _p;

    public PerlinNoise(int seed)
    {
        var rng  = new Random(seed);
        var perm = new int[256];
        for (int i = 0; i < 256; i++) perm[i] = i;

        // Fisher-Yates shuffle
        for (int i = 255; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (perm[i], perm[j]) = (perm[j], perm[i]);
        }

        // Double the table to avoid index wrapping in hot path
        _p = new int[512];
        for (int i = 0; i < 512; i++) _p[i] = perm[i & 255];
    }

    // Returns a value in approximately [-1, 1].
    public float Sample(float x, float y)
    {
        int xi = (int)MathF.Floor(x) & 255;
        int yi = (int)MathF.Floor(y) & 255;

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = _p[_p[xi]     + yi];
        int ab = _p[_p[xi]     + yi + 1];
        int ba = _p[_p[xi + 1] + yi];
        int bb = _p[_p[xi + 1] + yi + 1];

        return Lerp(v,
            Lerp(u, Grad(aa, xf,      yf),
                    Grad(ba, xf - 1f, yf)),
            Lerp(u, Grad(ab, xf,      yf - 1f),
                    Grad(bb, xf - 1f, yf - 1f)));
    }

    // Combines multiple noise layers. Result is normalised to approximately [-1, 1].
    public float OctaveSample(float x, float y, int octaves, float persistence, float lacunarity)
    {
        float total     = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue  = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total    += Sample(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    // Smoothstep: 6t^5 - 15t^4 + 10t^3
    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    private static float Lerp(float t, float a, float b) => a + t * (b - a);

    // 4-direction 2D gradient
    private static float Grad(int hash, float x, float y) => (hash & 3) switch
    {
        0 =>  x + y,
        1 => -x + y,
        2 =>  x - y,
        _ => -x - y
    };
}
