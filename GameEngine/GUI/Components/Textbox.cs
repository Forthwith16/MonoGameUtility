#pragma warning disable IDE0090 // Not a big fan of omitting types in new declarations

using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A textbox.
	/// </summary>
	public class Textbox : GUIBase
	{
		/// <summary>
		/// Creates a new textbox.
		/// </summary>
		/// <param name="name">The name of this textbox.</param>
		/// <param name="backgrounds">
		///	The backgrounds to display for the disabled, normal, hovered, and active states.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	These backgrounds can also be used to draw stylized text in stylized formats.
		///	<para/>
		///	This textbox will be responsible for initializing <paramref name="backgrounds"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="pencil_resource">The font asset used to write text.</param>
		/// <param name="pencil_color">The pencil color to write with.</param>
		/// <param name="cursor">
		///	This is the cursor to draw to indicate where new text will be inserted into the textbox.
		///	This will blink unless told otherwise with some requested frequency.
		///	It can also be hidden if desired.
		///	<para/>
		///	This textbox will be responsible for initializing <paramref name="cursor"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Textbox(string name, GameObjectLibrary backgrounds, string pencil_resource, Color pencil_color, ImageGameObject cursor, DrawableAffineObject? tooltip = null) : base(name,tooltip)
		{
			Library = backgrounds;
			Pencil = new TextGameObject(null,pencil_resource,"");
			Tint = pencil_color;
			Cursor = cursor;
			
			_tx = "";
			_ta = HorizontalTextAlignment.Left;
			_hp = 5.0f;
			_ml = -1;
			_fc = 0;
			_mhc = 0;
			
			_cp = 0;
			_icv = true;
			_cbt = 0.53;
			
			AutoCompleteEntries = null;

			OnTextChange += (a,b,c) => {};
			OnCursorMove += (a,b,c) => {};
			OnSubmit += (a,bc) => {};

			DoublingDown = false;
			IsInactive = true;
			IsHovering = false;

			ExternalTextSet = true;
			return;
		}

		/// <summary>
		/// Creates a new textbox.
		/// </summary>
		/// <param name="name">The name of this textbox.</param>
		/// <param name="backgrounds">
		///	The backgrounds to display for the disabled, normal, hovered, and active states.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	These backgrounds can also be used to draw stylized text in stylized formats.
		///	<para/>
		///	This textbox will be responsible for initializing <paramref name="backgrounds"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="pencil">The font used to write text.</param>
		/// <param name="pencil_color">The pencil color to write with.</param>
		/// <param name="cursor">
		///	This is the cursor to draw to indicate where new text will be inserted into the textbox.
		///	This will blink unless told otherwise with some requested frequency.
		///	It can also be hidden if desired.
		///	<para/>
		///	This textbox will be responsible for initializing <paramref name="cursor"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Textbox(string name, GameObjectLibrary backgrounds, SpriteFont pencil, Color pencil_color, ImageGameObject cursor, DrawableAffineObject? tooltip = null) : base(name,tooltip)
		{
			Library = backgrounds;
			Pencil = new TextGameObject(null,pencil,"");
			Tint = pencil_color;
			Cursor = cursor;
			
			_tx = "";
			_ta = HorizontalTextAlignment.Left;
			_hp = 5.0f;
			_ml = -1;
			_fc = 0;
			_mhc = 0;
			
			_cp = 0;
			_icv = true;
			_cbt = 0.53;
			
			AutoCompleteEntries = null;

			OnTextChange += (a,b,c) => {};
			OnCursorMove += (a,b,c) => {};
			OnSubmit += (a,bc) => {};

			DoublingDown = false;
			IsInactive = true;
			IsHovering = false;

			FrameShieldSubmission = false;
			ExternalTextSet = true;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			// Set up the library
			Library.Parent = this;
			Library.Renderer = Renderer;
			Library.DrawOrder = DrawOrder;
			Library.LayerDepth = LayerDepth;
			Library.Initialize();
			
			// We default to the normal state if we're enabled and the disabled state if not
			if(Enabled)
				Library.ActiveGameObjectName = NormalState;
			else
				Library.ActiveGameObjectName = DisabledState;

			// Set up the cursor
			Cursor.Parent = this;
			Cursor.Renderer = Renderer;
			Cursor.DrawOrder = DrawOrder + 2;
			Cursor.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 2);
			Cursor.Visible = false; // Cursors are only visible if we are the active component
			Cursor.Initialize();
			
			// Set up the pencil
			Pencil.Parent = Library; // Transform the button (in this class if necessary), not the text (except to align it)
			Pencil.Renderer = Renderer;
			Pencil.DrawOrder = DrawOrder + 1;
			Pencil.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);
			Pencil.Initialize();
			
			// Subscribe to the text and font change events in case we need to reposition (while trying to preserve other changes made)
			Pencil.FontChanged += (useless,nfont,ofont) => AlignText();
			Pencil.TextChanged += (useless,ntext,otext) =>
			{
				// If we would be doubling down on a text assignment, do nothing
				if(DoublingDown)
					return;

				// Otherwise, we only permit this text to be assigned in the set of Text, so send it back to what it once was
				DoublingDown = true;
				Pencil.Text = otext;
				DoublingDown = false;

				return; // Text is only legitimately changed through Text, and that will call AlignText for us, so we don't need to do that here
			};

			// Keep the layer depths up to date
			DrawOrderChanged += (a,b) =>
			{
				Library.DrawOrder = DrawOrder;
				Library.LayerDepth = LayerDepth;

				Cursor.DrawOrder = DrawOrder + 1;
				Cursor.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);

				Pencil.DrawOrder = DrawOrder + 2;
				Pencil.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 2);

				return;
			};

			// Initially align the text to make sure no one can out fox us
			AlignText();

			// Keep the text aligned
			OnTextChange += (a,b,c) =>
			{
				if(ExternalTextSet)
				{
					CursorPosition -= CursorPositionError; // This will do nothing if there is no error
					AlignText();
				}

				return;
			};

			// Keep our cursor in view
			OnCursorMove += (a,b,c) =>
			{
				if(ExternalTextSet)
				{
					FirstCharacter += CursorPositionError; // This will do nothing if there is no error
					AlignText();
				}

				return;
			};

			// We'll want to keep the text properly aligned in case our size changes when we state change, and if that clobbers transformation data, that's not our problem
			Library.StateChange += (useless,active,old) => AlignText();
			
			// Initialize all of the events that we personally care about
			// The user may have additional interests which they can deal with on their own time
			// If they care about when the background changes, they can provide appropriate delegates to the backgrounds
			// We first pick off the input events
			Game!.Window.KeyDown += HandleInput; // We should never load content without a game
			Game.Window.TextInput += HandleInput;

			OnClick += (a,b) =>
			{
				// If we're submitting this frame, we prevent all clicks
				if(FrameShieldSubmission)
					return;

				// If we've clicked with the left button on this and were inactive, then we become active regardless of if we used the mouse or digital navigation
				// Digital events when we are active are gobbled up by the input handler
				// Mouse events when we are already active either move the cursor (clicking on this) or deactive this (handled in update)
				if(b.Button == MouseButton.Left) // The MouseClick event generates Left for whatever input we assign to it, even if that's not the left mouse button
				{
					// We only need to do this part if we weren't already active
					if(IsInactive)
					{
						Library.ActiveGameObjectName = ActiveState;
						IsInactive = false;
					}

					// We also need to move the cursor to where we clicked (mouse events only)
					// We do this even if we weren't active before
					if(b.IsMouse)
					{
						// Special case empty textboxes for both speed and convenience since we need two position entries in general to decide between
						if(DisplayedText.Length == 0)
							CursorPosition = FirstCharacter;
						else
						{
							// Translate the mouse position into local coordinates
							float xpos = (InverseWorld * b.Position.ToVector2()).X;

							// Since we clicked on this textbox (that's the only way to get here with the mouse), then know that pos is in bounds
							// We just need to run a binary search to figure out which cursor position is most appropriate
							int l = 0;
							int r = DisplayedText.Length;

							// We'll remember the values we calculate, because we'll need at least two of them again at the end
							float[] positions = new float[DisplayedText.Length + 1];
							bool[] calculated = new bool[DisplayedText.Length + 1];

							// Just run a binary search to completion; it takes only a single more iteration on average, so that's fine
							// At the end, l will point to the first thing with a position greater than xpos (even if there is something precisely equal to xpos)
							// This will be out of bounds on the right but will be in bounds on the left
							while(l <= r)
							{
								int mid = (l + r) >> 1;
								
								// Save the data we calculate for later use
								positions[mid] = CalculateCursorXPosition(FirstCharacter + mid);
								calculated[mid] = true;
								
								if(positions[mid] > xpos) // If we're past where we clicked, then we should bring the right index closer
									r = mid - 1;
								else // If we haven't passed the point we clicked, then we should bring the left index closer
									l = mid + 1;
							}

							// Special case the boundaries to avoid having to deal with special cases later
							// The lower boundary can occur since the text is (usually) not flush with zero (when it can't occur, we only waste a clock cycle, so whatever)
							if(l == 0)
								CursorPosition = FirstCharacter;
							else if(l == DisplayedText.Length + 1)
								CursorPosition = FirstCharacter + DisplayedText.Length;
							else
							{
								// If we don't have a position 
								if(!calculated[l - 1])
									positions[l - 1] = CalculateCursorXPosition(FirstCharacter + l - 1);

								// Now we need to see if we're closer to l or l - 1
								float mid = 0.5f * (positions[l] + positions[l - 1]); // This is the middle of a character

								if(xpos <= mid)
									CursorPosition = FirstCharacter + l - 1;
								else
									CursorPosition = FirstCharacter + l;
							}
						}
					}
				}

				return;
			};

			OnHover += (a,b) =>
			{
				// If we're not the active component, then we should hover
				if(IsInactive)
					Library.ActiveGameObjectName = HoverState;

				IsHovering = true;
				return;
			};

			OnExit += (a,b) =>
			{
				// If we're not the active component, then we should go back to normal
				if(IsInactive)
					Library.ActiveGameObjectName = NormalState;

				IsHovering = false;
				return;
			};

			EnabledChanged += (a,b) =>
			{
				if(Enabled)
					Library.ActiveGameObjectName = NormalState; // If we're hovering (we can never be the active component when we first are enabled), the next update cycle will catch it and hover, which is fine
				else
				{
					Library.ActiveGameObjectName = DisabledState;
					
					IsInactive = true;
					IsHovering = false;
				}
			};

			return;
		}

		/// <summary>
		/// Processes keyboard input.
		/// </summary>
		/// <param name="sender">The object sending the event.</param>
		/// <param name="e">The event arguments.</param>
		protected void HandleInput(object? sender, InputKeyEventArgs e)
		{
			// If we're not active, then don't bother
			if(IsInactive)
				return;

			// Our internal text set should not trigger text alignment nonsense that we will deal with ourselves to avoid doubling down
			ExternalTextSet = false;

			// Record if we did anything that will need us to align the test (we assume that we have
			bool did_something = false;

			// We we execute these commands, we must set both CursorPosition and FirstCharacter since this is an internal text set, and those are not coupled together internally to save on alignment calls
			switch(e.Key)
			{
			case Keys.Left:
				if(CursorPosition > 0)
				{
					CursorPosition--;

					if(CursorPosition < FirstCharacter)
						FirstCharacter--;

					did_something = true;
				}

				break;
			case Keys.Right:
				if(CursorPosition < Text.Length)
				{
					CursorPosition++;

					if(CursorPosition > FirstCharacter + DisplayedText.Length)
						FirstCharacter++;

					did_something = true;
				}

				break;
			case Keys.Home:
				// If our cursor is already at position 0, then we're done (this is only possible if FirstCharacter is also 0)
				if(CursorPosition > 0)
				{
					CursorPosition = 0;
					FirstCharacter = 0;

					did_something = true;
				}

				break;
			case Keys.End:
				// If our cursor is already at the last position, then we're done (this is only possible if FirstCharacter is also MaximumHiddenCharacters)
				if(CursorPosition < Text.Length)
				{
					CursorPosition = Text.Length;
					FirstCharacter = Text.Length; // When we go to align our text, this will go to the correct value with only the addition of a couple clock cycles

					did_something = true;
				}

				break;
			default:
				break;
			}

			// Now that we've updated everything, we can align our text if we need to
			if(did_something)
				AlignText();

			// And now we can deactivate our internal text set
			ExternalTextSet = true;

			return;
		}

		/// <summary>
		/// Processes keyboard input.
		/// <para/>
		/// This will ignore the raw state of the keyboard and acknowledge whatever bizarre regional keyboard layout is in use.
		/// </summary>
		/// <param name="sender">The object sending the event.</param>
		/// <param name="e">The event arguments.</param>
		protected void HandleInput(object? sender, TextInputEventArgs e)
		{
			// If we're not active, then don't bother
			if(IsInactive)
				return;

			// Our internal text set should not trigger text alignment nonsense that we will deal with ourselves to avoid doubling down
			ExternalTextSet = false;
			
			// Record if we did anything that will need us to align the test (we assume that we have
			bool did_something = false;

			// Processing the input character is not as simple as appending it to the end of the string, because we have commands to parse
			// We also need to set the FirstCharacter based on what we do and move the cursor
			// We'll deal with command characters first
			if(char.IsControl(e.Character))
			{
				// We only care about a specific set of control characters and will simply discard the remainder
				switch(e.Character)
				{
				case '\t': // Tabs allow autocompletion so long as we have something to autocomplete to
					if(AutoCompleteEntries is not null)
					{
						// We autocomplete based off of where the cursor is
						Text = Text[0..CursorPosition].AutoComplete(AutoCompleteEntries);
						CursorPosition = Text.Length;
						FirstCharacter = Text.Length;

						did_something = true;
					}

					break;
				case '\b':
					if(CursorPosition > 0)
					{
						Text = Text.Remove(CursorPosition - 1,1);
						CursorPosition--;

						if(CursorPosition < FirstCharacter)
							FirstCharacter--;

						did_something = true;
					}

					break;
				case '\n': // This is what we SHOULD receive when pressing enter
				case '\r': // This is what we seem to receive when pressing enter
					// When we submit, we're done
					IsInactive = true;
					
					// Change our state now
					if(IsHovering)
						Library.ActiveGameObjectName = HoverState;
					else
						Library.ActiveGameObjectName = NormalState;
					
					// And now we can safely submit our text
					FrameShieldSubmission = true;
					OnSubmit(this,Text);
					
					break;
				default: // If we're discarding the control character, we don't need to do anything more
					break;
				}
			}
			else if(Pencil.Font is not null && Pencil.Font.GetGlyphs().ContainsKey(e.Character)) // Test to see if our font supports the proposed character
			{
				// Insert the character wherever the cursor is and move the cursor right one
				Text = Text.Insert(CursorPosition,e.Character.ToString());
				CursorPosition++;
				
				if(CursorPosition > FirstCharacter + DisplayedText.Length)
					FirstCharacter++;

				did_something = true;
			}
			
			// Now that we've updated everything, we can align our text if we need to
			if(did_something)
				AlignText();

			// And now we can deactivate our internal text set
			ExternalTextSet = true;

			return;
		}

		/// <summary>
		/// Aligns the text component of this textbox to the appropriate position according to our alignment.
		/// Also updates the displayed text since that can change as well.
		/// </summary>
		protected void AlignText()
		{
			// First, we need to pick the correct text to display
			PickDisplayedText();
			
			// Now that we have the correct text, we need to position it appropriately
			switch(TextAlignment)
			{
			case HorizontalTextAlignment.Left:
				Pencil.Transform = Matrix2D.Translation(HorizontalPadding,(Height - Pencil.Height) / 2.0f);
				break;
			case HorizontalTextAlignment.Center:
				Pencil.Transform = Matrix2D.Translation(0.5f * (Width - Pencil.Width),(Height - Pencil.Height) / 2.0f);
				break;
			case HorizontalTextAlignment.Right:
				Pencil.Transform = Matrix2D.Translation(Width - Pencil.Width - HorizontalPadding,(Height - Pencil.Height) / 2.0f);
				break;
			}
			
			// To finish, we align the cursor with wherever its text ended up
			if(IsCursorVisible)
				AlignCursor();
			
			return;
		}
		
		/// <summary>
		/// Picks the correct displayed text given the current state of the system.
		/// Also calculates the current maximum number of hideable characters in the process.
		/// </summary>
		protected void PickDisplayedText()
		{
			// If we have no font, we write nothing
			if(Pencil.Font is null)
			{
				DisplayedText = ""; // If this changes nothing, then this will do nothing, so hooray
				
				MaximumHiddenCharacters = 0;
				FirstCharacter = 0; // We can safely clobber whatever was in here

				return;
			}

			// Determine the maximum text space available
			float max_w = MathF.Max(Width - 2.0f * HorizontalPadding,0.0f); // If the text is empty and we have no backgrounds, we can rarely get here and be annoying

			// First check to see if we can just display the whole text, which should be fairly common
			// Our pencil is always forcibly untransformed, so we can just measure the string directly
			if(Pencil.Font.MeasureString(Text).X < max_w)
			{
				DisplayedText = Text; // We we've changed nothing, then this does nothing, and that's fine
				
				MaximumHiddenCharacters = 0;
				FirstCharacter = 0; // We can safely clobber whatever was in here

				return;
			}

			// If that failed, then we should perform a binary search first to find the maximum (left) hideable number of characters
			int l = 0;
			int r = Text.Length - 1;

			// Our binary search is on average only one iteration longer by not checking for if we've hit the boundary (and going to completion), but it makes us so we only have to compute roughly half as many expensive MeasureString operations on average
			// The result will be held in l
			while(l <= r)
			{
				int mid = (l + r) >> 1;

				if(Pencil.Font.MeasureString(Text[mid..^0]).X > max_w) // If we're too long, then this is no good, and we need to consider shorter suffixes
					l = mid + 1;
				else // Otherwise, we can consider longer strings
					r = mid - 1;
			}

			// This is the index of the first character of the longest suffix we can display, so there are exactly this many characters that are hidden
			// This value is always greater than 0 since the entire text cannot be displayed (we discarded that possibility as a special case)
			MaximumHiddenCharacters = l;

			// Now that we have a valid MaximumHiddenCharacters, we should match FirstCharacter to that bound
			if(FirstCharacter > MaximumHiddenCharacters)
				FirstCharacter = MaximumHiddenCharacters;

			// If our first character is exactly MaximumHidenCharacters, then we can just call it a day
			// We do want to special case this, because this case behaves poorly with binary search
			if(FirstCharacter == MaximumHiddenCharacters)
			{
				DisplayedText = Text[FirstCharacter..^0];
				return;
			}

			// Now that we have how many characters CAN be hidden, we also have an up-to-date value for the first character
			// We will perform another binary search now to discover how many characters we can display from our up-to-date first character
			// To do so, we only need to start l at FirstCharacter and focus on the prefixes of this substring instead of suffixes
			l = FirstCharacter;
			r = Text.Length - 1;

			// Similarly to before, our result will be held in r at the end
			// This r value will be exclusive, i.e. the first index that makes the string too long
			while(l <= r)
			{
				int mid = (l + r) >> 1;

				if(Pencil.Font.MeasureString(Text[FirstCharacter..mid]).X > max_w) // If we're too long, then we need to contract our prefix
					r = mid - 1;
				else // Otherwise, we should consider longer strings
					l = mid + 1;
			}

			// We can now assign the displayed text using the result of our binary search held in r
			DisplayedText = Text[FirstCharacter..r];

			return;
		}

		/// <summary>
		/// Aligns the cursor to the correct position.
		/// </summary>
		/// <remarks>This <i>does not</i> reset the blink timer since this may be aligning the cursor to where it already is.</remarks>
		protected void AlignCursor()
		{
			// If we don't have a font, we do nothing and the cursor would be at the emptystring position regardless, so that's perfect
			if(Pencil.Font is null)
				return;

			// Pencil holds the precomputed DisplayText and its measure, so we can do our work here fast
			// We have to use clamped values for calculating the cursor's x position because we don't update the cursor position at the same time we change the text
			switch(TextAlignment)
			{
			case HorizontalTextAlignment.Left:
				Cursor.Transform = Matrix2D.Translation(CalculateCursorXPosition(CursorPosition),(Height - Cursor.Height) / 2.0f);
				break;
			case HorizontalTextAlignment.Center:
				Cursor.Transform = Matrix2D.Translation(CalculateCursorXPosition(CursorPosition),(Height - Cursor.Height) / 2.0f);
				break;
			case HorizontalTextAlignment.Right:
				Cursor.Transform = Matrix2D.Translation(CalculateCursorXPosition(CursorPosition),(Height - Cursor.Height) / 2.0f);
				break;
			}
			
			return;
		}

		/// <summary>
		/// Calculates the cursor x position if it were equal to <paramref name="cp"/>.
		/// </summary>
		/// <param name="cp">The theoretical cursor position. This value must be between FirstCharacter and FirstCharacter + DisplayedText.Length.</param>
		/// <returns>
		///	Returns the x position of where the cursor should be placed when at position <paramref name="cp"/> when <paramref name="cp"/> is in bounds.
		///	If <paramref name="cp"/> is out of bounds or if the pencil has no Font yet, then this will return an error value of 0.0f;
		/// </returns>
		protected float CalculateCursorXPosition(int cp)
		{
			if(Pencil.Font is null)
				return 0.0f;

			switch(TextAlignment)
			{
			case HorizontalTextAlignment.Left:
				return Pencil.Font.MeasureString(DisplayedText[0..(MathHelper.Clamp(cp - FirstCharacter,0,DisplayedText.Length))]).X + HorizontalPadding;
			case HorizontalTextAlignment.Center:
				return 0.5f * (Width - Pencil.Width) + Pencil.Font.MeasureString(DisplayedText[0..(MathHelper.Clamp(cp - FirstCharacter,0,DisplayedText.Length))]).X;
			case HorizontalTextAlignment.Right:
				return Width - Pencil.Font.MeasureString(DisplayedText[(MathHelper.Clamp(cp - FirstCharacter,0,DisplayedText.Length))..^0]).X - HorizontalPadding;
			default:
				break;
			}

			return 0.0f;
		}

		/// <summary>
		/// Makes this textbox inactive when the focused component changes.
		/// </summary>
		/// <param name="sender">The GUICore sending this event.</param>
		/// <param name="e">The event details.</param>
		protected void FocusChanged(GUICore sender, FocusedComponentChangedEvent e)
		{
			// If we click anywhere other than on this, then we should become inactive
			if(Enabled && IsActive && ReferenceEquals(sender,Owner) && !ReferenceEquals(this,Owner.FocusedComponent))
			{
				IsInactive = true;

				if(IsHovering)
					Library.ActiveGameObjectName = HoverState;
				else
					Library.ActiveGameObjectName = NormalState;
			}

			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			// We can always set it to false since GUI components are updated after the GUICore handles input
			FrameShieldSubmission = false;

			Library.Update(delta);
			Pencil.Update(delta);
			Cursor.Update(delta);

			// If the cursor is visible and it blinks, then we'll update its time
			if(IsCursorVisible && CursorBlinks)
			{
				CursorDelay -= delta.ElapsedGameTime.TotalSeconds;

				// If it's time to blink, then blink
				if(CursorDelay <= 0.0)
				{
					Cursor.Visible = !Cursor.Visible;
					CursorDelay = CursorBlinkTime;
				}
			}

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Library.Draw(delta);
			Pencil.Draw(delta);

			if(IsActive && IsCursorVisible && Cursor.Visible && IsCursorInBounds)
				Cursor.Draw(delta);

			return;
		}
		
		protected override void UnloadContentAddendum()
		{
			// We have to remove ourselves from the game's text input event
			if(Game is not null)
				Game.Window.TextInput -= HandleInput;

			return;
		}

		public override void NotifyGameChange()
		{
			Library.Game = Game;
			Pencil.Game = Game;
			Cursor.Game = Game;

			base.NotifyGameChange();
			return;
		}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// Transform pos into the local coordinate system
			pos = InverseWorld * pos;

			// We need to calculate containment within an arbitrarily rotated, translated, and scaled rectangle
			// The obvious choice for doing this is to just make sure we're all on the 'inside' of the lines defined by the four corners of the untransformed boundary
			int W = Width; // Who knows how long these take to calculate
			int H = Height;

			Vector2 TL = new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
			Vector2 TR = new Vector2(W,0);
			Vector2 BL = new Vector2(0,H);
			Vector2 BR = new Vector2(W,H);

			// To check that we're on the 'inside', we need only check that the cross product points up clockwise (screen coordinates are left-handed)
			if((TR - TL).Cross(pos - TL) >= 0.0f && (BR - TR).Cross(pos - TR) >= 0.0f && (BL - BR).Cross(pos - BR) >= 0.0f && (TL - BL).Cross(pos - BL) >= 0.0f)
			{
				component = this;
				return true;
			}
			
			component = null;
			return false;
		}

		public override Rectangle Bounds
		{
			get
			{
				// We need to know where the four corners end up as the most extreme points
				// Then we can pick the min and max values to get a bounding rectangle
				int W = Width; // Who knows how long these take to calculate
				int H = Height;
				
				Vector2 TL = World * new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(W,0);
				Vector2 BL = World * new Vector2(0,H);
				Vector2 BR = World * new Vector2(W,H);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top));
			}
		}

		/// <summary>
		/// The means by which text is written in this textbox.
		/// </summary>
		public TextGameObject Pencil
		{get;}

		/// <summary>
		/// The pencil color.
		/// </summary>
		public override Color Tint
		{
			set
			{
				base.Tint = value;

				if(Pencil is null)
					return;

				Pencil.Tint = value;
				return;
			}
		}

		/// <summary>
		/// This is the text displayed in the textbox.
		/// </summary>
		public string Text
		{
			get => _tx;

			set
			{
				// If we're asked to do nothing, then do nothing
				if(_tx == value)
					return;

				// Remember the old value for later
				string old = _tx;

				// We need to trim the proposed text according to any rules we have
				if(MaxLength >= 0 && value.Length > MaxLength)
					value = value[0..MaxLength];

				// Assign the text
				_tx = value;
				
				// Now invoke our update event
				// We don't need to call AlignText or PickDisplayedText since this event invocation will do that for us
				OnTextChange(this,old,_tx);
				
				return;
			}
		}

		protected string _tx;

		/// <summary>
		/// If false, then our text set came from an internal source.
		/// This should not trigger TextAlign or FirstCharacter/cursor positioning.
		/// <para/>
		/// If true, then our text set is external.
		/// In this case, the FirstCharacter/cursor may need to be repositioned.
		/// A TextAlign call will also be necessary.
		/// </summary>
		protected bool ExternalTextSet
		{get; set;}
		
		/// <summary>
		/// If true, then we do not accept clicks until the next frame.
		/// This is true when we submit and is set to false the frame after.
		/// </summary>
		protected bool FrameShieldSubmission
		{get; set;}

		/// <summary>
		/// The maximum number of characters permitted in Text.
		/// This value defaults to -1.
		/// <para/>
		/// If this value is negative, then any number of characters are permitted.
		/// </summary>
		public int MaxLength
		{
			get => _ml;

			set
			{
				if(_ml == value)
					return;

				_ml = value;

				// If our text is too long, we trim it
				if(_ml >= 0 && Text.Length > _ml)
					Text = Text[0.._ml]; // ExternalTextSet should be true at this point, so this will trigger a text alignment as desired

				return;
			}
		}

		protected int _ml;

		/// <summary>
		/// This is the displayed text in the textbox.
		/// </summary>
		public string DisplayedText
		{
			get => Pencil.Text;

			protected set
			{
				DoublingDown = true;
				Pencil.Text = value;
				DoublingDown = false;

				return;
			}
		}

		/// <summary>
		/// If true, then we should be wary of doubleing down on the Pencil's textbox changing.
		/// </summary>
		protected bool DoublingDown
		{get; set;}

		/// <summary>
		/// This is the leftmost character index of Text to appear in DisplayedText.
		/// <para/>
		/// Attempting to set this to greater than MaximumHiddenCharacters will <i>not</i> cause it to clamp to the boundaries but be careful to ensure it is once MaximumHiddenCharacters is correct.
		/// It <i>will</i>, however, prevent it from being negative or longer than Text.Length.
		/// </summary>
		protected int FirstCharacter
		{
			get => _fc;

			set
			{
				// Assign the new first character
				// We don't need to align the text because these calls are purely internal, so the program will handle that for us
				// We will also trust that by the time text is displayed, this value makes sense with MaximumHiddenCharacters
				_fc = MathHelper.Clamp(value,0,Text.Length);
				return;
			}
		}

		protected int _fc;

		/// <summary>
		/// The current maximum number of characters that can be hidden.
		/// <para/>
		/// This value always maximizes the number of left hidden characters.
		/// <para/>
		/// Attempting to set this value to itself, a negative number, or longer than Text.Length does nothing.
		/// </summary>
		protected int MaximumHiddenCharacters
		{
			get => _mhc;

			set
			{
				if(value < 0 || value == _mhc)
					return;

				_mhc = MathHelper.Clamp(value,0,Text.Length);
				return;
			}
		}

		protected int _mhc;
		
		/// <summary>
		/// The text alignment of the textbox.
		/// </summary>
		public HorizontalTextAlignment TextAlignment
		{
			get => _ta;

			set
			{
				if(_ta == value)
					return;

				_ta = value;
				AlignText(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}

		protected HorizontalTextAlignment _ta;

		/// <summary>
		/// The horizontal padding between the textbox and its text.
		/// <para/>
		/// This value must be nonnegative.
		/// Attempting to set this to a negative value does nothing.
		/// </summary>
		public float HorizontalPadding
		{
			get => _hp;
			
			set
			{
				if(_hp == value || value < 0.0f)
					return;
				
				_hp = value;
				AlignText(); // We have to manually call this realignment since nothing will for us

				return;
			}
		}

		protected float _hp;

		/// <summary>
		/// The position of the cursor in the text.
		/// <para/>
		/// This value must be within the range [0,|Text|].
		/// Attempting to set it to a value out of bounds will result in it being clamped to the extrema.
		/// </summary>
		public int CursorPosition
		{
			get => _cp;

			set
			{
				if(_cp == value)
					return;

				int old = _cp;
				_cp = MathHelper.Clamp(value,0,Text.Length);
				
				// Reset the cursor blink time anytime we move the cursor
				Cursor.Visible = IsCursorVisible;
				CursorDelay = CursorBlinkTime;
				
				OnCursorMove(this,old,_cp);
				return;
			}
		}

		protected int _cp;

		/// <summary>
		/// Determines if the cursor position is in the displayed text area.
		/// </summary>
		protected bool IsCursorInBounds => CursorPosition >= FirstCharacter && CursorPosition <= FirstCharacter + DisplayedText.Length;

		/// <summary>
		/// Determines the amount of error (left or right) the cursor position is in.
		/// <para/>
		/// For example, if FirstCharacter were 1 and CursorPosition were 0, then this would return -1.
		/// If instead FirstCharacter were 1, DisplayedText's length were 5, and CursorPosition were 8, then this would return 2.
		/// </summary>
		protected int CursorPositionError
		{
			get
			{
				// If we set the cursor to a position that's not visible, then we should make it visible
				if(CursorPosition < FirstCharacter)
					return CursorPosition - FirstCharacter; // If we undershoot, we can easily just subtract CursorPosition from FirstCharacter
				else if(CursorPosition > FirstCharacter + DisplayedText.Length) // CursorPosition = i means the cursor is just BEFORE the character at index i, so the cursor can go DisplayText.Length characters right without forcing a shift
					return CursorPosition - FirstCharacter - DisplayedText.Length ; // We overshot in this case

				return 0; // No error
			}
		}

		/// <summary>
		/// If true, then the cursor is visible (but may be blinking and currently invisible).
		/// If false, then it is invisible (at all times).
		/// </summary>
		public bool IsCursorVisible
		{
			get => _icv;

			set
			{
				if(_icv == value)
					return;

				_icv = value;

				// If we made the cursor visible, we should align it properly (and reset its timer)
				// If we made it invisible, then who cares where it goes
				if(_icv)
				{
					CursorDelay = CursorBlinkTime;
					AlignCursor();
				}

				return;
			}
		}

		protected bool _icv;

		/// <summary>
		/// The text cursor.
		/// </summary>
		protected ImageGameObject Cursor
		{get; set;}

		/// <summary>
		/// This is how long in seconds the cursor takes to blink.
		/// It will remain visible for this length of time, hide for this length of time, and repeat indefiniately.
		/// The delay always resets to visible (assuming the cursor is visible) when the cursor moves.
		/// <para/>
		/// If this value is nonpositive, then the cursor will not blink.
		/// </summary>
		public double CursorBlinkTime
		{
			get => _cbt;

			set
			{
				_cbt = value;

				// We also reset cursor visibility when we change the blink time
				Cursor.Visible = IsCursorVisible;

				// Set the blink timer to whatever our new value is if it's not perma-no-blink; this is more intuitive behavior than letting the old timer time out
				if(CursorBlinks)
					CursorDelay = _cbt;

				return;
			}
		}

		protected double _cbt;

		/// <summary>
		/// If true, then the cursor blinks when it's visible.
		/// If false, then the cursor is always visible when it's not hidden.
		/// </summary>
		public bool CursorBlinks => _cbt > 0.0;

		/// <summary>
		/// This is how long remains (in seconds) before the cursor blinks.
		/// </summary>
		protected double CursorDelay
		{get; set;}

		/// <summary>
		/// This is a set of autocomplete entries.
		/// If this value is not null, then the tab key will permit autocompletion of the current Text value with these entries.
		/// Otherwise, the tab key is nonfunctional.
		/// <para/>
		/// This value defaults to null.
		/// </summary>
		public IEnumerable<string>? AutoCompleteEntries
		{get; set;}

		/// <summary>
		/// If true, then this textbox is active.
		/// </summary>
		protected bool IsActive
		{
			get => !IsInactive;
			set => IsInactive = !value;
		}

		/// <summary>
		/// If true, then this textbox is inactive.
		/// </summary>
		protected bool IsInactive
		{get; set;}

		/// <summary>
		/// If true, then this textbox is being hovered over.
		/// </summary>
		protected bool IsHovering
		{get; set;}

		/// <summary>
		/// The component library to use for drawing this textbox's backgrounds.
		/// </summary>
		protected GameObjectLibrary Library
		{get; set;}

		public override GUICore? Owner
		{
			get => base.Owner;

			set
			{
				if(ReferenceEquals(Owner,value))
					return;

				if(Owner is not null)
					Owner.OnFocusedComponentChanged -= FocusChanged;
				
				base.Owner = value;
				
				if(Owner is not null)
					Owner.OnFocusedComponentChanged += FocusChanged;

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
				Library.Renderer = value;
				Cursor.Renderer = value;
				
				if(Pencil is not null)
					Pencil.Renderer = value;

				return;
			}
		}
		
		/// <summary>
		/// The width of the textbox (sans transformations).
		/// </summary>
		public override int Width => Library.Width;

		/// <summary>
		/// The height of the textbox (sans transformations).
		/// </summary>
		public override int Height => Library.Height;

		public override bool SuspendsDigitalNavigation => IsActive;

		/// <summary>
		/// This event is called when this textbox's text changes.
		/// </summary>
		public event TextChanged OnTextChange;

		/// <summary>
		/// This event is called when this textbox's text changes.
		/// </summary>
		public event CursorMove OnCursorMove;

		/// <summary>
		/// This event is called when a digital click is performed while this textbox is already active.
		/// This usually corresponds to a user hitting enter while typing text.
		/// </summary>
		public event Submit OnSubmit;

		/// <summary>
		/// The name of the component to use for drawing a textbox's normal state.
		/// </summary>
		public const string NormalState = "n";

		/// <summary>
		/// The name of the component to use for drawing a textbox's hover state.
		/// </summary>
		public const string HoverState = "h";

		/// <summary>
		/// The name of the component to use for drawing a textbox when it is the active component.
		/// </summary>
		public const string ActiveState = "a";

		/// <summary>
		/// The name of the component to use for drawing a textbox's disabled state.
		/// </summary>
		public const string DisabledState = "d";
	}

	/// <summary>
	/// An event called when a textbox's text changes.
	/// </summary>
	/// <param name="sender">The textbox whose text has changed.</param>
	/// <param name="old_text">The old text value.</param>
	/// <param name="new_text">The new text value.</param>
	public delegate void TextChanged(Textbox sender, string old_text, string new_text);

	/// <summary>
	/// An event called when a textbox's cursor moves.
	/// </summary>
	/// <param name="sender">The textbox whose cursor has moved.</param>
	/// <param name="old_pos">The old cursor position.</param>
	/// <param name="new_pos">The new cursor position.</param>
	public delegate void CursorMove(Textbox sender, int old_pos, int new_pos);

	/// <summary>
	/// An event called when a digital click (enter/submit/etc) is performed while it is already active.
	/// </summary>
	/// <param name="sender">The textbox submitting a string.</param>
	/// <param name="text">The string submitted.</param>
	public delegate void Submit(Textbox sender, string text);
}