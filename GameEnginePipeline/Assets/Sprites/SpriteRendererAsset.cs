using GameEngine.Assets.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using GameEnginePipeline.Serialization;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// The details of a SpriteRenderer laid bare.
	/// </summary>
	[Asset(typeof(SpriteRenderer))]
	[JsonConverter(typeof(JsonSpriteRendererAssetConvereter))]
	public class SpriteRendererAsset : AssetBase
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		protected override void Serialize(string path) => this.SerializeJson(path);

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static SpriteRendererAsset? Deserialize(string path) => path.DeserializeJsonFile<SpriteRendererAsset>();

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
		protected SpriteRendererAsset(SpriteRenderer sr) : base()
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
