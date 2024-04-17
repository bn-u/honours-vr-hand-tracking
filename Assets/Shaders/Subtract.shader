Shader "Unlit/Subtract"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StartPos ("Start", Vector) = (.5, .5, .5)
        _EndPos ("End", Vector) = (0., 0., 0.)
        _Size ("Size", Vector) = (5, 0.1, 5)
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

            float3 _Size;
            float3 _StartPos;
            float3 _EndPos;

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
                return length(p)-0.8;
            }

            float differenceSDF(float distA, float distB)
            {
                return max(distA, -distB);
            }

            float unionSDF(float distA, float distB)
            {
                return min(distA, distB);
            }

            float dBox(float3 p)
            {
                p = abs(p)-_Size;
                return length(max(p, 0.))+min(max(p.x, max(p.y, p.z)), 0.);
            }

            float4x4 lookat(float3 pointA,float3 pointB)
            {
                float3 direction = normalize(pointA - pointB);

                float3 xAxis = normalize(cross(float3(0, 1, 0), direction));
                float3 yAxis = normalize(cross(direction, xAxis));
                float3 zAxis = direction;

                float4x4 rotationMatrix = {
                    xAxis.x, yAxis.x, zAxis.x, 0,
                    xAxis.y, yAxis.y, zAxis.y, 0,
                    xAxis.z, yAxis.z, zAxis.z, 0,
                    0,       0,       0,       1
                };

                return rotationMatrix;
            }


            float sceneSDF(float3 samplePoint)
            {
                float sphereDist = sphereSDF(samplePoint/1.2);

                float3 midpoint =
                (
                    (_EndPos+_StartPos)/2
                );

                float3 bp = samplePoint;
                bp -= midpoint;

                bp.xyz = mul(bp.xyz, lookat(_StartPos, midpoint));

                float rotate = dBox(bp);
                return differenceSDF(sphereDist, rotate);
            }

            float Raymarch(float3 ro, float3 rd)
            {
                float dO = 0;
                float dS;
                for(int i = 0; i < MAX_STEPS; i++)
                {
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
                    float3 p = ro + rd * d;
                    col.rgb = p;
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
