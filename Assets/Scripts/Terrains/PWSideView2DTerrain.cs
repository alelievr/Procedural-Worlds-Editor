using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;

public class PWSideView2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph();
	}
	
	public override object RenderChunk(object chunkData, Vector3 pos)
	{
		SideView2DData	chunk = (SideView2DData)chunkData;

		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		g.name = "chunk-" + pos;
		g.transform.parent = terrainRoot.transform;
		g.transform.position = pos;
		g.transform.localScale = Vector3.one * 10;
		// g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);

		return g;
	}
}
