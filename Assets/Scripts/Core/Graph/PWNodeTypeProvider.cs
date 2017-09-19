using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Node;

namespace PW.Core
{
    public static class PWNodeTypeProvider
    {
        class NodeTypeInfo
        {
            public Type				type = null;
            public PWColorPalette	color = PWColorPalette.Default;
			public string			name = null;
        }

		class NodeTypeInfoList
		{
			public string           title {get; set;}
			public List< NodeTypeInfo >   typeInfos = new List< NodeTypeInfo >();
		}

        static List< Type > allNodeTypes = new List< Type >
		{
			//Primitives:
            typeof(PWNodeSlider), typeof(PWNodeTexture2D), typeof(PWNodeMaterial), typeof(PWNodeConstant), typeof(PWNodeMesh), typeof(PWNodeGameObject), typeof(PWNodeColor), typeof(PWNodeSurfaceMaps),

			//Operations:
            typeof(PWNodeAdd), typeof(PWNodeCurve),

			//Debug:
            typeof(PWNodeDebugLog),

			//Noises and masks:
            typeof(PWNodeCircleNoiseMask), typeof(PWNodePerlinNoise2D),

			//Materializers:
            typeof(PWNodeSideView2DTerrain), typeof(PWNodeTopDown2DTerrain),

			//Graph specific:
			typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput),

			//Biomes:
            typeof(PWNodeBiomeData), typeof(PWNodeBiomeBinder), typeof(PWNodeWaterLevel),
        	typeof(PWNodeBiomeBlender), typeof(PWNodeBiomeSwitch), typeof(PWNodeBiomeTemperature),
            typeof(PWNodeBiomeWetness), typeof(PWNodeBiomeSurface), typeof(PWNodeBiomeTerrain),
		};

        static PWNodeTypeProvider()
		{
            //bake fields
        }

        public static  IEnumerable< Type >  GetAllNodeTypes()
        {
            return allNodeTypes;
        }

        public static	IEnumerable< Type >	GetMainEditorNodeInfos()
		{
			return null;
		}
	}
}