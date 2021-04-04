// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/gridshader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Amount ("Amount",Range(0,1))=1
		_GridType("GridType",Range(1,2))=1
		_GridColor("GridColor",Color)=(0,0,0,1)

		_Xstep("Xstep",Range(1,10))=1
		_Ystep("Ystep",Range(1,10))=1

		_Xoffset("Xoffset",Range(0,10))=0
		_Yoffset("Yoffset",Range(0,10))=0

		_Xmin1("Xmin",Range(0,1))=0
		_Xmax1("Xmax",Range(0,1))=1

		_Ymin1("Ymin",Range(0,1))=0
		_Ymax1("Ymax",Range(0,1))=1

		_Flashing("Flashing",Range(0,1))=0
		_Flashcount("Flashcount",Range(0,1))=0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);


				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Amount;

			uint _GridType;

			fixed4 _GridColor;

			uint _Xstep;
			uint _Ystep;

			uint _Xoffset;
			uint _Yoffset;

			float _Xmin;
			float _Xmax;
			float _Ymin;
			float _Ymax;

			uint _Flashing;
			uint _Flashcount;


			float amo2;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				uint yzure=0;

				float jjj=i.uv.x;
				float bbb=i.uv.y;
				fixed4 gridc=_GridColor;


				if(_Xmin<jjj && _Xmax>jjj && _Ymin<bbb && _Ymax>bbb)
				{
					jjj=jjj*_ScreenParams.xy.x;


					int jj2=int(jjj);
					jj2+=_Xoffset;

					bbb=bbb*_ScreenParams.xy.y;

					int bb2=int(bbb);
					bb2+=_Yoffset;

					amo2=_Amount;


					if(jj2%_Xstep==0 )
					{
						if(_Xstep>1)
						{
							fixed3 vec33=fixed3((gridc.x-col.r)*amo2,(gridc.y-col.g)*amo2,(gridc.z-col.b)*amo2);
							col=fixed4(col.r+vec33.x,col.g+vec33.y,col.b+vec33.z,1);
						}
					}

					if(_GridType==2 && _Xstep>1 && _Ystep>1)
					{
						if((jj2%(_Xstep*2))>_Xstep)yzure=int((_Ystep)/2);
						bb2+=yzure;
					}

					if(bb2%_Ystep==0 )
					{
						if(_Ystep>1)
						{
							fixed3 vec33=fixed3((gridc.x-col.r)*amo2,(gridc.y-col.g)*amo2,(gridc.z-col.b)*amo2);
							col=fixed4(col.r+vec33.x,col.g+vec33.y,col.b+vec33.z,1);
						}
					}
				}
				if(_Flashcount==1)
				{
					col=fixed4(col.r*0.95,col.g*0.95,col.b*0.95,1);
				}
				return col;
			}
			ENDCG
		}
	}
}
