using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public struct SerializableColor
	{
		[SerializeField]
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