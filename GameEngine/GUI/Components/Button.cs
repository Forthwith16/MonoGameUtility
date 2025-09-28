using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Sprites;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A button.
	/// </summary>
	public class Button : GUIBase
	{
		/// <summary>
		/// Creates a new button.
		/// </summary>
		/// <param name="name">The name of this button.</param>
		/// <param name="backgrounds">
		///	The backgrounds to display for the disabled, normal, hovered, and mid-click states.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	These backgrounds can also be used to draw stylized text in stylized formats.
		///	<para/>
		///	This button will be responsible for initializing <paramref name="backgrounds"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="text">
		///	Text to display over the center of the button.
		///	If no text is required, simply provide null and it will be ignored.
		///	<para/>
		///	This button will be responsible for initializing <paramref name="text"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Button(string name, GameObjectLibrary backgrounds, TextGameObject? text = null, DrawableAffineObject? tooltip = null) : base(name,tooltip)
		{
			Library = backgrounds;
			Text = text;
			
			return;
		}

		protected override void LoadAdditionalContent()
		{
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

			// Keep the layer depth up to date
			DrawOrderChanged += (a,b) =>
			{
				Library.DrawOrder = DrawOrder;
				Library.LayerDepth = LayerDepth;

				return;
			};

			if(Text is not null)
			{
				Text.Parent = Library; // Transform the button (in this class if necessary), not the text (except to align it)
				Text.Renderer = Renderer;
				Text.LayerDepth = DrawOrder + 1;
				Text.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);
				Text.Initialize();
				
				// Keep the layer depth up to date
				DrawOrderChanged += (a,b) =>
				{
					Text.LayerDepth = DrawOrder + 1;
					Text.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);

					return;
				};

				// Initial text alignment
				AlignText();

				// Subscribe to the text and font change events in case we need to reposition (while trying to preserve other changes made)
				Text.TextChanged += (useless,ntext,otext) => AlignText();
				Text.FontChanged += (useless,nfont,ofont) => AlignText();

				// We'll want to keep the text in the center in case our size changes when we state change, and if that clobbers transformation data, that's not our problem
				Library.StateChange += (useless,active,old) => AlignText();
			}

			// Initialize all of the events that we personally care about
			// The user may have additional interests which they can deal with on their own time
			// If they care about when the background changes, they can provide appropriate delegates to the backgrounds
			OnClick += (a,b) =>
			{
				if(b.Button == MouseButton.Left) // Left click only thanks
					Library.ActiveGameObjectName = ClickState;

				return;
			};

			OnRelease += (a,b) => Library.ActiveGameObjectName = HoverState; // We can't not be hovering when we click a button
			OnHover += (a,b) => Library.ActiveGameObjectName = HoverState;
			OnExit += (a,b) => Library.ActiveGameObjectName = NormalState;

			EnabledChanged += (a,b) =>
			{
				if(Enabled)
					Library.ActiveGameObjectName = NormalState; // If we're hovering, the next update cycle will catch it and hover, which is fine
				else
					Library.ActiveGameObjectName = DisabledState;
			};

			return;
		}

		/// <summary>
		/// Aligns the text component of this button to the center along both axes.
		/// </summary>
		protected void AlignText()
		{
			// We'll position the text so that it's centered on the button (and hopefully doesn't spill out over the edges, but that's not our problem)
			// Also, fonts are never allowed to be null in the proper course of things, so if we fail here, it is acceptable to fail spectacularly
			// We'll go through the full effort of calculating the correct result, but we don't have to like it
			// We only need to know where the midpoint goes so we can translate against it (this may clobber child translations, but whatevs) and then to the center of the button
			if(Text is not null)
				Text.Transform = Matrix2D.Translation((Width - Text.Width) / 2.0f,(Height - Text.Height) / 2.0f);
				
			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			Library.Update(delta);
			Text?.Update(delta);

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Library.Draw(delta);
			Text?.Draw(delta);

			return;
		}

		public override void NotifyGameChange()
		{
			Library.Game = Game;

			if(Text is not null)
				Text.Game = Game;

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
		/// The text to write on this button (if any).
		/// </summary>
		public TextGameObject? Text
		{get; protected set;}

		/// <summary>
		/// The component library to use for drawing this button.
		/// </summary>
		public GameObjectLibrary Library
		{get; protected set;}

		public override SpriteRenderer? Renderer
		{
			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;
				Library.Renderer = value;
				
				if(Text is not null)
					Text.Renderer = value;

				return;
			}
		}

		/// <summary>
		/// The width of the button (sans transformations).
		/// </summary>
		public override int Width => Library.Width;

		/// <summary>
		/// The height of the button (sans transformations).
		/// </summary>
		public override int Height => Library.Height;

		/// <summary>
		/// The name of the component to use for drawing a button's normal state.
		/// </summary>
		public const string NormalState = "n";

		/// <summary>
		/// The name of the component to use for drawing a button's hover state.
		/// </summary>
		public const string HoverState = "h";

		/// <summary>
		/// The name of the component to use for drawing a button's mid-click state.
		/// </summary>
		public const string ClickState = "c";

		/// <summary>
		/// The name of the component to use for drawing a button's disabled state.
		/// </summary>
		public const string DisabledState = "d";
	}
}
