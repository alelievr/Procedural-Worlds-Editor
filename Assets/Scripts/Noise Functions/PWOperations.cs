using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW
{
	public class PWOperations {
	
		public Sampler2D Add(Sampler2D s1, Sampler2D s2, bool alloc = false)
		{
			Sampler2D ret = s1;
			if (alloc)
				ret = new Sampler2D(s1.size, s1.step);
			ret.Foreach((x, y, val) => {
				return s1[x, y] + s2[x, y];
			});
			return ret;
		}

		//TODO: other operation functions and sampler3D version

	}
}