Shader "Custom/VertexColorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 barycentric; // 添加重心坐标信息
            float4 color0 : COLOR; // 顶点颜色信息
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // 使用重心坐标插值顶点颜色
            float3 vertexColor = IN.barycentric.x * IN.color0.rgb +
                                 IN.barycentric.y * IN.color0.rgb +
                                 IN.barycentric.z * IN.color0.rgb;
            
            o.Albedo = vertexColor; // 使用插值后的颜色作为表面颜色
        }

        void vert(inout appdata_full v)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            o.worldPos = mul(unity_ObjectToWorld, v.vertex); // 获取顶点世界坐标位置
            o.barycentric = v.texcoord; // 使用顶点的纹理坐标作为重心坐标（简化处理）
            o.color0 = v.color; // 获取顶点颜色
        }
        ENDCG
    }
    FallBack "Diffuse"
}
