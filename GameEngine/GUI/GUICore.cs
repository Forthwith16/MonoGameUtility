using GameEngine.Resources.Sprites;
using GameEngine.DataStructures.Sets;
using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.GUI.Components;
using GameEngine.GUI.Map;
using GameEngine.Input;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.EnumExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI
{
	/// <summary>
	/// The core engine that makes GUI components run.
	/// Users should add IGUI objects to this class, which will render them together.
	/// <para/>
	/// GUI components added to a GUICore will have their Initialize method called by this but not their Dispose method.
	/// Disposal is left to the finalizer or other managers.
	/// <para/>
	/// Top level GUI components directly placed in a GUI Core must have unique names.
	/// Names should not be changed whilst added to a GUI Core.
	/// </summary>
	public class GUICore : DrawableAffineObject, IRenderTargetDrawable
	{
		/// <summary>
		/// Creates a new GUI core with all default bindings.
		/// </summary>
		/// <param name="renderer">The renderer for the GUI core. This can be changed later.</param>
		/// <param name="enable_digital">Iff true, we will allow digital inputs to control this GUI core.</param>
		/// <param name="enable_mouse">Iff true, we allow mouse inputs to control this GUI core.</param>
		/// <param name="render_resource">The resource file to load for this group's internal SpriteRenderer. If null, this will use the default set of values.</param>
		public GUICore(SpriteRenderer? renderer, bool enable_digital = true, bool enable_mouse = true, string? render_resource = null) : base(null,null)
		{
			Input = new InputManager();
			
			// Mouse inputs
			Input.AddReferenceInput(MouseClick,() => GlobalConstants.GUIMouseLeftClickDefault);
			Input.AddReferenceInput(MouseX,() => GlobalConstants.GUIMouseXAxis);
			Input.AddReferenceInput(MouseY,() => GlobalConstants.GUIMouseYAxis);
			Input.AddReferenceInput(MouseXDelta,() => GlobalConstants.GUIMouseXAxisDelta);
			Input.AddReferenceInput(MouseYDelta,() => GlobalConstants.GUIMouseYAxisDelta);
			
			// Digital inputs
			Input.AddReferenceInput(DigitalClick,() => GlobalConstants.GUIDigitalClick);
			Input.AddReferenceInput(DigitalUp,() => GlobalConstants.GUIDigitalUp);
			Input.AddReferenceInput(DigitalDown,() => GlobalConstants.GUIDigitalDown);
			Input.AddReferenceInput(DigitalLeft,() => GlobalConstants.GUIDigitalLeft);
			Input.AddReferenceInput(DigitalRight,() => GlobalConstants.GUIDigitalRight);

			// Set the renderer
			Renderer = renderer;

			// Initialize state variables
			TopComponents = new Dictionary<string,DummyGUI>();
			UpdateChildren = new AVLSet<IGUI>(new GUIOrderComparer(TopComponents,true));
			DrawChildren = new AVLSet<IGUI>(new GUIOrderComparer(TopComponents,false));

			UsingMouse = false;
			UsingDigital = false;

			EnableDigital = enable_digital;
			EnableMouse = enable_mouse;
			
			AttemptingClick = null;
			TooltipHovering = false;
			DisqualifiedTooltip = null;
			TooltipBestOffset = null;

			ActiveComponent = null;
			LastActiveComponent = null;
			FocusedComponent = null;
			
			Map = new GUIMap();
			Transform = Matrix2D.Identity;

			Camera = Matrix2D.Identity;
			CameraInverse = Matrix.Identity;

			ClearColor = Color.Transparent;

			// Make sure our events have dummy handlers
			OnActiveComponentChanged += (a,b) => {};
			OnFocusedComponentChanged += (a,b) => {};
			RenderTargetDrawOrderChanged += (a,b) => {};

			// Assign initial magic numbers
			_dcrl = 0.15f;
			_td = 0.5f;
			_rtdo = 0;
			
			// Assign source/layer info
			_s = null;
			_ld = 0.0f;
			
			LocalRenderPath = render_resource;
			return;
		}

		protected override void LoadContent()
		{
			Input.Initialize();

			// Load the render engine (children do not need to be loaded as Initialize is responsible for calling LoadContent)
			RenderTarget = new RenderTarget2D(Game!.GraphicsDevice,Game.GraphicsDevice.Viewport.Width,Game.GraphicsDevice.Viewport.Height);
			LocalRenderer = LocalRenderPath is null ? new SpriteRenderer(Game.GraphicsDevice) : Game.Content.Load<SpriteRenderer>(LocalRenderPath);
			
			Source = new ImageGameObject(Renderer,RenderTarget);
			Source.Parent = this;
			Source.Initialize();

			// Initialize all the children
			foreach(IGUI component in UpdateChildren)
				component.Initialize(); // Always initialize regardless of Enabled or Visible

			return;
		}

		public override void Update(GameTime delta)
		{
			// Reset our input flags first
			UsingNoInput = true;

			// Update input next
			Input.Update(delta);

			// Update the digital release timer if needed
			if(DigitalReleaseTimer > 0.0f)
			{
				// We are finishing up a digital click, so this must be the case
				UsingDigital = true;

				// If we hit the release timer, then release the click on the active component
				if((DigitalReleaseTimer -= (float)delta.ElapsedGameTime.TotalSeconds) <= 0.0f)
				{
					DigitalReleaseTimer = 0.0f;
					ActiveComponent!.PerformRelease(GenerateMouseClickEvent(true,MouseButton.Left,false));
					ActiveComponent.PerformClicked(GenerateMouseClickedEvent(true,MouseButton.Left)); // Digital clicks are always complete
				}
			}
			else // We aren't waiting on a digital release, so we can process input normally
			{
				// Record the old active component for delta work
				IGUI? old_active = ActiveComponent;

				// Check if the mouse did something (this takes priority over digital navigation)
				// These bindings evaluate to true if the mouse delta is nonzero
				if(Enabled)
					if(EnableMouse && (ActiveComponent is null || !ActiveComponent.SuspendsMouseNavigation) && (Input[MouseXDelta].CurrentDigitalValue || Input[MouseYDelta].CurrentDigitalValue || Input[MouseClick].IsRisingEdge || Input[MouseClick].IsFallingEdge))
						UsingMouse = true;
					else if(EnableDigital && (ActiveComponent is null || !ActiveComponent.SuspendsDigitalNavigation) && (Input[DigitalUp].IsRisingEdge || Input[DigitalDown].IsRisingEdge || Input[DigitalLeft].IsRisingEdge || Input[DigitalRight].IsRisingEdge || Input[DigitalClick].IsRisingEdge))
						UsingDigital = true;

				// We can blindly reset the tooltips flag if we've performed any input since they all disable it
				if(UsingInput)
				{
					// We do this here instead of inside TooltipHovering because we need this to update every time we use the input instead of only on falling edges of TooltipHovering
					if(ActiveComponent is null || ActiveComponent.Tooltip is not null && ActiveComponent.Tooltip.Visible)
						DisqualifiedTooltip = ActiveComponent;

					TooltipHovering = false;
				}

				// If we're using the mouse, we need to check out what we're pointing at (if anything)
				if(UsingMouse && (ActiveComponent is null || !ActiveComponent.SuspendsMouseNavigation))
				{
					// Process input
					// We need to figure out what GUI component we're actually interacting with
					// Our bounding box heirarchy is basically a poor man's AABB tree, so let's just brute force this
					int max = -1;
					IGUI? max_comp = null;
					
					// We have to include the camera matrix in our mouse position so that we don't need to think about view coordinates in our GUI components
					Vector2 mouse_pos = World * new Vector2(Input[MouseX].CurrentAnalogValue,Input[MouseY].CurrentAnalogValue);
					
					// Find the topmost level thing that we intersect with
					// In the case of DrawOrder ties, the thing with the earliest name is selected, which is fine
					foreach(IGUI component in DrawChildren)
						if(component.Enabled && component.Visible && component.Bounds.Contains(mouse_pos) && component.Contains(mouse_pos,out IGUI? intersector) && intersector!.DrawOrder > max)
						{
							max = intersector!.DrawOrder;
							max_comp = intersector;
						}

					// Set the active component
					ActiveComponent = max_comp;

					// Set the tooltip hovering flags if we need to and unset them if we're hovering over nothing
					if(ActiveComponent is not null && !ReferenceEquals(ActiveComponent,DisqualifiedTooltip))
					{
						TooltipHovering = true;
						TooltipTimestamp = TooltipDelay;
						TooltipBestOffset = null;
					}
				}
				else if(UsingDigital && (ActiveComponent is null || !ActiveComponent.SuspendsDigitalNavigation))
				{
					// If we have an active component, then we already hovered it, so we don't need to worry about that
					// What we need to do then is check if we have used a digital input
					// We'll do up/down movements first and then process left/right movements (we allow both to happen together)
					IGUI? proposed_active = LastActiveComponent;
					bool moved = false;
					bool is_active = LastActiveComponent is null || ActiveComponent is not null; // We only want to move from a null state if we don't remember one to reactive on input

					// Up/down comes first
					if(Input[DigitalUp].IsRisingEdge && !Input[DigitalDown].IsRisingEdge)
					{
						if(is_active)
							proposed_active = Map.GetNext(proposed_active,GUIMapDirection.UP);

						moved = true;
					}
					else if(Input[DigitalDown].IsRisingEdge && !Input[DigitalUp].IsRisingEdge)
					{
						if(is_active)
							proposed_active = Map.GetNext(proposed_active,GUIMapDirection.DOWN);

						moved = true;
					}

					// Left/right comes next
					if(Input[DigitalLeft].IsRisingEdge && !Input[DigitalRight].IsRisingEdge)
					{
						if(is_active)
							proposed_active = Map.GetNext(proposed_active,GUIMapDirection.LEFT);

						moved = true;
					}
					else if(Input[DigitalRight].IsRisingEdge && !Input[DigitalLeft].IsRisingEdge)
					{
						if(is_active)
							proposed_active = Map.GetNext(proposed_active,GUIMapDirection.RIGHT);

						moved = true;
					}

					// We only want to make a component active if we did some work (and it will remain active after)
					if(moved)
					{
						// If we used the digital controls, then we should void the mouse's attempt to click something
						AttemptingClick = null;

						if(!is_active) // If we lost our active component but remember one, then we should reactivate it
							ActiveComponent = LastActiveComponent;
						else if(proposed_active is not null) // The active component should never be assigned to null once it leaves it via digital navigation
							ActiveComponent = proposed_active;
					}
				}

				// We only need to do the following logic if we used some sort of input
				if(UsingInput)
				{
					// We only need to do Enter/Exit logic if the active component changed
					if(old_active != ActiveComponent)
					{
						// We changed active components, so we can blindly set this to null and update it afterward
						// This is true regardless of whether we are using the mouse or digital controls
						AttemptingClick = null;

						// Trigger an Exit if needed
						if(old_active is not null && old_active.Enabled && old_active.Visible) // When GUI components become inactive or invisible, they're responsible for dealing with that state change
							old_active.PerformExit(GenerateMouseHoverEvent(UsingDigital,false));

						// Trigger a Hover if needed
						if(ActiveComponent is not null && ActiveComponent.Enabled) // Active components are always enabled and visible
							ActiveComponent.PerformHover(GenerateMouseHoverEvent(UsingDigital,true));
					}
					else if(ActiveComponent is not null)
					{
						MouseMoveEventArgs args = GenerateMouseMoveEvent(UsingDigital);

						if(args.Moved && ActiveComponent.Enabled)
							ActiveComponent.PerformMove(args);
					}
					
					// Trigger a click of some variety if needed
					if(ActiveComponent is null)
					{
						// A click can only come from a mouse, since a digital click must have an active component
						if(UsingMouse && Input[MouseClick].IsRisingEdge)
						{
							// When the active component is null but we clicked, then we should make the focused component null
							IGUI? lfc = FocusedComponent;
							FocusedComponent = null;

							OnFocusedComponentChanged(this,new FocusedComponentChangedEvent(this,null,lfc,true));
						}
					}
					else if(ActiveComponent.Enabled) // ActiveComponent is not null in this case
						if(UsingMouse)
						{
							// Presses
							if(Input[MouseClick].IsRisingEdge)
							{
								AttemptingClick = ActiveComponent;
								AttemptingClickButton = MouseButton.Left;

								// Update our focused component before we perform the click
								IGUI? lfc = FocusedComponent;
								FocusedComponent = ActiveComponent;

								OnFocusedComponentChanged(this,new FocusedComponentChangedEvent(this,FocusedComponent,lfc,true));

								// Now we can go through with the click
								ActiveComponent.PerformClick(GenerateMouseClickEvent(false,MouseButton.Left,true));
							}

							// Releases
							if(Input[MouseClick].IsFallingEdge)
							{
								ActiveComponent.PerformRelease(GenerateMouseClickEvent(false,MouseButton.Left,false));

								if(AttemptingClick == ActiveComponent)
								{
									AttemptingClick.PerformClicked(GenerateMouseClickedEvent(false,AttemptingClickButton));
									AttemptingClick = null;
								}
							}
						}
						else if(UsingDigital) // We've precalculated if we pressed a digital button
							if(Input[DigitalClick].IsRisingEdge) // Presses
							{
								// Update our focused component before we perform the click
								IGUI? lfc = FocusedComponent;
								FocusedComponent = ActiveComponent;

								OnFocusedComponentChanged(this,new FocusedComponentChangedEvent(this,FocusedComponent,lfc,false));

								// Now we can tgo through with the click
								ActiveComponent.PerformClick(GenerateMouseClickEvent(true,MouseButton.Left,true));
								DigitalReleaseTimer = DigitalClickReleaseLag;
							}
				}

				// Record the last known active component if we have one in case we need it for digital input later
				if(ActiveComponent is not null)
					LastActiveComponent = ActiveComponent;

				// Lastly, let everyone know that something changed if necessary
				if(old_active != ActiveComponent)
					OnActiveComponentChanged(this,new ActiveComponentChangedEvent(this,ActiveComponent,old_active,UsingMouse));

				// If we've done nothing, then we may need to pop up some tooltips
				// We do this last so that the new state has settled
				if(UsingNoInput && TooltipHovering)
				{
					// This has the potential to underflow eventually, but the user deserves it if it ever happens
					TooltipTimestamp -= (float)delta.ElapsedGameTime.TotalSeconds;

					// If it's time, make the tooltip visible
					// ActiveComponent is never null when TooltipHovering is true
					// We do not skip these calculations when the tooltip is already visible because the World might move it out from under us
					// The tooltip is being drawn using the camera, after all, so we need to make sure it's transform will always undo the camera
					if(TooltipTimestamp <= 0.0f && ActiveComponent!.Enabled && ActiveComponent.Tooltip is not null)
					{
						// If we've already calculated the best offset, just keep using it
						// It can't change, since it only depends on the screen and mouse
						// The only exception to this is changing the screen resolution, but this is a rare event that should disrupt the tooltip anyway in most cases
						if(TooltipBestOffset is null)
						{
							// We need the screen boundaries
							Rectangle screen = Game!.GraphicsDevice.Viewport.Bounds; // We shouldn't ever get here without Game being set

							// We need to position the tooltip so that it's near the mouse and actually readable
							// How we get the readable part depends on where in the screen it would be drawn, how big it is, and how big the screen is
							// We will look for the result that gives us maximum intersection with the screen rectangle, preferring down > right > left > up
							Rectangle mb = ((RenderTargetFriendlyGame)Game).Mouse!.Bounds; // Mouse bounds are always given in screen coordinates

							// Calculate the initial offset
							TooltipBestOffset = mb.BottomLeft();
							
							// Calculate the best bounds we have so far
							Rectangle bounds = ActiveComponent.Tooltip.Bounds;
							bounds.Offset(TooltipBestOffset.Value - bounds.Location);
							
							// We only need to do more if our most preferred bounds is not fully contained within the screen
							if(!screen.Contains(bounds))
							{
								// Record the best area so far
								Rectangle.Intersect(ref bounds,ref screen,out Rectangle area);
								int best_area = area.Area();

								// Shift the bounds to the default
								bounds.Offset(TooltipBestOffset.Value.UnaryMinus()); // Points don't understand the unary minus...

								// Calculate all of the possible locations we want to check for suitability in the most preferred order
								// Worst-case scenario (nothing fits on screen or even mostly fits), we align to the top left of the screen, so (0,0) comes last
								Point[] possible_offsets = {mb.BottomLeft() - new Point(bounds.Width - area.Width,0),
													   mb.TopLeft() - new Point(0,bounds.Height),
													   mb.TopLeft() - new Point(bounds.Width - area.Width,bounds.Height),
													   Point.Zero};
								
								foreach(Point p in possible_offsets)
								{
									// Shift the bounds to this point
									bounds.Offset(p);
									
									// If this offset is contained within the screen, skip everything that follows
									if(screen.Contains(bounds))
									{
										TooltipBestOffset = p;
										break;
									}

									// Calculate the area of this shift
									Rectangle.Intersect(ref bounds,ref screen,out area);
									
									// If we have a greater area, record it
									if(best_area < area.Area())
									{
										best_area = area.Area();
										TooltipBestOffset = p;
									}

									// Undo the bound shift for this point
									bounds.Offset(p.UnaryMinus()); // Points don't understand the unary minus...
								}
							}
						}

						// Position the tooltip before making it visible just in case something weird happens with the draw cycle
						// The World matrix is a camera matrix, which is backwards, so multiplying by it undoes everything it does
						// This will put the tooltip where the mouse seems to be on the screen (plus the translation to the relevant position)
						ActiveComponent.Tooltip.Transform = World * Matrix2D.Translation(TooltipBestOffset.Value.ToVector2());
						
						// Lastly, we make the tooltip visible to finish up now that it's in place
						ActiveComponent.Tooltip.Visible = true;
					}
				}
			}
			
			// Update each child
			foreach(IGUI component in UpdateChildren)
				if(component.Enabled) // We don't care if a component is visible as we want to update them either way so long as they are enabled
					component.Update(delta);

			// Update the source, though it probably doesn't need to be
			Source!.Update(delta);

			return;
		}

		#region Mouse Event Generation
		/// <summary>
		/// Generates a mouse click/release event.
		/// </summary>
		/// <param name="digital">If true, then this was a digital click. Otherwise this was a true mouse click.</param>
		/// <param name="b">The mouse button in question.</param>
		/// <param name="pressed">If true, then the mouse button was clicked. If false, then the mouse button was released.</param>
		/// <returns>Returns a new mouse event encoding all of the given information.</returns>
		protected MouseClickEventArgs GenerateMouseClickEvent(bool digital, MouseButton b, bool pressed)
		{
			if(digital)
			{
				Vector2 pos = ActiveComponent!.GetAffinePosition();
				int dx = 0, dy = 0;

				if(Input[DigitalRight].CurrentDigitalValue)
					dx++;

				if(Input[DigitalLeft].CurrentDigitalValue)
					dx--;

				if(Input[DigitalDown].CurrentDigitalValue)
					dy++;

				if(Input[DigitalUp].CurrentDigitalValue)
					dy--;

				return new MouseClickEventArgs(b,pressed,true,b == MouseButton.Left && pressed,(int)pos.X,(int)pos.Y,dx,dy);
			}
			
			// Translate the mouse position out of view coordinates
			Vector2 pos2 = World * new Vector2(Input[MouseX].CurrentAnalogValue,Input[MouseY].CurrentAnalogValue);

			return new MouseClickEventArgs(b,pressed,false,Input[MouseClick].CurrentDigitalValue,(int)pos2.X,(int)pos2.Y,(int)Input[MouseXDelta].CurrentAnalogValue,(int)Input[MouseYDelta].CurrentAnalogValue);
		}

		/// <summary>
		/// Generates a mouse clicked event (a true mouse click that has been pressed and then released without exiting).
		/// </summary>
		/// <param name="digital">If true, then this was a digital click. Otherwise this was a true mouse click.</param>
		/// <param name="b">The mouse button in question.</param>
		/// <returns>Returns a new mouse event encoding all of the given information.</returns>
		protected MouseClickedEventArgs GenerateMouseClickedEvent(bool digital, MouseButton b)
		{
			if(digital)
			{
				Vector2 pos = ActiveComponent!.GetAffinePosition();
				int dx = 0, dy = 0;

				if(Input[DigitalRight].CurrentDigitalValue)
					dx++;

				if(Input[DigitalLeft].CurrentDigitalValue)
					dx--;

				if(Input[DigitalDown].CurrentDigitalValue)
					dy++;

				if(Input[DigitalUp].CurrentDigitalValue)
					dy--;

				return new MouseClickedEventArgs(b,true,false,(int)pos.X,(int)pos.Y,dx,dy);
			}
			
			// Translate the mouse position out of view coordinates
			Vector2 pos2 = World * new Vector2(Input[MouseX].CurrentAnalogValue,Input[MouseY].CurrentAnalogValue);

			return new MouseClickedEventArgs(b,false,false,(int)pos2.X,(int)pos2.Y,(int)Input[MouseXDelta].CurrentAnalogValue,(int)Input[MouseYDelta].CurrentAnalogValue);
		}

		/// <summary>
		/// Generates a mouse hover/exit event resulting from a jump.
		/// </summary>
		/// <param name="hover">If true, then the mouse entered something. If false, then the mouse exited something.</param>
		/// <returns>Returns a new mouse event encoding all of the given information.</returns>
		protected MouseHoverEventArgs GenerateMouseHoverEvent(bool hover)
		{
			Vector2 pos = ActiveComponent!.GetAffinePosition();
			return new MouseHoverEventArgs(hover,false,(int)pos.X,(int)pos.Y,0,0);
		}

		/// <summary>
		/// Generates a mouse hover/exit event.
		/// </summary>
		/// <param name="digital">If true, then this was a digital hover. Otherwise this was a true mouse hover.</param>
		/// <param name="hover">If true, then the mouse entered something. If false, then the mouse exited something.</param>
		/// <returns>Returns a new mouse event encoding all of the given information.</returns>
		protected MouseHoverEventArgs GenerateMouseHoverEvent(bool digital, bool hover)
		{
			if(digital)
			{
				Vector2 pos = ActiveComponent!.GetAffinePosition();
				int dx = 0, dy = 0;

				if(Input[DigitalRight].CurrentDigitalValue)
					dx++;

				if(Input[DigitalLeft].CurrentDigitalValue)
					dx--;

				if(Input[DigitalDown].CurrentDigitalValue)
					dy++;

				if(Input[DigitalUp].CurrentDigitalValue)
					dy--;

				return new MouseHoverEventArgs(hover,true,false,(int)pos.X,(int)pos.Y,dx,dy);
			}

			// Translate the mouse position out of view coordinates
			Vector2 pos2 = World * new Vector2(Input[MouseX].CurrentAnalogValue,Input[MouseY].CurrentAnalogValue);

			return new MouseHoverEventArgs(hover,false,Input[MouseClick].CurrentDigitalValue,(int)pos2.X,(int)pos2.Y,(int)Input[MouseXDelta].CurrentAnalogValue,(int)Input[MouseYDelta].CurrentAnalogValue);
		}

		/// <summary>
		/// Generates a mouse move event.
		/// </summary>
		/// <param name="digital">If true, then this was a digital move. Otherwise this was a true mouse move.</param>
		/// <returns>Returns a new mouse event encoding all of the given information.</returns>
		protected MouseMoveEventArgs GenerateMouseMoveEvent(bool digital)
		{
			if(digital)
			{
				Vector2 pos = ActiveComponent!.GetAffinePosition();
				int dx = 0, dy = 0;

				if(Input[DigitalRight].CurrentDigitalValue)
					dx++;

				if(Input[DigitalLeft].CurrentDigitalValue)
					dx--;

				if(Input[DigitalDown].CurrentDigitalValue)
					dy++;

				if(Input[DigitalUp].CurrentDigitalValue)
					dy--;

				return new MouseMoveEventArgs(true,false,(int)pos.X,(int)pos.Y,dx,dy);
			}

			// Translate the mouse position out of view coordinates
			Vector2 pos2 = World * new Vector2(Input[MouseX].CurrentAnalogValue,Input[MouseY].CurrentAnalogValue);

			return new MouseMoveEventArgs(false,Input[MouseClick].CurrentDigitalValue,(int)pos2.X,(int)pos2.Y,(int)Input[MouseXDelta].CurrentAnalogValue,(int)Input[MouseYDelta].CurrentAnalogValue);
		}
		#endregion

		public void DrawRenderTarget(GameTime delta)
		{
			// We can sometimes get here before LoadContent does its job...apparently
			if(LocalRenderer is null)
				return;

			// First set the render target
			Game!.GraphicsDevice.SetRenderTarget(RenderTarget); // We should only get here if Game is set

			// Now clear the screen
			Game.GraphicsDevice.Clear(ClearColor);

			// We need to invert the camera matrix so that the camera behaves naturally (e.g. moving the camera up moves the world down)
			LocalRenderer!.Transform = CameraInverse;
			LocalRenderer.Begin();
			
			foreach(IGUI component in DrawChildren)
				if(component.Visible) // We don't care if a GUI component is enabled here since they can still be draw but are greyed out or whatever
					component.Draw(delta);

			LocalRenderer.End();
			return;
		}

		public override void Draw(GameTime delta)
		{
			Source!.Draw(delta); // We only need to draw our render texture, and Source is always set when loading
			return;
		}

		/// <summary>
		/// Adds a top-level GUI component to this GUICore.
		/// </summary>
		/// <param name="component">The component to add. This component must not already be added to this GUICore for this add to succeed.</param>
		/// <returns>Returns true if the component could be added and false otherwise.</returns>
		/// <remarks>The GUI component's parent is <i>not</i> set to this since the GUICore's transform represents the camera matrix.</remarks>
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
			component.Owner = this;
			component.Renderer = LocalRenderer;

			// Add a vertex to the map for the new vertex
			AddToMap(component);

			// Lastly, initialize if we already are
			if(Initialized)
			{
				component.NotifyGameChange(); // We (normally) only are initialized if we are added to the game
				component.Initialize();
			}

			return true;
		}

		/// <summary>
		/// Adds a sequence of top-level GUI components to this GUICore.
		/// </summary>
		/// <param name="components">The components to add. Each must not be part of this GUICore already in order to succeed.</param>
		/// <returns>Returns true if at least one component was added and false otherwise.</returns>
		public bool AddAll(IEnumerable<IGUI> components)
		{
			bool ret = false;

			foreach(IGUI component in components)
				ret |= Add(component);

			return ret;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in UpdateChildren.
		/// </summary>
		private void UpdateUpdateChildren(object? sender, EventArgs e)
		{
			if(sender is not IGUI component || !TopComponents.TryGetValue(component.Name,out DummyGUI? dummy))
				return;
			
			if(UpdateChildren.Remove(dummy)) // This should never fail, but just in case
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

			if(DrawChildren.Remove(dummy)) // This should never fail, but just in case
			{
				dummy.DrawOrder = component.DrawOrder; // This lets us Add properly
				DrawChildren.Add(component);
			}

			return;
		}

		/// <summary>
		/// Removes a top-level GUI component from this GUICore.
		/// <para/>
		/// This can also be used to remove children components from the map and GUICore state.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if <paramref name="component"/> was removed and false otherwise.</returns>
		public bool Remove(IGUI component)
		{
			// We should do nothing if we are not the component's owner
			if(component.Owner != this)
				return false;

			// Expel the component from our records
			VoidComponent(component);

			// Remove the corresponding vertex from the map
			Map.RemoveVertex(component);

			// Remove the component from our children lists
			// We MUST do TopComponents last, since it lets us find things in our children
			if(!UpdateChildren.Remove(component) || !DrawChildren.Remove(component) || !TopComponents.Remove(component.Name))
				return false;

			// Now let's unsubscribe to key events
			component.UpdateOrderChanged -= UpdateUpdateChildren;
			component.DrawOrderChanged -= UpdateDrawChildren;

			// WE MOVED THIS FROM THE TOP WITH THE CURRENT IF STATEMENT
			// We will always void the component's owner (if it is this) and void its usage in this GUICore regardless of whether it is a top level component or hidden as a child component
			// We void the component's owner blindly though this may have already happened via remove a component from its parent component
			// We will not, however, dispose of the component because it may be used later, and we'll let the finalizer take care of that
			component.Owner = null;

			return true;
		}

		/// <summary>
		/// Removes a sequence of top-level GUI components from this GUICore.
		/// </summary>
		/// <param name="components">The components to remove.</param>
		/// <returns>Returns true if at least one component was removed from this and false otherwise.</returns>
		public bool RemoveAll(IEnumerable<IGUI> components)
		{
			bool ret = false;

			foreach(IGUI component in components)
				ret |= Remove(component);

			return ret;
		}

		/// <summary>
		/// Removes all components from this GUICore.
		/// This will not change any other settings nor remove events.
		/// </summary>
		/// <remarks>If this somehow fails, returns false. Otherwise, returns true in all circumstances.</remarks>
		public bool Clear()
		{
			while(UpdateChildren.Count > 0)
				if(!Remove(UpdateChildren.First()))
					return false;

			return true;
		}

		/// <summary>
		/// Jumps the active component to the one provided.
		/// </summary>
		/// <param name="component">The component to jump to. If this is null, the active component will be set to null.</param>
		/// <returns>Returns true if the jump was successful and false otherwise.</returns>
		/// <remarks>This <b>does not</b> trigger an OnFocusedComponentChanged event since that only occurs when a click occurs.</remarks>
		public bool JumpToComponent(IGUI? component)
		{
			// If the component we were provided is not owned by this, do nothing
			if(component is not null && component.Owner != this)
				return false;

			// If we're not changing anything, don't bother and just return a vacuous true
			if(ActiveComponent == component)
				return true;
			
			IGUI? old_active = ActiveComponent;
			ActiveComponent = component;

			// If we're NOT assigning null to the active component, we need to update LastActiveComponent, since it remebers the last nonnull active component
			// Otherwise, we can leave LastActiveComponent alone
			if(component is not null)
				LastActiveComponent = ActiveComponent;

			// We need to issue exit and hover events if necessary
			if(old_active is not null && old_active.Enabled && old_active.Visible) // When GUI components become inactive or invisible, they're responsible for dealing with that state change
				old_active.PerformExit(GenerateMouseHoverEvent(false));

			// Trigger a Hover if needed
			if(ActiveComponent is not null && ActiveComponent.Enabled) // Active components are always enabled and visible
				ActiveComponent.PerformHover(GenerateMouseHoverEvent(true));

			OnActiveComponentChanged(this,new ActiveComponentChangedEvent(this,ActiveComponent,old_active));
			return true;
		}

		/// <summary>
		/// Adds a (potentially hidden to the GUICore) component to this GUICore's navigation map.
		/// </summary>
		/// <param name="component">The component to add.</param>
		/// <returns>Returns true if the component was added and false otherwise.</returns>
		public bool AddToMap(IGUI component)
		{return Map.AddVertex(component);}

		/// <summary>
		/// Removes any GUICore state information that utilizes <paramref name="component"/>.
		/// <para/>
		/// This is primarily intended for GUI components that contain children to void active component information.
		/// </summary>
		/// <param name="component">The component whose state information we should expel from this GUICore (if any).</param>
		public void VoidComponent(IGUI component)
		{
			// Remove any bogus state information
			if(ActiveComponent == component)
				ActiveComponent = null;

			if(LastActiveComponent == component)
			{
				if(TooltipHovering)
					TooltipHovering = false;

				LastActiveComponent = null;
			}

			return;
		}

		/// <summary>
		/// Connects the component <paramref name="src"/> to component <paramref name="dst"/> in the direction of <paramref name="dir"/>.
		/// This will cause them to be traversable with the GUICore's digital input system.
		/// <para/>
		/// If <paramref name="src"/> or <paramref name="dst"/> do not belong to this GUICore, then no change will be made.
		/// </summary>
		/// <param name="src">The component to leave. If this is null, it will set the null/unknown movement.</param>
		/// <param name="dst">The component to go to. If this value is null, it will go to the null/unknown state.</param>
		/// <param name="dir">The digital direction.</param>
		/// <param name="symmetric">If true, then the reverse edge from <paramref name="dst"/> to <paramref name="src"/> will be added as well.</param>
		/// <returns>Returns true if the connection was made and false otherwise.</returns>
		public bool ConnectComponents(IGUI? src, IGUI? dst, GUIMapDirection dir, bool symmetric = false)
		{
			// Make sure we have things that this owns before we try to do anything
			if(src is not null && src.Owner != this || dst is not null && dst.Owner != this)
				return false;

			// SetEdge will take care of all of the null check logic to add things appropriately
			Map.SetEdge(dir,src,dst);

			if(symmetric)
				Map.SetEdge(dir.Reflect(),dst,src);
			
			return true;
		}

		/// <summary>
		/// Obtains the digital connection out of <paramref name="src"/> in the direction of <paramref name="dir"/>.
		/// </summary>
		/// <param name="src">The GUI component to obtain an outbound connection from.</param>
		/// <param name="dir">The direction to look for an outbound connection.</param>
		/// <returns>Returns the desired outbound direction. This may be null.</returns>
		public IGUI? GetConnection(IGUI? src, GUIMapDirection dir)
		{return Map.GetNext(src,dir);}

		/// <summary>
		/// Determines if there is a digital connection out of <paramref name="src"/> in the direction of <paramref name="dir"/>.
		/// </summary>
		/// <param name="src">The GUI component to check for an outbound connection.</param>
		/// <param name="dir">The direction to look for an outbound connection.</param>
		/// <returns>Returns true if there is the desired outbound direction and false otherwise.</returns>
		public bool HasConnection(IGUI? src, GUIMapDirection dir)
		{return Map.GetNext(src,dir) is not null;} // Digital navigation never goes to null once it leaves it

		/// <summary>
		/// The set of top components of this sorted by name.
		/// </summary>
		protected Dictionary<string,DummyGUI> TopComponents
		{get;}

		/// <summary>
		/// The children of this GUICore in update order.
		/// </summary>
		protected AVLSet<IGUI> UpdateChildren
		{get;}

		/// <summary>
		/// The children of this GUICore in draw order.
		/// </summary>
		protected AVLSet<IGUI> DrawChildren
		{get;}

		/// <summary>
		/// The number of top-level GUI components in this GUICore.
		/// </summary>
		public int Count => UpdateChildren.Count;

		/// <summary>
		/// The active GUI component (if any).
		/// </summary>
		public IGUI? ActiveComponent
		{get; protected set;}

		/// <summary>
		/// The last known active GUI component (if any).
		/// </summary>
		public IGUI? LastActiveComponent
		{get; protected set;}

		/// <summary>
		/// This is the current focused component.
		/// <para/>
		/// The focused component is the last component clicked on.
		/// </summary>
		public IGUI? FocusedComponent
		{get; protected set;}

		/// <summary>
		/// The map for digital control over this GUICore.
		/// </summary>
		protected GUIMap Map
		{get; set;}

		public override RenderTargetFriendlyGame? Game
		{
			get => base.Game;

			protected internal set
			{
				base.Game = value;

				foreach(IGUI c in DrawChildren) // UpdateChildren = DrawChildren, just ordered differently
					c.NotifyGameChange();

				return;
			}
		}

		public override SpriteRenderer? Renderer
		{
			get => base.Renderer;

			set
			{
				if(Source is not null)
					Source.Renderer = value;

				base.Renderer = value;
				return;
			}
		}

		/// <summary>
		/// The SpriteRenderer for all GUI drawing in this GUICore.
		/// </summary>
		public SpriteRenderer? LocalRenderer
		{
			get => _lr;
			
			protected set
			{
				if(ReferenceEquals(_lr,value) || value is null)
					return;

				_lr = value;

				foreach(IGUI component in DrawChildren)
					component.Renderer = value;
				
				return;
			}
		}

		protected SpriteRenderer? _lr;

		/// <summary>
		/// The path to the local renderer resource.
		/// If this is null, then the default renderer will be used instead.
		/// </summary>
		private string? LocalRenderPath
		{get;}

		/// <summary>
		/// The texture that we will render to.
		/// </summary>
		protected RenderTarget2D? RenderTarget
		{get; set;}

		public int RenderTargetDrawOrder
		{
			get => _rtdo;

			set
			{
				if(_rtdo == value)
					return;

				int old = _rtdo;
				_rtdo = value;

				RenderTargetDrawOrderChanged!(this,new OrderChangeEvent(_rtdo,old));
				return;
			}
		}

		protected int _rtdo;

		public override float LayerDepth
		{
			get => Source is null ? _ld : Source.LayerDepth;
			
			set
			{
				if(Source is null)
				{
					_ld = value;
					return;
				}

				Source.LayerDepth = value;
				return;
			}
		}

		private float _ld;

		/// <summary>
		/// The color to clear the background of the render texture to.
		/// This defaults to Color.Transparent.
		/// </summary>
		public Color ClearColor
		{get; set;}

		/// <summary>
		/// This is the image component that will actually draw this component.
		/// It's texture source is a render target.
		/// </summary>
		public ImageGameObject? Source
		{
			get => _s;
			
			protected set
			{
				float layer = _s is null ? _ld : _s.LayerDepth;

				_s = value;

				if(_s is not null)
					_s.LayerDepth = layer;

				return;
			}
		}

		private ImageGameObject? _s;

		/// <summary>
		/// Always returns SpriteEffects.None on get.
		/// <para/>
		/// Does nothing on set.
		/// </summary>
		public override SpriteEffects Effect
		{
			get => SpriteEffects.None;

			set
			{return;}
		}

		/// <summary>
		/// Always returns Color.White on get.
		/// <para/>
		/// Assigns provided color to each drawable component's Tint value on set.
		/// </summary>
		public override Color Tint
		{
			get => Color.White;

			set
			{
				// This happens when we are executing the base constructor
				if(DrawChildren is null)
					return;

				foreach(IGUI component in DrawChildren)
					component.Tint = value;
				
				return;
			}
		}

		/// <summary>
		/// The input manager for all GUI operations in this GUICore.
		/// </summary>
		public InputManager Input
		{get;}

		/// <summary>
		/// The blend mode to draw with.
		/// <para/>
		/// This value defaults to NonPremultiplied (the null value defaults to AlphaBlend).
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
		/// The width of the GUICore (sans transformations but including child transformations).
		/// </summary>
		public override int Width
		{
			get
			{
				// Go through all of the bounds and find the minimum and maximum x position
				// This technically includes transformations, but its not the GUICore's transformations, so whatevs
				int min = int.MaxValue;
				int max = int.MinValue;

				foreach(IGUI component in DrawChildren)
				{
					Rectangle b = component.Bounds;

					if(b.Left < min)
						min = b.Left;

					if(b.Right > max)
						max = b.Right;
				}

				return max - min;
			}
		}

		/// <summary>
		/// The height of the GUICore (sans transformations but including child transformations).
		/// </summary>
		public override int Height
		{
			get
			{
				// Go through all of the bounds and find the minimum and maximum y position
				// This technically includes transformations, but its not the GUICore's transformations, so whatevs
				int min = int.MaxValue;
				int max = int.MinValue;

				foreach(IGUI component in DrawChildren)
				{
					Rectangle b = component.Bounds;

					if(b.Top < min)
						min = b.Top;

					if(b.Bottom > max)
						max = b.Bottom;
				}

				return max - min;
			}
		}

		/// <summary>
		/// If true, digital inputs are allowed.
		/// If false, digital inputs may not control this GUICore (at least not directly).
		/// </summary>
		public bool EnableDigital
		{get; set;}

		/// <summary>
		/// If true, mouse inputs are allowed.
		/// If false, mouse inputs may not control this GUICore (at least not directly).
		/// </summary>
		public bool EnableMouse
		{get; set;}

		/// <summary>
		/// If true, we are currently using the mouse for GUI input.
		/// If false, then either UsingDigital is true or no input is being processed.
		/// </summary>
		protected bool UsingMouse
		{
			get => _um;
			
			set
			{
				_um = value;

				if(_um)
					UsingDigital = false;

				return;
			}
		}

		protected bool _um;

		/// <summary>
		/// If true, we are currently using digital input for GUI input.
		/// If false, then either UsingMouse is true or no input is being processed.
		/// </summary>
		protected bool UsingDigital
		{
			get => _ud;

			set
			{
				_ud = value;

				if(_ud)
					UsingMouse = false;

				return;
			}
		}

		protected bool _ud;

		/// <summary>
		/// If true, then we are currently processing an input (either mouse or digital).
		/// If false, then no input is being processed.
		/// <para/>
		/// Assigning this to false sets UsingMouse and UsingDigital to false.
		/// Assigning this to true does nothing.
		/// </summary>
		protected bool UsingInput
		{
			get => UsingMouse || UsingDigital;

			set
			{
				if(!value)
					UsingMouse = UsingDigital = false;

				return;
			}
		}

		/// <summary>
		/// If true, then no input is being processed.
		/// If false, then we are currently processing an input (either mouse or digital).
		/// <para/>
		/// Assigning this to true sets UsingMouse and UsingDigital to false.
		/// Assigning this to false does nothing.
		/// </summary>
		protected bool UsingNoInput
		{
			get => !(UsingMouse || UsingDigital);
			
			set
			{
				if(value)
					UsingMouse = UsingDigital = false;

				return;
			}
		}

		/// <summary>
		/// The length of time remaining until a digital click resolves.
		/// </summary>
		protected float DigitalReleaseTimer
		{get; set;}

		/// <summary>
		/// If this is not null, then we are attempting to fully click on something.
		/// This value is for mouse clicking and is not used for digital clicks, as they are always completed.
		/// </summary>
		protected IGUI? AttemptingClick
		{get; set;}

		/// <summary>
		/// This is the button we are attempting to click (if we are attempting to click anything).
		/// If AttemptingClick is null, this value has no meaning.
		/// </summary>
		protected MouseButton AttemptingClickButton
		{get; set;}

		/// <summary>
		/// The length of time after a digital click until it releases.
		/// </summary>
		public float DigitalClickReleaseLag
		{
			get => _dcrl;

			set
			{
				if(value < 0.0f)
					return;

				_dcrl = value;
				return;
			}
		}

		protected float _dcrl;

		/// <summary>
		/// This is the length of time the mouse must remain stationary over a GUI component to pop up its tooltip (if it has one).
		/// </summary>
		public float TooltipDelay
		{
			get => _td;

			set
			{
				if(value < 0.0f)
					return;

				_td = value;
				return;
			}
		}

		protected float _td;

		/// <summary>
		/// If true, then the mouse is hovering over a GUIcomponent and the timer is counting down until a tooltip pops up.
		/// Moving the mouse resets the timer when over a GUIcomponent and disables this if there is no ActiveComponent, while using digital navigation disables this.
		/// </summary>
		protected bool TooltipHovering
		{
			get => _th;

			set
			{
				// If we're changing nothing, don't bother with anything
				if(_th == value)
					return;

				// If we're disabling tooltips, then we must have had an ActiveComponent, and it gets copied into LastActiveComponent
				if(!value && _th && LastActiveComponent!.Tooltip is not null)
					LastActiveComponent.Tooltip.Visible = false;

				_th = value;
				return;
			}
		}

		protected bool _th;

		/// <summary>
		/// This is the length of time still required before a tooltip pops up.
		/// <para/>
		/// This value has no meaning if TooltipHovering is false.
		/// </summary>
		protected float TooltipTimestamp
		{get; set;}

		/// <summary>
		/// If a potential tooltip popup comes from this, it is disqualified from consideration.
		/// </summary>
		protected IGUI? DisqualifiedTooltip
		{get; set;}

		/// <summary>
		/// If this value exists, we've precalculated the best offset for a tooltip.
		/// </summary>
		protected Point? TooltipBestOffset
		{get; set;}

		/// <summary>
		/// This event is called whenever the active component of this GUICore changes.
		/// There may be some delay between when the active component is actually changed and when the change is fully processed and officially assigned.
		/// </summary>
		public event ActiveComponentChanged OnActiveComponentChanged;

		/// <summary>
		/// This event is called whenever the focused component of this GUICore changes.
		/// There may be some delay between when the focused component is actually changed and when the change is fully processed and officially assigned.
		/// </summary>
		public event FocusedComponentChanged OnFocusedComponentChanged;

		public event EventHandler<EventArgs>? RenderTargetDrawOrderChanged;

		/// <summary>
		/// The binding name for a mouse click.
		/// </summary>
		public const string MouseClick = "MC";

		/// <summary>
		/// The binding name for the mouse x position.
		/// </summary>
		public const string MouseX = "MX";

		/// <summary>
		/// The binding name for the mouse y position.
		/// </summary>
		public const string MouseY = "MY";

		/// <summary>
		/// The binding name for the mouse x position delta.
		/// </summary>
		public const string MouseXDelta = "MXD";

		/// <summary>
		/// The binding name for the mouse y position delta.
		/// </summary>
		public const string MouseYDelta = "MYD";

		/// <summary>
		/// The binding name for the digital up movement.
		/// </summary>
		public const string DigitalUp = "U";

		/// <summary>
		/// The binding name for the digital down movement.
		/// </summary>
		public const string DigitalDown = "D";

		/// <summary>
		/// The binding name for the digital left movement.
		/// </summary>
		public const string DigitalLeft = "L";

		/// <summary>
		/// The binding name for the digital right movement.
		/// </summary>
		public const string DigitalRight = "R";

		/// <summary>
		/// The binding name for the digital click.
		/// </summary>
		public const string DigitalClick = "C";
	}

	/// <summary>
	/// An event invoked when the active component of a GUICore changes.
	/// </summary>
	/// <param name="sender">The GUICore sending the event.</param>
	/// <param name="e">The event data.</param>
	public delegate void ActiveComponentChanged(GUICore sender, ActiveComponentChangedEvent e);

	/// <summary>
	/// An event invoked when the focused component of a GUICore changes.
	/// </summary>
	/// <param name="sender">The GUICore sending the event.</param>
	/// <param name="e">The event data.</param>
	public delegate void FocusedComponentChanged(GUICore sender, FocusedComponentChangedEvent e);
}
