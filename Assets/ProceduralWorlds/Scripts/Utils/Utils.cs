using UnityEngine;
using System.Collections.Generic;
using ProceduralWorlds.Core;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ProceduralWorlds
{
	public static class Utils
	{
	
		static Texture2D	debugTexture;
		static int			localMaxLog = 0;
		static int			localMaxLogWarning = 0;
		static int			localMaxLogError = 0;

		public static Rect DecalRect(Rect r, Vector2 decal, bool newRect = false)
		{
			if (newRect)
				r = new Rect(r);
			r.x += decal.x;
			r.y += decal.y;
			return r;
		}

		public static Vector2 Round(Vector2 v)
		{
			v.x = Mathf.Round(v.x);
			v.y = Mathf.Round(v.y);
			return v;
		}
		
		public static Vector3 Round(Vector3 v)
		{
			v.x = Mathf.Round(v.x);
			v.y = Mathf.Round(v.y);
			v.z = Mathf.Round(v.z);
			return v;
		}
		
		public static Vector4 Round(Vector4 v)
		{
			v.x = Mathf.Round(v.x);
			v.y = Mathf.Round(v.y);
			v.z = Mathf.Round(v.z);
			v.w = Mathf.Round(v.w);
			return v;
		}

		public static void Swap< T >(ref T v1, ref T v2)
		{
			T tmp = v1;
			v1 = v2;
			v2 = tmp;
		}

		public static Rect CreateRect(Vector2 P1, Vector2 P2)
        {
            Vector2 D = P1 - P2;
			Rect R = new Rect();

            if (D.x < 0)
                R.x = P1.x;
            else
                R.x = P2.x;
            if (D.y < 0)
                R.y = P1.y;
            else
                R.y = P2.y;
            R.width = Mathf.Abs(D.x);
            R.height = Mathf.Abs(D.y);
            return R;
        }

		public static void DrawDebugTexture(Rect rect, Color c)
		{
			if (debugTexture == null)
				debugTexture = new Texture2D(1, 1);
			debugTexture.SetPixel(0, 0, c);
			debugTexture.Apply();
			GUI.DrawTexture(rect, debugTexture);
		}

		public static Gradient CreateRainbowGradient()
		{
			return CreateGradient(
				new KeyValuePair< float, Color >(0.00f, Color.red),
				new KeyValuePair< float, Color >(0.24f, Color.yellow),
				new KeyValuePair< float, Color >(0.50f, Color.green),
				new KeyValuePair< float, Color >(0.77f, Color.cyan),
				new KeyValuePair< float, Color >(1.00f, Color.blue)
			);
		}
		
		public static Gradient CreateGradient(params KeyValuePair< float, Color>[] datas)
		{
			return CreateGradient(GradientMode.Blend, datas);
		}

		public static Gradient CreateGradient(GradientMode mode, params KeyValuePair< float, Color>[] datas)
		{
			Gradient			grad = new Gradient();
			GradientColorKey[]	colorKeys = new GradientColorKey[datas.Length];
			GradientAlphaKey[]	alphaKeys = new GradientAlphaKey[datas.Length];

			for (int i = 0; i < datas.Length; i++)
			{
				colorKeys[i].time = datas[i].Key;
				colorKeys[i].color = datas[i].Value;
				alphaKeys[i].time = datas[i].Key;
				alphaKeys[i].alpha = datas[i].Value.a;
			}
			grad.SetKeys(colorKeys, alphaKeys);
			grad.mode = mode;

			return grad;
		}
		
		public static bool Compare(this Gradient gradient, Gradient otherGradient)
		{
			if (gradient.alphaKeys.Length != otherGradient.alphaKeys.Length ||
				gradient.colorKeys.Length != otherGradient.colorKeys.Length)
				return false;
			
			for (int i = 0; i < gradient.colorKeys.Length; i++)
				if (gradient.colorKeys[i].color != otherGradient.colorKeys[i].color || gradient.colorKeys[i].time != otherGradient.colorKeys[i].time)
					return false;
					
			for (int i = 0; i < gradient.alphaKeys.Length; i++)
				if (gradient.alphaKeys[i].alpha != otherGradient.alphaKeys[i].alpha || gradient.alphaKeys[i].time != otherGradient.alphaKeys[i].time)
					return false;
			
			return true;
		}
		
		public static bool Compare(this Color c1, Color c2)
		{
			for (int i = 0; i < 4; i++)
				if (c1[i] != c2[i])
					return false;
			return true;
		}

		public static void LogMax(object p, int maxLog)
		{
			if (localMaxLog >= maxLog)
				return ;
			Debug.Log(p);
			localMaxLog++;
		}
		
		public static void LogWarningMax(object p, int maxLog)
		{
			if (localMaxLogWarning >= maxLog)
				return ;
			Debug.LogWarning(p);
			localMaxLogWarning++;
		}
		
		public static void LogErrorMax(object p, int maxLog)
		{
			if (localMaxLogError >= maxLog)
				return ;
			Debug.LogError(p);
			localMaxLogError++;
		}

		public static Texture2D CreateTexture2DColor(Color c)
		{
			Texture2D	ret;

			ret = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
			ret.wrapMode = TextureWrapMode.Repeat;
			ret.SetPixel(0, 0, c);
			ret.Apply();

			return ret;
		}

		public static bool Overlap(float x1, float x2, float y1, float y2)
		{
			return (x1 <= y2 && y1 <= x2);
		}

		public static float GapWidth(float x1, float x2, float y1, float y2)
		{
			float	ret = 0;

			if (y1 < x1)
				ret = (x1 - y2);
			else
				ret = (y1 - x2);
			
			return ret;
		}

		public static float PositiveMod(float x, float mod)
		{
			float m = x % mod;
			return (m < 0) ? m + mod : m;
		}
		
		public static int PositiveMod(int x, int mod)
		{
			int m = x % mod;
			return (m < 0) ? m + mod : m;
		}
    }
}