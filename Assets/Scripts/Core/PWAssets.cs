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
	public class PWAssets {

		public static Texture2DArray	GenerateTexture2DArray(IEnumerable< Texture2D > texs)
		{
			bool				isLinear = false;
			int					i;
			Texture2DArray		ret;

			//generate and store Texture2DArray if not found
			int	texCount = texs.Count();
			if (texCount == 0)
			{
				Debug.LogWarning("no texture detected in any biomes");
				return null;
			}
			var firstTexture = texs.First();
			try {
				ret = new Texture2DArray(firstTexture.width, firstTexture.height, texCount, firstTexture.format, firstTexture.mipmapCount > 1, isLinear);
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

		public static Texture2DArray	GenerateOrLoadTexture2DArray(string fName, IEnumerable< Texture2D > texs)
		{
			Texture2DArray	ret;

			if (String.IsNullOrEmpty(fName) || texs == null)
			{
				Debug.LogError("asset file null or empty !");
				return null;
			}
			string assetFile = Path.Combine(PWConstants.resourcePath, fName + ".asset");

			ret = Resources.Load< Texture2DArray >(assetFile);
			if (ret != null)
				return ret;
				
			#if UNITY_EDITOR
				ret = GenerateTexture2DArray(texs);
				AssetDatabase.CreateAsset(ret, assetFile);
				AssetDatabase.SaveAssets();
			#else
				Debug.LogError("Cannot save TextureArray in play mode !" + assetFile);
				return null;
			#endif
			return ret;
		}
	
		public static Texture2DArray	GenerateOrLoadBiomeTexture2DArray(BiomeSwitchTree bst, string fName)
		{
			var biomeTextures = bst.GetBiomes().OrderBy(kp => kp.Key).Select(kp => kp.Value.surfaceMaps.albedo).Where(a => a != null);
			return GenerateOrLoadTexture2DArray(fName, biomeTextures);
		}
	}
}