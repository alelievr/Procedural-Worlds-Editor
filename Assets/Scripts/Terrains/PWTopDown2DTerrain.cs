using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;

public class PWTopDown2DTerrain : PWTerrainBase< TopDown2DData > {

	ChunkStorage< TopDown2DData > chunks = new ChunkStorage< TopDown2DData >();

	Vector3		pos = Vector3.zero;

	// Use this for initialization
	void Start () {
		InitGraph();
	}
	
	// Update is called once per frame
	void Update () {
		if (!chunks.isLoaded(pos))
		{
			Debug.LogWarning("loading chunk: " + pos);
			var data = chunks.AddChunk(pos, RequestChunk(pos, 42));

			GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
			g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", data.texture);
		}
	}
}
