Shader "Drawable/Utils/CustomRenderTexture Draw"
{
    Properties
    {
        MaskImage ("Mask Image", 2D) = "white" {}
        DrawColor("Draw Color", Color) = (1,0,0,1)
        DrawSensitivity("Draw Sensitivity", Float) = 1
        UVMap ("UVMap", 2D) = "black" {}
    }

    SubShader
    {
        Lighting Off

        Pass
        {
            Name "Update"
            CGPROGRAM
            
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            sampler2D MaskImage;
            fixed3 DrawColor;
            sampler2D UVMap;
            float DrawSensitivity;

            half4 frag(v2f_customrendertexture i) : SV_Target
            {
                half4 Prev = tex2D(_SelfTexture2D, i.globalTexcoord.xy);
                half4 AdvancedUV = tex2D(UVMap, i.globalTexcoord.xy);
                half4 Mask = tex2D(MaskImage, AdvancedUV.rg);
                return lerp(Prev, half4(DrawColor,1), Mask.r * DrawSensitivity);
            }
            ENDCG
        }
    }
}