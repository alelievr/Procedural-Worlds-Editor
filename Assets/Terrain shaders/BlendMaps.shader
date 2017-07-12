Shader "Custom/Sample2DArrayTexture"
{
    Properties
    {
        _InputTextures ("Tex", 2DArray) = "" {}
        _BlendMaps ("BlendMaps", 2DArray) = "" {}
        _SliceRange ("Slices", Range(0,32)) = 6
        _ShowBlendMap ("Show blend map", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #define MAX_TEX_NUMBER      4 //max number of managed input textures ( / 4) so 8 will be 32 max in textures

            #pragma vertex vert
            #pragma fragment frag
            // to use texture arrays we need to target DX10/OpenGLES3 which
            // is shader model 3.5 minimum
            #pragma target 3.5
            
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _SliceRange;
            float _ShowBlendMap;

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, vertex);
                o.uv.xy = vertex.xy + .5;
                return o;
            }
            
            UNITY_DECLARE_TEX2DARRAY(_InputTextures);
            UNITY_DECLARE_TEX2DARRAY(_BlendMaps);

            half4 frag (v2f i) : SV_Target
            {
                half4       col = half4(0, 0, 0, 0);

                if (_SliceRange > 0)
                    return UNITY_SAMPLE_TEX2DARRAY(_InputTextures, float3(i.uv, _SliceRange));

                for (int j = 0; j < MAX_TEX_NUMBER; j += 4) //max of 32 textures for Topdown Tilemap biomes
            	{
                    float4 b = UNITY_SAMPLE_TEX2DARRAY(_BlendMaps, float3(i.uv, j));

                    if (_ShowBlendMap > 0)
                    {
                        if (length(b) == 0)
                            b = float4(1, 1, 1, 1);
						col += b;
                    }
                    else
                    {
                        col += UNITY_SAMPLE_TEX2DARRAY(_InputTextures, float3(i.uv, j + 0)) * b.r;
                        col += UNITY_SAMPLE_TEX2DARRAY(_InputTextures, float3(i.uv, j + 1)) * b.g;
                        col += UNITY_SAMPLE_TEX2DARRAY(_InputTextures, float3(i.uv, j + 2)) * b.b;
                        col += UNITY_SAMPLE_TEX2DARRAY(_InputTextures, float3(i.uv, j + 3)) * b.a;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}

