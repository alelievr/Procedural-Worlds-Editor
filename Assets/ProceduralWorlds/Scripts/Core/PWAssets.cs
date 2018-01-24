using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Biomator;
using System.IO;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PW.Core
{
	public class PWAssets
	{

		public static Texture2DArray	GenerateTexture2DArray(IEnumerable< Texture2D > texs)
		{
			bool				isLinear = false;
			int					i;
			Texture2DArray		ret;

			//generate and store Texture2DArray if not found
			int	texCount = texs.Count();
			if (texCount == 0)
			{
				return new Texture2DArray(1, 1, 1, TextureFormat.RGBA32, false);
			}
			var firstTexture = texs.First();
			bool mipmap = firstTexture.mipmapCount > 1;
			try {
				ret = new Texture2DArray(firstTexture.width, firstTexture.height, texCount, firstTexture.format, mipmap, isLinear);
			} catch (Exception e) {
				Debug.LogError(e);
				return null;
			}
			i = 0;
			
			foreach (var tex in texs)
			{
				if (tex.width != firstTexture.width || tex.height != firstTexture.height)
				{
					Debug.LogError("Texture " + tex + " does not match with first biome texture size w:" + firstTexture.width + "/h:" + firstTexture.height);
					continue ;
				}
				for (int j = 0; j < tex.mipmapCount; j++)
					Graphics.CopyTexture(tex, 0, j, ret, i, j);
				i++;
			}
			ret.anisoLevel = firstTexture.anisoLevel;
			ret.filterMode = firstTexture.filterMode;
			ret.wrapMode = firstTexture.wrapMode;
			ret.Apply();
			return ret;
		}

		public static Texture2DArray	GenerateOrLoadTexture2DArray(string fName, IEnumerable< Texture2D > texs, bool forceReload = false)
		{
			Texture2DArray	ret;

			if (String.IsNullOrEmpty(fName) || texs == null)
			{
				Debug.LogError("asset file null or empty !");
				return null;
			}

			ret = Resources.Load< Texture2DArray >(Path.GetFileNameWithoutExtension(fName));
			if (ret == null)
				Debug.LogError("Texture2DArray not found: " + fName);
			if (ret != null && ret.depth == texs.Count() && !forceReload)
				return ret;
				
			#if UNITY_EDITOR
				ret = GenerateTexture2DArray(texs);
				if (ret == null)
				{
					Debug.LogError("Can't create texture array, no albedo found in any biomes");
					return null;
				}
				AssetDatabase.CreateAsset(ret, fName + ".asset");
				AssetDatabase.SaveAssets();
			#else
				Debug.LogError("Cannot save TextureArray in play mode !" + assetFile);
				return null;
			#endif
			return ret;
		}
	
		public static Texture2DArray	GenerateOrLoadBiomeTexture2DArray(BiomeSwitchTree bst, string fName, bool forceReload = false)
		{
			List< Texture2D > biomeTextures = new List< Texture2D >();
			var biomeSurfaces = bst.GetBiomes().OrderBy(kp => kp.Key).Select(kp => kp.Value.biomeSurfaces);
			
			foreach (var biomeSurface in biomeSurfaces)
				if (biomeSurface.surfaceSwitches != null)
					foreach (var surfaceSwitch in biomeSurface.surfaceSwitches)
						if (surfaceSwitch.surfaceType == BiomeSurfaceType.SurfaceMaps)
							if (surfaceSwitch.surfaceMaps != null)
								if (surfaceSwitch.surfaceMaps.albedo != null && !biomeTextures.Contains(surfaceSwitch.surfaceMaps.albedo))
									biomeTextures.Add(surfaceSwitch.surfaceMaps.albedo);
									
			return GenerateOrLoadTexture2DArray(fName, biomeTextures, forceReload);
		}
	}
}