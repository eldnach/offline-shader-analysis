Shader "TestShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct output
            {
                fixed4 col : SV_Target;
                float depth : SV_Depth;
            };
            
            sampler2D _MainTex;

            #pragma multi_compile _ REGISTER_USAGE 
            #pragma multi_compile _ STACK_ALLOCATIONS 
            #pragma multi_compile _ Z_WRITE

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }

            #define NUM 1028

            output frag (v2f i) 
            {
                output o;
                o.col = float4(1.0, 1.0, 1.0, 1.0);
                
                if (REGISTER_USAGE) {
                    float v = i.uv.x + 0.001f; 

                    float4 x0 = float4(v, v*1.1f, v*1.2f, v*1.3f);
                    float4 x1 = sin(x0) * v;
                    float4 x2 = exp(x1) + x0;
                    float4 x3 = log2(x2) * v;

                    // Each variable must remain live because it's needed for the final sum and for the calculation of the next variable
                    float4 x4 = x3 + x2;
                    float4 x5 = sin(x4) * v;
                    float4 x6 = exp(x5) + x3;
                    float4 x7 = log2(x6) * x4;

                    float4 x8 = x7 + x6;
                    float4 x9 = sin(x8) * v;
                    float4 x10 = exp(x9) + x7;
                    float4 x11 = log2(x10) * x8;
                    
                    float4 x12 = x11 + x10;
                    float4 x13 = sin(x12) * v;
                    float4 x14 = exp(x13) + x11;
                    float4 x15 = log2(x14) * x12;
                    
                    float4 x16 = x15 + x14;
                    float4 x17 = sin(x16) * v;
                    float4 x18 = exp(x17) + x15;
                    float4 x19 = log2(x18) * x16;
                    
                    float4 x20 = x19 + x18;
                    float4 x21 = sin(x20) * v;
                    float4 x22 = exp(x21) + x19;
                    float4 x23 = log2(x22) * x20;
                    
                    float4 x24 = x23 + x22;
                    float4 x25 = sin(x24) * v;
                    float4 x26 = exp(x25) + x23;
                    float4 x27 = log2(x26) * x24;

                    float4 x28 = x27 + x26;
                    float4 x29 = sin(x28) * v;
                    float4 x30 = exp(x29) + x27;
                    float4 x31 = log2(x30) * x28;

                    float4 x32 = x31 + x30;
                    float4 x33 = sin(x32) * v;
                    float4 x34 = exp(x33) + x31;
                    float4 x35 = log2(x34) * x32;

                    float4 x36 = x35 + x34;
                    float4 x37 = sin(x36) * v;
                    float4 x38 = exp(x37) + x35;
                    float4 x39 = log2(x38) * x36;
                    
                    float4 x40 = x39 + x38;
                    float4 x41 = sin(x40) * v;
                    float4 x42 = exp(x41) + x39;
                    float4 x43 = log2(x42) * x40;
                    
                    float4 x44 = x43 + x42;
                    float4 x45 = sin(x44) * v;
                    float4 x46 = exp(x45) + x43;
                    float4 x47 = log2(x46) * x44;
                    
                    float4 x48 = x47 + x46;
                    float4 x49 = sin(x48) * v;
                    float4 x50 = exp(x49) + x47;
                    float4 x51 = log2(x50) * x48;
                    
                    float4 x52 = x51 + x50;
                    float4 x53 = sin(x52) * v;
                    float4 x54 = exp(x53) + x51;
                    float4 x55 = log2(x54) * x52;
                    
                    float4 x56 = x55 + x54;
                    float4 x57 = sin(x56) * v;
                    float4 x58 = exp(x57) + x55;
                    float4 x59 = log2(x58) * x56;
                    
                    float4 x60 = x59 + x58;
                    float4 x61 = sin(x60) * v;
                    float4 x62 = exp(x61) + x59;
                    float4 x63 = log2(x62) * x60;

                    float final_sum = 0.0f;
                    final_sum += x0.x + x1.x + x2.x + x3.x; 
                    final_sum += x4.x + x5.x + x6.x + x7.x;
                    final_sum += x8.x + x9.x + x10.x + x11.x;
                    final_sum += x12.x + x13.x + x14.x + x15.x;
                    final_sum += x16.x + x17.x + x18.x + x19.x;
                    final_sum += x20.x + x21.x + x22.x + x23.x;
                    final_sum += x24.x + x25.x + x26.x + x27.x;
                    final_sum += x28.x + x29.x + x30.x + x31.x;
                    final_sum += x32.x + x33.x + x34.x + x35.x;
                    final_sum += x36.x + x37.x + x38.x + x39.x;
                    final_sum += x40.x + x41.x + x42.x + x43.x;
                    final_sum += x44.x + x45.x + x46.x + x47.x;
                    final_sum += x48.x + x49.x + x50.x + x51.x;
                    final_sum += x52.x + x53.x + x54.x + x55.x;
                    final_sum += x56.x + x57.x + x58.x + x59.x;
                    final_sum += x60.x + x61.x + x62.x + x63.x;

                    // Add a texture sample to prevent the compiler from caching the entire result
                    float4 sample = tex2D(_MainTex, i.uv);
                    final_sum += sample.x * 0.00;

                    o.col = float4(final_sum, final_sum, final_sum, 1.0f);

                }
                
                if (STACK_ALLOCATIONS) {

                    int index = int(NUM % 2);
                    index = (i.uv.x * NUM) % NUM; 
            
                    float arr[NUM];   
                    arr[0] = 0.0;
                    arr[NUM-1] = 1.0;
                    arr[index] = 0.5;

                    o.col += float4( arr[0], arr[NUM-1], arr[index], 1.0);

                } 
                
                if (Z_WRITE) {

                    o.depth = float4(1.0, 1.0, 1.0, 1.0);

                } 

                return o;
            }
            ENDCG
        }
    }
}