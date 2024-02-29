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

float3 LightDirection; // As before, we expect this value to be prenormalized

// Represents how shiny our object is; this can be a uniform variable (constant) or on a per vertex basis
// Typical values for metals are 100-500, and mirrors theoretically have an infinite level of shininess (don't actually do that)
// Matte surfaces have values close to 0 or exactly 0
float Shininess;

// The color of the specular light the object reflects (this is what light is NOT absorbed, i.e. reflected, rather than light emitted)
float4 SpecularColor;

// This is how intense the specular light is, which can independently scale the specular component of light without affecting the shininess or the specular color
// It represents what proportion of incident light is actually reflected via the specular component
float SpecularIntensity;

// The camera position (in world space) is needed to determine how we're looking at the object
// We CAN calculate this here in the shader using the View matrix, but it's easier to do it once CPU side and then send it off to the GPU as a constant
float3 CameraPosition;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 WorldPosition : TEXCOORD0; // The choice to have this variable is partially documented in Ambient.fx; since Position will not be the interpolated (x,y,z,w) positional value, we need a separate output variable to keep that data around for us
	float4 Normal : TEXCOORD1;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	output.WorldPosition = mul(input.Position,World);
	float4 vpos = mul(output.WorldPosition,View);
	output.Position = mul(vpos,Projection);
	
	// We will not normalize the normal vector here since when we lerp, it will undo that work
	output.Normal = mul(input.Normal,NormalMatrix);
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
	// We need to calculate the reflection vector of either how we look at the point this fragment is at (with respect to its normal) or with how the light bounces off of it
	// This will allow us to calculate the angle between the light reflection and our eye, which influences the specular component
	float3 n = normalize(input.Normal.xyz); // Remember, the normal vector gets linearly interpolated, so we need to normalize it HERE, not in the vertex shader
	
	// Check if we are casting light onto a surface that faces the light
	// We need the light direction and normal to be facing toward each other, meaning their dot product is negative
	if(dot(n,-LightDirection) <= 0.0f)
		return saturate(AmbientColor * AmbientIntensity);
	
	// This is the reflection vector
	// Calculating this by hand, we would want 2 * dot(LightDirection,n) * n - LightDirection)
	// This projects LightDirection onto n and subtracts twice the projection vector to get us to the other side with the same angle (pointing the wrong way)
	// Then it multiplies everything by -1 so that the light vector points the same direction as the view vector
	// ...or we can just use the intrinsic reflect function of HLSL
	float3 r = reflect(LightDirection,n);
	
	// We provide the camera position in world space and have to obtain the vector that points toward it from wherever this fragment is
	float3 v = normalize(CameraPosition - input.WorldPosition.xyz);
	
	// To calculate the specular component, we multiply color by intensity and then take shininess into account
	// We multiply by the angle between r and v taken to the Shininess power
	// This will make objects VERY shiny when r and v are nearly parallel, and the shininess fades off as we go further away
	float4 specular = SpecularIntensity * SpecularColor * pow(max(dot(r,v),0.0f),Shininess);
	
	// Below are variations on how you might calculate the specular component
	// In this variation, we ensure that specular color cannot produce colors that are not available from our directional light
	//float4 specular = SpecularIntensity * clamp(SpecularColor,float4(0.0f,0.0f,0.0f,0.0f),LightColor * LightIntensity) * max(pow(dot(r,v),Shininess),0.0f);
	
	// In this variation, we include the diffuse color's magnitude; this allows us to make it so areas with high diffuse have higher specular and areas with lower diffuse have lower specular
	// Note that to use this, you must first calculate the diffuse lighting
	//float4 specular = SpecularIntensity * SpecularColor * max(pow(dot(r,v),Shininess),0.0f) * length(diffuse);
	
	return saturate(AmbientColor * AmbientIntensity + specular);
}

technique Specular
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}