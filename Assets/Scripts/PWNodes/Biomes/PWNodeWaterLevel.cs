using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace PW
{
	public class PWNodeWaterLevel : PWNode {

		[PWInput]
		public Sampler		noise;

		[PWOutput]
		public BiomeData	biomeData;

		public float		waterLevel;
		
		public override void OnNodeGUI()
		{
			waterLevel = EditorGUILayout.FloatField("WaterLevel", waterLevel);

			if (noise != null)
			{
				string[]	biomeDataTypes;

				if (noise.GetType() == typeof(Sampler2D))
				{
					biomeDataTypes = Enum.GetNames(typeof(BiomeDataType));
					biomeDataTypes = biomeDataTypes.Where(b => !b.Contains("3D")).ToArray();
				}
			}

			//TODO: finish this node
		}

		public override void OnNodeProcess()
		{
		}

	}
}