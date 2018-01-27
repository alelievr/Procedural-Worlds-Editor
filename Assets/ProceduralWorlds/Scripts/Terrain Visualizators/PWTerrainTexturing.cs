using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

public class PWTerrainTexturing
{
	public Texture2D		biomeColorTexture;
	public Dictionary< short, Vector4 >	biomeColorTextureUvs = new Dictionary< short, Vector4 >();

	public void LoadBiomeSurfaces(Dictionary< short, BiomeSurfaces > surfaces)
	{
		List< Color >	colors = new List< Color >();
		int				i = 0;

		//temporary stuff here (does not handle surface condition switches)
		foreach (var kp in surfaces)
		{
			foreach (var surfaceSwitch in kp.Value.surfaceSwitches)
			{
				if (surfaceSwitch.surface.type == BiomeSurfaceType.Color)
				{
					colors.Add(surfaceSwitch.surface.color.baseColor);
					biomeColorTextureUvs.Add(kp.Key, new Vector4(.5f, i + .5f, .5f, i + .5f));
					i++;
				}
			}
		}

		biomeColorTexture = new Texture2D(1, colors.Count, TextureFormat.RGB24, false);
		biomeColorTexture.filterMode = FilterMode.Bilinear;
		biomeColorTexture.SetPixels(colors.ToArray());
		biomeColorTexture.Apply();
	}

}
