using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProceduralWorlds;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.IsoSurfaces;

public class TopDown2DTerrainHex : TerrainBase< TopDownChunkData >
{
	public float	yPosition;
	public bool		heightDisplacement;
	public float	heightScale = .1f;

	Hex2DIsoSurface	isoSurface = new Hex2DIsoSurface();

	protected override void OnTerrainEnable()
	{
		generateBorders = false;
		isoSurface.generateUvs = true;
	}

	public override object	OnChunkCreate(TopDownChunkData chunk, Vector3 pos)
	{
		if (chunk == null)
			return null;
		
		//turn 2d grid position to 2d hex position:
		float hexMinRadius = Mathf.Cos(Mathf.Deg2Rad * 30);
		pos.x *= hexMinRadius;
		pos.z *= hexMinRadius * hexMinRadius;

		GameObject g = CreateChunkObject(pos);
		
		MeshRenderer mr = g.AddComponent< MeshRenderer >();
		MeshFilter mf = g.AddComponent< MeshFilter >();

		if (heightDisplacement)
			isoSurface.SetHeightDisplacement(chunk.terrain as Sampler2D, heightScale);
		else
			isoSurface.SetHeightDisplacement(null, 0);

		Mesh m = isoSurface.Generate(chunkSize);

		mf.sharedMesh = m;

		Shader topDown2DBasicTerrainShader = Shader.Find("ProceduralWorlds/Basic terrain");
		if (topDown2DBasicTerrainShader == null)
			topDown2DBasicTerrainShader = Shader.Find("Standard");
		Material mat = new Material(topDown2DBasicTerrainShader);
		mr.sharedMaterial = mat;
		return g;
	}

	public override void 	OnChunkDestroy(TopDownChunkData terrainData, object userStoredObject, Vector3 pos)
	{
		GameObject g = userStoredObject as GameObject;

		if (g != null)
			DestroyImmediate(g);
	}

	public override void	OnChunkRender(TopDownChunkData chunk, object chunkGameObject, Vector3 pos)
	{
		if (chunk == null)
			return ;
		
		GameObject		g = chunkGameObject as GameObject;

		if (g == null) //if gameobject have been destroyed by user and reference was lost.
			RequestCreate(chunk, pos);
	}

	public override Vector3 GetChunkPosition(Vector3 pos)
	{
		pos.y = yPosition;

		return pos;
	}
}
