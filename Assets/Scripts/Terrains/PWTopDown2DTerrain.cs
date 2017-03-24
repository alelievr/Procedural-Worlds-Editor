using UnityEngine;
using PW;

public class PWTopDown2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph(graph);
	}
	
	public override object OnChunkCreate(ChunkData cd, Vector3 pos)
	{
		TopDown2DData	chunk = (TopDown2DData)cd;

		GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
		g.name = "chunk-" + pos;
		g.transform.parent = terrainRoot.transform;
		g.transform.position = pos;
		g.transform.rotation = Quaternion.Euler(90, 0, 0);
		g.transform.localScale = Vector3.one * 10;
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
		return g;
	}

	public override void OnChunkRender(ChunkData cd, object chunkGameObject, Vector3 pos)
	{
		GameObject		g = chunkGameObject as GameObject;
		TopDown2DData	chunk = (TopDown2DData)cd;

		if (g == null) //if gameobject have been destroyed by user and reference was lost.
			RequestCreate(cd, pos);
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
