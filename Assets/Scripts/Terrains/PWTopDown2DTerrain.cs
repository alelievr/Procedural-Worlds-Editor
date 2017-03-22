using UnityEngine;
using PW;

public class PWTopDown2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph();
	}
	
	public override void RenderChunk(object chunkData, Vector3 pos)
	{
		TopDown2DData	chunk = (TopDown2DData)chunkData;

		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		g.name = "chunk-" + pos;
		g.transform.parent = terrainRoot.transform;
		g.transform.position = pos;
		g.transform.localScale = Vector3.one * 10;
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
