using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PWUtils {

	public static Rect DecalRect(Rect r, Vector2 decal, bool newRect = false)
	{
		if (newRect)
			r = new Rect(r);
		r.x += decal.x;
		r.y += decal.y;
		return r;
	}

}
