using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A component that draws an image.
	/// </summary>
	public class ImageComponent : DrawableAffineComponent
	{
		/// <summary>
		/// Creates an image component whose child class will take care of assigning Source itself.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		protected ImageComponent(Game game, SpriteBatch? renderer) : base(game,renderer,Color.White)
		{
			Source = null;
			Resource = null;
			
			LayerDepth = 0.0f;
			return;
		}

		/// <summary>
		/// Creates an image component by loading a texture via <paramref name="game"/>'s content manager with the name <paramref name="resource"/>.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="resource">The name of the texture resource to load.</param>
		public ImageComponent(Game game, SpriteBatch? renderer, string resource) : base(game,renderer,Color.White)
		{
			Source = null;
			Resource = resource;
			
			LayerDepth = 0.0f;
			return;
		}

		/// <summary>
		/// Creates an image component from the provided texture.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="texture">The texture of this image.</param>
		public ImageComponent(Game game, SpriteBatch? renderer, Texture2D texture) : base(game,renderer,Color.White)
		{
			Source = texture;
			Resource = null;

			LayerDepth = 0.0f;
			return;
		}

		/// <summary>
		/// Loads any content associated with this component.
		/// This will be loaded into the game's content manager (Game.Content) and must be unloaded through its Unload function.
		/// </summary>
		protected override void LoadContent()
		{
			if(Source is null && Resource is not null)
				Source = Game.Content.Load<Texture2D>(Resource);
			
			return;
		}
		
		/// <summary>
		/// Draws this image component.
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
		/// The source texture we use to draw this component.
		/// <para/>
		/// This value will always be non-null after LoadContent but may be null before.
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
		/// This value can be larger than Source is, in which case the SpriteBatch's wrap mode (SamplerState) will determine how out of bounds values are processed.
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
		/// This is the resource name of this component's texture if we need to load one.
		/// </summary>
		protected string? Resource;
	}
}
