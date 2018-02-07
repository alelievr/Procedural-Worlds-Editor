using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using System;

namespace PW
{
	public class PWOperations
	{

		Sampler2D Operation2D(Sampler2D s1, Sampler2D s2, bool alloc, Func< float, float, float > callback)
		{
			Sampler2D ret = s1;
			if (alloc)
				ret = new Sampler2D(s1.size, s1.step);
			ret.Foreach((x, y, val) => {
				return callback(s1[x, y], s2[x, y]);
			});
			return ret;
		}
	
		public Sampler2D Add(Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 + f2);
		}
		
		public Sampler2D Sub(Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 - f2);
		}
		
		public Sampler2D Mul(Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 * f2);
		}
		
		public Sampler2D Div(Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			return Operation2D(s1, s2, alloc, (f1, f2) => f1 / f2);
		}

	}
}