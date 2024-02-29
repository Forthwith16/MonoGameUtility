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

// We need to know what texture to read from to draw a texture
// This value here specifies where to read the texture from the GPU
// It is a pointer to a texture buffer somewhere
texture ModelTexture;

// This describes how we sample from our texture
sampler2D TextureSampler = sampler_state
{
	Texture = (ModelTexture); // This tells the sampler what texture to sample from (the parentheses ARE necessary)
	MagFilter = Linear; // This tells the sampler how to magnify the texture when necessary
	MinFilter = Linear; // This tells the sampler how to shrink the texture when necessary
	AddressU = Clamp; // This tells the sampler how to wrap out of bounds u coordintes
	AddressV = Clamp; // This tells the sampler how to wrap out of bounds v coordintes
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	
	// Texture coordiantes are typically referred to as UV coordinates to distinguish them from the usual 3D axes xyz (and w, which is almost always ignored)
	// These are indices into a (usually 2D) texture (when 3D, there is a third coordinate)
	// They are floats ranging from 0 to 1 (both inclusive)
	// A value of 0 <= u <= 1 indexes into u% of the width of the texture (v, v%, height)
	// The way it samples from the texture (because it is almost never a single pixel we want to sample from) depends on the sampler settings
	// Usually, we perform a linear interpolation between the four pixels around a sample location
	// If we index out of bounds (i.e. < 0 or > 1), then how the GPU deals with that depends on the wrap settings
	// You can perform a linear wrap (i.e. modulo 1), you can clamp so that you don't overrun the bounds, and there are a few other options
	// Here, we are using a single 2D texture and need only one set of UV coordinates per vertex (which are supplied with the model)
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = mul(vpos,Projection);
	
	output.TextureCoordinate = input.TextureCoordinate;
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	float4 tcolor = tex2D(TextureSampler,input.TextureCoordinate);
	tcolor.a = 1.0f; // Be careful to get the right alpha value for this application
	
	return tcolor; // Setting the color directly like this from the texture could be thought of as emission light (light that the model itself emits as a light source [but usually not a light source that we calculating into other models' lighting])
}

technique Texture
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}