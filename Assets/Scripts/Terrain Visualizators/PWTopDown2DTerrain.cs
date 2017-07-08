using UnityEngine;
using PW;
using PW.Core;

public class PWTopDown2DTerrain : PWTerrainBase {

	void Start () {
		InitGraph(graph);
	}
	
	public override object OnChunkCreate(ChunkData cd, Vector3 pos)
	{
		if (cd == null)
			return null;
		
		TopDown2DData	chunk = (TopDown2DData)cd;

		GameObject g = CreateChunkObject(pos, PrimitiveType.Quad);
		g.transform.rotation = Quaternion.Euler(90, 0, 0);
		g.transform.localScale = Vector3.one * 10;
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
		return g;
	}

	public override void OnChunkRender(ChunkData cd, object chunkGameObject, Vector3 pos)
	{
		if (cd == null)
			return ;
		GameObject		g = chunkGameObject as GameObject;
		TopDown2DData	chunk = (TopDown2DData)cd;

		if (g == null) //if gameobject have been destroyed by user and reference was lost.
		{
			chunkGameObject = RequestCreate(cd, pos);
			g = chunkGameObject as GameObject;
		}
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
