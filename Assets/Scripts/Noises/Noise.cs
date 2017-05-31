using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Noise
{
	public class Noise {
	
		public string	name;
		public bool		hasGraphicAcceleration;
	
		public Noise()
		{
			hasGraphicAcceleration = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
		}
	}
}