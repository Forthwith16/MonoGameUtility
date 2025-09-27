using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// Allows the user to select one item from a selection of choices, but only one is displayed at any time.
	/// <br/><br/>
	/// The text of the displayed items is taken from the item's ToString function on add and is not updated if an item's ToString results changes later.
	/// </summary>
	public class Selector<T> : GUIBase where T : notnull
	{
		/// <summary>
		/// Creates a new selector.
		/// It defaults to a Horizontal grouping with a Split alignment, item wrap around, and centered text.
		/// </summary>
		/// <param name="name">The name of this selector.</param>
		/// <param name="decrement">The button to use to decrement the selected item. This selector will be responsible for initializing, updating, and drawing <paramref name="decrement"/> but will take no responsibility for disposing of it.</param>
		/// <param name="increment">The button to use to increment the selected item. This selector will be responsible for initializing, updating, and drawing <paramref name="increment"/> but will take no responsibility for disposing of it.</param>
		/// <param name="pencil">The means by which the selected item text is drawn.</param>
		/// <param name="background">
		///	The background that pencil will draw on (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="background"/> but will take no responsibility for disposing of it.
		///	If this value is null, no background will be drawn and a dummy component will be created.
		/// </param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Selector(string name, Button decrement, Button increment, TextGameObject pencil, DrawableAffineObject? background = null, DrawableAffineObject? tooltip = null) : base(name,tooltip)
		{
			Decrement = decrement;
			Increment = increment;
			Pencil = pencil;
			Background = background ?? new DummyAffineGameObject();

			Items = new List<T>();
			ItemText = new List<string>();
			MaximumItemDimensions = Vector2.Zero;

			WrapAround = true;

			OnSelectionChanged += (a,b,c,d,e) => {};

			SelectedItemIndex = -1;
			DoubleDipping = false;

			_g = ButtonGrouping.Horizontal;
			_a = ButtonAlignment.Split;

			_hta = HorizontalTextAlignment.Center;
			_vta = VerticalTextAlignment.Center;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			// Load our basic components
			// All of the basic properties are kept in sync already, and the transform will be assigned when we align all of our components
			Decrement.Parent = this;
			Decrement.DrawOrder = DrawOrder;
			Decrement.Initialize();
			
			Increment.Parent = this;
			Increment.DrawOrder = DrawOrder;
			Increment.Initialize();
			
			Background.Parent = this;
			Background.DrawOrder = DrawOrder;
			Background.LayerDepth = LayerDepth; // It's possible to get here without assigning the layer depth away from the always on top 0
			Background.Initialize();
			
			Pencil.Parent = Background;
			Pencil.DrawOrder = DrawOrder + 1;
			Pencil.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);
			Pencil.Text = SelectedItemIndex > -1 ? ItemText[SelectedItemIndex] : ""; // This will prevent anyone from being clever and assigning something random to the initial text
			Pencil.Initialize();
			
			// The pencil's font is not necessarily loaded until now, so we need to calculate our item text dimensions now
			CalculateTextSizes(); // This will align our components for us

			// Subscribe to every event we must
			DrawOrderChanged += (sender,e) =>
			{
				// We know Buttons only require DrawOrder
				Decrement.DrawOrder = DrawOrder;
				Increment.DrawOrder = DrawOrder;

				// But we may need to set LayerDepth for these, since we know nothing about them
				Pencil.DrawOrder = DrawOrder + 1;
				Pencil.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);

				Background.DrawOrder = DrawOrder;
				Background.LayerDepth = LayerDepth;
			};

			OnSelectionChanged += (a,b,c,d,e) =>
			{
				DoubleDipping = true;
				Pencil.Text = SelectedItemIndex > -1 ? ItemText[SelectedItemIndex] : "";
				DoubleDipping = false;
				
				AlignComponents();
				return;
			};
			
			Pencil.FontChanged += (a,b,c) => CalculateTextSizes();
			Pencil.TextChanged += (a,old_text,c) =>
			{
				// If we're trying to change the text via internal means, then we should do nothing
				if(DoubleDipping)
					return;

				a.Text = old_text;
				return;
			};
			
			Decrement.OnClick += (a,b) =>
			{
				// If we left clicked, then we should decrement the selected index
				if(b.Button == MouseButton.Left)
					if(SelectedItemIndex > 0) // If the selected index is at least 1, we know we can safely decrement it
						SelectedItemIndex--;
					else if(WrapAround)
						SelectedItemIndex = Count - 1; // This always works even if Count = 0

				return;
			};

			Decrement.Library.StateChange += (a,b,c) => AlignComponents();

			Increment.OnClick += (a,b) =>
			{
				// If we left clicked, then we should increment the selected index
				if(b.Button == MouseButton.Left)
					if(SelectedItemIndex < Count - 1) // If the selected index is strictly less than the maximum index, then we can safely increment the index
						SelectedItemIndex++; // This works even when the selected index is -1, since if Count = 0, then we would have the condition -1 < -1 failing to get here
					else if(WrapAround && Count > 0)
						SelectedItemIndex = 0;

				return;
			};
			
			Increment.Library.StateChange += (a,b,c) => AlignComponents();

			return;
		}

		/// <summary>
		/// This handles active component change events so that we can redirect to the correct element of this GUI component when it is selected.
		/// </summary>
		/// <param name="sender">The GUICore whose active component changed.</param>
		/// <param name="value">The event data.</param>
		protected void OnNext(GUICore sender, ActiveComponentChangedEvent value)
		{
			// In case someone calls this for malicious reasons
			if(Owner is null)
				return;
			
			// If the active component is us, we need to redirect to a button
			if(ReferenceEquals(value.ActiveComponent,this))
				if(ReferenceEquals(Owner.GetConnection(value.LastActiveComponent,Map.GUIMapDirection.LEFT),this))
					if(Grouping == ButtonGrouping.Vertical)
						if(value.LastActiveComponent is not null && value.LastActiveComponent.GetAffinePosition().Y >= Decrement.GetAffinePosition().Y) // If we have no clear destination, pick the approximately closer one
							Owner.JumpToComponent(Decrement);
						else
							Owner.JumpToComponent(Increment); // We prefer to default to Increment if we don't know where we came from
					else
						Owner.JumpToComponent(Increment); // Moving left into this in a horizontal position means we move to increment
				else if(ReferenceEquals(Owner.GetConnection(value.LastActiveComponent,Map.GUIMapDirection.RIGHT),this))
					if(Grouping == ButtonGrouping.Vertical)
						if(value.LastActiveComponent is not null && value.LastActiveComponent.GetAffinePosition().Y >= Decrement.GetAffinePosition().Y) // If we have no clear destination, pick the approximately closer one
							Owner.JumpToComponent(Decrement);
						else
							Owner.JumpToComponent(Increment); // We prefer to default to Increment if we don't know where we came from
					else
						Owner.JumpToComponent(Decrement); // Moving right into this in a horizontal position means we move to decrement
				else if(ReferenceEquals(Owner.GetConnection(value.LastActiveComponent,Map.GUIMapDirection.UP),this))
					if(Grouping == ButtonGrouping.Vertical)
						Owner.JumpToComponent(Decrement); // Moving up into this in a vertical position means we move to decrement
					else
						if(value.LastActiveComponent is null || value.LastActiveComponent.GetAffinePosition().X >= Increment.GetAffinePosition().X) // If we have no clear destination, pick the approximately closer one
							Owner.JumpToComponent(Increment);
						else
							Owner.JumpToComponent(Decrement); // We prefer to default to Increment if we don't know where we came from
				else if(ReferenceEquals(Owner.GetConnection(value.LastActiveComponent,Map.GUIMapDirection.DOWN),this))
					if(Grouping == ButtonGrouping.Vertical)
						Owner.JumpToComponent(Increment); // Moving up into this in a vertical position means we move to increment
					else
						if(value.LastActiveComponent is null || value.LastActiveComponent.GetAffinePosition().X >= Increment.GetAffinePosition().X) // If we have no clear destination, pick the approximately closer one
							Owner.JumpToComponent(Increment);
						else
							Owner.JumpToComponent(Decrement); // We prefer to default to Increment if we don't know where we came from
				else // If we don't know where we came from, then we'll just default to Increment
					Owner.JumpToComponent(Increment);
			
			return;
		}

		/// <summary>
		/// Calculates the size of the text for every item.
		/// </summary>
		protected void CalculateTextSizes()
		{
			MaximumItemDimensions = Vector2.Zero;

			if(Pencil.Font is null)
				return;

			foreach(string text in ItemText)
			{
				Vector2 text_dim = Pencil.Font!.MeasureString(text);
				MaximumItemDimensions = new Vector2(MathF.Max(text_dim.X,MaximumItemDimensions.X),MathF.Max(text_dim.Y,MaximumItemDimensions.Y));
			}

			// Add in the padding on each side of the text
			MaximumItemDimensions += 2.0f * new Vector2(HorizontalTextPadding,VerticalTextPadding);

			AlignComponents();
			return;
		}

		/// <summary>
		/// Aligns all of the components of this selector.
		/// </summary>
		protected void AlignComponents()
		{
			// First calculate the effective text dimensions so that we don't have to think about it ever again
			Vector2 text_dim = new Vector2(MathF.Max(MaximumItemDimensions.X,Background.Width),MathF.Max(MaximumItemDimensions.Y,Background.Height));
			
			// Calculate some other dimensions just once so we don't have to think about them
			// Hopefully the compiler will do some serious arithmetic optimization here, because I can't be bothered
			float w = Width;
			float h = Height;

			float incx = Increment.Width;
			float incy = Increment.Height;

			float decx = Decrement.Width;
			float decy = Decrement.Height;

			// We will first do the basic component alignment for each of the three major components: decrement, increment, and background
			switch(Alignment)
			{
			case ButtonAlignment.Left:
				if(Grouping == ButtonGrouping.Vertical)
				{
					// Calculate the base height (if the buttons determine the height, this will be 0)
					float base_h = (h - decy - incy) / 2.0f;

					Increment.Transform = Matrix2D.Translation(incx >= decx ? 0.0f : (decx - incx) / 2.0f,base_h);
					Decrement.Transform = Matrix2D.Translation(decx >= incx ? 0.0f : (incx - decx) / 2.0f,incy + base_h);
					Background.Transform = Matrix2D.Translation(MathF.Max(incx,decx),(h - text_dim.Y) / 2.0f); // If the text height IS the height, then the y offset here will be 0 as desired
				}
				else
				{
					Decrement.Transform = Matrix2D.Translation(0.0f,(h - decy) / 2.0f);
					Increment.Transform = Matrix2D.Translation(decx,(h - incy) / 2.0f);
					Background.Transform = Matrix2D.Translation(decx + incx,(h - text_dim.Y) / 2.0f);
				}
					
				break;
			case ButtonAlignment.Right:
				if(Grouping == ButtonGrouping.Vertical)
				{
					float base_h = (h - decy - incy) / 2.0f;

					Increment.Transform = Matrix2D.Translation(text_dim.X + (incx >= decx ? 0.0f : (decx - incx) / 2.0f),base_h);
					Decrement.Transform = Matrix2D.Translation(text_dim.X + (decx >= incx ? 0.0f : (incx - decx) / 2.0f),incy + base_h);
					Background.Transform = Matrix2D.Translation(0.0f,(h - text_dim.Y) / 2.0f);
				}
				else
				{
					Decrement.Transform = Matrix2D.Translation(text_dim.X,(h - decy) / 2.0f);
					Increment.Transform = Matrix2D.Translation(text_dim.X + decx,(h - incy) / 2.0f);
					Background.Transform = Matrix2D.Translation(0.0f,(h - text_dim.Y) / 2.0f);
				}
					
				break;
			case ButtonAlignment.Top:
				if(Grouping == ButtonGrouping.Vertical)
				{
					Increment.Transform = Matrix2D.Translation((w - incx) / 2.0f,0.0f);
					Decrement.Transform = Matrix2D.Translation((w - decx) / 2.0f,incy);
					Background.Transform = Matrix2D.Translation((w - text_dim.X) / 2.0f,decy + incy);
				}
				else
				{
					float base_w = (w - decx - incx) / 2.0f;

					Decrement.Transform = Matrix2D.Translation(base_w,decy >= incy ? 0.0f : (incy - decy) / 2.0f);
					Increment.Transform = Matrix2D.Translation(base_w + decx,incy >= decy ? 0.0f : (decy - incy) / 2.0f);
					Background.Transform = Matrix2D.Translation((w - text_dim.X) / 2.0f,MathF.Max(decy,incy));
				}
					
				break;
			case ButtonAlignment.Bottom:
				if(Grouping == ButtonGrouping.Vertical)
				{
					Increment.Transform = Matrix2D.Translation((w - incx) / 2.0f,text_dim.Y);
					Decrement.Transform = Matrix2D.Translation((w - decx) / 2.0f,text_dim.Y + incy);
					Background.Transform = Matrix2D.Translation((w - text_dim.X) / 2.0f,0.0f);
				}
				else
				{
					float base_w = (w - decx - incx) / 2.0f;

					Decrement.Transform = Matrix2D.Translation(base_w,text_dim.Y + (decy >= incy ? 0.0f : (incy - decy) / 2.0f));
					Increment.Transform = Matrix2D.Translation(base_w + decx,text_dim.Y + (incy >= decy ? 0.0f : (decy - incy) / 2.0f));
					Background.Transform = Matrix2D.Translation((w - text_dim.X) / 2.0f,0.0f);
				}
					
				break;
			case ButtonAlignment.Split:
				if(Grouping == ButtonGrouping.Vertical)
				{
					Increment.Transform = Matrix2D.Translation((w - incx) / 2.0f,0.0f);
					Background.Transform = Matrix2D.Translation((w - text_dim.X) / 2.0f,incy);
					Decrement.Transform = Matrix2D.Translation((w - decx) / 2.0f,incy + text_dim.Y);
				}
				else
				{
					Decrement.Transform = Matrix2D.Translation(0.0f,(h - decy) / 2.0f);
					Background.Transform = Matrix2D.Translation(decx,(h - text_dim.Y) / 2.0f);
					Increment.Transform = Matrix2D.Translation(decx + text_dim.X,(h - incy) / 2.0f);
				}
					
				break;
			default:
				break;
			}
			
			// Lastly, we align the text inside of its maximum dimensions and background
			// If the text overruns the background, then so be it; pick a better background
			switch(HorizontalTextAlignment)
			{
			case HorizontalTextAlignment.Left:
				switch(VerticalTextAlignment)
				{
				case VerticalTextAlignment.Top:
					Pencil.Transform = Matrix2D.Translation(HorizontalTextPadding,VerticalTextPadding);
					break;
				case VerticalTextAlignment.Bottom:
					Pencil.Transform = Matrix2D.Translation(HorizontalTextPadding,text_dim.Y - Pencil.Height - VerticalTextPadding); // Pencil dimensions are precalculated, so they're fast
					break;
				case VerticalTextAlignment.Center:
					Pencil.Transform = Matrix2D.Translation(HorizontalTextPadding,(text_dim.Y - Pencil.Height) / 2.0f);
					break;
				}

				break;
			case HorizontalTextAlignment.Right:
				switch(VerticalTextAlignment)
				{
				case VerticalTextAlignment.Top:
					Pencil.Transform = Matrix2D.Translation(text_dim.X - Pencil.Width - HorizontalTextPadding,VerticalTextPadding);
					break;
				case VerticalTextAlignment.Bottom:
					Pencil.Transform = Matrix2D.Translation(text_dim.X - Pencil.Width - HorizontalTextPadding,text_dim.Y - Pencil.Height - VerticalTextPadding);
					break;
				case VerticalTextAlignment.Center:
					Pencil.Transform = Matrix2D.Translation(text_dim.X - Pencil.Width - HorizontalTextPadding,(text_dim.Y - Pencil.Height) / 2.0f);
					break;
				}

				break;
			case HorizontalTextAlignment.Center:
				switch(VerticalTextAlignment)
				{
				case VerticalTextAlignment.Top:
					Pencil.Transform = Matrix2D.Translation((text_dim.X - Pencil.Width) / 2.0f,VerticalTextPadding);
					break;
				case VerticalTextAlignment.Bottom:
					Pencil.Transform = Matrix2D.Translation((text_dim.X - Pencil.Width) / 2.0f,text_dim.Y - Pencil.Height - VerticalTextPadding);
					break;
				case VerticalTextAlignment.Center:
					Pencil.Transform = Matrix2D.Translation((text_dim.X - Pencil.Width) / 2.0f,(text_dim.Y - Pencil.Height) / 2.0f);
					break;
				}

				break;
			}

			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			Decrement.Update(delta);
			Increment.Update(delta);
			Pencil.Update(delta);
			Background.Update(delta);

			// We must ensure that the navigational links to external locations are kept up to date (if we have an owner)
			if(Grouping == ButtonGrouping.Vertical)
				ConnectVerticalButtons();
			else
				ConnectHorizontalButtons();

			return;
		}

		/// <summary>
		/// Links the buttons together horizontally.
		/// </summary>
		protected void ConnectHorizontalButtons()
		{
			if(Owner is null)
				return;

			IGUI? left = Owner.GetConnection(this,Map.GUIMapDirection.LEFT);
			IGUI? right = Owner.GetConnection(this,Map.GUIMapDirection.RIGHT);
			IGUI? up = Owner.GetConnection(this,Map.GUIMapDirection.UP);
			IGUI? down = Owner.GetConnection(this,Map.GUIMapDirection.DOWN);

			Owner.ConnectComponents(Decrement,left,Map.GUIMapDirection.LEFT);
			Owner.ConnectComponents(Decrement,Increment,Map.GUIMapDirection.RIGHT);
			Owner.ConnectComponents(Decrement,up,Map.GUIMapDirection.UP);
			Owner.ConnectComponents(Decrement,down,Map.GUIMapDirection.DOWN);
			
			Owner.ConnectComponents(Increment,Decrement,Map.GUIMapDirection.LEFT);
			Owner.ConnectComponents(Increment,right,Map.GUIMapDirection.RIGHT);
			Owner.ConnectComponents(Increment,up,Map.GUIMapDirection.UP);
			Owner.ConnectComponents(Increment,down,Map.GUIMapDirection.DOWN);

			return;
		}

		/// <summary>
		/// Links the buttons together vertically.
		/// </summary>
		protected void ConnectVerticalButtons()
		{
			if(Owner is null)
				return;

			IGUI? left = Owner!.GetConnection(this,Map.GUIMapDirection.LEFT);
			IGUI? right = Owner.GetConnection(this,Map.GUIMapDirection.RIGHT);
			IGUI? up = Owner.GetConnection(this,Map.GUIMapDirection.UP);
			IGUI? down = Owner.GetConnection(this,Map.GUIMapDirection.DOWN);

			Owner.ConnectComponents(Decrement,left,Map.GUIMapDirection.LEFT);
			Owner.ConnectComponents(Decrement,right,Map.GUIMapDirection.RIGHT);
			Owner.ConnectComponents(Decrement,Decrement,Map.GUIMapDirection.UP);
			Owner.ConnectComponents(Decrement,down,Map.GUIMapDirection.DOWN);
			
			Owner.ConnectComponents(Increment,left,Map.GUIMapDirection.LEFT);
			Owner.ConnectComponents(Increment,right,Map.GUIMapDirection.RIGHT);
			Owner.ConnectComponents(Increment,up,Map.GUIMapDirection.UP);
			Owner.ConnectComponents(Increment,Decrement,Map.GUIMapDirection.DOWN);

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Decrement.Draw(delta);
			Increment.Draw(delta);
			Pencil.Draw(delta);
			Background.Draw(delta);

			return;
		}

		/// <summary>
		/// Adds an item to the set of selectable items.
		/// </summary>
		/// <param name="item">The item to add.</param>
		public void AddItem(T item)
		{
			Items.Add(item);
			ItemText.Add(item.ToString() ?? "null");

			CalculateTextSizes();
			return;
		}

		/// <summary>
		/// Inserts an item to the set of selectable items.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <param name="index">Where to add the item.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than Count.</exception>
		/// <remarks>This will increment SelectedItemIndex if this is inserted before it.</remarks>
		public void InsertItem(T item, int index)
		{
			Items.Insert(index,item);
			ItemText.Insert(index,item.ToString() ?? "null");

			// Do the text size calculations early
			CalculateTextSizes();

			// We want to preserve the selected item
			if(index < SelectedItemIndex)
				SelectedItemIndex++;

			return;
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The index to remove the item from.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than Count.</exception>
		/// <remarks>This will set SelectedItemIndex to -1 if we remove the selected item or decrement SelectedItemIndex if we removed something from before the selected item.</remarks>
		public void RemoveItemAt(int index)
		{
			Items.RemoveAt(index);
			ItemText.RemoveAt(index);

			// Do the text size calculations early
			CalculateTextSizes();

			// Preserve the selected item if possible
			if(index == SelectedItemIndex)
				SelectedItemIndex = -1;
			else if(index < SelectedItemIndex)
				SelectedItemIndex--;

			return;
		}

		public override void NotifyGameChange()
		{
			Decrement.Game = Game;
			Increment.Game = Game;
			Pencil.Game = Game;
			Background.Game = Game;

			base.NotifyGameChange();
			return;
		}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{return Decrement.Contains(pos,out component,include_children) || Increment.Contains(pos,out component,include_children);}

		public override Rectangle Bounds
		{
			get
			{
				// We have to calculate the bounds of the largest possible size for the pencil ourselves since it may not have those bounds currently
				float W = MaximumItemDimensions.X;
				float H = MaximumItemDimensions.Y;

				Vector2 TL = World * new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(W,0);
				Vector2 BL = World * new Vector2(0,H);
				Vector2 BR = World * new Vector2(W,H);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				// Even when Background is a dummy, it's Bounds should still be the local origin, so unioning with it should be fine
				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top)).Union(Decrement.Bounds).Union(Increment.Bounds).Union(Background.Bounds);
			}
		}

		public override int Width
		{
			get
			{
				float textx = MathF.Max(MaximumItemDimensions.X,Background.Width);

				switch(Alignment)
				{
				case ButtonAlignment.Left:
				case ButtonAlignment.Right:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(textx + Math.Max(Decrement.Width,Increment.Width));
					
					return (int)MathF.Ceiling(textx + Decrement.Width + Increment.Width);
				case ButtonAlignment.Top:
				case ButtonAlignment.Bottom:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(Math.Max(textx,MathF.Max(Decrement.Width,Increment.Width)));
					
					return (int)MathF.Ceiling(MathF.Max(textx,Decrement.Width + Increment.Width));
				case ButtonAlignment.Split:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(Math.Max(textx,MathF.Max(Decrement.Width,Increment.Width)));
					
					return (int)MathF.Ceiling(textx + Decrement.Width + Increment.Width);
				default:
					break;
				}

				return 0;
			}
		}

		public override int Height
		{
			get
			{
				float texty = MathF.Max(MaximumItemDimensions.Y,Background.Height);

				switch(Alignment)
				{
				case ButtonAlignment.Left:
				case ButtonAlignment.Right:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(MathF.Max(texty,Decrement.Height + Increment.Height));
					
					return (int)MathF.Ceiling(MathF.Max(texty,MathF.Max(Decrement.Height,Increment.Height)));
				case ButtonAlignment.Top:
				case ButtonAlignment.Bottom:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(texty + Decrement.Height + Increment.Height);
					
					return (int)MathF.Ceiling(texty + MathF.Max(Decrement.Height,Increment.Height));
				case ButtonAlignment.Split:
					if(Grouping == ButtonGrouping.Vertical)
						return (int)MathF.Ceiling(texty + Decrement.Height + Increment.Height);
					
					return (int)MathF.Ceiling(Math.Max(texty,MathF.Max(Decrement.Height,Increment.Height)));
				default:
					break;
				}

				return 0;
			}
		}

		/// <summary>
		/// The button used to decrement the selected item.
		/// </summary>
		public Button Decrement
		{get; protected set;}

		/// <summary>
		/// The button used to increment the selected item.
		/// </summary>
		public Button Increment
		{get; protected set;}

		/// <summary>
		/// The means by which the selected item text is written.
		/// </summary>
		public TextGameObject Pencil
		{get; protected set;}

		/// <summary>
		/// The background to draw underneath the text.
		/// <br/><br/>
		/// If no proper background is provided, a dummy component is placed here.
		/// </summary>
		public DrawableAffineObject Background
		{get; protected set;}

		/// <summary>
		/// This is the index of the currently selected item.
		/// <br/><br/>
		/// This can be set to any value that is at least -1 and less than Count.
		/// If it is set to -1, then no item will be selected.
		/// Attempting to set the selected item index to any other value will do nothing.
		/// </summary>
		public int SelectedItemIndex
		{
			get => _sii;

			set
			{
				if(value < -1 || value >= Count || value == _sii)
					return;

				int oii = SelectedItemIndex;
				T? oi = SelectedItem;

				_sii = value;

				OnSelectionChanged(this,oii,oi,SelectedItemIndex,SelectedItem);
				return;
			}
		}

		protected int _sii;

		/// <summary>
		/// If true, then we are double dipping into a text change and want to avoid that.
		/// </summary>
		protected bool DoubleDipping
		{get; set;}

		/// <summary>
		/// The selected item.
		/// If the selected item index is not valid, default(<typeparamref name="T"/>) is returned instead.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if attempting to set this to an item that doesn't exist.</exception>
		public T? SelectedItem
		{
			get => SelectedItemIndex >= 0 && SelectedItemIndex < Count ? Items[SelectedItemIndex] : default(T);

			set
			{
				if(value is null)
				{
					SelectedItemIndex = -1;
					return;
				}

				int index = Items.IndexOf(value);

				if(index < 0)
					throw new ArgumentException();

				SelectedItemIndex = index;
				return;
			}
		}

		/// <summary>
		/// Obtains the potentially selected item at index <paramref name="index"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or at least Count.</exception>
		public T this[int index] => Items[index];

		/// <summary>
		/// The number of items that may potentially be selected.
		/// </summary>
		public int Count => Items.Count;

		/// <summary>
		/// The items that can be selected.
		/// </summary>
		protected List<T> Items
		{get; set;}

		/// <summary>
		/// The text for the selected items.
		/// </summary>
		protected List<string> ItemText
		{get; set;}

		/// <summary>
		/// The maximum dimension of the selectable item text.
		/// </summary>
		protected Vector2 MaximumItemDimensions
		{get; set;}

		/// <summary>
		/// If true, then item selections will wrap around on the boundaries.
		/// If false, then item selections will hard stop at the boundaries.
		/// </summary>
		public bool WrapAround
		{get; set;}

		/// <summary>
		/// This selector's button grouping.
		/// See the enum's documentation for the details of this value's meaning.
		/// </summary>
		public ButtonGrouping Grouping
		{
			get => _g;

			set
			{
				if(_g == value)
					return;

				_g = value;
				AlignComponents();

				return;
			}
		}

		protected ButtonGrouping _g;

		/// <summary>
		/// This selector's button alignment.
		/// See the enum's documentation for the details of this value's meaning.
		/// </summary>
		public ButtonAlignment Alignment
		{
			get => _a;

			set
			{
				if(_a == value)
					return;

				_a = value;
				AlignComponents();

				return;
			}
		}

		protected ButtonAlignment _a;

		/// <summary>
		/// How the text is aligned with the buttons horizontally.
		/// The text is aligned within its maximum dimensions.
		/// </summary>
		public HorizontalTextAlignment HorizontalTextAlignment
		{
			get => _hta;

			set
			{
				if( _hta == value)
					return;

				_hta = value;
				AlignComponents();

				return;
			}
		}

		protected HorizontalTextAlignment _hta;

		/// <summary>
		/// How the text is aligned with the buttons vertically.
		/// The text is aligned within its maximum dimensions.
		/// </summary>
		public VerticalTextAlignment VerticalTextAlignment
		{
			get => _vta;

			set
			{
				if( _vta == value)
					return;

				_vta = value;
				AlignComponents();

				return;
			}
		}

		protected VerticalTextAlignment _vta;

		/// <summary>
		/// The horizontal text padding from its boundary.
		/// </summary>
		public float HorizontalTextPadding
		{
			get => _htp;

			set
			{
				if(_htp == value)
					return;

				_htp = value;
				CalculateTextSizes();

				return;
			}
		}

		protected float _htp;

		/// <summary>
		/// The vertical text padding from its boundary.
		/// </summary>
		public float VerticalTextPadding
		{
			get => _vtp;

			set
			{
				if(_vtp == value)
					return;

				_vtp = value;
				CalculateTextSizes();
				
				return;
			}
		}

		protected float _vtp;

		public override GUICore? Owner
		{
			set
			{
				if(ReferenceEquals(Owner,value))
					return;

				if(Owner is not null)
					Owner.OnActiveComponentChanged -= OnNext;

				base.Owner = value;

				if(Owner is not null)
					Owner.OnActiveComponentChanged += OnNext;

				// These are never permitted to be null after construction
				Decrement.Owner = value;
				Increment.Owner = value;

				return;
			}
		}

		public override SpriteBatch? Renderer
		{
			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;

				// These are never permitted to be null after construction
				Decrement.Renderer = value;
				Increment.Renderer = value;
				Pencil.Renderer = value;
				Background.Renderer = value;

				return;
			}
		}

		/// <summary>
		/// This event is called when the selected item changes.
		/// </summary>
		public event SelectedItemChanged<T> OnSelectionChanged;
	}

	/// <summary>
	/// Represents a button grouping's orientation.
	/// <list type="bullet">
	///	<li>Horizontal: Aligns the buttons so that they are side by side.</li>
	///	<li>Vertical: Aligns the buttons so that one is on top of the other.</li>
	/// </list>
	/// </summary>
	public enum ButtonGrouping
	{
		Vertical,
		Horizontal
	}

	/// <summary>
	/// Represents the button positions.
	/// <list type="bullet">
	///	<li>Left: Aligns the buttons on the left of the displayed item.</li>
	///	<li>Right: Aligns the buttons on the right of the displayed item.</li>
	///	<li>Top: Aligns the buttons above the displayed item.</li>
	///	<li>Bottom: Aligns the buttons below the displayed item.</li>
	///	<li>Split: Splits the buttons to opposite sides of the displayed item. If the grouping is Vertical, then they are placed on the top/bottom. If the grouping is Horizontal, then they are placed on the left/right.</li>
	/// </list>
	/// </summary>
	public enum ButtonAlignment
	{
		Left,
		Right,
		Top,
		Bottom,
		Split
	}

	/// <summary>
	/// An event that occurs when the selected item changes.
	/// </summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="sender">The selector sending the event.</param>
	/// <param name="old_selected_index">The old selected index.</param>
	/// <param name="old_item">The old selected item.</param>
	/// <param name="new_selected_index">The new selected index.</param>
	/// <param name="new_item">The new selected item.</param>
	public delegate void SelectedItemChanged<T>(Selector<T> sender, int old_selected_index, T? old_item, int new_selected_index, T? new_item) where T : notnull;
}