using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;
using Debug = UnityEngine.Debug;

namespace PW.Core
{
	using Node;

	public enum PWGraphProcessMode
	{
		Normal,		//output a disaplayable terrain (with isosurface / oth)
		Geologic,	//output a structure containing all maps for a chunk (terrain, wet, temp, biomes, ...)
	}

	[CreateAssetMenu(fileName = "New ProceduralWorld", menuName = "Procedural World", order = 1)]
	[System.SerializableAttribute]
	public class PWMainGraph : PWGraph {

		//Editor datas:
		public Vector2					leftBarScrollPosition;
		public Vector2					selectorScrollPosition;
		public float					maxStep;

		//tell if the user choose a preset (first screen)
		public bool						presetChoosed;

		//chunk relative datas
		public int						chunkSize;
		public float					step;
		public float					geologicTerrainStep;

		public PWGraphTerrainType		outputType;
		public PWGraphProcessMode		processMode;

		[System.NonSerialized]
		Vector3							currentChunkPosition;

		//Precomputed data part:
		public TerrainDetail			terrainDetail = new TerrainDetail();
		public int						geologicDistanceCheck;

		[System.NonSerialized]
		public GeologicBakedDatas		geologicBakedDatas = new GeologicBakedDatas();
		

		//parameter events:
		public event System.Action				OnChunkSizeChanged;
		public event System.Action				OnStepChanged;
		public event System.Action				OnChunkPositionChanged;
		
		void		BakeNeededGeologicDatas()
		{
			float		oldStep = step;
			processMode = PWGraphProcessMode.Geologic;
			step = geologicTerrainStep;

			for (int x = 0; x < geologicDistanceCheck; x++)
				for (int y = 0; y < geologicDistanceCheck; y++)
					Process();

			processMode = PWGraphProcessMode.Normal;
			step = oldStep;
		}

		public override void OnEnable()
		{
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}

    }
}
