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

// This one is going to require some explaination
// Suppose you have a normal vector n and a tanget vector t at the same position
// Naturally, dot(n,t) = 0, which you want to preserve
// Let W be the world matrix, then you want it to be the case that dot(Nn,Wt) = 0 as well, where N is the normal matrix
// It turns out that W = N is (almost always) not the case
// To get our result, we have dot(Nn,Wt) = T(Nn) Wt = 0 since this turns Nn into a row vector which calculates the dot product with the column vector Wt
// But then T(Nn) = T(n)T(N), so we have T(n)T(N)Wt = 0
// If we have N = T(W^-1), then T(N) = T(T(W^-1)) = W^-1, which leaves us with T(n) W^-1 W t = 0
// The W^-1 and W annihilate each other, so we have T(n) t = dot(n,t) = 0, which is what we wanted
// Thus the normal matrix is the transpose of the inverse of the world matrix
matrix NormalMatrix;

float4 AmbientColor;
float AmbientIntensity;

float3 DiffuseLightDirection; // This must be a unit vector for correct results (though you can embed DiffuseIntensity into the length of this if clever)
float4 DiffuseColor;
float DiffuseIntensity;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0; // We will need our vertex normals to calculate diffuse lighting; these should be provided with the model (and may not be actually normal to the primitives but rather an average normal for adjacent primitives for better results)
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0; // We'll be passing off a diffuse color to the pixel shader (better but slower lighting results would not bother with this)
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = mul(vpos,Projection);
	
	// Note that we are performing the difuse lighting calculation in the vertex shader
	// This is faster but of poorly quality than doing it per pixel (in exactly the same way) in the pixel shader
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Diffuse light scatters in all directions
	// To determine how much light we see, we need to determine how well aligned the surface we're drawing is with the light source
	// With a directional light, this is pretty easy, since the light direction is always the same
	// The only thing that changes is the normal vector for our vertex, so we need to calculate what that looks like in world space (which is where the light direction is)
	float4 normal = normalize(mul(input.Normal,NormalMatrix));
	
	// Diffuse light is most effective when the directional light is parallel, not effective at all when perpendicular, and nonsensical when anti-parallel
	// It scales with the cos of the angle between the light direction and the normal, which happens to be the dot product (assuming the normal and light direction are unit vectors)
	float diffusion_effectivity = dot(normal.xyz,DiffuseLightDirection);
	
	// The saturate function will clamp each component of the color to between 0 and 1
	output.Color = saturate(DiffuseColor * DiffuseIntensity * diffusion_effectivity);
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	return saturate(input.Color + AmbientColor * AmbientIntensity);
}

technique Diffuse
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}