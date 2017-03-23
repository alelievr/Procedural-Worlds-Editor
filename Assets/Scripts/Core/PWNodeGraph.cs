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
		public bool							dragginGraph = false;
		[SerializeField]
		public bool							mouseAboveNodeAnchor = false;
		[SerializeField]
		public int							localWindowIdCount;
		[SerializeField]
		public string						firstInitialization;
		[SerializeField]
		public bool							realMode;
		
		[SerializeField]
		public PWAnchorInfo					startDragAnchor;
		[SerializeField]
		public bool							draggingLink = false;
		
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
	}
}