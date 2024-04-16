Shader "Unlit/Sphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Pos ("Pos", Vector) = (.5, .5, .5)
        _Size ("Size", Vector) = (1., 1., 1.)
        _Rotation ("Rotation", Vector) = (0., 0., 0.)
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
            float3 _Pos;
            float3 _Rotation;

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

            float2x2 Rot(float a)
            {
                //rotation matrix
                float s = sin(a);
                float c = cos(a);
                return transpose(float2x2(c, -s, s, c));
            }

            float differenceSDF(float distA, float distB)
            {
                return max(distA, -distB);
            }

            float unionSDF(float distA, float distB)
            {
                return min(distA, distB);
            }

            float intersectionSDF(float distA, float distB)
            {
                return max(distA, distB);
            }

            /*float pointDist()
            {
                //calculate the distance between points
                float3 l = (_EndPos - _StartPos);


                //pythagoras 
                float endPyth = pow(_EndPos.x,2) + pow(_EndPos.y,2) + pow(_EndPos.z,2);
                float startPyth = pow(_StartPos.x,2) + pow(_StartPos.y,2) + pow(_StartPos.z,2);

                float pdot = dot(_EndPos, _StartPos);

                //rotation
                float theta = acos(pdot/(cross(sqrt(startPyth),sqrt(endPyth))));
                
                return theta;
            }
            */

            float dBox(float3 p)
            {
                p = abs(p)-_Size;
                return length(max(p, 0.))+min(max(p.x, max(p.y, p.z)), 0.);
            }
            

            float sceneSDF(float3 samplePoint)
            {
                //float pd = pointDist();
                float sphereDist = sphereSDF(samplePoint/1.2);

               // float3 forwardVector = normalize(_EndPos - _StartPos);

                /*float3 midpoint =
                (
                    (_EndPos+_StartPos)/2
                );
                */

                float3 bp = samplePoint;
                //set location
                //bp -= _StartPos;
                bp -= _Pos;

                //set rotation (based on pi)

                bp.yz = mul(bp.yz,Rot((float)_Rotation.x)); //x

                bp.xz = mul(bp.xz,Rot((float)_Rotation.y)); //y

                bp.xy = mul(bp.xy,Rot((float)_Rotation.z)); //z


                float rotate = dBox(bp);
                return differenceSDF(sphereDist, rotate);
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
