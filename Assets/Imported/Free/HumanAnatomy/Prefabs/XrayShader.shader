Shader "Custom/Xray"
{
    SubShader
    {
        Tags { "Queue" = "Transparent+1" }

        Pass { Blend Zero One }
    }
}
