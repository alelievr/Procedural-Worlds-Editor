using UnityEngine;
using System.Collections.Generic;
using System;

//TODO: remove this file
namespace PW.Core
{
	public enum BiomeTerrainModifierType
	{
		Curve,
		Max,
	}

    public enum BiomeTerrainModifierInput
    {
        Height,
        Wetness,
        Temperature,
        //TODO: oth
    }

	/*
	**	Store the terrain modification performed by biome
	*/
	[Serializable]
	public class BiomeTerrainModifer
	{
		public BiomeTerrainModifierType		type;

		//position in the node
		public float						y;

		//Curve modifier:
		public SerializableAnimationCurve	curve = new SerializableAnimationCurve();
        [NonSerialized]
        public AnimationCurve               nCurve = null;

		//Max modifier:
		public Sampler						inputMaxTerrain;
		
		//TODO: other modifiers
	}

	[Serializable]
	public class BiomeTerrain
	{
		[SerializeField]
		public List< BiomeTerrainModifer >	terrainModifiers = new List< BiomeTerrainModifer >();

        public float ComputeValue(int x, int y, float inVal)
        {
            foreach (var tm in terrainModifiers)
            {
                if (tm.nCurve == null)
                    tm.nCurve = (AnimationCurve)tm.curve;
                switch (tm.type)
                {
                    case BiomeTerrainModifierType.Curve:
                        return tm.nCurve.Evaluate(inVal);
                    case BiomeTerrainModifierType.Max:
                        //TODO
                        break ;
                }
            }
            return (inVal);
        }
	}
}