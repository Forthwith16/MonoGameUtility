using GameEngine.Framework;
using GameEngine.Sprites;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A game object that draws an image.
	/// </summary>
	[JsonConverter(typeof(ImageGameObjectConverter))]
	public class ImageGameObject : DrawableAffineObject
	{
		/// <summary>
		/// Creates an image game object whose child class will take care of assigning Source itself.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		protected ImageGameObject(SpriteRenderer? renderer) : base(renderer,Color.White)
		{
			Source = null;
			Resource = null;
			
			return;
		}

		/// <summary>
		/// Creates an image game object by loading a texture via <paramref name="game"/>'s content manager with the name <paramref name="resource"/>.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="resource">The name of the texture resource to load.</param>
		public ImageGameObject(SpriteRenderer? renderer, string resource) : base(renderer,Color.White)
		{
			Source = null;
			Resource = resource;

			SourceRect = null;
			return;
		}

		/// <summary>
		/// Creates an image game object from the provided texture.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="texture">The texture of this image.</param>
		public ImageGameObject(SpriteRenderer? renderer, Texture2D texture) : base(renderer,Color.White)
		{
			Source = texture;
			Resource = null;

			SourceRect = null;
			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will have a fresh ID.</item>
		///	<item>This will have the same parent as <paramref name="other"/>, but it will leave Children unpopulated.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		public ImageGameObject(ImageGameObject other) : base(other)
		{
			Source = other.Source;
			Resource = other.Resource;

			SourceRect = other.SourceRect;
			return;
		}

		/// <summary>
		/// Loads any content associated with this game object.
		/// This will be loaded into the game's content manager (Game.Content) and must be unloaded through its Unload function.
		/// </summary>
		protected override void LoadContent()
		{
			if(Source is null && Resource is not null)
				Source = Game!.Content.Load<Texture2D>(Resource);
			
			return;
		}
		
		/// <summary>
		/// Draws this image game object.
		/// </summary>
		/// <param name="delta">The elapsed time since the last draw.</param>
		public override void Draw(GameTime delta)
		{
			if(Renderer is null)
				return;
			
			// First grab the transformation components we'll need to draw this with.
			World.Decompose(out Vector2 t,out float r,out Vector2 s);

			// Now we can draw!
			Renderer.Draw(Source,t,SourceRect,Tint,-r,Vector2.Zero,s,Effect,LayerDepth);
			
			return;
		}

		/// <summary>
		/// The source texture we use to draw this game object.
		/// </summary>
		protected Texture2D? Source
		{
			get => _s;

			set
			{
				if(value is null)
					return;
				
				_s = value;
				return;
			}
		}

		protected Texture2D? _s;

		/// <summary>
		/// This is the source rectangle to sample Source from during a draw call.
		/// This value can be larger than Source is, in which case the SpriteRenderer's wrap mode (SamplerState) will determine how out of bounds values are processed.
		/// When it is null, it draws all of Source.
		/// <para/>
		/// This value defaults to null.
		/// </summary>
		public Rectangle? SourceRect
		{get; set;}

		/// <summary>
		/// The raw pixel width of image drawn.
		/// This does not account for scaling, rotations, etc.
		/// <para/>
		/// When both SourceRect and Source are null, this defaults to 0.
		/// </summary>
		public override int Width => SourceRect is null ? (Source is null ? 0 : Source.Width) : SourceRect.Value.Width;

		/// <summary>
		/// The raw pixel height of image drawn.
		/// This does not account for scaling, rotations, etc.
		/// <para/>
		/// When both SourceRect and Source are null, this defaults to 0.
		/// </summary>
		public override int Height => SourceRect is null ? (Source is null ? 0 : Source.Height) : SourceRect.Value.Height;

		/// <summary>
		/// This is the resource name of this game object's texture if we need to load one.
		/// </summary>
		protected string? Resource;
	}

	/// <summary>
	/// Converts ImageGameObjects to/from JSON.
	/// </summary>
	file class ImageGameObjectConverter : JsonBaseConverter<ImageGameObject>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			throw new JsonException();





			if(JsonGameObjectConverter.ReadStandardGameObjectProperty(ref reader,property,ops,out object? p))
				return p;

			throw new JsonException();
		}

		protected override ImageGameObject ConstructT(Dictionary<string,object?> properties)
		{
			throw new JsonException();


			


			ImageGameObject ret = new ImageGameObject(null,(string)properties["Source"]!);

			ret.Enabled = (bool)properties["Enabled"]!;
			ret.UpdateOrder = (int)properties["UpdateOrder"]!;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, ImageGameObject value, JsonSerializerOptions ops)
		{
			throw new JsonException();


			


			JsonGameObjectConverter.WriteStandardProperties(writer,value,ops);
			return;
		}
	}
}
