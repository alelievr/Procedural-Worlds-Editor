using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;
using Debug = UnityEngine.Debug;

namespace ProceduralWorlds.Core
{
	using Node;

	[System.SerializableAttribute]
	public class WorldGraph : BaseGraph
	{
		//Editor datas:
		public Vector2					leftBarScrollPosition;
		public float					maxStep;

		//chunk relative datas
		[TextSerializeField, SerializeField] private int	_seed;
		public int								seed
		{
			get{ return _seed; }
			set
			{
				if (_seed != value)
				{
					_seed = value;
					if (OnSeedChanged != null)
						OnSeedChanged();
				}
			}
		}

		[TextSerializeField, SerializeField] private int	_chunkSize;
		public float							nonModifiedChunkSize { get { return _chunkSize; } }
		public int								chunkSize
		{
			get { return (!realMode && scaledPreviewEnabled) ? scaledPreviewChunkSize : _chunkSize; }
			set
			{
				if (_chunkSize != value && !scaledPreviewEnabled)
				{
					_chunkSize = value;
					if (OnChunkSizeChanged != null)
						OnChunkSizeChanged();
				}
			}
		}

		[TextSerializeField, SerializeField] private Vector3	_chunkPosition;
		public Vector3							chunkPosition
		{
			get { return _chunkPosition; }
			set
			{
				if (_chunkPosition != value)
				{
					_chunkPosition = value;
					if (OnChunkPositionChanged != null)
						OnChunkPositionChanged();
				}
			}
		}

		[TextSerializeField, SerializeField] private float	_step;
		public float							nonModifiedStep { get { return _step; } }
		public float							step
		{
			get { return (!realMode && scaledPreviewEnabled) ? scaledPreviewRatio * _step : _step; }
			set
			{
				if (_step != value && !scaledPreviewEnabled)
				{
					_step = value;
					if (OnStepChanged != null)
						OnStepChanged();
				}
			}
		}

		//Geologic datas
		[TextSerializeField, SerializeField]
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
		
		[TextSerializeField]
		public bool						scaledPreviewEnabled;
		[TextSerializeField]
		public bool						hideChunkLines;
		[TextSerializeField, SerializeField]
		float							_scaledPreviewRatio = 8;
		public float					scaledPreviewRatio
		{
			get { return _scaledPreviewRatio; }
			set
			{
				if (_scaledPreviewRatio != value)
				{
					_scaledPreviewRatio = value;
					if (OnStepChanged != null)
						OnStepChanged();
				}
			}
		}
		[TextSerializeField, SerializeField]
		int								_scaledPreviewChunkSize = 32;
		public int						scaledPreviewChunkSize
		{
			get { return _scaledPreviewChunkSize; }
			set
			{
				if (_scaledPreviewChunkSize != value)
				{
					_scaledPreviewChunkSize = value;
					if (OnChunkSizeChanged != null)
						OnChunkSizeChanged();
				}
			}
		}

		public BaseGraphTerrainType		terrainType;

		[System.NonSerialized]
		Vector3							currentChunkPosition;

		//Precomputed data part:
		public TerrainDetail			terrainDetail = new TerrainDetail();
		public int						geologicDistanceCheck;
		[System.NonSerialized]
		public GeologicBakedDatas		geologicBakedDatas = new GeologicBakedDatas();

		//parameter events:
		public event Action< float >	OnGeologicStepChanged;

		//chunk params events:
		public event Action				OnSeedChanged;
		public event Action				OnChunkSizeChanged;
		public event Action				OnStepChanged;
		public event Action				OnChunkPositionChanged;

		//state bools:
		public bool						processedFromBiome { get; private set; }
		
		void		BakeNeededGeologicDatas()
		{
			float		oldStep = step;
			processMode = GraphProcessMode.Geologic;
			step = geologicTerrainStep;

			for (int x = 0; x < geologicDistanceCheck; x++)
				for (int y = 0; y < geologicDistanceCheck; y++)
					Process();

			processMode = GraphProcessMode.Normal;
			step = oldStep;
		}

		public override void Initialize()
		{
			base.Initialize();

			//default world values:
			chunkSize = 16;
			step = 1;
			maxStep = 4;
			name = "New Procedural Graph";
			
			geologicTerrainStep = 8;
			geologicDistanceCheck = 2;
	
			processMode = GraphProcessMode.Normal;
		}

		public override void InitializeInputAndOutputNodes()
		{
			inputNode = CreateNewNode< NodeWorldGraphInput >(new Vector2(-100, 0), "World Graph Input", true, false);
			outputNode = CreateNewNode< NodeWorldGraphOutput >(new Vector2(100, 0), "World Graph Output", true, false);
		}

		public override void OnEnable()
		{
			graphType = BaseGraphType.World;
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
		
		public void ProcessFrom(BiomeGraph biomeGraph)
		{
			processedFromBiome = true;
			Process();
			processedFromBiome = false;
		}

		public FinalTerrain GetOutputTerrain()
		{
			return (outputNode as NodeWorldGraphOutput).finalTerrain;
		}
    }
}
