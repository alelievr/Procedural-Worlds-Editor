using UnityEngine;

namespace PW
{
	public static class PWUtils {
	
		static Texture2D debugTexture;

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
	}
}