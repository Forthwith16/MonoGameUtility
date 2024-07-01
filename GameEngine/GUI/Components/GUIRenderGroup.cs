using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// Groups GUI components together.
	/// This is accomplished via rendering them to a texture and then drawing the texture.
	/// Note that this will implicitly make this its containing GUI components' parent without ever setting the parameter.
	/// <para/>
	/// This GUI component's render target <b><u>MUST</u></b> be drawn before its containing GUICore, otherwise it will interrupt the GUICore's render target draw.
	/// To accomplish this, set its RenderTargetDrawOrder to a lower value than its containing GUICore's.
	/// <para/>
	/// Note that because this is first rendered to a texture and then draw to the screen from that texture, its first rendering is all of the resolution it has available.
	/// Ensure that the render quality is sufficiently high to permit any rotation/scaling required of it.
	/// </summary>
	/// <remarks>
	///	Using Immediate mode for GUICore is incompatible with ComponentGroups (and other nested renderers) since Immediate mode SpriteBatches set the GraphicsDevice state immediately.
	///	In general, it is inefficient regardless, so this is no great loss.
	///	<para/>
	///	This class will take no responsibility for the disposal of items added to it.
	///	However, it will initialize them.
	///	<para/>
	///	There is no equivalent GUIComponentGroup class.
	///	To move GUI components as a group without a separate rendering, assign their parent to be a common AffineObject.
	///	The GUICore transform represents a camera and is <i>never</i> assigned to be the parent of a GUI component, so this does not clobber transformation hierarchy.
	/// </remarks>
	public class GUIRenderGroup : GUIBase, IRenderTargetDrawable
	{
		/// <summary>
		/// Creates a new GUI render group.
		/// </summary>
		/// <param name="game">The game this will belong to.</param>
		/// <param name="name">The name of the component.</param>
		/// <param name="w">The fixed draw width of the component.</param>
		/// <param name="h">The fixed draw height of the component.</param>
		/// <param name="bgc">
		///	The clear color of this component.
		///	This can be used to draw a solid color background.
		///	Alternatively, use an alpha value of 0.0f for full transparency.
		///	<para/>
		///	When null, this will default to Color.Transparent.
		/// </param>
		public GUIRenderGroup(RenderTargetFriendlyGame game, string name, int w, int h, Color? bgc = null) : base(game,name)
		{
			Width = w;
			Height = h;
			
			ClearColor = bgc ?? Color.Transparent;
			
			UpdateChildren = new SortedList<IGUI,IGUI>(new OrderComparer(true));
			DrawChildren = new SortedList<IGUI,IGUI>(new OrderComparer(false));

			Camera = Matrix2D.Identity;

			RenderTargetDrawOrderChanged += (a,b) => {};
			_rtdo = 0;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			// Register ourselves as having a render target that needs drawing before ordinary draws
			// This is added independent of the ordinary component hierarchy since it will also belong to a GUICore component hierarchy for ordinary Update/Draw calls
			Game.AddRenderTargetComponent(this);
			
			// Initialize all the children
			foreach(IGUI component in UpdateChildren.Keys)
				component.Initialize(); // Always initialize regardless of Enabled or Visible

			// Load the render engine first
			RenderTarget = new RenderTarget2D(Game.GraphicsDevice,Width,Height);
			LocalRenderer = new SpriteBatch(Game.GraphicsDevice);

			// Initialize our drawing material
			Source = new ImageComponent(Game,Renderer,RenderTarget);
			Source.Parent = this;
			Source.LayerDepth = (this as IGUI).LayerDepth;
			Source.Initialize();

			// Make sure the source stays at the right layer depth
			DrawOrderChanged += (a,b) => Source.LayerDepth = (this as IGUI).LayerDepth;

			return; // Base LoadContent does nothing
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			// Update each child
			foreach(IGUI component in UpdateChildren.Keys)
				if(component.Enabled) // We don't care if a component is visible as we want to update them either way so long as they are enabled
					component.Update(delta);
			
			Source!.Update(delta);
			return; // Base Update does nothing
		}

		public void DrawRenderTarget(GameTime delta)
		{
			// First set the render target
			Game.GraphicsDevice.SetRenderTarget(RenderTarget);

			// Now clear the screen
			Game.GraphicsDevice.Clear(ClearColor);

			// Now start the local renderer (it's fine to make multiple SpriteRenderer Begin calls since they only set the GraphicsDevice state on End (unless using Immediate)
			LocalRenderer!.Begin(SpriteSortMode.BackToFront,Blend,Wrap,DepthRecord,Cull,Shader,Camera.Invert().SwapChirality());
			
			foreach(IGUI component in DrawChildren.Keys)
				if(component.Visible) // We don't care if a GUI component is enabled here since they can still be draw but are greyed out or whatever
					component.Draw(delta);

			LocalRenderer.End();
			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Source!.Draw(delta); // We only need to draw our render texture
			return;
		}

		protected override void UnloadContentAddendum()
		{
			Source!.Dispose();
			return;
		}

		/// <summary>
		/// Adds a GUI component to this group.
		/// </summary>
		/// <param name="component">The component to add. This component must not already be added to this group for this add to succeed.</param>
		/// <returns>Returns true if the component could be added and false otherwise.</returns>
		/// <remarks>
		///	The GUI component's parent is <i>not</i> set to this.
		///	The group's transform represents its place in world coordinates.
		///	The group's camera matrix represents the local view coordinates.
		///	The local world space coordinates are defined entirely by <paramref name="component"/>'s local world coordinates.
		/// </remarks>
		public bool Add(IGUI component)
		{
			// Check that we're not double adding
			if(UpdateChildren.ContainsKey(component))
				return false;

			// Add the component to our children lists
			UpdateChildren.Add(component,component);
			DrawChildren.Add(component,component);

			// Now let's subscribe to key events (we will need named functions so we can unsubscribe on removal)
			component.UpdateOrderChanged += UpdateUpdateChildren;
			component.DrawOrderChanged += UpdateDrawChildren;

			// We do not need to bind the component's parent to this since we get to place the camera transform into the sprite batch's Begin call
			// We do, however, need to set its owner and renderer
			component.Owner = Owner;
			component.Renderer = LocalRenderer;

			// Lastly, initialize if we already are
			if(Initialized)
				component.Initialize();

			return true;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in UpdateChildren.
		/// </summary>
		private void UpdateUpdateChildren(object? sender, EventArgs e)
		{
			if(sender is not IGUI component)
				return;
			
			if(UpdateChildren.Remove(component))
				UpdateChildren.Add(component,component);

			return;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in DrawChildren.
		/// </summary>
		private void UpdateDrawChildren(object? sender, EventArgs e)
		{
			if(sender is not IGUI component)
				return;

			if(DrawChildren.Remove(component))
				DrawChildren.Add(component,component);

			return;
		}

		/// <summary>
		/// Removes a top-level GUI component from this GUICore.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if <paramref name="component"/> was removed and false otherwise.</returns>
		public bool Remove(IGUI component)
		{
			// Remove the component from our children lists
			if(!UpdateChildren.Remove(component) || !DrawChildren.Remove(component))
				return false;

			// Now let's unsubscribe from key events
			component.UpdateOrderChanged -= UpdateUpdateChildren;
			component.DrawOrderChanged -= UpdateDrawChildren;

			// For good practice, we also void the component's owner (this will also void its state data containing this)
			// We will not, however, dispose of the component because it may be used later, and we'll let the finalizer take care of that
			component.Owner = null;

			return true;
		}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// Translate pos into this component's local coordinates
			// The World transform inverse aligns us with the local coordinate system by undoing the world space transform
			pos = InverseWorld * pos;
			
			// First check if pos is even within this group at all
			Vector2 TL = new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
			Vector2 TR = new Vector2(Width,0);
			Vector2 BL = new Vector2(0,Height);
			Vector2 BR = new Vector2(Width,Height);

			// To check that we're on the 'inside', we need only check that the cross product points up clockwise (screen coordinates are left-handed)
			if((TR - TL).Cross(pos - TL) < 0.0f || (BR - TR).Cross(pos - TR) < 0.0f || (BL - BR).Cross(pos - BR) < 0.0f && (TL - BL).Cross(pos - BL) < 0.0f)
			{
				component = null;
				return false;
			}

			// We know we're at least within this group
			component = this;
			
			// If we don't want children, we're done
			if(!include_children)
				return true;

			// We will now need to translate pos into the local coordinate system inside of this group to see if we've clicked on anything inside of it
			// The Camera transform gets us into the internal coordinate system by undoing the internal camera space transform
			pos = Camera * pos;

			// Process input
			// We need to figure out what GUI component we're actually interacting with
			// Our bounding box heirarchy is basically a poor man's AABB tree, so let's just brute force this
			int max = -1; // We set this to -1 because clicking on ANYTHING is better than clicking on the group (everything is drawn on top of the background)

			// Find the topmost level thing that we intersect with
			// In the case of DrawOrder ties, the thing with the earliest name is selected, which is fine
			foreach(IGUI child in DrawChildren.Keys)
				if(child.Enabled && child.Visible && child.Bounds.Contains(pos) && child.Contains(pos,out IGUI? intersector) && intersector!.DrawOrder > max)
				{
					max = intersector!.DrawOrder;
					component = intersector;
				}
			
			return true;
		}

		/// <summary>
		/// The texture that we will render to.
		/// </summary>
		protected RenderTarget2D? RenderTarget
		{get; set;}

		public override GUICore? Owner
		{
			get
			{return base.Owner;}

			set
			{
				if(ReferenceEquals(base.Owner,value))
					return;

				base.Owner = value;

				foreach(IGUI child in UpdateChildren.Keys)
					child.Owner = value;

				foreach(IGUI child in DrawChildren.Keys)
					child.Owner = value;

				return;
			}
		}

		public override SpriteBatch? Renderer
		{
			protected get => base.Renderer;

			set
			{
				if(ReferenceEquals(base.Renderer,value) || value is null)
					return;

				base.Renderer = value;
				
				if(Source is not null)
					Source.Renderer = value;

				return;
			}
		}

		/// <summary>
		/// The means by which our children will render themselves into RenderTarget.
		/// </summary>
		protected SpriteBatch? LocalRenderer
		{
			get => _lr;
			
			set
			{
				if(ReferenceEquals(_lr,value) || value is null)
					return;
				
				_lr = value;

				// Children should use the local renderer
				foreach(IGUI component in DrawChildren.Values)
					component.Renderer = LocalRenderer;

				return;
			}
		}

		protected SpriteBatch? _lr;

		/// <summary>
		/// The color to clear the background of the render texture to.
		/// </summary>
		public Color ClearColor
		{get; set;}

		/// <summary>
		/// This is the image component that will actually draw this component.
		/// It's texture source is a render target.
		/// </summary>
		public ImageComponent? Source
		{get; protected set;}

		/// <summary>
		/// The camera transform.
		/// This is distinct from the world transform.
		/// Transform moves this GUI component around in world space.
		/// Camera moves this GUI component's contents around in its sub-world space.
		/// <para/>
		/// This matrix will be inverted at draw time so that camera movement corresponds to natural movement.
		/// i.e. moving the camera down moves the view down instead of the contents down, thus moving the view up.
		/// </summary>
		public Matrix2D Camera
		{get; set;}

		/// <summary>
		/// The children of this GUICore in update order.
		/// </summary>
		protected SortedList<IGUI,IGUI> UpdateChildren
		{get; init;}

		/// <summary>
		/// The children of this GUICore in draw order.
		/// </summary>
		protected SortedList<IGUI,IGUI> DrawChildren
		{get; init;}

		/// <summary>
		/// The number of top-level GUI components in this GUICore.
		/// </summary>
		public int Count => UpdateChildren.Count;

		/// <summary>
		/// A blend mode for GUI components to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to AlphaBlend).
		/// </summary>
		public BlendState? Blend
		{get; set;}

		/// <summary>
		/// A sampler wrap mode for GUI components to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to LinearClamp).
		/// </summary>
		public SamplerState? Wrap
		{get; set;}

		/// <summary>
		/// A shader for GUI components to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to None).
		/// </summary>
		public DepthStencilState? DepthRecord
		{get; set;}

		/// <summary>
		/// The cull state used when drawing GUI components.
		/// <para/>
		/// This value defaults to null (which in turn deaults to CullCounterClockwise).
		/// </summary>
		public RasterizerState? Cull
		{get; set;}

		/// <summary>
		/// A shader for GUI components to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite effect).
		/// </summary>
		public Effect? Shader
		{get; set;}

		public override int Width
		{get;}

		public override int Height
		{get;}

		public override Rectangle Bounds
		{
			get
			{
				// We need to know where the four corners end up as the most extreme points
				// Then we can pick the min and max values to get a bounding rectangle
				Vector2 TL = World * new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(Width,0);
				Vector2 BL = World * new Vector2(0,Height);
				Vector2 BR = World * new Vector2(Width,Height);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top));
			}
		}

		/// <summary>
		/// The minimal bounding box for all internal contents of this group in the internal world coordinate system.
		/// At minimum, the bounds are (0,0,Width,Height).
		/// </summary>
		public Rectangle InternalBounds
		{
			get
			{
				if(Count == 0)
					return new Rectangle(0,0,Width,Height);

				// Go through all of the bounds and find the minimum and maximum x and y positions
				// This technically includes transformations, but its not the GUICore's transformations, so whatevs
				float min_x = 0.0f;
				float max_x = Width;

				float min_y = 0.0f;
				float max_y = Height;

				foreach(IGUI component in DrawChildren.Keys)
				{
					Rectangle b = component.Bounds;

					// You can't succeed at both, so an else if is fine
					if(b.Left < min_x)
						min_x = b.Left;
					else if(b.Right > max_x)
						max_x = b.Right;

					// You can't succeed at both, so an else if is fine
					if(b.Top < min_y)
						min_y = b.Top;
					else if(b.Bottom > max_y)
						max_y = b.Bottom;
				}

				return new Rectangle((int)min_x,(int)min_y,(int)MathF.Ceiling(max_x - min_x),(int)MathF.Ceiling(max_y - min_y));
			}
		}

		public int RenderTargetDrawOrder
		{
			get => _rtdo;

			set
			{
				if(_rtdo == value)
					return;

				int old = _rtdo;
				_rtdo = value;
				RenderTargetDrawOrderChanged(this,new RenderTargetDrawOrderEventArgs(this,_rtdo,old));

				return;
			}
		}

		protected int _rtdo;

		public event EventHandler<RenderTargetDrawOrderEventArgs> RenderTargetDrawOrderChanged;
	}
}
