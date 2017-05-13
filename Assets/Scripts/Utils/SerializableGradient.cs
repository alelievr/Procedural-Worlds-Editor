using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PW
{
	[System.SerializableAttribute]
	public class SerializableGradient {
	
		[SerializeField]
		public SerializableGradientAlphaKey[]	alphaKeys;
		[SerializeField]
		public SerializableGradientColorKey[]	colorKeys;
		[SerializeField]
		public GradientMode						mode;

		public SerializableGradient()
		{
			colorKeys = new SerializableGradientColorKey[0];
			alphaKeys = new SerializableGradientAlphaKey[0];
		}

		public SerializableGradient(Gradient g)
		{
			colorKeys = new SerializableGradientColorKey[g.colorKeys.Length];
			for (int i = 0; i < g.colorKeys.Length; i++)
				colorKeys[i] = (SerializableGradientColorKey)g.colorKeys[i];

			alphaKeys = new SerializableGradientAlphaKey[g.alphaKeys.Length];
			for (int i = 0; i < g.alphaKeys.Length; i++)
				alphaKeys[i] = (SerializableGradientAlphaKey)g.alphaKeys[i];
			mode = g.mode;
		}

		public static explicit operator SerializableGradient(Gradient g)
		{
			return new SerializableGradient(g);
		}
	
		public static implicit operator Gradient(SerializableGradient sg)
		{
			Gradient g = new Gradient();
			
			if (sg != null)
			{
				GradientColorKey[] gck = null;
				GradientAlphaKey[] gak = null;
				gck = new GradientColorKey[sg.colorKeys.Length];
				for (int i = 0; i < sg.colorKeys.Length; i++)
					gck[i] = sg.colorKeys[i];
				gak = new GradientAlphaKey[sg.alphaKeys.Length];
				for (int i = 0; i < sg.alphaKeys.Length; i++)
					gak[i] = sg.alphaKeys[i];
				g.SetKeys(gck, gak);
				g.mode = sg.mode;
			}
			return g;
		}
	}
	
	[System.SerializableAttribute]
	public class SerializableGradientAlphaKey
	{
		public float				alpha;
		public float				time;

		public SerializableGradientAlphaKey(GradientAlphaKey gak)
		{
			alpha = gak.alpha;
			time = gak.time;
		}

		public static implicit operator GradientAlphaKey(SerializableGradientAlphaKey sgak)
		{
			return new GradientAlphaKey(sgak.alpha, sgak.time);
		}

		public static explicit operator SerializableGradientAlphaKey(GradientAlphaKey gak)
		{
			return new SerializableGradientAlphaKey(gak);
		}
	}
	
	[System.SerializableAttribute]
	public class SerializableGradientColorKey
	{
		public SerializableColor	color;
		public float				time;

		public SerializableGradientColorKey(GradientColorKey gck)
		{
			color = (SerializableColor)gck.color;
			time = gck.time;
		}

		public static implicit operator GradientColorKey(SerializableGradientColorKey sgck)
		{
			return new GradientColorKey(sgck.color, sgck.time);
		}

		public static explicit operator SerializableGradientColorKey(GradientColorKey gck)
		{
			return new SerializableGradientColorKey(gck);
		}
	}
}