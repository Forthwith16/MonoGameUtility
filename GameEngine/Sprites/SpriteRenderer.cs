using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Sprites
{
	/// <summary>
	/// An extended version of a sprite
	/// </summary>
	public class SpriteRenderer : SpriteBatch
	{
		/// <summary>
		/// Creates a sprite renderer.
		/// </summary>
		/// <param name="g">The graphics device used to draw with.</param>
		public SpriteRenderer(GraphicsDevice g) : base(g)
		{
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
	}
}
