Shader "Unlit/Test"
{
    Properties
    {
        _MainTex ("iChannel0", 2D) = "white" {}
        _SecondTex ("iChannel1", 2D) = "white" {}
        _ThirdTex ("iChannel2", 2D) = "white" {}
        _FourthTex ("iChannel3", 2D) = "white" {}
        _Mouse ("Mouse", Vector) = (0.5, 0.5, 0.5, 0.5)
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Built-in properties
            sampler2D _MainTex;   float4 _MainTex_TexelSize;
            sampler2D _SecondTex; float4 _SecondTex_TexelSize;
            sampler2D _ThirdTex;  float4 _ThirdTex_TexelSize;
            sampler2D _FourthTex; float4 _FourthTex_TexelSize;
            float4 _Mouse;
            float _GammaCorrect;
            float _Resolution;

            // GLSL Compatability macros
            #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
            #define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
            #define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
            #define iResolution float3(_Resolution, _Resolution, _Resolution)
            #define iFrame (floor(_Time.y / 60))
            #define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
            #define iDate float4(2020, 6, 18, 30)
            #define iSampleRate (44100)
            /*#define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)
                */

            // Global access to uv data
            static v2f vertex_output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

            static const int MAX_MARCHING_STEPS = 255;
            static const float MIN_DIST = 0.;
            static const float MAX_DIST = 100.;
            static const float EPSILON = 0.0001;
            float intersectSDF(float distA, float distB)
            {
                return max(distA, distB);
            }

            float unionSDF(float distA, float distB)
            {
                return min(distA, distB);
            }

            float differenceSDF(float distA, float distB)
            {
                return max(distA, -distB);
            }

            float cubeSDF(float3 p)
            {
                float3 d = abs(p)-float3(1., 1., 1.);
                float insideDistance = min(max(d.x, max(d.y, d.z)), 0.);
                float outsideDistance = length(max(d, 0.));
                return insideDistance+outsideDistance;
            }

            float sphereSDF(float3 p)
            {
                return length(p)-1.;
            }

            float sceneSDF(float3 samplePoint)
            {
                float sphereDist = sphereSDF(samplePoint/1.2)*1.2;
                float cubeDist = cubeSDF(samplePoint);
                return intersectSDF(cubeDist, sphereDist);
            }

            float shortestDistanceToSurface(float3 eye, float3 marchingDirection, float start, float end)
            {
                float depth = start;
                for (int i = 0;i<MAX_MARCHING_STEPS; i++)
                {
                    float dist = sceneSDF(eye+depth*marchingDirection);
                    if (dist<EPSILON)
                    {
                        return depth;
                    }
                    
                    depth += dist;
                    if (depth>=end)
                    {
                        return end;
                    }
                    
                }
                return end;
            }

            float3 rayDirection(float fieldOfView, float2 size, float2 fragCoord)
            {
                float2 xy = fragCoord-size/2.;
                float z = size.y/tan(radians(fieldOfView)/2.);
                return normalize(float3(xy, -z));
            }

            float3 estimateNormal(float3 p)
            {
                return normalize(float3(sceneSDF(float3(p.x+EPSILON, p.y, p.z))-sceneSDF(float3(p.x-EPSILON, p.y, p.z)), sceneSDF(float3(p.x, p.y+EPSILON, p.z))-sceneSDF(float3(p.x, p.y-EPSILON, p.z)), sceneSDF(float3(p.x, p.y, p.z+EPSILON))-sceneSDF(float3(p.x, p.y, p.z-EPSILON))));
            }

            float3 phongContribForLight(float3 k_d, float3 k_s, float alpha, float3 p, float3 eye, float3 lightPos, float3 lightIntensity)
            {
                float3 N = estimateNormal(p);
                float3 L = normalize(lightPos-p);
                float3 V = normalize(eye-p);
                float3 R = normalize(reflect(-L, N));
                float dotLN = dot(L, N);
                float dotRV = dot(R, V);
                if (dotLN<0.)
                {
                    return float3(0., 0., 0.);
                }
                
                if (dotRV<0.)
                {
                    return lightIntensity*(k_d*dotLN);
                }
                
                return lightIntensity*(k_d*dotLN+k_s*pow(dotRV, alpha));
            }

            float3 phongIllumination(float3 k_a, float3 k_d, float3 k_s, float alpha, float3 p, float3 eye)
            {
                const float3 ambientLight = 0.5*float3(1., 1., 1.);
                float3 color = ambientLight*k_a;
                //light POS to unity SUN POS
                float3 light1Pos = float3(4.*sin(_Time.y), 2., 4.*cos(_Time.y));
                float3 light1Intensity = float3(0.4, 0.4, 0.4);
                color += phongContribForLight(k_d, k_s, alpha, p, eye, light1Pos, light1Intensity);
                /*float3 light2Pos = float3(2.*sin(0.37*_Time.y), 2.*cos(0.37*_Time.y), 2.);
                float3 light2Intensity = float3(0.4, 0.4, 0.4);
                color += phongContribForLight(k_d, k_s, alpha, p, eye, light2Pos, light2Intensity);
                */
                return color;
            }

            float4x4 viewMatrix(float3 eye, float3 center, float3 up)
            {
                float3 f = normalize(center-eye);
                float3 s = normalize(cross(f, up));
                float3 u = cross(s, f);
                return transpose(float4x4(float4(s, 0.), float4(u, 0.), float4(-f, 0.), float4(0., 0., 0., 1)));
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float3 viewDir = rayDirection(45., iResolution.xy, fragCoord);
                float3 eye = float3(8., 5., 7.);
                float4x4 viewToWorld = viewMatrix(eye, float3(0., 0., 0.), float3(0., 1., 0.));
                float3 worldDir = (mul(viewToWorld,float4(viewDir, 0.))).xyz;
                float dist = shortestDistanceToSurface(eye, worldDir, MIN_DIST, MAX_DIST);
                if (dist>MAX_DIST-EPSILON)
                {
                    fragColor = float4(0., 0., 0., 0.);
                }
                
                float3 p = eye+dist*worldDir;
                float3 K_a = float3(0.2, 0.2, 0.2);
                float3 K_d = float3(0.7, 0.2, 0.2);
                float3 K_s = float3(1., 1., 1.);
                float shininess = 10.;
                float3 color = phongIllumination(K_a, K_d, K_s, shininess, p, eye);
                fragColor = float4(color, 1.);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}

