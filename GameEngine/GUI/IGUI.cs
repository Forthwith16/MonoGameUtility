using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI
{
	/// <summary>
	/// Defines the bare bones of what it means to be a GUI component.
	/// <para/>
	/// Users should add GUI components to a GUICore.
	/// </summary>
	public interface IGUI : IAffineComponent2D, IUpdateable, IDrawable, IDisposable
	{
		/// <summary>
		/// Called to initialize a GUI component.
		/// </summary>
		public void Initialize();

		/// <summary>
		/// Draws this GUI component.
		/// <para/>
		/// For children to be drawn correctly, see LayerDepth.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		public new void Draw(GameTime delta);

		/// <summary>
		/// Determines if this GUI component contains the point <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The point (in world coordinates) to check for containment.</param>
		/// <param name="component">The GUI component where the <i>topmost</i> containment (a child could overlap its parent) occurred if there was one; null otherwise.</param>
		/// <param name="include_children">If true, we include (enabled and visible) children in the potential collision.</param>
		/// <returns>Returns true if this or a child contained <paramref name="p"/> and false otherwise.</returns>
		public bool Contains(Point p, out IGUI? component, bool include_children = true);

		/// <summary>
		/// Determines if this GUI component contains the position <paramref name="pos"/>.
		/// </summary>
		/// <param name="pos">The position (in world coordinates) to check for containment.</param>
		/// <param name="component">The GUI component where the <i>topmost</i> containment (a child could overlap its parent) occurred if there was one; null otherwise.</param>
		/// <param name="include_children">If true, we include (enabled and visible) children in the potential collision.</param>
		/// <returns>Returns true if this or a child contained <paramref name="pos"/> and false otherwise.</returns>
		public bool Contains(Vector2 pos, out IGUI? component, bool include_children = true);

		/// <summary>
		/// Performs a mouse click (rising edge only).
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformClick(MouseClickEventArgs args);

		/// <summary>
		/// Performs a mouse click (falling edge only).
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformRelease(MouseClickEventArgs args);

		/// <summary>
		/// Performs a mouse click (neither the rising nor falling edge but the proper click that occurs after both).
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformClicked(MouseClickedEventArgs args);

		/// <summary>
		/// Performs a mouse hover.
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformHover(MouseHoverEventArgs args);

		/// <summary>
		/// Performs a mouse move.
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformMove(MouseMoveEventArgs args);

		/// <summary>
		/// Performs a mouse exit.
		/// </summary>
		/// <param name="args">The event data.</param>
		public void PerformExit(MouseHoverEventArgs args);

		/// <summary>
		/// Transforms a DrawOrder value into a LayerDepth for use with a GUICore.
		/// </summary>
		/// <param name="order">The order to convert.</param>
		/// <returns>Returns 1 / <paramref name="order"/>, the appropriate layer depth to draw a GUI component with draw order <paramref name="order"/>.</returns>
		public static float DrawOrderToLayerDepth(int order) => 1.0f / order;

		/// <summary>
		/// The game this belongs to.
		/// </summary>
		public RenderTargetFriendlyGame Game
		{get;}

		/// <summary>
		/// The name of the GUI element.
		/// </summary>
		public string Name
		{get;}

		/// <summary>
		/// This is the GUICore this component belongs to.
		/// This is null until it is added to a GUICore.
		/// <para/>
		/// Note that even when this value is not null, that does not mean it is a top-level component of a GUICore.
		/// <para/>
		/// This is responsible for assigning the Owner of each of its child GUI components (if it has any).
		/// </summary>
		/// <remarks>
		/// This <i>is</i> responsible for adding/removing itself (and through the recursive assignment of Owner) its children to the GUICore it is assigned to.
		/// However, use the new owner's AddToMap and the old owner's Remove methods to keep containment data up to date.
		/// The new owner's Add method <i>should not</i> be used unless this GUI component can <i>only</i> be a top-level component (which is exceedingly rare).
		/// </remarks>
		public GUICore? Owner
		{get; set;}

		/// <summary>
		/// This is the tooltip for this GUI component (if it has one).
		/// </summary>
		public DrawableAffineComponent? Tooltip
		{get;}

		/// <summary>
		/// This is the SpriteBatch used to draw this GUI component.
		/// If this is unassigned (null), then the draw will be skipped.
		/// <para/>
		/// This should always be equal to Owner.Renderer but serves as a way to notify compilcated GUI components of renderer changes without having to check every draw cycle.
		/// </summary>
		public SpriteBatch? Renderer
		{set;}

		/// <summary>
		/// If true, this GUI component will suspend normal digital navigation when it is active.
		/// If false, this has no effect.
		/// </summary>
		public bool SuspendsDigitalNavigation
		{get;}

		/// <summary>
		/// A bounding box for this GUI component, including its children.
		/// </summary>
		public Rectangle Bounds
		{get;}

		/// <summary>
		/// The tint to apply to this image component.
		/// <para/>
		/// This defaults to Color.White for true color.
		/// </summary>
		public Color Tint
		{get; set;}

		/// <summary>
		/// This is the drawing layer.
		/// This value must be within [0,1].
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteBatch's SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// <para/>
		/// To ensure that children are drawn correctly, the GUICore renders BackToFront by default.
		/// It is suggested to draw on layers of the form 1/n (usually for 1-indexed children at most n levels deep in the GUI tree) so that it is easier to specify what should be drawn on top of what.
		/// One can obtain n from the DrawOrder of this component.
		/// </summary>
		public float LayerDepth
		{get => DrawOrderToLayerDepth(DrawOrder);}

		/// <summary>
		/// The width of the GUI component (sans transformations).
		/// </summary>
		public int Width
		{get;}

		/// <summary>
		/// The height of the GUI component (sans transformations).
		/// </summary>
		public int Height
		{get;}

		/// <summary>
		/// An event called when the mouse first clicked while hovering over this GUI component.
		/// </summary>
		public event Click OnClick;

		/// <summary>
		/// An event called when the mouse released a click while hovering over this GUI component.
		/// </summary>
		public event Release OnRelease;

		/// <summary>
		/// An event called when the mouse is clicked while hovering over this GUI component.
		/// That is it must click and release on this component without exiting it.
		/// </summary>
		public event Clicked OnClicked;

		/// <summary>
		/// An event called when the mouse starts hovering over this GUI component.
		/// </summary>
		public event Hovered OnHover;

		/// <summary>
		/// An event called when the mouse moves over this GUI component after having entered it but before leaving it.
		/// </summary>
		public event Moved OnMove;

		/// <summary>
		/// An event called when the mouse stops hovering over this GUI component.
		/// </summary>
		public event Exited OnExit;
	}

	/// <summary>
	/// A delegate to call when a GUI element is first clicked.
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Click(IGUI sender, MouseClickEventArgs args);

	/// <summary>
	/// A delegate to call after a GUI element is no longer being clicked (i.e. the mouse was released).
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Release(IGUI sender, MouseClickEventArgs args);
	
	/// <summary>
	/// A delegate to call after a GUI element is clicked on (i.e. the mouse clicked it and released on it without exiting).
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Clicked(IGUI sender, MouseClickedEventArgs args);

	/// <summary>
	/// A delegate to call when a GUI element is hovered over.
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Hovered(IGUI sender, MouseHoverEventArgs args);

	/// <summary>
	/// A delegate to call when a GUI element is moved over.
	/// This occurs after the hover frame and ends the frame before the exit frame.
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Moved(IGUI sender, MouseMoveEventArgs args);

	/// <summary>
	/// A delegate to call when a GUI element is no longer hovered over.
	/// </summary>
	/// <param name="sender">The GUI component sending the event.</param>
	/// <param name="args">The event data.</param>
	public delegate void Exited(IGUI sender, MouseHoverEventArgs args);
}
