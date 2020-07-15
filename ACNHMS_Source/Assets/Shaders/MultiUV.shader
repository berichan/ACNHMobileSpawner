Shader "Unlit/MultiUV"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_DetailAlbedoMap ("Decal Texture", 2D) = "white" {}
		_DetailPower ("Detail Power", Range (0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent Cutout" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _DetailAlbedoMap;
            float4 _DetailAlbedoMap_ST;
			float _DetailPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _DetailAlbedoMap);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_DetailAlbedoMap, i.uv2) * (tex2D(_MainTex, i.uv) + _DetailPower);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
