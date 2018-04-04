using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeTexture2D : BaseNode {

		[Output("Texture 2D")]
		public Texture2D		outputTexture;

		[Output("Material")]
		public Material			outputMaterial;

		public bool				isMaterialOutput;
		public Vector2			tiling = Vector2.one;
		public Vector2			offset;
		public bool				preview = true;

		public override void OnNodeCreation()
		{
			name = "Texture 2D";
		}

		public void UpdateProps()
		{
			if (isMaterialOutput)
			{
				SetAnchorVisibility("outputTexture", Visibility.Gone);
				SetAnchorVisibility("outputMaterial", Visibility.Visible);
				RemoveAllLinksFromAnchor("outputTexture");
			}
			else
			{
				SetAnchorVisibility("outputTexture", Visibility.Visible);
				SetAnchorVisibility("outputMaterial", Visibility.Gone);
				RemoveAllLinksFromAnchor("outputMaterial");
			}
		}

		public void CreateNewMaterial()
		{
			outputMaterial = new Material(Shader.Find("Unlit/Texture"));
			if (outputTexture != null)
				outputMaterial.SetTexture("_MainTex", outputTexture);
			UpdateMaterialProperties();
		}

		public void UpdateMaterialProperties()
		{
			outputMaterial.SetTextureOffset("_MainTex", offset);
			outputMaterial.SetTextureScale("_MainTex", tiling);
			if (outputTexture != null)
				outputMaterial.SetTexture("_MainTex", outputTexture);
		}

		public override void OnNodeProcess()
		{
			if (outputMaterial == null && isMaterialOutput)
				CreateNewMaterial();
		}
	}
}
