using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Core;
using PW.Node;

//Utils for PWNode class
namespace PW
{
	public partial class PWNode
	{

		Color FindColorFromtypes(SerializableType[] types)
		{
			Color defaultColor = PWColorScheme.GetColor("defaultAnchor");

			foreach (var type in types)
			{
				Color c = PWColorScheme.GetAnchorColorByType(type.GetType());
				if (!c.Compare(defaultColor))
					return c;
			}
			return defaultColor;
		}

		PWAnchorType			InverAnchorType(PWAnchorType type)
		{
			if (type == PWAnchorType.Input)
				return PWAnchorType.Output;
			else if (type == PWAnchorType.Output)
				return PWAnchorType.Input;
			return PWAnchorType.None;
		}
		
		public static PWLinkType GetLinkTypeFromType(Type fieldType)
		{
			if (fieldType == typeof(Sampler2D))
				return PWLinkType.Sampler2D;
			if (fieldType == typeof(Sampler3D))
				return PWLinkType.Sampler3D;
			if (fieldType == typeof(Vector3) || fieldType == typeof(Vector3i))
				return PWLinkType.ThreeChannel;
			if (fieldType == typeof(Vector4))
				return PWLinkType.FourChannel;
			return PWLinkType.BasicData;
		}
		
	}
}