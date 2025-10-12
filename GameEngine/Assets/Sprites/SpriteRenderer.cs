using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.Sprites
{
	/// <summary>
	/// An extended version of a sprite
	/// </summary>
	public class SpriteRenderer : SpriteBatch, IAsset
	{
		/// <summary>
		/// Creates a sprite renderer with no name.
		/// </summary>
		/// <param name="g">The graphics device used to draw with.</param>
		public SpriteRenderer(GraphicsDevice g) : base(g)
		{
			AssetName = "";

			Order = SpriteSortMode.BackToFront;
			Blend = BlendState.NonPremultiplied;
			Wrap = SamplerState.LinearClamp;
			DepthStencil = DepthStencilState.None;
			Cull = RasterizerState.CullCounterClockwise;
			Shader = null;
			Transform = null;
			
			return;
		}
		
		/// <summary>
		/// Creates a sprite renderer.
		/// </summary>
		/// <param name="g">The graphics device used to draw with.</param>
		public SpriteRenderer(string name, GraphicsDevice g) : base(g)
		{
			AssetName = name;

			Order = SpriteSortMode.BackToFront;
			Blend = BlendState.NonPremultiplied;
			Wrap = SamplerState.LinearClamp;
			DepthStencil = DepthStencilState.None;
			Cull = RasterizerState.CullCounterClockwise;
			Shader = null;
			Transform = null;
			
			return;
		}

		/// <summary>
		/// Creates a deep copy of <paramref name="other"/>, or at least a copy deep enough to operate normally.
		/// </summary>
		/// <param name="other">The SpriteRenderer to copy.</param>
		public SpriteRenderer(SpriteRenderer other) : base(other.GraphicsDevice)
		{
			AssetName = other.AssetName;

			Order = other.Order;
			Blend = other.Blend;
			Wrap = other.Wrap;
			DepthStencil = other.DepthStencil;
			Cull = other.Cull;
			Shader = other.Shader;
			Transform = other.Transform;
			
			return;
		}

		/// <summary>
		/// Begins a new sprite and text batch with the memorized render state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if Begin is called twice without an End call between.</exception>
		public void Begin()
		{
			Begin(Order,Blend,Wrap,DepthStencil,Cull,Shader,Transform);
			return;
		}

		/// <summary>
		/// Draws a texture.
		/// </summary>
		/// <param name="texture">The texture to draw.</param>
		/// <param name="transform">
		/// The transform to apply to the texture.
		/// This will be composed into translation, rotation, and scale.
		/// This does not properly support shear or reflection transforms.
		/// The latter are supported through <paramref name="effects"/>.
		/// </param>
		/// <param name="source_rect">The source rectangle of <paramref name="texture"/> to draw with.</param>
		/// <param name="mask">A color mask.</param>
		/// <param name="effects">Supports reflection effects.</param>
		/// <param name="layer_depth">
		/// This is the drawing layer.
		/// This value must be within [0,1], though this will not be enforced here.
		/// <para/>
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// </param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="transform"/> cannot be decomposed.</exception>
		public void Draw(Texture2D texture, Matrix2D transform, Rectangle? source_rect, Color mask, SpriteEffects effects, float layer_depth)
		{
			if(!transform.Decompose(out Vector2 t,out float r,out Vector2 s))
				throw new ArgumentException();

			Draw(texture,t,source_rect,mask,-r,Vector2.Zero,s,effects,layer_depth);
			return;
		}

		/// <summary>
		/// Draws a string
		/// </summary>
		/// <param name="font">The font to draw with.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="transform">
		/// The transform to apply to the texture.
		/// This will be composed into translation, rotation, and scale.
		/// This does not properly support shear or reflection transforms.
		/// The latter are supported through <paramref name="effects"/>.
		/// </param>
		/// <param name="mask">A color mask.</param>
		/// <param name="effects">Supports reflection effects.</param>
		/// <param name="layer_depth">
		/// This is the drawing layer.
		/// This value must be within [0,1], though this will not be enforced here.
		/// <para/>
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// </param>
		/// <param name="rtl">If true, text is drawn right to left. If false, it is drawn left to right.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="transform"/> cannot be decomposed.</exception>
		public void Draw(SpriteFont font, string text, Matrix2D transform, Color mask, SpriteEffects effects, float layer_depth, bool rtl = false)
		{
			if(!transform.Decompose(out Vector2 t,out float r,out Vector2 s))
				throw new ArgumentException();

			DrawString(font,text,t,mask,-r,Vector2.Zero,s,effects,layer_depth,rtl);
			return;
		}

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
		/// This value defaults to NonPremultiplied (the null value defaults to AlphaBlend).
		/// </summary>
		public BlendState? Blend
		{get; set;}

		/// <summary>
		/// A sampler wrap mode to draw with.
		/// <para/>
		/// This value defaults to LinearClamp (the null value default).
		/// </summary>
		public SamplerState? Wrap
		{get; set;}

		/// <summary>
		/// The manner of depth stencil to draw with.
		/// <para/>
		/// This value defaults to None (the null value default).
		/// </summary>
		public DepthStencilState? DepthStencil
		{get; set;}

		/// <summary>
		/// The cull state used when drawing.
		/// <para/>
		/// This value defaults to CullCounterClockwise (the null value default).
		/// </summary>
		public RasterizerState? Cull
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

		public string AssetName
		{get;}
	}
}
