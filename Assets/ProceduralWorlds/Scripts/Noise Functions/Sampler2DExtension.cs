using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds
{
	public static class Sampler2DExtension
	{

		static Sampler2D Operation2D(Sampler2D s1, Sampler2D s2, bool alloc, Func< float, float, float > callback)
		{
			Sampler2D ret = s1;
			if (alloc)
				ret = new Sampler2D(s1.size, s1.step);
			ret.Foreach((x, y, val) => {
				return callback(s1[x, y], s2[x, y]);
			});
			return ret;
		}
	
		public static Sampler2D Add(this Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 + f2);
		}
		
		public static Sampler2D Sub(this Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 - f2);
		}
		
		public static Sampler2D Mul(this Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 * f2);
		}
		
		public static Sampler2D Div(this Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 / f2);
		}

	}
}