using UnityEngine;
using PW;
using PW.Core;

public class PWSideView2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph(graph);
	}
	
	public override object OnChunkCreate(ChunkData chunkData, Vector3 pos)
	{
		// SideView2DData	chunk = (SideView2DData)chunkData;

		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		g.name = "chunk-" + pos;
		g.transform.parent = terrainRoot.transform;
		g.transform.position = pos;
		g.transform.localScale = Vector3.one * 10;
		// g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);

		return g;
	}
}
