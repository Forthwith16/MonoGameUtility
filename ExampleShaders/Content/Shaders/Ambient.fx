// Don't worry about these for now
// The meaning of these constants will be dealt with later
#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// In HLSL, you must declate variables before you use them in a file (just like C)
// Typically, we declare ALL of our variables and types first and then write our functions after
// We will use five UNIFORM variables: the MVP matrices, the ambient light color, and the ambient light intensity
// These variables are uniform in the sense that every vertex drawn with this shader (per vertex batch, i.e. model mesh) has the same value for these
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// The MVP matrices can be combined ahead of time into one matrix on the CPU for, in some cases, greater efficiency
// We will not do so here, and there are reasons not to as well as we will see in future shaders
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Note: matrix is syntactic sugar for matrix<float,4,4>
// You can also use float4x4
matrix World; // The model matrix that gets us from local space to world space
matrix View; // The camera matrix that gets us from world space to view space
matrix Projection; // The projection matrix that gets us from view space to screen space

float4 AmbientColor;
float AmbientIntensity;

// Next we will create the input to the vertex shader
// Every vertex is given this input, which we use to transform into an output to the pixel shader
struct VertexShaderInput
{
	// POSITION0 is a SEMANTIC (i.e. keyword) that describes the vertex position in local (object) space
	// In this case, it is an input semantic
	// All variables used to store data before, between, or after shaders must be identified by a semantic
	// A full list of them is available here: msdn.microsoft.com/en-us/library/bb509647(VS.85).aspx
	float4 Position : POSITION0;
};

// Next we define the output to our vertex shader
// In this case, it will be identical in appearance to the input, but the meaning of Position will be different
struct VertexShaderOutput
{
	float4 Position : POSITION0;
};

// This will be the main function for our vertex shader
// You can call it anything you want, but a name that emphasizes this is usually a good idea
VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	// We could multiply the input position by all three matrices at once, but let's do it step by step
	// We will want the position in view space for lighting calculations (or other spaces for other calculations) in future shaders anyway
	float4 wpos = mul(input.Position,World);
	float4 vpos = mul(wpos,View);
	output.Position = mul(vpos,Projection);
	
	return output;
}

// This will be the main function for our pixel (fragment) shader
// Like with the vertex shader, you can call this whatever you want, but usually you want to pick a suitable name
// The extra : COLOR0 allows us to return the color directly rather than defining a struct with the color singleton inside of it
float4 MainPS(VertexShaderOutput input) : COLOR0
{
	// Because we are only doing ambient lighting, the pixel shader is nearly trivial
	return AmbientColor * AmbientIntensity;
}

// Lastly, we need to tell the compiler how to use what we've written
// The technique definition below tells the compiler what the entry points are for the shaders we want to utilize
// We may also want to have multiple passes over our data
// You might want to do this if, for example, you have more lights than a shader can support as uniform input in a single pass
// Multiple passes will simply add results from each pass together according to a specified blend function and blend mode (typically for lighting you would use ADD)
// In general, however, it's best to stick to a single pass whenever possible as adding more passes complicates everything
technique Ambient
{
	// You can call your pass whatever you want, but in general, it's hard to go wrong with Pass# or some other descriptive name
	pass Pass1
	{
		// The keyword compile tells the compiler to compile the code into something the GPU will understand
		// The mysterious VS_SHADERMODEL tells the compiler what version of the shader language we want to use
		// There are a lot of different ones available, but not all GPUs will support all shader language versions
		// In general, it's best to use the lowest level you can get away with without ruining your shader implementation
		// However, you may also want to create multiple versions of a shader so that people running your game on a potato CAN and people running with cutting edge hardware get all the cool bells and whistles
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}