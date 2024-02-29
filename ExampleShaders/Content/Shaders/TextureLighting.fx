#if OPENGL
	#define SV_POSITION POSITION
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

float3 CameraPosition;

texture ModelTexture;

sampler2D TextureSampler = sampler_state
{
	Texture = (ModelTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 WorldPosition : TEXCOORD0;
	float4 Normal : TEXCOORD1;
	float2 TextureCoordinate : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = output.WorldPosition = mul(vpos,Projection);
	
	output.Normal = mul(input.Normal,NormalMatrix);
	
	output.TextureCoordinate = input.TextureCoordinate;
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float3 n = normalize(input.Normal.xyz);
	
	float diffusion_effectivity = max(dot(n,-LightDirection),0.0f);
	float4 diffuse = LightColor * LightIntensity * diffusion_effectivity;
	
	float3 r = reflect(LightDirection,n);
	float3 v = normalize(CameraPosition - input.WorldPosition.xyz);
	float4 specular = SpecularIntensity * SpecularColor * pow(max(dot(r,v),0.0f),Shininess);

	// How you choose to combine texture colors with lighting data is a matter of preference and a matter of how you're modeling lighting
	// You can choose to combine it with just the diffuse color since that's what scattering light for people to see
	// You might combine it with all light sources, since the model is ultimately reflecting light it does not absorb
	// You might go for something more exotic, too
	// We'll just blend the color in with the diffuse color for this example
	float4 tcolor = AmbientColor * AmbientIntensity + diffuse * tex2D(TextureSampler,input.TextureCoordinate) + specular;
	tcolor.a = 1.0f;
	
	return saturate(tcolor);
}

technique Phong
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}