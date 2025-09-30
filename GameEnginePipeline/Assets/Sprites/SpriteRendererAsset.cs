using GameEngine.Readers;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// The details of a SpriteRenderer laid bare.
	/// </summary>
	[JsonConverter(typeof(JsonSpriteRendererAssetConvereter))]
	public class SpriteRendererAsset : IAsset
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public void Serialize(string path) => this.SerializeJson(path);

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static SpriteRendererAsset? Deserialize(string path) => path.DeserializeJsonFile<SpriteRendererAsset>();

		/// <summary>
		/// The order that sprites are sorted when drawn.
		/// <para/>
		/// This value defaults to BackToFront.
		/// </summary>
		public SpriteSortMode Order
		{get; set;} = SpriteSortMode.BackToFront;

		/// <summary>
		/// A blend mode to draw with.
		/// <para/>
		/// This value defaults to AlphaBlend.
		/// </summary>
		public BasicBlendState Blend
		{get; set;} = BasicBlendState.AlphaBlend;

		/// <summary>
		/// A sampler wrap mode to draw with.
		/// <para/>
		/// This value defaults to LinearClamp.
		/// </summary>
		public BasicSamplerState Wrap
		{get; set;} = BasicSamplerState.LinearClamp;

		/// <summary>
		/// The manner of depth stencil to draw with.
		/// <para/>
		/// This value defaults to None.
		/// </summary>
		public BasicDepthStencilState DepthStencil
		{get; set;} = BasicDepthStencilState.None;

		/// <summary>
		/// The cull state used when drawing.
		/// <para/>
		/// This value defaults to CullCounterClockwise.
		/// </summary>
		public BasicRasterizerState Cull
		{get; set;} = BasicRasterizerState.CullCounterClockwise;

		/// <summary>
		/// A shader to draw with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite Effect).
		/// </summary>
		public string? ShaderSource
		{get; set;} = null;
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
				ret.ShaderSource = (string?)otemp;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer,SpriteRendererAsset value,JsonSerializerOptions ops)
		{
			writer.WriteEnum("Order",value.Order);
			writer.WriteEnum("Blend",value.Blend);
			writer.WriteEnum("Wrap",value.Wrap);
			writer.WriteEnum("DepthRecord",value.DepthStencil);
			writer.WriteEnum("Cull",value.Cull);
			
			if(value.ShaderSource is null)
				writer.WriteNull("ShaderSource");
			else
				writer.WriteString("ShaderSource",value.ShaderSource);

			return;
		}
	}
}
