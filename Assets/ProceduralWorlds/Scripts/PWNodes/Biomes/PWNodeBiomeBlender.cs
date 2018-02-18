using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeBlender : PWNode
	{

		[PWInput]
		public PWArray< PartialBiome >	inputBiomes = new PWArray< PartialBiome >();

		[PWOutput]
		public BlendedBiomeTerrain		outputBlendedBiomeTerrain = new BlendedBiomeTerrain();

		[SerializeField]
		float				biomeBlendPercent = .1f;

		[SerializeField]
		BiomeBlendList	blendMatrix = new BiomeBlendList();

		[SerializeField]
		bool				biomeCoverageRecap = false;

		[System.NonSerialized]
		bool				updateBiomeMap = true;

		string				updateBiomeMapKey = "BiomeBlender";

		public override void OnNodeCreation()
		{
			name = "Biome blender";
		}

		public override void OnNodeEnable()
		{
			OnReload += OnReloadCallback;

			delayedChanges.BindCallback(updateBiomeMapKey, (unused) => {
				BiomeData data = GetBiomeData();

				FillBiomeMap(data);
				updateBiomeMap = true;

				NotifyReload();
			});
			
			if (inputBiomes.GetValues().Count == 0)
				return ;
		}

		public override void OnNodeDisable()
		{
			OnReload -= OnReloadCallback;
		}

		BiomeData	GetBiomeData()
		{
			var partialbiomes = inputBiomes.GetValues();
			
			if (partialbiomes.Count == 0)
				return null;
			
			var biomeDataRef = partialbiomes.FirstOrDefault(pb => pb != null && pb.biomeDataReference != null);

			if (biomeDataRef == null)
				return null;

			return biomeDataRef.biomeDataReference;
		}

		public override void OnNodeGUI()
		{
			BiomeData biomeData = GetBiomeData();

			if (biomeData == null)
			{
				EditorGUILayout.LabelField("biomes not connected !");
				return ;
			}
			else
			{
				EditorGUIUtility.labelWidth = 120;
				EditorGUI.BeginChangeCheck();
				biomeBlendPercent = PWGUI.Slider("Biome blend ratio: ", biomeBlendPercent, 0f, .5f);
				if (EditorGUI.EndChangeCheck())
					delayedChanges.UpdateValue(updateBiomeMapKey);
				blendMatrix.UpdateMatrixIfNeeded(biomeData);

				EditorGUI.BeginChangeCheck();
				blendMatrix.DrawList(biomeData, visualRect);
				if (EditorGUI.EndChangeCheck())
					FillBiomeMap(biomeData);
			}

			if (biomeData != null)
			{
				if (biomeData.biomeMap != null)
					PWGUI.BiomeMap2DPreview(biomeData);
				//TODO: biome 3D preview
			}
			else
				EditorGUILayout.LabelField("no biome data");
			
			if (updateBiomeMap)
			{
				PWGUI.SetUpdateForField(1, true);
				updateBiomeMap = false;
			}

			var biomeCoverage = biomeData.biomeSwitchGraph.GetBiomeCoverage();

			bool biomeCoverageError = biomeCoverage.Any(b => b.Value > 0 && b.Value < 1);

			GUIStyle biomeCoverageFoloutStyle = (biomeCoverageError) ? PWStyles.errorFoldout : EditorStyles.foldout;

			if (biomeCoverageRecap = EditorGUILayout.Foldout(biomeCoverageRecap, "Biome coverage recap", biomeCoverageFoloutStyle))
			{
				if (biomeData != null && biomeData.biomeSwitchGraph != null)
				{
					foreach (var biomeCoverageKP in biomeCoverage)
						if (biomeCoverageKP.Value > 0)
						{
							string paramName = biomeData.GetBiomeKey(biomeCoverageKP.Key);
							EditorGUILayout.LabelField(paramName, (biomeCoverageKP.Value * 100).ToString("F2") + "%");
						}
				}
				else
					EditorGUILayout.LabelField("Null biome data/biome tree");
			}
		}
		
		public override void OnNodeProcessOnce()
		{
			var partialBiomes = inputBiomes.GetValues();

			foreach (var partialBiome in partialBiomes)
				partialBiome.biomeGraph.ProcessOnce();

			BuildBiomeSwitchGraph();
		}

		public override void OnNodeProcess()
		{
			if (inputBiomes.Count == 0 || inputBiomes.GetValues().All(b => b == null))
				return ;

			var partialBiomes = inputBiomes.GetValues();
			var biomeData = GetBiomeData();

			if (biomeData == null)
				return ;
			
			//run the biome tree precomputing once all the biome tree have been parcoured
			if (!biomeData.biomeSwitchGraph.isBuilt)
				BuildBiomeSwitchGraph();
			
			FillBiomeMap(biomeData);

			outputBlendedBiomeTerrain.biomes.Clear();

			//once the biome data is filled, we call the biome graphs corresponding to the biome id
			foreach (var id in biomeData.ids)
			{
				foreach (var partialBiome in partialBiomes)
				{
					if (partialBiome == null)
						continue ;
					
					if (id == partialBiome.id)
					{
						if (partialBiome.biomeGraph == null)
							continue ;
						
						partialBiome.biomeGraph.SetInput(partialBiome);
						partialBiome.biomeGraph.Process();

						if (!partialBiome.biomeGraph.hasProcessed)
						{
							Debug.LogError("[PWBiomeBlender] Can't process properly the biome graph '" + partialBiome.biomeGraph + "'");
							continue ;
						}

						Biome b = partialBiome.biomeGraph.GetOutput();

						if (outputBlendedBiomeTerrain.biomes.Contains(b))
						{
							Debug.LogError("[PWBiomeBlender] Duplicate biome in the biome graph: " + b.name + " (" + b.id + ")");
							continue ;
						}

						outputBlendedBiomeTerrain.biomes.Add(b);
					}
				}
			}

			outputBlendedBiomeTerrain.biomeData = biomeData;
		}

		void FillBiomeMap(BiomeData biomeData)
		{
			biomeData.biomeSwitchGraph.FillBiomeMap(biomeData, blendMatrix, biomeBlendPercent);
			updateBiomeMap = true;
		}

		void OnReloadCallback(PWNode from)
		{
			BuildBiomeSwitchGraph();
			
			var biomeData = GetBiomeData();
			
			//if the reload does not comes from the editor
			if (from != null)
			{
				FillBiomeMap(biomeData);
				updateBiomeMap = true;
			}
		}
		
		void BuildBiomeSwitchGraph()
		{
			BiomeData biomeData = GetBiomeData();

			if (biomeData == null)
			{
				Debug.LogError("[PWBiomeBlender] Can't access to partial biome data, did you forgot the BiomeGraph in a biome node ?");
				return ;
			}

			biomeData.biomeSwitchGraph.BuildGraph(biomeData);
		}
	}
}
