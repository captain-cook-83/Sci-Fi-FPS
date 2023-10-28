Shader "Cc83/Planar Projected Shadows"
{
    Properties
    {
        _GroundHeight ("GroundHeight", Float) = 0.0
        _ShadowColor ("ShadowColor", Color) = (0, 0, 0, 1)
        _ShadowFalloff ("ShadowFalloff", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        LOD 100

        Pass
        {
//            Tags { "LightMode"="PlanarProjectedShadows" }
//            Stencil
//            {
//                Ref 0
//                Comp Equal
//                Pass IncrWrap
//                Fail Keep
//                ZFail Keep
//            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float _GroundHeight;
                half4 _ShadowColor;
                float _ShadowFalloff;
            CBUFFER_END
            
            float3 ShadowProjectPos(float4 vertPos)
	        {
		        float3 shadowPos;

                half3 lightDir = normalize(_MainLightPosition.xyz);
                float3 worldPos = TransformObjectToWorld(vertPos.xyz).xyz;
                
		        shadowPos.y = min(worldPos.y, _GroundHeight);
		        shadowPos.xz = worldPos.xz - lightDir.xz * max(0 , worldPos.y - _GroundHeight) / lightDir.y;

		        return shadowPos;
	        }

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;

                const float3 shadowPos = ShadowProjectPos(v.positionOS);
                o.positionCS = TransformWorldToHClip(shadowPos);
                o.color = _ShadowColor;

                //得到中心点世界坐标，计算阴影衰减
                // const float3 center = float3(unity_ObjectToWorld[0].w, _GroundHeight, unity_ObjectToWorld[2].w);        // unity_ObjectToWorld 矩阵每一行的第四个分量分别对应 Transform 的 xyz
                // const float falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff);
                // o.color.a *= falloff;
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
