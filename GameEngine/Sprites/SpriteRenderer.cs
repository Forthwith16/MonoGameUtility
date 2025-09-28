using GameEngine.Framework;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Sprites
{
	/// <summary>
	/// An extended version of a sprite
	/// </summary>
	[JsonConverter(typeof(JsonSpriteRendererConverter))]
	public class SpriteRenderer : SpriteBatch
	{
		/// <summary>
		/// Creates a sprite renderer.
		/// </summary>
		/// <param name="game">The game whose GraphicsDevice to draw upon for rendering.</param>
		public SpriteRenderer(RenderTargetFriendlyGame game) : base(game.GraphicsDevice)
		{
			Owner = game;

			Order = SpriteSortMode.BackToFront;
			Blend = BlendState.NonPremultiplied;
			Wrap = SamplerState.LinearClamp;
			DepthRecord = DepthStencilState.None;
			Cull = RasterizerState.CullCounterClockwise;
			Shader = null;
			Transform = null;

			return;
		}

		/// <summary>
		/// Creates a sprite renderer.
		/// </summary>
		/// <param name="id">The ID of the game whose GraphicsDevice to draw upon for rendering.</param>
		public SpriteRenderer(GameID id) : base(((Game)id).GraphicsDevice)
		{
			Owner = id;

			Order = SpriteSortMode.BackToFront;
			Blend = BlendState.NonPremultiplied;
			Wrap = SamplerState.LinearClamp;
			DepthRecord = DepthStencilState.None;
			Cull = RasterizerState.CullCounterClockwise;
			Shader = null;
			Transform = null;

			return;
		}

		/// <summary>
		/// Begins a new sprite and text batch with the memorized render state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if Begin is called twice without an End call between.</exception>
		public void Begin()
		{
			Begin(Order,Blend,Wrap,DepthRecord,Cull,Shader,Transform);
			return;
		}

		/// <summary>
		/// The game whose GraphicsDevice we use to draw with.
		/// </summary>
		public RenderTargetFriendlyGame Owner
		{get;}

		/// <summary>
		/// The order that sprites are sorted when drawn.
		/// <para/>
		/// This value defaults to BackToFront.
		/// </summary>
		public SpriteSortMode Order
		{get; set;}

		/// <summary>
		/// A blend mode to draw with.
		/// <para/>
		/// This value defaults to NonPremultiplied.
		/// </summary>
		public BlendState Blend
		{get; set;}

		/// <summary>
		/// A sampler wrap mode to draw with.
		/// <para/>
		/// This value defaults to LinearClamp.
		/// </summary>
		public SamplerState Wrap
		{get; set;}

		/// <summary>
		/// The manner of depth stencil to draw with.
		/// <para/>
		/// This value defaults to None.
		/// </summary>
		public DepthStencilState DepthRecord
		{get; set;}

		/// <summary>
		/// The cull state used when drawing.
		/// <para/>
		/// This value defaults to CullCounterClockwise.
		/// </summary>
		public RasterizerState Cull
		{get; set;}

		/// <summary>
		/// A shader to draw with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite Effect).
		/// </summary>
		public Effect? Shader
		{get; set;}

		/// <summary>
		/// An additional transform to draw with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the identity matrix).
		/// </summary>
		public Matrix? Transform
		{get; set;}
	}

	/// <summary>
	/// Converts a SpriteRenderer to/from JSON.
	/// </summary>
	/// <remarks>For this to deserialize properly, it must have the correct GameID.</remarks>
	file class JsonSpriteRendererConverter : JsonBaseConverter<SpriteRenderer>
	{
		public JsonSpriteRendererConverter()
		{
			IDConverter = null;
			return;
		}

		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			if(IDConverter is null)
				IDConverter = (JsonConverter<GameID>)ops.GetConverter(typeof(GameID));

			switch(property)
			{
			case "ID":
				
			default:
				throw new JsonException();
			}
		}

		protected override SpriteRenderer ConstructT(Dictionary<string,object?> properties)
		{
			SpriteRenderer ret = new SpriteRenderer((GameID)properties["ID"]!);



			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, SpriteRenderer value, JsonSerializerOptions ops)
		{
			if(IDConverter is null)
				IDConverter = (JsonConverter<GameID>)ops.GetConverter(typeof(GameID));

			writer.WritePropertyName("ID");
			IDConverter.Write(writer,value.Owner.ID,ops);



			return;
		}

		/// <summary>
		/// Converts game IDs to/from JSON.
		/// </summary>
		private JsonConverter<GameID>? IDConverter;
	}
}
