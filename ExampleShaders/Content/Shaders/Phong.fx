﻿#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

matrix NormalMatrix;

float4 AmbientColor;
float AmbientIntensity;

float3 LightDirection;
float4 LightColor;
float LightIntensity;

float Shininess;
float4 SpecularColor;
float SpecularIntensity;

float3 ViewVector;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Normal : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = mul(vpos,Projection);
	
	output.Normal = mul(input.Normal,NormalMatrix);
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	// With phong lighting, we would generally provided an ambient, diffuse, and specular light color for a directional light
	// We would also provide a material color for each of these light components and multiply them into the calculated color
	// For example, a diffuse material of (0,1,0) will reflect 100% of incident green light and 0% of incident red or blue light
	// In this example, however, we do not concern ourselves with materials and focus on the lighting
	// In effect, all materials are (1,1,1)
	float3 n = normalize(input.Normal.xyz);
	
	if(dot(n,LightDirection) <= 0.0f)
		return saturate(AmbientColor * AmbientIntensity);
	
	float diffusion_effectivity = dot(n,LightDirection);
	float diffuse = LightColor * LightIntensity * diffusion_effectivity;
	
	float3 r = reflect(LightDirection,n);
	float3 v = normalize(mul(float4(ViewVector,1.0f),World).xyz);
	float4 specular = SpecularIntensity * SpecularColor * max(pow(dot(r,v),Shininess),0.0f);

	return saturate(AmbientColor * AmbientIntensity + diffuse + specular);
}

technique Phong
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}