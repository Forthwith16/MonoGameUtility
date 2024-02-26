#if OPENGL
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

float3 DiffuseLightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Normal : TEXCOORD0; // It doesn't really matter what semantic you use (which is why glsl is better) so long as you match their SIZE NOT TYPE: https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = mul(vpos,Projection);
	
	// We will not normalize the normal vector here since when we lerp, it will undo that work
	output.Normal = mul(input.Normal,NormalMatrix);
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float diffusion_effectivity = dot(normalize(input.Normal).xyz,DiffuseLightDirection);
	return saturate(DiffuseColor * DiffuseIntensity * diffusion_effectivity + AmbientColor * AmbientIntensity);;
}

technique PixelDiffuse
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}