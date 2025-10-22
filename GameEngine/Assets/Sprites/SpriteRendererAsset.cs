using GameEngine.Assets.Serialization;
using GameEngine.Resources;
using GameEngine.Resources.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Assets.Sprites
{
	/// <summary>
	/// The details of a SpriteRenderer laid bare.
	/// </summary>
	[AssetLoader(typeof(SpriteRendererAsset),nameof(FromFile))]
	public partial class SpriteRendererAsset
	{
		protected override void Serialize(string path, string root, bool overwrite_dependencies = false)
		{
			// We have no way of decompiling a shader, so we have to try to copy/paste it if possible
			if(StandardShouldSerializeCheck(ShaderSource,Path.GetDirectoryName(path) ?? "",overwrite_dependencies,out string? dst) && ShaderSource.Resource!.Name != "") // StandardShouldSerializeCheck does the null check on ShaderSource.ConcreteAsset for us
			{
				string src = Path.GetFullPath(Path.Combine(root,ShaderSource.Resource.Name));

				// We have no reason to perform identity copy/pastes
				if(src != dst)
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(dst) ?? "");
						File.Copy(src,dst);
					}
					catch
					{} // If something goes wrong, we don't want to crash horribly
			}

			// Now we can serialize our sprite sheet proper
			this.SerializeJson(path);

			return;
		}

		protected override void AdjustFilePaths(string path, string root)
		{
			StandardAssetSourcePathAdjustment(ShaderSource,path,ShaderSource.Unnamed ? GenerateFreeFile(Path.Combine(root,path),".fx") : "");
			return;
		}

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		protected static SpriteRendererAsset? FromFile(string path) => path.DeserializeJsonFile<SpriteRendererAsset>();

		protected override IResource? Instantiate(Linker link)
		{
			SpriteRenderer ret = new SpriteRenderer(link.Graphics);

			ret.Shader = ShaderSource.Resource;
			ret.Order = Order;

			switch(Blend)
			{
			case BasicBlendState.Additive:
				ret.Blend = BlendState.Additive;
				break;
			case BasicBlendState.AlphaBlend:
				ret.Blend = BlendState.AlphaBlend;
				break;
			case BasicBlendState.NonPremultiplied:
				ret.Blend = BlendState.NonPremultiplied;
				break;
			case BasicBlendState.Opaque:
				ret.Blend = BlendState.Opaque;
				break;
			}

			switch(Wrap)
			{
			case BasicSamplerState.PointClamp:
				ret.Wrap = SamplerState.PointClamp;
				break;
			case BasicSamplerState.PointWrap:
				ret.Wrap = SamplerState.PointWrap;
				break;
			case BasicSamplerState.LinearClamp:
				ret.Wrap = SamplerState.LinearClamp;
				break;
			case BasicSamplerState.LinearWrap:
				ret.Wrap = SamplerState.LinearWrap;
				break;
			case BasicSamplerState.AnisotropicClamp:
				ret.Wrap = SamplerState.AnisotropicClamp;
				break;
			case BasicSamplerState.AnisotropicWrap:
				ret.Wrap = SamplerState.AnisotropicWrap;
				break;
			}

			switch(DepthStencil)
			{
			case BasicDepthStencilState.Default:
				ret.DepthStencil = DepthStencilState.Default;
				break;
			case BasicDepthStencilState.DepthRead:
				ret.DepthStencil = DepthStencilState.DepthRead;
				break;
			case BasicDepthStencilState.None:
				ret.DepthStencil = DepthStencilState.None;
				break;
			}

			switch(Cull)
			{
			case BasicRasterizerState.CullNone:
				ret.Cull = RasterizerState.CullNone;
				break;
			case BasicRasterizerState.CullClockwise:
				ret.Cull = RasterizerState.CullClockwise;
				break;
			case BasicRasterizerState.CullCounterClockwise:
				ret.Cull = RasterizerState.CullCounterClockwise;
				break;
			}

			return ret;
		}
	}
	
	[JsonConverter(typeof(JsonSpriteRendererAssetConvereter))]
	public partial class SpriteRendererAsset : AssetBase
	{
		/// <summary>
		/// The default constructor.
		/// It sets every value to its default.
		/// </summary>
		public SpriteRendererAsset() : base()
		{
			Order = SpriteSortMode.BackToFront;
			Blend = BasicBlendState.AlphaBlend;
			Wrap = BasicSamplerState.LinearClamp;
			DepthStencil = BasicDepthStencilState.None;
			Cull = BasicRasterizerState.CullCounterClockwise;
			ShaderSource = new AssetSource<Effect>();

			return;
		}

		/// <summary>
		/// Creates a sprite renderer asset from <paramref name="sr"/>.
		/// </summary>
		/// <param name="sr">The sprite renderer to turn into its asset form.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="sr"/> has a custom Blend, Wrap, DepthStencil, or Cull. This cannot accommodate custom values for these types.</exception>
		protected internal SpriteRendererAsset(SpriteRenderer sr) : base()
		{
			// We can directly copy the sort order
			Order = sr.Order;

			// We can also directly copy the shader source name if we have it
			ShaderSource = new AssetSource<Effect>(sr.Shader);

			// Now we need to do the hard parts
			// Find out if we have a standard blend state
			if(sr.Blend == BlendState.Additive)
				Blend = BasicBlendState.Additive;
			else if(sr.Blend == BlendState.AlphaBlend)
				Blend = BasicBlendState.AlphaBlend;
			else if(sr.Blend == BlendState.NonPremultiplied)
				Blend = BasicBlendState.NonPremultiplied;
			else if(sr.Blend == BlendState.Opaque)
				Blend = BasicBlendState.Opaque;
			else
				throw new ArgumentException("Nonstandard blend state");

			// Find out if we have a standard wrap state
			if(sr.Wrap == SamplerState.PointClamp)
				Wrap = BasicSamplerState.PointClamp;
			else if(sr.Wrap == SamplerState.PointWrap)
				Wrap = BasicSamplerState.PointWrap;
			else if(sr.Wrap == SamplerState.LinearClamp)
				Wrap = BasicSamplerState.LinearClamp;
			else if(sr.Wrap == SamplerState.LinearWrap)
				Wrap = BasicSamplerState.LinearWrap;
			else if(sr.Wrap == SamplerState.AnisotropicClamp)
				Wrap = BasicSamplerState.AnisotropicClamp;
			else if(sr.Wrap == SamplerState.AnisotropicWrap)
				Wrap = BasicSamplerState.AnisotropicWrap;
			else
				throw new ArgumentException("Nonstandard sampler state");

			// Find out if we have a standard depth stencil
			if(sr.DepthStencil == DepthStencilState.Default)
				DepthStencil = BasicDepthStencilState.Default;
			else if(sr.DepthStencil == DepthStencilState.DepthRead)
				DepthStencil = BasicDepthStencilState.DepthRead;
			else if(sr.DepthStencil == DepthStencilState.None)
				DepthStencil = BasicDepthStencilState.None;
			else
				throw new ArgumentException("Nonstandard depth stencil state");
			
			// Find out if we have a standard cull
			if(sr.Cull == RasterizerState.CullClockwise)
				Cull = BasicRasterizerState.CullClockwise;
			else if(sr.Cull == RasterizerState.CullCounterClockwise)
				Cull = BasicRasterizerState.CullCounterClockwise;
			else if(sr.Cull == RasterizerState.CullNone)
				Cull = BasicRasterizerState.CullNone;
			else
				throw new ArgumentException("Nonstandard cull state");

				return;
		}

		/// <summary>
		/// The order that sprites are sorted when drawn.
		/// <para/>
		/// This value defaults to BackToFront.
		/// </summary>
		public SpriteSortMode Order;

		/// <summary>
		/// A blend mode to draw with.
		/// <para/>
		/// This value defaults to AlphaBlend.
		/// </summary>
		public BasicBlendState Blend;

		/// <summary>
		/// A sampler wrap mode to draw with.
		/// <para/>
		/// This value defaults to LinearClamp.
		/// </summary>
		public BasicSamplerState Wrap;

		/// <summary>
		/// The manner of depth stencil to draw with.
		/// <para/>
		/// This value defaults to None.
		/// </summary>
		public BasicDepthStencilState DepthStencil;

		/// <summary>
		/// The cull state used when drawing.
		/// <para/>
		/// This value defaults to CullCounterClockwise.
		/// </summary>
		public BasicRasterizerState Cull;

		/// <summary>
		/// A shader to draw with.
		/// This path must be relative to the renderer source file.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite Effect).
		/// </summary>
		public AssetSource<Effect> ShaderSource;
	}

	/// <summary>
	/// Represents the basic blend states that can be serialized.
	/// </summary>
	public enum BasicBlendState
	{
		Null,
		Additive,
		AlphaBlend,
		NonPremultiplied,
		Opaque
	}

	/// <summary>
	/// Represents the basic sampler states that can be serialized.
	/// </summary>
	public enum BasicSamplerState
	{
		Null,
		PointClamp,
		PointWrap,
		LinearClamp,
		LinearWrap,
		AnisotropicClamp,
		AnisotropicWrap
	}

	/// <summary>
	/// Represents the basic depth stencil states that can be serialized.
	/// </summary>
	public enum BasicDepthStencilState
	{
		Null,
		None,
		Default,
		DepthRead
	}

	/// <summary>
	/// Represents the basic rasterizer states that can be serialized.
	/// </summary>
	public enum BasicRasterizerState
	{
		Null,
		CullNone,
		CullClockwise,
		CullCounterClockwise
	}

	/// <summary>
	/// Converts SpriteRendererAssets to/from JSON.
	/// </summary>
	file class JsonSpriteRendererAssetConvereter : JsonBaseConverter<SpriteRendererAsset>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader,string property,JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Order":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<SpriteSortMode>();
			case "Blend":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<BasicBlendState>();
			case "Wrap":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<BasicSamplerState>();
			case "DepthRecord":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<BasicDepthStencilState>();
			case "Cull":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<BasicRasterizerState>();
			case "ShaderSource":
				if(reader.HasNextNull())
					return null;

				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			default:
				throw new JsonException();
			}
		}

		protected override SpriteRendererAsset ConstructT(Dictionary<string,object?> properties)
		{
			SpriteRendererAsset ret = new SpriteRendererAsset();

			if(properties.TryGetValue("Order",out object? otemp))
				ret.Order = (SpriteSortMode)otemp!;

			if(properties.TryGetValue("Blend",out otemp))
				ret.Blend = (BasicBlendState)otemp!;

			if(properties.TryGetValue("Wrap",out otemp))
				ret.Wrap = (BasicSamplerState)otemp!;

			if(properties.TryGetValue("DepthRecord",out otemp))
				ret.DepthStencil = (BasicDepthStencilState)otemp!;

			if(properties.TryGetValue("Cull",out otemp))
				ret.Cull = (BasicRasterizerState)otemp!;

			if(properties.TryGetValue("ShaderSource",out otemp))
				ret.ShaderSource.AssignRelativePath((string?)otemp);

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer,SpriteRendererAsset value,JsonSerializerOptions ops)
		{
			writer.WriteEnum("Order",value.Order);
			writer.WriteEnum("Blend",value.Blend);
			writer.WriteEnum("Wrap",value.Wrap);
			writer.WriteEnum("DepthRecord",value.DepthStencil);
			writer.WriteEnum("Cull",value.Cull);
			
			if(value.ShaderSource.RelativePath is null)
				writer.WriteNull("ShaderSource");
			else
				writer.WriteString("ShaderSource",value.ShaderSource.RelativePath);

			return;
		}
	}
}
