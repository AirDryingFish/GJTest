Shader "MyShader/BlackScreen" {
Properties
{
_Color("Main Color", Color) = (1,1,1,1)
_Float1("改变颜色",Range(0,1)) = 1.0
_MainTex("Base (RGB)", 2D) = "white" {}
}
SubShader
{
Pass
{
CGPROGRAM
#pragma vertex vert_img         //vert_img在UnityCG.cginc里有预定义的函数体，是最普通的顶点处理函数  
#pragma fragment frag             
#include "UnityCG.cginc"  

fixed4 _Color;
sampler2D _MainTex;             //这两个变量一定要在CGPROGRAM中定义一次，不要看Properties里有就不写了  
float1 _Float1;

float4 frag(v2f_img i) : COLOR
{
float4 col = tex2D(_MainTex, i.uv) * _Float1;    //简单的两个float4点乘，刚好乘白色不变，乘纯黑就黑透，不一定是纯线性关系但表现效果没问题  
return col;
}
ENDCG
}
}
Fallback off
}
