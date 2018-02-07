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

	[System.SerializableAttribute]
	public class PWMainGraph : PWGraph
	{

		//Editor datas:
		public Vector2					leftBarScrollPosition;

		//chunk relative datas
		[SerializeField]
		float							_geologicTerrainStep;
		public float					geologicTerrainStep
		{
			get { return _geologicTerrainStep; }
			set
			{
				if (_geologicTerrainStep != value)
				{
					_geologicTerrainStep = value;
					if (OnGeologicStepChanged != null)
						OnGeologicStepChanged(_geologicTerrainStep);
				}
			}
		}

		public PWGraphTerrainType		terrainType;
		public PWGraphProcessMode		processMode;
		public MaterializerType			materializerType;

		[System.NonSerialized]
		Vector3							currentChunkPosition;

		//Precomputed data part:
		public TerrainDetail			terrainDetail = new TerrainDetail();
		public int						geologicDistanceCheck;

		[System.NonSerialized]
		public GeologicBakedDatas		geologicBakedDatas = new GeologicBakedDatas();
		
		//parameter events:
		public event Action< float >		OnGeologicStepChanged;
		//button triggered events:
		
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

		public override void Initialize()
		{
			base.Initialize();
			
			geologicTerrainStep = 8;
			geologicDistanceCheck = 2;
	
			processMode = PWGraphProcessMode.Normal;
		}

		public override void OnEnable()
		{
			graphType = PWGraphType.Main;
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
    }
}
