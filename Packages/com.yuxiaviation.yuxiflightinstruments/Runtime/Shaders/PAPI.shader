// PAPI light Shader

Shader "Unlit/PAPI"
{
    Properties
    {
        //half4 _Color = (1,1,1,1)
        //half4 _CorrectColor = (1, 1, 1, 1)
        //half4 _IncorrectColor = (1, 0, 0, 1)
        //float _MinAngle = 2.5
        //float3 _WorldUp = (0, 1, 0)
        _Color("Color", Color) = (1,1,1,1)

        [HDR]_CorrectColor("Down Color", Color) = (1,0,0,1)

        [HDR]_IncorrectColor("Up Color", Color) = (1,1,1,1)

        //_AngelOffset("AngelOffset", Float) = 0
        _AngleThreshold("Minimum Angle", Float) = 2.5
        _WorldUp("World Up",Vector) = (0,1,0)
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma fragmentoption ARB_precision_hint_fastest

        struct Input
        {
            float3 worldNormal;
            float2 uv_MainTex;
            float3 viewDir;
        };

        sampler2D _MainTex;

        half4 _Color;
        half4 _CorrectColor;
        
        half4 _IncorrectColor;
        
        float _AngleThreshold;
        float3 _WorldUp;


        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            const float RadToDeg = 180.0 / UNITY_PI;

            // Calculate the angle between the viewing direction and the normal of the PAPI light's surface
            //float3 localLeft = mul(unity_ObjectToWorld, float4(0, 0, 1, 0)).xyz;
            //float angle = RadToDeg * acos(dot(IN.viewDir, normalize(cross(IN.worldNormal, localLeft))));
            //上面这种算法在场景视图里好使，但是playmode就不行，怪

            float3 LocalNormal = mul(unity_WorldToObject, float4(IN.worldNormal, 0));
            float3 WorldNormal = mul(unity_ObjectToWorld, float4(LocalNormal, 0)).xyz;
            float3 LocalLeft = normalize(cross(WorldNormal, _WorldUp));
            float angle = RadToDeg * acos(dot(cross(WorldNormal, LocalLeft), IN.viewDir));
            // If the angle is within the correct range, set the color to the correct color
            if (angle >= _AngleThreshold)
            {
                o.Albedo = _CorrectColor;
                o.Emission = _CorrectColor.rgb;
            }
            // Otherwise, set the color to a different value to indicate that the viewing angle is incorrect
            else
            {
                o.Albedo = _IncorrectColor;
                o.Emission = _IncorrectColor.rgb;
            }
        }

        ENDCG
    }
}
