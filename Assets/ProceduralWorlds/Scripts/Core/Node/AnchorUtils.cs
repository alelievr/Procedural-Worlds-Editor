using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralWorlds.Core
{
	public static class AnchorUtils
	{
	
		static bool				AreAssignable(Type from, Type to)
		{
			if (to == typeof(object))
				return true;
			
			if (from == null || to == null)
				return false;

			//if to or from are PWArray, we replace the to/from type by their generic type
			if (to.IsGenericType && to.GetGenericTypeDefinition() == typeof(PWArray<>))
				to = to.GetGenericArguments()[0];
			if (from.IsGenericType && from.GetGenericTypeDefinition() == typeof(PWArray<>))
				from = from.GetGenericArguments()[0];

			if (from == to)
				return true;
			
			//Allow parrent -> child assignation but also child -> parrent
			if (from.IsAssignableFrom(to) || to.IsAssignableFrom(from))
				return true;

			return false;
		}

		public static bool		AnchorAreAssignable(Anchor from, Anchor to, bool verbose = false)
		{
			if (from.anchorType == to.anchorType || from.nodeRef == to.nodeRef)
				return false;

			//swap anchor if from is input and to is output
			Type fromType = (from.anchorType == AnchorType.Output) ? from.fieldType : to.fieldType;
			Type toType = (to.anchorType == AnchorType.Input) ? to.fieldType : from.fieldType;

			if (verbose)
			{
				Debug.Log("fromType: " + fromType + ", toType: " + toType);
				Debug.Log(fromType + " can be placed into " + toType + ": " + AreAssignable(fromType, toType));
			}
			return AreAssignable(fromType, toType);
		}
	
	}
}