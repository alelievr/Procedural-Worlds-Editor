using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public enum	PWOutputType
	{
		NONE,
		SIDEVIEW_2D,
		TOPDOWNVIEW_2D,
		PLANE_3D,
		SPHERICAL_3D,
		CUBIC_3D,
		DENSITY_1D,
		DENSITY_2D,
		DENSITY_3D,
		MESH,
	}

	[System.SerializableAttribute]
	public class PWNodeGraph : ScriptableObject {
	
		[SerializeField]
		public List< PWNode >				nodes = new List< PWNode >();
		
		[SerializeField]
		public HorizontalSplitView			h1;
		[SerializeField]
		public HorizontalSplitView			h2;
	
		[SerializeField]
		public Vector2						leftBarScrollPosition;
		[SerializeField]
		public Vector2						selectorScrollPosition;
	
		[SerializeField]
		public new string					name;
		[SerializeField]
		public string						saveName;
		[SerializeField]
		public Vector2						graphDecalPosition;
		[SerializeField]
		public int							localWindowIdCount;
		[SerializeField]
		public string						firstInitialization;
		[SerializeField]
		public bool							realMode;
		
		[SerializeField]
		public string						searchString = "";

		[SerializeField]
		public bool							presetChoosed;

		[SerializeField]
		public int							chunkSize;
		[SerializeField]
		public int							seed;

		[SerializeField]
		public PWOutputType					outputType;

		[SerializeField]
		public List< PWNodeGraph >			subGraphs = new List< PWNodeGraph >();
		[SerializeField]
		public PWNodeGraph					parent = null;

		[SerializeField]
		public PWNode						inputNode;
		[SerializeField]
		public PWNode						outputNode;

		[System.NonSerializedAttribute]
		public bool							unserializeInitialized = false;

		public void ProcessGraph()
		{
			//here nodes are sorted by compute-order
			//TODO: rework this to get a working in-depth node process call
			if (parent != null)
				inputNode.Process();
			foreach (var node in nodes)
				if (node != null)
					node.Process();
			foreach (var graph in subGraphs)
				graph.outputNode.Process();
		}

		//TODO here browse on nodes and setSeed / setPosition.
		public void	UpdateSeed(int seed)
		{
			this.seed = seed;
			ForeachAllNodes((n) => n.seed = seed, true, true);
		}

		public void UpdateChunkPosition(Vector3 chunkPos)
		{
			ForeachAllNodes((n) => n.chunkPosition = chunkPos, true, true);
		}

		public void UpdateChunkSize(int chunkSize)
		{
			this.chunkSize = chunkSize;
			ForeachAllNodes((n) => n.chunkSize = chunkSize, true, true);
		}

		public void ForeachAllNodes(System.Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, PWNodeGraph graph = null)
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
			if (recursive)
				foreach (var subgraph in graph.subGraphs)
					ForeachAllNodes(callback, recursive, graphInputAndOutput, subgraph);
		}
	}
}