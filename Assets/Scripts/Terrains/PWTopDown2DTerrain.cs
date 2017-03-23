using UnityEngine;
using PW;

public class PWTopDown2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph(graph);
	}
	
	public override object RenderChunk(object chunkData, Vector3 pos)
	{
		TopDown2DData	chunk = (TopDown2DData)chunkData;

		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		g.name = "chunk-" + pos;
		g.transform.parent = terrainRoot.transform;
		g.transform.position = pos;
		g.transform.rotation = Quaternion.Euler(90, 0, 0);
		g.transform.localScale = Vector3.one * 10;
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
		return g;
	}

	public override void UpdateChunkRender(object chunkData, object chunkGameObject, Vector3 pos)
	{
		GameObject g = chunkGameObject as GameObject;
		TopDown2DData	chunk = (TopDown2DData)chunkData;

		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
