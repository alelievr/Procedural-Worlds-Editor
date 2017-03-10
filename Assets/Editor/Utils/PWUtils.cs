using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public static class PWUtils {
	
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
	}

	[System.SerializableAttribute]
	public struct SerializableColor
	{
		Vector4		color;

		public SerializableColor(Color c)
		{
			color.x = c.r;
			color.y = c.g;
			color.z = c.b;
			color.w = c.a;
		}

		public SerializableColor(Color32 c)
		{
			color.x = c.r / 255f;
			color.y = c.g / 255f;
			color.z = c.b / 255f;
			color.w = c.a / 255f;
		}

		public Color GetColor()
		{
			return new Color(color.x, color.y, color.z, color.w);
		}

		public static explicit operator SerializableColor(Color c)
		{
			return new SerializableColor(c);
		}

		public static implicit operator Color(SerializableColor c)
		{
			return c.GetColor();
		}
	}
}