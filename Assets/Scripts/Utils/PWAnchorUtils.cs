using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	public static class PWAnchorUtils
	{
	
		static bool				AreAssignable(Type from, Type to)
		{
			if (to == typeof(object))
				return true;

			if (to == typeof(PWValues))
			{
				//TODO: check for allowed types from PWValues
				return true;
			}

			if (from == to)
				return true;
			
			if (from.IsAssignableFrom(to))
				return true;

			return false;
		}

		public static bool		AnchorAreAssignable(PWAnchor from, PWAnchor to, bool verbose = false)
		{
			if (from.anchorType == to.anchorType)
				return false;

			//swap anchor if from is input and to is output
			Type fromType = (from.anchorType == PWAnchorType.Output) ? from.fieldType : to.fieldType;
			Type toType = (to.anchorType == PWAnchorType.Input) ? to.fieldType : from.fieldType;

			if (verbose)
			{
				Debug.Log("fromType: " + fromType + ", toType: " + toType);
				Debug.Log(fromType.ToString() + " can be placed into " + toType.ToString() + ": " + AreAssignable(fromType, toType));
			}
			return AreAssignable(fromType, toType);
		}
	
	}
}