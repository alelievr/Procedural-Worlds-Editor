using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeCircleNoiseMask : BaseNode
	{
	
		[Input]
		public Sampler2D	samp;

		[Output]
		public Sampler2D	output;

		public float		blur = .5f;
		public float		radius = .5f;
		public bool			updateEachProcess = false;

		Sampler2D			mask;

		public override void OnNodeCreation()
		{
			name = "2D noise mask";
		}

		public override	void OnNodeEnable()
		{
			mask = new Sampler2D(chunkSize, step);
		}

		public void	CreateNoiseMask()
		{
			if (samp == null)
				return ;
			
			Vector2		center = new Vector2(samp.size / 2, samp.size / 2);
			float		maxDist = samp.size * radius; //simplified max dist to get better noiseMask.

			mask.Resize(samp.size);
			mask.Foreach((x, y) => {
				float val = 1 - (Vector2.Distance(new Vector2(x, y), center) / maxDist);
				return val;
			});
		}

		public override void OnNodeProcess()
		{
			if (updateEachProcess)
			{
				CreateNoiseMask();
				samp.Foreach((x, y, val) => {return val * (mask[x, y]);});
			}
			output = samp;
		}
	}
}
