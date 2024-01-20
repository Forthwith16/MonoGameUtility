using Microsoft.Xna.Framework;

namespace GameEngine.Framework
{
	/// <summary>
	/// Outlines how a component draws to render targets.
	/// </summary>
	/// <remarks>
	/// This interface does not specify a publicly available render target.
	/// The principle intent is to allow a drawable object to render itself to a render target.
	/// It may thereafter draw itself to the screen and/or it may allow something else to use its render target.
	/// It could, in fact, also have multiple render targets.
	/// </remarks>
	public interface IRenderTargetDrawable
	{
		/// <summary>
		/// Performs any render target drawing necessary for this component.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last DrawRenderTarget call.</param>
		public void DrawRenderTarget(GameTime delta);

		/// <summary>
		/// The order IRenderTargetDrawables should be drawn in.
		/// Lower values are drawn before larger values.
		/// </summary>
		public int RenderTargetDrawOrder
		{get; set;}
		
		/// <summary>
		/// If true, this is visible and DrawRenderTarget will be called.
		/// If false, this is not visible and DrawRenderTarget will not be called.
		/// </summary>
		public bool Visible
		{get;}

		/// <summary>
		/// Called when RenderTargetDrawOrder changes.
		/// </summary>
		public event EventHandler<EventArgs> RenderTargetDrawOrderChanged;

		/// <summary>
		/// Called when the visibility of this changes.
		/// </summary>
		public event EventHandler<EventArgs> VisibleChanged;
	}


}
