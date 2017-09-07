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

		public bool						presetChoosed;

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
		
	#region Graph baking (prepare regions that will be computed once)

		void BakeNode(Type t)
		{
			var dico = new Dictionary< string, FieldInfo >();
			bakedNodeFields[t.AssemblyQualifiedName] = dico;
	
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				dico[field.Name] = field;
		}

		void		BakeNeededGeologicDatas()
		{
			float		oldStep = step;
			processMode = PWGraphProcessMode.Geologic;
			step = geologicTerrainStep;

			for (int x = 0; x < geologicDistanceCheck; x++)
				for (int y = 0; y < geologicDistanceCheck; y++)
					ProcessGraph();

			UpdateChunkPosition(currentChunkPosition);
			processMode = PWGraphProcessMode.Normal;
			step = oldStep;
		}

	#endregion

	#region Graph initialization
	
		public void OnEnable()
		{
			//bake node fields to accelerate data transfer from node to node.
			bakedNodeFields.Clear();
			foreach (var nodeType in allNodeTypeList)
				BakeNode(nodeType);

			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] != null)
				{
					nodes[i].RunNodeAwake();
					nodesDictionary[nodes[i].nodeId] = nodes[i];
				}
				else
				{
					nodes.RemoveAt(i);
					i--;
				}
			}
			if (inputNode != null)
				nodesDictionary[inputNode.nodeId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.nodeId] = outputNode;

			foreach (var node in nodes)
				node.UpdateCurrentGraph(this);
			if (inputNode != null)
				inputNode.UpdateCurrentGraph(this);
			if (outputNode != null)
				outputNode.UpdateCurrentGraph(this);
		}

	#endregion

	#region Graph processing



	#endregion

	#region Graph API

		public void			UpdateSeed(int seed)
		{
			this.seed = seed;
			ForeachAllNodes((n) => n.seed = seed, true, true);
		}

		public void			UpdateChunkPosition(Vector3 chunkPos)
		{
			currentChunkPosition = chunkPos;
			ForeachAllNodes((n) => n.chunkPosition = chunkPos, true, true);
		}

		public void			UpdateChunkSize(int chunkSize)
		{
			this.chunkSize = chunkSize;
			ForeachAllNodes((n) => n.chunkSize = chunkSize, true, true);
		}

		public void			UpdateStep(float step)
		{
			this.step = step;
			ForeachAllNodes((n) => n.step = step, true, true);
		}

		public PWNode		FindNodebyId(int nodeId)
		{
			if (nodesDictionary.ContainsKey(nodeId))
				return nodesDictionary[nodeId];
			return null;
		}

		public void			ForeachAllNodes(System.Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, PWNodeGraph graph = null)
		{
			if (graph == null)
				graph = this;

			foreach (var node in graph.nodes)
				callback(node);

			if (graphInputAndOutput)
			{
				callback(graph.inputNode);
				callback(graph.outputNode);
			}
		}

	#endregion
    }
}
