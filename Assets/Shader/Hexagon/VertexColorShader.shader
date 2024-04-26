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
            float3 barycentric; // �������������Ϣ
            float4 color0 : COLOR; // ������ɫ��Ϣ
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // ʹ�����������ֵ������ɫ
            float3 vertexColor = IN.barycentric.x * IN.color0.rgb +
                                 IN.barycentric.y * IN.color0.rgb +
                                 IN.barycentric.z * IN.color0.rgb;
            
            o.Albedo = vertexColor; // ʹ�ò�ֵ�����ɫ��Ϊ������ɫ
        }

        void vert(inout appdata_full v)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            o.worldPos = mul(unity_ObjectToWorld, v.vertex); // ��ȡ������������λ��
            o.barycentric = v.texcoord; // ʹ�ö��������������Ϊ�������꣨�򻯴���
            o.color0 = v.color; // ��ȡ������ɫ
        }
        ENDCG
    }
    FallBack "Diffuse"
}
