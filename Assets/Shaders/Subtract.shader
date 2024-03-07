Shader "Unlit/Subtract"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _startPos ("start", Vector) = (.25, .5, .5)
        _endPos ("end", Vector) = (.25, .5, .5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 0.001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.ro = _WorldSpaceCameraPos;//mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1));
                o.hitPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float sphereSDF(float3 p)
            {
                return length(p)-1.;
            }

            float cubeSDF(float3 p)
            {
                float3 d = abs(p)-float3(1., 1., 1.);
                float insideDistance = min(max(d.x, max(d.y, d.z)), 0.);
                float outsideDistance = length(max(d, 0.));
                return insideDistance+outsideDistance;
            }

            float intersectSDF(float distA, float distB)
            {
                return max(distA, distB);
            }

            float sceneSDF(float3 samplePoint)
            {
                float sphereDist = sphereSDF(samplePoint/1.2)*1.2;
                float cubeDist = cubeSDF(samplePoint);
                return intersectSDF(cubeDist, sphereDist);
            }

            float Raymarch(float3 ro, float3 rd)
            {
                float dO = 0;
                float dS;
                for(int i = 0; i < MAX_STEPS; i++)
                {
                    //float sphereDist = sphereSDF(samplePoint/1.2)*1.2;
                    float3 p = ro + dO * rd;
                    dS = sceneSDF(p);
                    dO += dS;
                    if (dS < SURF_DIST || dO > MAX_DIST) break;
                }

                return dO;
            }





            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv-.5;
                float3 ro = i.ro;
                float3 rd = normalize(i.hitPos - ro);

                float d = Raymarch(ro, rd);
                fixed4 col = 0;

                if(d<MAX_DIST)
                {
                    col.r = 1;
                }
                else
                {
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }
}
