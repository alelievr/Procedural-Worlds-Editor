using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Noises;

namespace ProceduralWorlds.Nodes
{
	public class NodeFlat2D : BaseNode
	{
		[Output]
		public Sampler2D	output;
		
		public float		value;

		Flat2D				flat;
		float				oldValue;

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "Flat 2D";
		}

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
			flat = new Flat2D();
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
				output = new Sampler2D(chunkSize, step);
			output.ResizeIfNeeded(chunkSize, step);
			flat.UpdateParams(value);
			flat.ComputeSampler2D(output, Vector3.zero);
			oldValue = value;
		}

	}
}