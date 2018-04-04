using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Noises;

namespace ProceduralWorlds.Nodes
{
	public class NodeFlat3D : BaseNode
	{
		[Output]
		public Sampler3D	output;

		public float		value;

		Flat3D				flat;
		float				oldValue;

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "Flat 3D";
		}

		public override void OnNodeEnable()
		{
			flat = new Flat3D();
			UpdateOutput();
		}

		public override void OnNodeProcess()
		{
			if (oldValue == value)
				return ;
			
			UpdateOutput();
		}

		public void UpdateOutput()
		{
			if (output == null)
				output = new Sampler3D(chunkSize, step);
			output.ResizeIfNeeded(chunkSize, step);
			flat.UpdateParams(value);
			flat.ComputeSampler3D(output, Vector3.zero);
			oldValue = value;
		}

	}
}