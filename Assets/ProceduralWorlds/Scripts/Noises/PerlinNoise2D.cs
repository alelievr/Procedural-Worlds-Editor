using UnityEngine;
using ProceduralWorlds.Core;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace ProceduralWorlds.Noises
{
	public class PerlinNoise2D : Noise
    {
		public int octaves;

        Vector2[] octaveOffsets;

		static int[] p = {151,160,137,91,90,15,
           131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
           190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
           88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
           77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
           102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
           135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
           5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
           223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
           129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
           251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
           49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
           138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
           151,160,137,91,90,15,
           131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
           190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
           88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
           77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
           102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
           135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
           5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
           223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
           129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
           251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
           49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
           138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
       };

		#if NET_4_6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#endif
        static float PerlinValue(float x, float y, int seed = 0)
        {
            x += 12.254f * seed;
            y += 31.964f * seed;
            int X = (int)((x < 0) ? x - 1 : x) & 255,
                Y = (int)((y < 0) ? y - 1 : y) & 255;
            x -= ((x < 0) ? (int)x - 1 : (int)x);
            y -= ((y < 0) ? (int)y - 1 : (int)y);
            float u = Fade(x),
                  v = Fade(y);
            int A = p[X] + Y, AA = p[A], AB = p[A + 1],
                B = p[X + 1] + Y, BA = p[B], BB = p[B + 1];

            return Lerp(v, Lerp(u, Grad(p[AA + 1], x, y),
                       Grad(p[BA + 1], x - 1, y)),
                   Lerp(u, Grad(p[AB + 1], x, y - 1),
                       Grad(p[BB + 1], x - 1, y - 1)));
        }

		#if NET_4_6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#endif
        public static float GenerateNoise(float x,
                float y,
                int octaves = 2,
                float frequency = 1,
                float lacunarity = 1,
                float persistence = 1,
                int seed = -1)
        {
            // Debug.Log("generating perlin at: " + x + "/" + y);
            float ret = 0;
            x *= frequency * noiseScale;
            y *= frequency * noiseScale;

            for (int i = 0; i < octaves; i++)
            {
                float val = PerlinValue(x * frequency, y * frequency, seed);
                ret += val * persistence;
                x *= lacunarity;
                y *= lacunarity;
                persistence *= persistence;
                frequency *= lacunarity;
            }
            return (ret + 1f) / 2f;
        }
		#if NET_4_6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#endif
        static float Fade(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }

		#if NET_4_6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#endif
        static float Lerp(float t, float a, float b) { return a + t * (b - a); }

		#if NET_4_6
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		#endif
        static float Grad(int hash, float x, float y)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y,
                  v = h < 4 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public PerlinNoise2D(int seed, float scale = 1, int octaves = 2, float persistence = 1, float lacunarity = 1)
        {
            UpdateParams(seed, scale, octaves, persistence, lacunarity);
        }

        public void UpdateParams(int seed, float scale, int octaves, float persistence, float lacunarity)
        {
            this.seed = seed;
            this.scale = scale;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
        }
    
		public override void ComputeSampler2D(Sampler2D samp, Vector3 position)
		{
			if (samp == null)
				Debug.LogError("null sampler send to Noise ComputeSampler !");
			
			if (false)//(hasGraphicAcceleration)
			{
				//compute shader here
			}
			else
			{
                samp.Foreach((x, y) => {
                    return GenerateNoise(position.x + x, position.z + y, octaves, samp.step * scale, lacunarity, persistence, seed);
                });
			}
		}

		public override float GetValue(Vector3 position)
		{
            return GenerateNoise(position.x, position.y, octaves, scale, 1, 1, seed);
		}
	}
}