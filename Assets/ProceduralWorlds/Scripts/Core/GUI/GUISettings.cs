using UnityEngine;
using System;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Core
{
	public enum PWGUIFieldType
	{
		Color,
		Text,
		Slider,
		IntSlider,
		TexturePreview,
		Sampler2DPreview,
		BiomeMapPreview,
		Texture2DArrayPreview,
		FadeBlock,
	}

	[Serializable]
	public class PWGUISettings
	{
		public Vector2			windowPosition;
		public PWGUIFieldType	fieldType;

		[System.NonSerialized]
		public bool		firstRender = true;

		//we put all possible datas for each inputs because unity serialization does not support inheritence :(

		//text field:
		public bool					editing;
		
		//colorPicker:
		public SerializableColor	c;
		public Vector2				thumbPosition;

		//Sampler2D:
		public FilterMode			filterMode;
		public SerializableGradient	serializableGradient;
		[System.NonSerialized]
		public bool					update;
		[System.NonSerialized]
		public bool					samplerTextureUpdated = false;
		public bool					debug;

		//verson of the debug bool only updated durin Layout passes (use this for conditional debug display)
		[System.NonSerialized]
		bool						_frameSafeDebug;
		public bool					frameSafeDebug
		{
			get
			{
				if (Event.current.type == EventType.Layout)
					_frameSafeDebug = debug;
				return _frameSafeDebug;
			}
		}
		[System.NonSerialized]
		public Rect					savedRect;

		[System.NonSerialized]
		public Gradient				gradient;
		public Texture2D			texture;

		//Texture:
		// public FilterMode		filterMode; //duplicated
		public ScaleMode			scaleMode;
		public float				scaleAspect;
		public Material				material;

		//Texture2DArray:
		[System.NonSerialized]
		public Texture2D[]			textures;

		//Editor utils:
		[System.NonSerialized]
		public int					popupHeight;

		//Sampler value to update textures:
		[System.NonSerialized]
		public Sampler2D			sampler2D;
		[System.NonSerialized]
		public BiomeData			biomeData;

		//Fade block:
		[System.NonSerialized]
		public object				fadeStatus;
		public bool					faded;
		
		public PWGUISettings()
		{
			update = false;
		}
	}
}