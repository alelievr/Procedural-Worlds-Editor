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

		Color GetAnchorDominantColor(PWAnchorInfo from, PWAnchorInfo to)
		{
			if (from.anchorColor.Compare(PWColorScheme.GetColor("greyAnchor")) || from.anchorColor.Compare(PWColorScheme.GetColor("whiteAnchor")))
				return to.anchorColor;
			if (to.anchorColor.Compare(PWColorScheme.GetColor("greyAnchor")) || to.anchorColor.Compare(PWColorScheme.GetColor("whiteAnchor")))
				return from.anchorColor;
			return to.anchorColor;
		}

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
		
		void UpdateDependentStatus()
		{
			isDependent = false;
			ForeachPWAnchors((data, singleAnchor, i) => {
				if (data.anchorType == PWAnchorType.Input && data.required && singleAnchor.visibility == PWVisibility.Visible)
					isDependent = true;
			}, false, false);
		}
		
		static bool			AnchorAreAssignable(Type fromType, PWAnchorType fromAnchorType, bool fromGeneric, SerializableType[] fromAllowedTypes, PWAnchorInfo to, bool verbose = false)
		{
			bool ret = false;

			if ((fromType != typeof(PWValues) && to.fieldType != typeof(PWValues)) //exclude PWValues to simple assignation (we need to check with allowedTypes)
				&& (fromType.IsAssignableFrom(to.fieldType) || fromType == typeof(object) || to.fieldType == typeof(object)))
			{
				if (verbose)
					Debug.Log(fromType.ToString() + " is assignable from " + to.fieldType.ToString());
				return true;
			}

			if (fromGeneric || to.generic)
			{
				if (verbose)
					Debug.Log("from type is generic");
				SerializableType[] types = (fromGeneric) ? fromAllowedTypes : to.allowedTypes;
				Type secondType = (fromGeneric) ? to.fieldType : fromType;
				foreach (Type firstT in types)
					if (fromGeneric && to.generic)
					{
						if (verbose)
							Debug.Log("to type is generic");
							
						if (firstT == typeof(object))
						{
							ret = true;
							break ;
						}

						foreach (Type toT in to.allowedTypes)
						{
							if (verbose)
								Debug.Log("checking assignable from " + firstT + " to " + toT);

							if (toT == typeof(object))
							{
								ret = true;
								break ;
							}

							if (firstT.IsAssignableFrom(toT))
								ret = true;
						}
					}
					else
					{
						if (verbose)
							Debug.Log("checking assignable from " + firstT + " to " + secondType);
						if (firstT.IsAssignableFrom(secondType))
							ret = true;
					}
			}
			else
			{
				if (verbose)
					Debug.Log("non-generic types, checking assignable from " + fromType + " to " + to.fieldType);
				if (fromType.IsAssignableFrom(to.fieldType) || to.fieldType.IsAssignableFrom(fromType))
					ret = true;
			}
			if (verbose)
				Debug.Log("result: " + ret);
			return ret;
		}

		public static bool		AnchorAreAssignable(PWAnchorInfo from, PWAnchorInfo to, bool verbose = false)
		{
			if (from.anchorType == to.anchorType)
				return false;
			return AnchorAreAssignable(from.fieldType, from.anchorType, from.generic, from.allowedTypes, to, verbose);
		}

	}
}