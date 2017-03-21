using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public abstract class PWTerrainBase< T > : MonoBehaviour, PWTerrainInterface< T > {
		public Vector3			position;
		public int				viewDistance;
		public PWChunkLoadMode	loadMode;
		public PWNodeGraph		graph;

		private PWNodeGraphOutput	graphOutput;
	
		public void InitGraph()
		{
			graphOutput = graph.outputNode as PWNodeGraphOutput;
			//TODO: initialize graph for computing.
		}
	
		public T RequestChunk(Vector3 pos, int seed)
		{
			graph.ProcessGraph();
			return (T)graphOutput.inputValues.At(0);
		}
	}
}