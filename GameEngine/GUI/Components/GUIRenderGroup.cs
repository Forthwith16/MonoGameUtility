using GameEngine.DataStructures.Sets;
using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Maths;
using GameEngine.Sprites;
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
	///	Using Immediate mode for GUICore is incompatible with ComponentGroups (and other nested renderers) since Immediate mode SpriteRendereres set the GraphicsDevice state immediately.
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
		public GUIRenderGroup(string name, int w, int h, Color? bgc = null) : base(name)
		{
			Width = w;
			Height = h;
			
			ClearColor = bgc ?? Color.Transparent;
			
			TopComponents = new Dictionary<string,DummyGUI>();
			UpdateChildren = new AVLSet<IGUI>(new GUIOrderComparer(TopComponents,true));
			DrawChildren = new AVLSet<IGUI>(new GUIOrderComparer(TopComponents,false));

			Camera = Matrix2D.Identity;
			CameraInverse = Matrix.Identity;

			RenderTargetDrawOrderChanged += (a,b) => {};
			_rtdo = 0;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			// Register ourselves as having a render target that needs drawing before ordinary draws
			// This is added independent of the ordinary component hierarchy since it will also belong to a GUICore component hierarchy for ordinary Update/Draw calls
			Game!.AddRenderTargetComponent(this); // We only (normally) get here after setting Game
			
			// Initialize all the children
			foreach(IGUI component in UpdateChildren)
				component.Initialize(); // Always initialize regardless of Enabled or Visible

			// Load the render engine first
			RenderTarget = new RenderTarget2D(Game.GraphicsDevice,Width,Height);

			LocalRenderer = new SpriteRenderer(Game.GraphicsDevice);
			LocalRenderer.Blend = BlendState.NonPremultiplied;

			// Initialize our drawing material
			Source = new ImageGameObject(Renderer,RenderTarget);
			Source.Parent = this;
			Source.DrawOrder = DrawOrder;
			Source.LayerDepth = LayerDepth;
			Source.Initialize();

			// Make sure the source stays at the right layer depth
			DrawOrderChanged += (a,b) =>
			{
				Source.DrawOrder = DrawOrder;
				Source.LayerDepth = LayerDepth;

				return;
			};
			
			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			// Update each child
			foreach(IGUI component in UpdateChildren)
				if(component.Enabled) // We don't care if a component is visible as we want to update them either way so long as they are enabled
					component.Update(delta);
			
			Source!.Update(delta);
			return; // Base Update does nothing
		}

		public void DrawRenderTarget(GameTime delta)
		{
			// First set the render target
			Game!.GraphicsDevice.SetRenderTarget(RenderTarget); // We only get here (normally) after setting Game

			// Now clear the screen
			Game.GraphicsDevice.Clear(ClearColor);

			// Now start the local renderer (it's fine to make multiple SpriteRenderer Begin calls since they only set the GraphicsDevice state on End (unless using Immediate)
			LocalRenderer!.Transform = CameraInverse;
			LocalRenderer.Begin();
			
			foreach(IGUI component in DrawChildren)
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
			// Adding the component to TopComponents is top priority
			DummyGUI dummy = new DummyGUI(component.Name);

			dummy.UpdateOrder = component.UpdateOrder;
			dummy.DrawOrder = component.DrawOrder;

			// Add the component to our children lists
			// We MUST add to TopComponents first, since that lets us put things into our children
			if(!TopComponents.TryAdd(component.Name,dummy) || !UpdateChildren.Add(component) || !DrawChildren.Add(component))
				return false;

			// Now let's subscribe to key events (we will need named functions so we can unsubscribe on removal)
			component.UpdateOrderChanged += UpdateUpdateChildren;
			component.DrawOrderChanged += UpdateDrawChildren;

			// We do not need to bind the component's parent to this since we get to place the camera transform into the sprite batch's Begin call
			// We do, however, need to set its owner and renderer
			component.Owner = Owner;
			component.Renderer = LocalRenderer;

			// Lastly, initialize if we already are
			if(Initialized)
			{
				component.NotifyGameChange();
				component.Initialize();
			}

			return true;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in UpdateChildren.
		/// </summary>
		private void UpdateUpdateChildren(object? sender, EventArgs e)
		{
			if(sender is not IGUI component || !TopComponents.TryGetValue(component.Name,out DummyGUI? dummy))
				return;
			
			if(UpdateChildren.Remove(component))
			{
				dummy.UpdateOrder = component.UpdateOrder; // This lets us Add properly
				UpdateChildren.Add(component);
			}

			return;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in DrawChildren.
		/// </summary>
		private void UpdateDrawChildren(object? sender, EventArgs e)
		{
			if(sender is not IGUI component || !TopComponents.TryGetValue(component.Name,out DummyGUI? dummy))
				return;

			if(DrawChildren.Remove(component))
			{
				dummy.DrawOrder = component.DrawOrder; // This lets us Add properly
				DrawChildren.Add(component);
			}

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
			// We MUST do TopComponents last, since it lets us find things in our children
			if(!UpdateChildren.Remove(component) || !DrawChildren.Remove(component) || !TopComponents.Remove(component.Name))
				return false;

			// Now let's unsubscribe from key events
			component.UpdateOrderChanged -= UpdateUpdateChildren;
			component.DrawOrderChanged -= UpdateDrawChildren;

			// For good practice, we also void the component's owner (this will also void its state data containing this)
			// We will not, however, dispose of the component because it may be used later, and we'll let the finalizer take care of that
			component.Owner = null;

			return true;
		}

		public override void NotifyGameChange()
		{
			foreach(IGUI c in DrawChildren) // UpdateChildren = DrawChildren, just ordered differently
				c.NotifyGameChange();

			base.NotifyGameChange();
			return;
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
			foreach(IGUI child in DrawChildren)
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
			set
			{
				if(ReferenceEquals(base.Owner,value))
					return;

				base.Owner = value;

				foreach(IGUI child in UpdateChildren)
					child.Owner = value;

				foreach(IGUI child in DrawChildren)
					child.Owner = value;

				return;
			}
		}

		public override SpriteRenderer? Renderer
		{
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
		protected SpriteRenderer? LocalRenderer
		{
			get => _lr;
			
			set
			{
				if(ReferenceEquals(_lr,value) || value is null)
					return;
				
				_lr = value;

				// Children should use the local renderer
				foreach(IGUI component in DrawChildren)
					component.Renderer = LocalRenderer;

				return;
			}
		}

		protected SpriteRenderer? _lr;

		/// <summary>
		/// The color to clear the background of the render texture to.
		/// </summary>
		public Color ClearColor
		{get; set;}

		/// <summary>
		/// This is the image game object that will actually draw this component.
		/// It's texture source is a render target.
		/// </summary>
		public ImageGameObject? Source
		{get; protected set;}

		/// <summary>
		/// The camera transform.
		/// This is distinct from the world transform.
		/// Transform moves this GUI component around in world space.
		/// Camera moves this GUI component's contents around in its sub-world space.
		/// <para/>
		/// This matrix will be inverted at draw time so that camera movement corresponds to natural movement.
		/// i.e. moving the camera down moves the view down instead of the contents down, thus moving the contents up.
		/// </summary>
		public Matrix2D Camera
		{
			get => _c;

			set
			{
				_c = value;
				CameraInverseStale = true;
			}
		}
		
		private Matrix2D _c;

		/// <summary>
		/// The inverse camera matrix used for drawing in LocalRenderer.
		/// </summary>
		private Matrix CameraInverse
		{
			get
			{
				if(CameraInverseStale)
					_ci = _c.Invert().SwapChirality();

				return _ci;
			}

			set
			{
				_ci = value;
				CameraInverseStale = false;

				return;
			}
		}

		private Matrix _ci;

		/// <summary>
		/// If true, then the camera inverse is stale.
		/// </summary>
		private bool CameraInverseStale
		{get; set;}

		/// <summary>
		/// The set of top components of this sorted by name.
		/// </summary>
		protected Dictionary<string,DummyGUI> TopComponents
		{get;}

		/// <summary>
		/// The children of this GUICore in update order.
		/// </summary>
		protected AVLSet<IGUI> UpdateChildren
		{get; init;}

		/// <summary>
		/// The children of this GUICore in draw order.
		/// </summary>
		protected AVLSet<IGUI> DrawChildren
		{get; init;}

		/// <summary>
		/// The number of top-level GUI components in this GUICore.
		/// </summary>
		public int Count => UpdateChildren.Count;



		/// <summary>
		/// The blend mode to draw with.
		/// <para/>
		/// This value defaults to NonPremultiplied (null defaults to AlphaBlend).
		/// </summary>
		public BlendState? Blend
		{
			get => LocalRenderer?.Blend;
			
			set
			{
				if(LocalRenderer is not null)
					LocalRenderer.Blend = value;

				return;
			}
		}

		/// <summary>
		/// The sampler wrap mode to draw with.
		/// <para/>
		/// This value defaults to LinearClamp (the null value default).
		/// </summary>
		public SamplerState? Wrap
		{
			get => LocalRenderer?.Wrap;
			
			set
			{
				if(LocalRenderer is not null)
					LocalRenderer.Wrap = value;

				return;
			}
		}

		/// <summary>
		/// The shader for to draw with.
		/// <para/>
		/// This value defaults to None (the null value default).
		/// </summary>
		public DepthStencilState? DepthStencil
		{
			get => LocalRenderer?.DepthStencil;
			
			set
			{
				if(LocalRenderer is not null)
					LocalRenderer.DepthStencil = value;

				return;
			}
		}

		/// <summary>
		/// The cull state to draw with.
		/// <para/>
		/// This value defaults to CullCounterClockwise (the null value default).
		/// </summary>
		public RasterizerState? Cull
		{
			get => LocalRenderer?.Cull;
			
			set
			{
				if(LocalRenderer is not null)
					LocalRenderer.Cull = value;

				return;
			}
		}

		/// <summary>
		/// The shader to draw with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite Effect).
		/// </summary>
		public Effect? Shader
		{
			get => LocalRenderer?.Shader;
			
			set
			{
				if(LocalRenderer is not null)
					LocalRenderer.Shader = value;

				return;
			}
		}

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

				foreach(IGUI component in DrawChildren)
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

				if(RenderTargetDrawOrderChanged is not null)
					RenderTargetDrawOrderChanged(this,new OrderChangeEvent(old,_rtdo));

				return;
			}
		}

		protected int _rtdo;

		public event EventHandler<EventArgs>? RenderTargetDrawOrderChanged;
	}
}
