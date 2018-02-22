using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeTexture2D : PWNode {

		[PWOutput("Texture 2D")]
		public Texture2D		outputTexture;

		[PWOutput("Material")]
		public Material			outputMaterial;

		public bool				isMaterialOutput = false;
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
				SetAnchorVisibility("outputTexture", PWVisibility.Gone);
				SetAnchorVisibility("outputMaterial", PWVisibility.Visible);
				RemoveAllLinksFromAnchor("outputTexture");
			}
			else
			{
				SetAnchorVisibility("outputTexture", PWVisibility.Visible);
				SetAnchorVisibility("outputMaterial", PWVisibility.Gone);
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
