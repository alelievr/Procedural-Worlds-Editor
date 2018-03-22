using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds
{
	public static class VectorUtils
	{

		public static Vector3 Floor(Vector3 v)
		{
			v.x = Mathf.Floor(v.x);
			v.y = Mathf.Floor(v.y);
			v.z = Mathf.Floor(v.z);

			return v;
		}

		public static Vector2 Floor(Vector2 v)
		{
			v.x = Mathf.Floor(v.x);
			v.y = Mathf.Floor(v.y);

			return v;
		}

	}
}
	