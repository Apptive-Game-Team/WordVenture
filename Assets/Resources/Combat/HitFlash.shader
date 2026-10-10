// 스프라이트의 알파 모양만 남기고 색은 _FlashColor로 채운다. SpriteRenderer.color로는 스프라이트를 밝힐 수 없어서 쓴다.
// HitFlashVfx가 Resources/Combat/HitFlash.mat으로 불러 쓴다. 조명을 받지 않고 WebGL(GLES3)에서도 돈다.
Shader "Hidden/WordVenture/HitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        // 기본 sprite shader와 같이 선언해야 값이 1로 채워진다. 선언이 없으면 _Flip이 0이 되어 sprite가 한 점으로 줄어든다.
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnitySprites.cginc"

            struct FlashVertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct FlashFragmentInput
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _FlashColor;

            FlashFragmentInput vert (FlashVertexInput input)
            {
                FlashFragmentInput output;
                // SpriteRenderer.flipX/flipY는 shader에서 _Flip으로 뒤집는다. 기본 sprite shader와 같은 함수를 쓴다.
                output.vertex = UnityObjectToClipPos(UnityFlipSprite(input.vertex.xyz, _Flip));
                output.texcoord = input.texcoord;
                output.color = input.color;
                return output;
            }

            fixed4 frag (FlashFragmentInput input) : SV_Target
            {
                fixed4 texel = tex2D(_MainTex, input.texcoord);
                // 페이드는 SpriteRenderer.color의 알파(정점 색)로 조절한다.
                return float4(_FlashColor.rgb, texel.a * input.color.a);
            }
            ENDCG
        }
    }
}
