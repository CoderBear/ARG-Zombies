
Shader "Puppet2D/BlackAndWhite"
{
   Properties {
  	  _Color ("Tint Color", Color) = (0.5,0.5,0.5,1.0)
      _MainTex ("Texture Image", 2D) = "white" {} 
    
   }
   SubShader {
   Tags { "Queue"="Transparent" }
   Blend SrcAlpha OneMinusSrcAlpha
      Pass { 
      
		
         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
         #include "UnityCG.cginc"
 
         uniform sampler2D _MainTex;    
         uniform float4 _MainTex_ST; 

        
 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
            float4 vertColor : COLOR;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.uv = TRANSFORM_TEX(input.texcoord.xy,_MainTex);
           
            output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
            
            
            return output;
         }
 
         fixed4 frag(vertexOutput input) : COLOR
         {

            
            fixed4 c = tex2D(_MainTex,input.uv);

            return float4(float3(1),c.a); 

         }
 
         ENDCG
      }
   }

}