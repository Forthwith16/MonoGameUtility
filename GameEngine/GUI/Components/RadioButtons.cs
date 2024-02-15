using GameEngine.DataStructures.Grid;
using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.GUI.Map;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A set of radio buttons.
	/// <para/>
	/// Digital navigation at the boundary when using default digital navigation is controlled by assigning navigational links out of this parent class.
	/// When digital navigation enters this class, it will be automatically redirected to, in this order of availablility: the selected button, the button closest to the origin, left on this.
	/// </summary>
	public class RadioButtons : GUIBase
	{
		/// <summary>
		/// Creates a new, empty set of radio buttons.
		/// </summary>
		/// <param name="game">The game these radio buttons will belong to.</param>
		/// <param name="name">The name of these radio buttons.</param>
		/// <param name="backgrounds">
		///	The backgrounds to display for the disabled and normal states.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	This radio button set will be responsible for initializing <paramref name="backgrounds"/> but will take no responsibility for disposing of it.
		///	<para/>
		///	If null is provided, there will be no backgrounds (they default to DummyAffineComponents).
		/// </param>
		/// <param name="h_padding">The horizontal padding between radio buttons.</param>
		/// <param name="v_padding">The vertical padding between radio buttons.</param>
		/// <param name="left_aligned">If true, then the origin (of radio button positions) will be on the left of the group. If false, it will be on the right.</param>
		/// <param name="top_aligned">If true, then the origin (of radio button positions) will be on the top of the group. If false, it will be on the bottom.</param>
		public RadioButtons(RenderTargetFriendlyGame game, string name, ComponentLibrary? backgrounds, float h_padding = 5.0f, float v_padding = 5.0f, bool left_aligned = true, bool top_aligned = true) : base(game,name,null)
		{
			// Create a space for our buttons
			Buttons = new Dictionary<string,Checkbox>();
			ButtonGrid = new InfiniteGrid<Checkbox>(true);
			
			// If we have no backgrounds provided, then we need to generate some dummies to avoid special casing null
			if(backgrounds is null)
			{
				ComponentLibrary dummies = new ComponentLibrary(game);

				dummies.Add(NormalState,new DummyAffineComponent(game));
				dummies.Add(DisabledState,new DummyAffineComponent(game));

				Backgrounds = dummies;
			}
			else
				Backgrounds = backgrounds;

			// Assign the alignment values
			_la = left_aligned;
			_ta = top_aligned;

			// Assign the padding values
			_hp = h_padding;
			_vp = v_padding;

			// It is a pain to keep track of if radio buttons change their bounding boxes, so let's just not bother
			// Instead, let's just blindly align the radio buttons again (it may not cost less, but it won't cost that much either)
			// It's not like we'll be doing it that frequently or that we'll ever have that many buttons involved
			SelectionChanged += (a,b,c) => AlignAllButtons();
			
			// The initial selection is nothing
			_srb = null;

			// Initialize the navigation variables
			_udn = true;

			DefaultExternalLeft = null;
			DefaultExternalRight = null;
			DefaultExternalUp = null;
			DefaultExternalDown = null;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			Backgrounds.Initialize();
			Backgrounds.Transform = Matrix2D.Identity;
			Backgrounds.Parent = this;
			Backgrounds.Renderer = Renderer;
			Backgrounds.LayerDepth = (this as IGUI).LayerDepth;
			Backgrounds.StateChange += (a,b,c) => AlignAllButtons();

			// The might (and should) be buttons already, and these are the sorts of things we only want to do once we're ready for initialization
			// So we just wait until initialization to do them
			foreach(Checkbox c in Buttons.Values)
			{
				c.Initialize();
				c.Parent = this;
				c.Renderer = Renderer;
				c.DrawOrder = DrawOrder + 1;

				// We don't need to add a handler to each c's StateChanged since SelectionChanged for this class will catch all of the state changes
				// This avoids the doubling up of a button being checked usually requiring another button to be unchecked
				// We also omit assigning each c's Transform because we will do that later with mandatory button alignments
				// We DO, however, need a handler for font and text changes
				if(c.Text is not null)
				{
					// Text's nullity/value never changes, so we only need to do this in init
					c.Text.TextChanged += (a,b,c) => AlignAllButtons();
					c.Text.FontChanged += (a,b,c) => AlignAllButtons();
				}
			}

			// We default to the normal state if we're enabled and the disabled state if not
			if(Enabled)
			{
				Backgrounds.ActiveComponentName = NormalState;
				
				foreach(Checkbox c in Buttons.Values)
					c.Enabled = true;
			}
			else
			{
				Backgrounds.ActiveComponentName = DisabledState;

				foreach(Checkbox c in Buttons.Values)
					c.Enabled = false;
			}

			// Keep the layer depth up to date
			DrawOrderChanged += (a,b) =>
			{
				Backgrounds.LayerDepth = (this as IGUI).LayerDepth;

				foreach(Checkbox c in Buttons.Values)
					c.DrawOrder = DrawOrder + 1;

				return;
			};

			// We need to enforce that only one thing is ever checked
			foreach(Checkbox c in Buttons.Values)
				c.StateChanged += (a,b) =>
				{
					if(b)
						SelectedRadioButton = c.Name;
					else if(SelectedRadioButton == c.Name) // !b
						a.Checked = true; // We can't unselect a radio button

					return;
				};

			// Initialize all of the events that we personally care about
			// The user may have additional interests which they can deal with on their own time
			// If they care about when the background changes, they can provide appropriate delegates to the backgrounds
			EnabledChanged += (a,b) =>
			{
				// We default to the normal state if we're enabled and the disabled state if not
				if(Enabled)
				{
					Backgrounds.ActiveComponentName = NormalState;

					foreach(Checkbox c in Buttons.Values)
						c.Enabled = true;
				}
				else
				{
					Backgrounds.ActiveComponentName = DisabledState;

					foreach(Checkbox c in Buttons.Values)
						c.Enabled = false;
				}

				return;
			};

			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			Backgrounds.Update(delta);

			foreach(Checkbox c in Buttons.Values)
				c.Update(delta);

			// We must ensure that the navigational links to external locations are kept up to date (if we have an owner)
			if(Owner is not null)
			{
				IGUI? left = Owner.GetConnection(this,GUIMapDirection.LEFT);
				bool l = DefaultExternalLeft != left;

				if(l)
					DefaultExternalLeft = left;

				IGUI? right = Owner.GetConnection(this,GUIMapDirection.RIGHT);
				bool r = DefaultExternalRight != right;

				if(r)
					DefaultExternalRight = right;

				IGUI? up = Owner.GetConnection(this,GUIMapDirection.UP);
				bool u = DefaultExternalUp != up;

				if(u)
					DefaultExternalUp = up;

				IGUI? down = Owner.GetConnection(this,GUIMapDirection.DOWN);
				bool d = DefaultExternalDown != down;

				if(d)
					DefaultExternalDown = down;

				// We can just blindly call our assignment function since it will check for pointlessness
				if(UseDefaultNavigation)
					AssignDefaultExternalEdges(l,r,u,d);
			}

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Backgrounds.Draw(delta);

			foreach(Checkbox c in Buttons.Values)
				c.Draw(delta);

			return;
		}

		/// <summary>
		/// Adds a radio button to the first available position.
		/// This order is determined by placing at the bottom of the first column that is not of maximum length.
		/// If all columns are equal length, then it is placed at the bottom of the frist column in a new row.
		/// </summary>
		/// <param name="c">The radio button to add.</param>
		/// <returns>Returns true if the radio button was added and false otherwise.</returns>
		public bool AddRadioButton(Checkbox c)
		{
			// Figure out where to put the radio button
			// We initially look for the first column that doesn't have maximum length
			int maxy = ButtonGrid.MaxY;
			int maxx = ButtonGrid.MaxX;

			Point p = Point.Zero; // To make the compiler stop complaining, we have to do this even though p clearly is initialized by the time we get to our use of it
			bool notdone = true;

			for(int x = 0;notdone && x <= maxx;x++)
				for(int y = 0;notdone && y <= maxy;y++)
					if(ButtonGrid.PositionEmpty(x,y))
					{
						p = new Point(x,y);
						notdone = false;
					}

			// If we didn't find a spot, we put it at the end of the first column in a new row
			if(notdone)
				p = new Point(0,maxy + 1);

			return AddRadioButton(c,p);
		}

		/// <summary>
		/// Adds a radio button to the specified position.
		/// Note that a radio button can only be added to an open position.
		/// </summary>
		/// <param name="c">The radio button to add.</param>
		/// <param name="p">
		///	The grid position to place the radio button.
		///	If this location is already occupied, then the radio button will be rejected.
		///	Similarly, if this position contains a negative index, the radio button will be rejected.
		/// </param>
		/// <returns>Returns true if the radio button was added and false otherwise.</returns>
		public bool AddRadioButton(Checkbox c, int x, int y)
		{return AddRadioButton(c,new Point(x,y));}

		/// <summary>
		/// Adds a radio button to the specified position.
		/// Note that a radio button can only be added to an open position.
		/// </summary>
		/// <param name="c">The radio button to add.</param>
		/// <param name="p">
		///	The grid position to place the radio button.
		///	If this location is already occupied, then the radio button will be rejected.
		///	Similarly, if this position contains a negative index, the radio button will be rejected.
		/// </param>
		/// <returns>Returns true if the radio button was added and false otherwise.</returns>
		public bool AddRadioButton(Checkbox c, Point p)
		{
			// We can't add radio buttons that are already present
			// If the position is occupied or if we're trying to add to a false location, then we can't do anything
			// If we fail to add the button to our map or our grid, then we also are stuck
			if(ButtonGrid.PositionOccupied(p) || Buttons.ContainsKey(c.Name) || ButtonGrid.ContainsItem(c) || !ButtonGrid.Add(p,c)) // ButtonGrid is faster with value lookups
				return false;
			
			// First assign the button's owner
			c.Owner = Owner;

			// If we are our owner's active component, then this must be the first button, so immediately redirect the active component to this button
			if(Owner is not null && Owner.ActiveComponent == this)
				Owner.JumpToComponent(c);

			// Add the button to the list for easy acccess
			Buttons.Add(c.Name,c);

			// Now we may need to stitch together the updated button map
			if(UseDefaultNavigation)
				AssignDefaultNavigationalLinks(c,true); // We need to update the links we change, so we make them symmetric here

			// If we're already initialized, we need to make sure the checkbox is ready to go
			if(Initialized)
			{
				c.Initialize();
				c.Parent = this;
				c.Renderer = Renderer;
				c.DrawOrder = DrawOrder + 1;

				if(c.Text is not null)
				{
					// Text's nullity/value never changes, so we only need to do this in init
					c.Text.TextChanged += (a,b,c) => AlignAllButtons();
					c.Text.FontChanged += (a,b,c) => AlignAllButtons();
				}
			}

			AlignAllButtons(); // Nothing will call this for us with an add, so we must
			return true;
		}

		/// <summary>
		/// Assigns to each radio button its default digital navigational links.
		/// It also initializes all null transitions to the radio button closest to the origin.
		/// <para/>
		/// Default digial navigation via the arrow keys will produce strict directional movement.
		/// In short, left goes left (and only left) to the nearest radio button (if one exists) and does not wrap around.
		/// Similarly for all other directions.
		/// </summary>
		public void AssignDefaultNavigationalLinks()
		{
			// If we have no buttons, we do nothing
			if(Buttons.Count == 0)
				return;

			// Assign the radio button links
			foreach(Checkbox c in Buttons.Values)
				AssignDefaultNavigationalLinks(c); // We don't make the links symmetric here because we're going to hit all of them anyway, so don't double assign

			return;
		}

		/// <summary>
		/// Selects the point closest to the origin that has a button.
		/// </summary>
		/// <returns>Returns the point closest to the origin or null if no such point exists.</returns>
		protected Point? FindBestOrigin()
		{
			// This is the point containing the button to link all null transitions to
			bool found_one = false;
			Point min = new Point(int.MaxValue,int.MaxValue);

			// Special case the origin since it's usually present
			if(ButtonGrid.PositionOccupied(0,0))
			{
				min = Point.Zero;
				found_one = true;
			}
			else // Determining if searching by distance or components is faster is a pain, so let's just search by components
				foreach(Point p in ButtonGrid.Positions)
					if(p.TaxicabDistance() < min.TaxicabDistance())
					{
						min = p;
						found_one = true;
					}

			return found_one ? min : null;
		}

		/// <summary>
		/// Assigns the default digital navigation links to <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The button to give default digital navigation to.</param>
		/// <param name="symmetric">If true, then the links are forced to be symmetric. If false, then the links are only added out of <paramref name="c"/>.</param>
		/// <see cref="AssignDefaultNavigationalLinks()"/>
		protected void AssignDefaultNavigationalLinks(Checkbox c, bool symmetric = false)
		{
			// If we have no owner, get lost
			// If we're asking this to be done for something not in the grid, get lost
			if(Owner is null || !ButtonGrid.TryGetPosition(c,out Point pos))
				return;
			
			// Now that we have a point to search from, we can hunt down the nearest radio button in each direction
			// Find something to the left
			int left = ButtonGrid.MinX;
			
			for(int i = pos.X - 1;i >= left;i--)
				if(ButtonGrid.PositionOccupied(i,pos.Y))
				{
					if(LeftAligned)
						Owner.ConnectComponents(c,ButtonGrid[i,pos.Y],GUIMapDirection.LEFT,symmetric);
					else // Right aligned
						Owner.ConnectComponents(c,ButtonGrid[i,pos.Y],GUIMapDirection.RIGHT,symmetric);

					break;
				}
				else if(i == left) // Set the external default left edge
					if(LeftAligned)
						Owner.ConnectComponents(c,DefaultExternalLeft,GUIMapDirection.LEFT);
					else // Right aligned
						Owner.ConnectComponents(c,DefaultExternalRight,GUIMapDirection.RIGHT);

			// Find something to the right
			int right = ButtonGrid.MaxX;

			for(int i = pos.X + 1;i <= right;i++)
				if(ButtonGrid.PositionOccupied(i,pos.Y))
				{
					if(LeftAligned)
						Owner.ConnectComponents(c,ButtonGrid[i,pos.Y],GUIMapDirection.RIGHT,symmetric);
					else // Right aligned
						Owner.ConnectComponents(c,ButtonGrid[i,pos.Y],GUIMapDirection.LEFT,symmetric);

					break;
				}
				else if(i == right) // Set the external default right edge
					if(LeftAligned)
						Owner.ConnectComponents(c,DefaultExternalRight,GUIMapDirection.RIGHT);
					else // Right aligned
						Owner.ConnectComponents(c,DefaultExternalLeft,GUIMapDirection.LEFT);

			// Find something above
			int top = ButtonGrid.MinY;

			for(int i = pos.Y - 1;i >= top;i--)
				if(ButtonGrid.PositionOccupied(pos.X,i))
				{
					if(TopAligned)
						Owner.ConnectComponents(c,ButtonGrid[pos.X,i],GUIMapDirection.UP,symmetric);
					else // Bottom aligned
						Owner.ConnectComponents(c,ButtonGrid[pos.X,i],GUIMapDirection.DOWN,symmetric);

					break;
				}
				else if(i == top) // Set the external default up edge
					if(TopAligned)
						Owner.ConnectComponents(c,DefaultExternalUp,GUIMapDirection.UP);
					else // Bottom aligned
						Owner.ConnectComponents(c,DefaultExternalDown,GUIMapDirection.DOWN);

			// Find something below
			int bottom = ButtonGrid.MaxY;

			for(int i = pos.Y + 1;i <= bottom;i++)
				if(ButtonGrid.PositionOccupied(pos.X,i))
				{
					if(TopAligned)
						Owner.ConnectComponents(c,ButtonGrid[pos.X,i],GUIMapDirection.DOWN,symmetric);
					else // Bottom aligned
						Owner.ConnectComponents(c,ButtonGrid[pos.X,i],GUIMapDirection.UP,symmetric);

					break;
				}
				else if(i == bottom) // Set the external default down edge
					if(TopAligned)
						Owner.ConnectComponents(c,DefaultExternalDown,GUIMapDirection.DOWN);
					else // Bottom aligned
						Owner.ConnectComponents(c,DefaultExternalUp,GUIMapDirection.UP);

			return;
		}

		/// <summary>
		/// Flips every horizontal navigational link so that left is right and right is left.
		/// </summary>
		public void FlipHorizontalNavigationalLinks()
		{
			// If we have no owner, then we need do nothing
			if(Owner is null)
				return;

			// Flip all of the radio button links
			foreach(Checkbox c in Buttons.Values)
			{
				// We'll reach all the symmetrical cases eventually, so just do the normal edge assignments for now
				IGUI? l = Owner.GetConnection(c,GUIMapDirection.LEFT);

				// A lot of things can map to null but don't actually take us there, because our navigation doesn't support that
				// As such, we don't have to worry about assigning something null
				Owner.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.RIGHT),GUIMapDirection.LEFT);
				Owner.ConnectComponents(c,l,GUIMapDirection.RIGHT);
			}

			// To finish, we have a special case for the external edges
			// Flipping the links also changes (not flips) what external edge they should follow
			if(UseDefaultNavigation)
				AssignDefaultExternalEdges(true,true,false,false);

			return;
		}

		/// <summary>
		/// Flips every vertical navigational link so that up is down and down is up.
		/// </summary>
		public void FlipVerticalNavigationalLinks()
		{
			// If we have no owner, then we need do nothing
			if(Owner is null)
				return;

			// Flip all of the radio button links
			foreach(Checkbox c in Buttons.Values)
			{
				// We'll reach all the symmetrical cases eventually, so just do the normal edge assignments for now
				IGUI? u = Owner.GetConnection(c,GUIMapDirection.UP);
				
				// A lot of things can map to null but don't actually take us there, because our navigation doesn't support that
				// As such, we don't have to worry about assigning something null
				Owner.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.DOWN),GUIMapDirection.UP);
				Owner.ConnectComponents(c,u,GUIMapDirection.DOWN);
			}

			// To finish, we have a special case for the external edges
			// Flipping the links also changes (not flips) what external edge they should follow
			if(UseDefaultNavigation)
				AssignDefaultExternalEdges(false,false,true,true);

			return;
		}

		/// <summary>
		/// Assigns the default external edges to radio buttons on the designated edges.
		/// If all of <paramref name="left"/>, <paramref name="right"/>, <paramref name="up"/>, and <paramref name="down"/> are false, this function does nothing.
		/// <para/>
		/// This function will only work if <b>every default internal edge</b> is properly defined.
		/// </summary>
		/// <param name="left">If true, then buttons on the left edge will be assigned their default exit transition.</param>
		/// <param name="right">If true, then buttons on the right edge will be assigned their default exit transition.</param>
		/// <param name="up">If true, then buttons on the top edge will be assigned their default exit transition.</param>
		/// <param name="down">If true, then buttons on the bottom edge will be assigned their default exit transition.</param>
		protected void AssignDefaultExternalEdges(bool left, bool right, bool up, bool down)
		{
			// If we have no owner, do nothing
			// Similarly, if none of our parameters are true, do nothing
			if(Owner is null || !(left || right || up || down))
				return;

			// We can simply scan through each radio button since we know all internal edges are correct
			// Anytime we see something that is an external edge or a null edge, we can perform the assignment (this may be a self-assignment, but that's okay)
			foreach(Checkbox c in Buttons.Values)
			{
				Checkbox? e = null;
				
				// A single button could be on every edge, so none of these are else ifs
				if(left && ((e = Owner.GetConnection(c,GUIMapDirection.LEFT) as Checkbox) is null || !ButtonGrid.ContainsItem(e))) // ButtonsGrid is faster for value lookups
					Owner.ConnectComponents(c,DefaultExternalLeft,GUIMapDirection.LEFT);

				if(right && ((e = Owner.GetConnection(c,GUIMapDirection.RIGHT) as Checkbox) is null || !ButtonGrid.ContainsItem(e))) // ButtonsGrid is faster for value lookups
					Owner.ConnectComponents(c,DefaultExternalRight,GUIMapDirection.RIGHT);

				if(up && ((e = Owner.GetConnection(c,GUIMapDirection.UP) as Checkbox) is null || !ButtonGrid.ContainsItem(e))) // ButtonsGrid is faster for value lookups
					Owner.ConnectComponents(c,DefaultExternalUp,GUIMapDirection.UP);

				if(down && ((e = Owner.GetConnection(c,GUIMapDirection.DOWN) as Checkbox) is null || !ButtonGrid.ContainsItem(e))) // ButtonsGrid is faster for value lookups
					Owner.ConnectComponents(c,DefaultExternalDown,GUIMapDirection.DOWN);
			}

			return;
		}

		/// <summary>
		/// Removes a radio button from this.
		/// </summary>
		/// <param name="name">The name of the radio button to remove.</param>
		/// <param name="preserve_links">
		///	If true, then the up/down and left/right links of the button named <paramref name="name"/> will be stitched together.
		///	The right link will become the left's right link, for example, if the left link is not null or external.
		///	Similarly for the right, up, and down links.
		///	<para/>
		///	If using default digital navigation, then this value is ignored (i.e. always considered true).
		/// </param>
		/// <returns>Returns true if the radio button was removed and false otherwise.</returns>
		public bool RemoveRadioButton(string name, bool preserve_links = true)
		{
			// If we provided a bogus name, do nothing
			if(!Buttons.TryGetValue(name,out Checkbox? c))
				return false;

			// Grab the to be deleted button's link data
			IGUI? left = null;
			IGUI? right = null;
			IGUI? up = null;
			IGUI? down = null;

			Checkbox? cleft = null;
			Checkbox? cright = null;
			Checkbox? cup = null;
			Checkbox? cdown = null;

			bool left_is_internal = false;
			bool right_is_internal = false;
			bool up_is_internal = false;
			bool down_is_internal = false;

			// We will be stitching links together if we are asked to do so or if we're using default navigation (and have an onwer, of course)
			if(Owner is not null && (preserve_links || UseDefaultNavigation))
			{
				left = Owner.GetConnection(c,GUIMapDirection.LEFT);
				right = Owner.GetConnection(c,GUIMapDirection.RIGHT);
				up = Owner.GetConnection(c,GUIMapDirection.UP);
				down = Owner.GetConnection(c,GUIMapDirection.DOWN);

				cleft = left as Checkbox;
				cright = right as Checkbox;
				cup = up as Checkbox;
				cdown = down as Checkbox;

				left_is_internal = cleft is not null && ButtonGrid.ContainsItem(cleft);
				right_is_internal = cright is not null && ButtonGrid.ContainsItem(cright);
				up_is_internal = cup is not null && ButtonGrid.ContainsItem(cup);
				down_is_internal = cdown is not null && ButtonGrid.ContainsItem(cdown);
			}

			// If we remove what's selected, then we need to void the selection
			// We do this first to make sure we don't cause anything to explode
			// If we fail the remove calls later, it's fine to have the selected button nulled, so that scenario is fine to put ourselves into
			if(c.Name == SelectedRadioButton)
				SelectedRadioButton = null;

			// Do our best to remove the radio button with the provided name
			if(!Buttons.Remove(name) || !ButtonGrid.RemoveItem(c))
				return false;

			// Update the links to reflect the missing radio button
			// If we're using default navigation or asked to preserve links, then we must stitch them together
			// If we're not, then removing c from our (shared) owner will also clobber any links it was involved in, so we don't have to think about it in that case
			if(Owner is not null && (preserve_links || UseDefaultNavigation))
			{
				 // We only care about stitching internal components' links (we could use symmetric options, but it saves maybe a couple clock cycles, so let's just not to make the code clear)
				 if(left_is_internal)
					Owner.ConnectComponents(left,right,GUIMapDirection.RIGHT);

				 if(right_is_internal)
					Owner.ConnectComponents(right,left,GUIMapDirection.LEFT);

				 if(up_is_internal)
					Owner.ConnectComponents(up,down,GUIMapDirection.DOWN);

				 if(down_is_internal)
					Owner.ConnectComponents(down,up,GUIMapDirection.UP);
			}
			
			// Removing c from our (shared) owner will do most of the removal logic we need it to (despite not being a top-level component)
			if(Owner is not null)
				Owner.Remove(c);

			AlignAllButtons(); // We have to realign if a button is removed
			return true;
		}

		/// <summary>
		/// Removes a radio button from this.
		/// </summary>
		/// <param name="c">The radio button to remove.</param>
		/// <returns>Returns true if the radio button was removed and false otherwise.</returns>
		public bool RemoveRadioButton(Checkbox c)
		{return RemoveRadioButton(c.Name);}

		/// <summary>
		/// Removes a radio button from this.
		/// </summary>
		/// <param name="p">
		///  The grid position to remove a radio button from.
		///  If this location is not occupied, then this will do nothing.
		///	Similarly, if this position contains a negative index, this will do nothing.
		/// </param>
		/// <returns>Returns true if a radio button was removed and false otherwise.</returns>
		public bool RemoveRadioButton(Point p)
		{
			if(!ButtonGrid.TryGetItem(p,out Checkbox? c))
				return false;

			return RemoveRadioButton(c.Name);
		}

		/// <summary>
		/// Determines if this group of radio buttons contains the radio button named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The radio button name to check for containment.</param>
		/// <returns>Returns true if this contains the radio button named <paramref name="name"/> and false otherwise.</returns>
		public bool ContainsRadioButton(string name)
		{return Buttons.ContainsKey(name);}

		/// <summary>
		/// Determines if this group of radio buttons contains the radio button <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The radio button to check for containment.</param>
		/// <returns>Returns true if this contains the radio button <paramref name="c"/> and false otherwise.</returns>
		public bool ContainsRadioButton(Checkbox c)
		{return ButtonGrid.ContainsItem(c);}

		/// <summary>
		/// Determines if this group of radio buttons contains a radio button at point <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The point to check for a radio button at.</param>
		/// <returns>Returns true if <paramref name="p"/> contains a radio button and false otherwise.</returns>
		public bool ContainsRadioButton(Point p)
		{return ButtonGrid.PositionOccupied(p);}

		/// <summary>
		/// Aligns all of the radio buttons to their approved positions.
		/// </summary>
		protected void AlignAllButtons()
		{
			// Precalculate these for convenience more than anything, since they should be fast
			int W = ButtonGrid.MaxX;
			int H = ButtonGrid.MaxY;

			// We will store the max width/height of each row/column in these arrays
			int[] row_widths = new int[W + 1];
			int[] column_heights = new int[H + 1];

			// Initialize everything to 0 so we don't get any weird results
			for(int i = 0;i <= W;i++)
				row_widths[i] = 0;

			for(int i = 0;i <= H;i++)
				column_heights[i] = 0;

			// We don't have to iterate over the grid
			// We can just iterate over the buttons and record their width/height in the appropriate row/column if it is bigger than what we've yet seen
			foreach(KeyValuePair<Point,Checkbox> p in ButtonGrid)
			{
				row_widths[p.Key.X] = Math.Max(row_widths[p.Key.X],ButtonGrid[p.Key].Width);
				column_heights[p.Key.Y] = Math.Max(column_heights[p.Key.Y],ButtonGrid[p.Key].Height);
			}

			// We can now calculate the location where each item should be positioned in a given row/column
			// We do not reuse the previous arrays because the padding may not be an integral amount, and we want to preserve that in the position
			float[] row_pos = new float[W + 1];
			float[] column_pos = new float[H + 1];

			// Rows first
			row_pos[0] = LeftAligned ? HorizontalPadding : (-HorizontalPadding - row_widths[0]);

			for(int i = 1;i <= W;i++)
				if(LeftAligned)
					row_pos[i] = row_pos[i - 1] + row_widths[i - 1] + HorizontalPadding;
				else // Right aligned
					row_pos[i] = row_pos[i - 1] - HorizontalPadding - row_widths[i];

			// Now columns
			column_pos[0] = TopAligned ? VerticalPadding : (-VerticalPadding - column_heights[0]);

			for(int i = 1;i <= H;i++)
				if(TopAligned)
					column_pos[i] = column_pos[i - 1] + column_heights[i - 1] + VerticalPadding;
				else // Bottom aligned
					column_pos[i] = column_pos[i - 1] - VerticalPadding - column_heights[i];

			// Now we need to assign a transform to each button that places it at the correct position
			foreach(KeyValuePair<Point,Checkbox> p in ButtonGrid)
				p.Value.Transform = Matrix2D.Translation(row_pos[p.Key.X],column_pos[p.Key.Y]);

			return;
		}
		
		/// <summary>
		/// This handles active component change events so that we can redirect to the selected radio button (or the 'first' radio button) if this gains focus.
		/// </summary>
		/// <param name="sender">The GUICore whose active component changed.</param>
		/// <param name="value">The event data.</param>
		protected void OnNext(GUICore sender, ActiveComponentChangedEvent value)
		{
			// In case someone calls this for malicious reasons
			if(Owner is null)
				return;

			// If the active component is us, we need to redirect to the best available button (if any)
			if(value.ActiveComponent == this)
			{
				if(SelectedRadioButton is null || !Owner.JumpToComponent(Buttons[SelectedRadioButton]))
				{
					Point? p = FindBestOrigin();

					if(p is not null)
						Owner.JumpToComponent(ButtonGrid[p.Value]); // If this fails, we have no recourse but to stay put, so good luck
				}
			}
			
			return;
		}

		/// <summary>
		/// Determines if this radio button group contains the position <paramref name="pos"/>.
		/// This is true if the mouse is over any of the radio <i>buttons</i>.
		/// If the mouse is only over the text or the background, it is ignored.
		/// </summary>
		/// <param name="pos">The position (in world coordinates) to check for containment.</param>
		/// <param name="component">The GUI component where the <i>topmost</i> containment (a child could overlap its parent) occurred if there was one; null otherwise.</param>
		/// <param name="include_children">If true, we include (enabled and visible) children in the potential collision.</param>
		/// <returns>Returns true if this or a child contained <paramref name="pos"/> and false otherwise.</returns>
		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// We only care about the radio buttons, so just check all of them for containment (they are not children, so always do it)
			// Checkboxes are (or are at least supposed to) be independent of each other, so we don't need to check for which one is on top
			foreach(Checkbox c in Buttons.Values)
				if(c.Contains(pos,out component,include_children))
					return true;
			
			component = null;
			return false;
		}

		public override Rectangle Bounds
		{
			get
			{
				// We don't care about the radio button group's background, so just union all of the radio button bounds together
				// We can't precalculate anything because the world may move out from under us without notice
				// We also can't just keep the extrema radio buttons since they may not actually be the extrema (either ever or currently)
				Rectangle? ret = null;

				foreach(Checkbox c in Buttons.Values)
					if(ret is null)
						ret = c.Bounds;
					else
						ret = ret.Value.Union(c.Bounds);
				
				return ret ?? Rectangle.Empty;
			}
		}

		/// <summary>
		/// Obtains a radio button from this group of radio buttons.
		/// </summary>
		/// <param name="name">The name of the radio button to fetch.</param>
		/// <returns>Returns the radio button named <paramref name="name"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not the name of a radio button.</exception>
		public Checkbox this[string name] => Buttons[name];

		/// <summary>
		/// Indexes into the radio button grid at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position to index into.</param>
		/// <returns>Returns the item at position <paramref name="p"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if position <paramref name="p"/> does not contain an item.</exception>
		protected Checkbox this[Point p] => ButtonGrid[p];

		/// <summary>
		/// The set of radio buttons for this radio button group.
		/// </summary>
		protected Dictionary<string,Checkbox> Buttons
		{get; set;}

		/// <summary>
		/// This is the grid representation of the checkboxes.
		/// </summary>
		protected InfiniteGrid<Checkbox> ButtonGrid
		{get; set;}

		/// <summary>
		/// The component library to use for drawing this radio button group's backgrounds.
		/// </summary>
		protected ComponentLibrary Backgrounds
		{get; set;}

		public override GUICore? Owner
		{
			get => base.Owner;

			set
			{
				// If we're doing nothing, we should DO nothing
				if(ReferenceEquals(this,value))
					return;

				// If we need to unsubscribe, do so first to avoid potentially processing any more events
				if(Owner is not null)
					Owner.OnActiveComponentChanged -= OnNext;

				// We need to transfer the edges from the old owner to the new owner before we do anything else
				foreach(Checkbox c in Buttons.Values)
					if(value is not null)
					{
						value.AddToMap(c);

						// Copy old direction connections
						if(Owner is not null)
						{
							value.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.LEFT),GUIMapDirection.LEFT);
							value.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.RIGHT),GUIMapDirection.RIGHT);
							value.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.UP),GUIMapDirection.UP);
							value.ConnectComponents(c,Owner.GetConnection(c,GUIMapDirection.DOWN),GUIMapDirection.DOWN);
						}
					}
				
				// Now that we've copied every edge, we need to remove each radio button from the old owner
				foreach(Checkbox c in Buttons.Values)
					c.Owner = value;

				// Now that we've cleaned up our mess, we can set the new owner
				bool owner_was_null = Owner is null;
				base.Owner = value;

				// If we didn't have an owner to copy edges from but do now and we're using default navigational links, go ahead and assign them now
				if(owner_was_null && value is not null && UseDefaultNavigation)
					AssignDefaultNavigationalLinks();

				// Lastly, subscribe to our new owner if we can
				if(Owner is not null)
					Owner.OnActiveComponentChanged += OnNext;

				return;
			}
		}

		public override SpriteBatch? Renderer
		{
			protected get => base.Renderer;

			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;

				Backgrounds.Renderer = value;

				foreach(Checkbox c in Buttons.Values)
					c.Renderer = value;

				return;
			}
		}

		/// <summary>
		/// The selected radio button's name.
		/// If this value is null, no radio button is selected.
		/// </summary>
		public string? SelectedRadioButton
		{
			get => _srb;

			set
			{
				if(_srb == value || value is not null && !Buttons.ContainsKey(value))
					return;

				// Record the old value for later
				string? old = _srb;

				// Update the selection
				_srb = value;

				// Uncheck the old selected value (if any)
				if(old is not null)
					this[old].Checked = false;

				// If we have a selection now, check it
				if(_srb is not null)
					this[_srb].Checked = true; // Check the newly checked thing (this may do nothing if the click event already checked it)

				SelectionChanged(this,old,_srb);
				return;
			}
		}

		protected string? _srb;

		/// <summary>
		/// The width of the group of radio buttons (sans transformations).
		/// </summary>
		public override int Width
		{
			get
			{
				float w = 0.0f;

				// The min and max x positions vary in meaning depending on our horizontal alignment
				// Regardless, the transformations are pure translation, so we can read their positions off of them and add any dimensions needed
				if(LeftAligned)
				{
					foreach(Checkbox c in ButtonGrid.Items) // We need to grab the rightmost point of the rightmost thing
						w = MathF.Max(w,c.Transform[0,2] + c.Width);

					w += HorizontalPadding;
				}
				else // Right aligned
				{
					foreach(Checkbox c in ButtonGrid.Items) // We need to grab the leftmost point of the leftmost thing
						w = MathF.Max(w,-c.Transform[0,2]);

					w += HorizontalPadding;
				}

				// If we have a background width (that we don't overspill), we'll want to return that; otherwise, we use w
				return Math.Max(Backgrounds.Width,(int)MathF.Ceiling(w));
			}
		}

		/// <summary>
		/// The height of the group of radio buttons (sans transformations).
		/// </summary>
		public override int Height
		{
			get
			{
				float h = 0.0f;

				// The min and max y positions vary in meaning depending on our vertical alignment
				// Regardless, the transformations are pure translation, so we can read their positions off of them and add any dimensions needed
				if(TopAligned)
				{
					foreach(Checkbox c in ButtonGrid.Items) // We need to grab the bottommost point of the bottommost thing
						h = MathF.Max(h,c.Transform[1,2] + c.Height);

					h += VerticalPadding;
				}
				else // Bottom aligned
				{
					foreach(Checkbox c in ButtonGrid.Items) // We need to grab the topmost point of the topmost thing
						h = MathF.Max(h,-c.Transform[1,2]);

					h += VerticalPadding;
				}

				// If we have a background width (that we don't overspill), we'll want to return that; otherwise, we use w
				return Math.Max(Backgrounds.Height,(int)MathF.Ceiling(h));
			}
		}

		/// <summary>
		/// If true, then the radio buttons are left aligned in their group.
		/// While the text in each radio button can be left or right aligned, their layout will put the origin on the left.
		/// <para/>
		/// If false, then the radio buttons are right aligned instead.
		/// </summary>
		public bool LeftAligned
		{
			get => _la;

			set
			{
				if(_la == value)
					return;

				_la = value;

				FlipHorizontalNavigationalLinks();
				AlignAllButtons(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}

		protected bool _la;

		/// <summary>
		/// If true, then the radio buttons are right aligned in their group.
		/// While the text in each radio button can be left or right aligned, their layout will put the origin on the right.
		/// <para/>
		/// If false, then the radio buttons are left aligned instead.
		/// </summary>
		public bool RightAligned
		{
			get => !LeftAligned;
			set => LeftAligned = !value;
		}

		/// <summary>
		/// If true, then the radio buttons are top aligned in their group.
		/// While the text in each radio button can be left or right aligned, their layout will put the origin on the top.
		/// <para/>
		/// If false, then the radio buttons are bottom aligned instead.
		/// </summary>
		public bool TopAligned
		{
			get => _ta;

			set
			{
				if(_ta == value)
					return;

				_ta = value;

				FlipVerticalNavigationalLinks();
				AlignAllButtons(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}
		
		protected bool _ta;

		/// <summary>
		/// If true, then the radio buttons are bottom aligned in their group.
		/// While the text in each radio button can be left or right aligned, their layout will put the origin on the bottom.
		/// <para/>
		/// If false, then the radio buttons are top aligned instead.
		/// </summary>
		public bool BottomAligned
		{
			get => !TopAligned;
			set => TopAligned = !value;
		}

		/// <summary>
		/// The horizontal padding between radio buttons.
		/// <para/>
		/// This value can be negative.
		/// </summary>
		public float HorizontalPadding
		{
			get => _hp;
			
			set
			{
				if(_hp == value)
					return;
				
				_hp = value;
				AlignAllButtons(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}

		protected float _hp;

		/// <summary>
		/// The horizontal padding between radio buttons.
		/// <para/>
		/// This value can be negative.
		/// </summary>
		public float VerticalPadding
		{
			get => _vp;

			set
			{
				if(_vp == value)
					return;

				_vp = value;
				AlignAllButtons(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}

		protected float _vp;

		/// <summary>
		/// An event called when the selected radio button from this group is changed.
		/// </summary>
		public event RadioSelectionChanged SelectionChanged;

		/// <summary>
		/// If true, then navigation links will be automatically assigned.
		/// Default digial navigation via the arrow keys will produce strict directional movement.
		/// In short, left goes left (and only left) to the nearest radio button (if one exists) and does not wrap around.
		/// Similarly for all other directions.
		/// </summary>
		public bool UseDefaultNavigation
		{
			get => _udn;

			set
			{
				if(_udn == value)
					return;

				_udn = value;

				// On a rising edge, we need to assign all default navigational links
				if(_udn)
					AssignDefaultNavigationalLinks();

				return;
			}
		}

		protected bool _udn;

		/// <summary>
		/// The default left navigational link for a component on the left edge of navigation.
		/// </summary>
		protected IGUI? DefaultExternalLeft
		{get; set;}

		/// <summary>
		/// The default right navigational link for a component on the right edge of navigation.
		/// </summary>
		protected IGUI? DefaultExternalRight
		{get; set;}

		/// <summary>
		/// The default up navigational link for a component on the top edge of navigation.
		/// </summary>
		protected IGUI? DefaultExternalUp
		{get; set;}

		/// <summary>
		/// The default down navigational link for a component on the bottom edge of navigation.
		/// </summary>
		protected IGUI? DefaultExternalDown
		{get; set;}

		/// <summary>
		/// The name of the component to use for drawing a radio button group's background's normal state.
		/// </summary>
		public const string NormalState = "n";

		/// <summary>
		/// The name of the component to use for drawing a radio button group's background's disabled state.
		/// </summary>
		public const string DisabledState = "d";
	}

	/// <summary>
	/// An event called when the selected radio button changes.
	/// </summary>
	/// <param name="sender">The radio button group sending the event.</param>
	/// <param name="old_selection">The name of the old selected radio button. Radio buttons are indexed by name. If this value is null, no radio button was selected.</param>
	/// <param name="new_selection">The name of the newly selected radio button. Radio buttons are indexed by name. If this value is null, no radio button is selected.</param>
	public delegate void RadioSelectionChanged(RadioButtons sender, string? old_selection, string? new_selection);
}
