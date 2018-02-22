using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeCurve : PWNode
	{

		//maybe a function to change the visibility when user is dragging a link of Biome type ?
		//or a button to witch the node type

		[PWInput("Terrain input")]
		public Sampler		inputTerrain;

		[PWOutput("Terrain output")]
		public Sampler		outputTerrain;

		public AnimationCurve				curve;
		public SerializableAnimationCurve	sCurve = new SerializableAnimationCurve();

		public override void 	OnNodeCreation()
		{
			name = "Curve";
			curve = (AnimationCurve)sCurve;
		}

		public void				CurveTerrain()
		{
			if (inputTerrain == null)
				return ;
			
			Sampler samp = inputTerrain.Clone(outputTerrain);

			if (samp.type == SamplerType.Sampler2D)
			{
				float d = samp.max - samp.min;
				(samp as Sampler2D).Foreach((x, y, val) => {
					if (float.IsNaN(val))
						return 0;
					return curve.Evaluate(val / d) * d;
				});
			}
			else if (samp.type == SamplerType.Sampler3D)
			{
				float d = samp.max - samp.min;
				(samp as Sampler3D).Foreach((x, y, z, val) => {
					if (float.IsNaN(val))
						return 0;
					return curve.Evaluate(val / d) * d;
				});
			}

			outputTerrain = samp;
		}

		public override void	OnNodeProcess()
		{
			if (inputTerrain == null)
			{
				Debug.LogError("[PWNodeCurve] null inputTerrain received in input !");
				return ;
			}
			
			CurveTerrain();
		}
		
	}
}