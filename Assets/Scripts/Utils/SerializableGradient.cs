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

		public SerializableGradient(Gradient g)
		{
			colorKeys = g.colorKeys.Cast< SerializableGradientColorKey >().ToArray();
			alphaKeys = g.alphaKeys.Cast< SerializableGradientAlphaKey >().ToArray();
			mode = g.mode;
		}

		public static explicit operator SerializableGradient(Gradient g)
		{
			return new SerializableGradient(g);
		}
	
		public static implicit operator Gradient(SerializableGradient sg)
		{
			Gradient g = new Gradient();
			g.SetKeys(sg.colorKeys.Cast< GradientColorKey >().ToArray(), sg.alphaKeys.Cast< GradientAlphaKey >().ToArray());
			g.mode = sg.mode;
			return g;
		}
	}
	
	[System.SerializableAttribute]
	public class SerializableGradientAlphaKey
	{
		public float				alpha;
		public float				time;

		public static implicit operator GradientAlphaKey(SerializableGradientAlphaKey sgak)
		{
			return new GradientAlphaKey(sgak.alpha, sgak.time);
		}
	}
	
	[System.SerializableAttribute]
	public class SerializableGradientColorKey
	{
		public SerializableColor	color;
		public float				time;

		public static implicit operator GradientColorKey(SerializableGradientColorKey sgck)
		{
			return new GradientColorKey(sgck.color, sgck.time);
		}
	}
}