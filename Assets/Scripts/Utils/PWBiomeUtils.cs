using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public static class PWBiomeUtils {
	
		public static Dictionary< BiomeDataType, Type >	biomeDataTypes = new Dictionary< BiomeDataType, Type >();
	
		static PWBiomeUtils()
		{
			biomeDataTypes[BiomeDataType.BiomeLess] = typeof(BiomeLessData);

			biomeDataTypes[BiomeDataType.WaterlessHeight] = typeof(WaterlessBiomeHeightData);
			biomeDataTypes[BiomeDataType.Height] = typeof(BiomeHeightData);
			biomeDataTypes[BiomeDataType.WaterlessHeight3D] = typeof(WaterlessBiomeHeightData3D);
			biomeDataTypes[BiomeDataType.Height3D] = typeof(BiomeHeightData3D);

			biomeDataTypes[BiomeDataType.WaterlessWetness] = typeof(WaterlessWetnessBiomeData);
			biomeDataTypes[BiomeDataType.Wetness] = typeof(WetnessBiomeData);
			biomeDataTypes[BiomeDataType.WaterlessWetness3D] = typeof(WaterlessWetnessBiomeData3D);
			biomeDataTypes[BiomeDataType.Wetness3D] = typeof(WetnessBiomeData3D);

			biomeDataTypes[BiomeDataType.WaterlessEnhancedClimate] = typeof(WaterlessEnhancedClimateBiomeData);
			biomeDataTypes[BiomeDataType.EnhancedClimate] = typeof(EnhancedClimateBiomeData);
			biomeDataTypes[BiomeDataType.WaterlessEnhancedClimate3D] = typeof(WaterlessEnhancedClimateBiomeData3D);
			biomeDataTypes[BiomeDataType.EnhancedClimate3D] = typeof(EnhancedClimateBiomeData3D);

			biomeDataTypes[BiomeDataType.WaterlessComplexClimate] = typeof(WaterlessComplexClimateBiomeData);
			biomeDataTypes[BiomeDataType.ComplexClimate] = typeof(ComplexClimateBiomeData);
			biomeDataTypes[BiomeDataType.WaterlessComplexClimate3D] = typeof(WaterlessComplexClimateBiomeData3D);
			biomeDataTypes[BiomeDataType.ComplexClimate3D] = typeof(ComplexClimateBiomeData3D);

			biomeDataTypes[BiomeDataType.Custom] = typeof(CustomBiomeData);
			biomeDataTypes[BiomeDataType.Custom3D] = typeof(CustomBiomeData3D);
		}
	
	
	}
}