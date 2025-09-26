using GameEngine.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A game object that draws text.
	/// </summary>
	public class TextGameObject : DrawableAffineObject
	{
		/// <summary>
		/// Creates a game object that writes a message.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="font">The font to write with (this can be changed later).</param>
		/// <param name="msg">The message to write (this can be changed later).</param>
		/// <param name="c">The color to write with. If null, the color will default to white.</param>
		public TextGameObject(SpriteBatch? renderer, string font, string msg, Color? c = null) : base(renderer,c)
		{
			MessageDimensions = Vector2.Zero;

			_tx = msg;
			Resource = font;
			Font = null;
			
			TextChanged += (a,b,c) => {};
			FontChanged += (a,b,c) => {};

			return;
		}

		/// <summary>
		/// Creates a game object that writes a message.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="font">The font to write with (this can be changed later).</param>
		/// <param name="msg">The message to write (this can be changed later).</param>
		/// <param name="c">The color to write with. If null, the color will default to white.</param>
		public TextGameObject(SpriteBatch? renderer, SpriteFont font, string msg, Color? c = null) : base(renderer,c)
		{
			MessageDimensions = Vector2.Zero;

			_tx = msg;
			Resource = null;
			Font = font; // The event isn't called when the old font is null

			TextChanged += (a,b,c) => { };
			FontChanged += (a,b,c) => {};

			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will have a fresh ID.</item>
		///	<item>This will have the same parent as <paramref name="other"/>, but it will leave Children unpopulated.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		public TextGameObject(TextGameObject other) : base(other)
		{
			MessageDimensions = other.MessageDimensions;

			_tx = other._tx;
			Resource = other.Resource;
			Font = other.Font;

			TextChanged += (a,b,c) => { };
			FontChanged += (a,b,c) => {};

			return;
		}

		/// <summary>
		/// Loads any content associated with this game object.
		/// This will be loaded into the game's content manager (Game.Content) and must be unloaded through its Unload function.
		/// </summary>
		protected override void LoadContent()
		{
			if(Font is null && Resource is not null)
				Font = Game!.Content.Load<SpriteFont>(Resource); // The event isn't called when the old font is null

			return;
		}

		/// <summary>
		/// Draws this text game object.
		/// </summary>
		/// <param name="delta">The elapsed time since the last draw.</param>
		public override void Draw(GameTime delta)
		{
			if(Renderer is null)
				return;

			// First grab the transformation components we'll need to draw this with.
			World.Decompose(out Vector2 t,out float r,out Vector2 s);

			// Now we can draw!
			// We assume Font is not null here because we would throw an error on the content load if it were
			Renderer.DrawString(Font!,Text,t,Tint,-r,Vector2.Zero,s,Effect,LayerDepth);

			return;
		}

		/// <summary>
		/// The font we write with.
		/// <para/>
		/// This is always non-null after LoadContent but may be null before then.
		/// </summary>
		public SpriteFont? Font
		{
			get => _f;
			
			set
			{
				if(value is null || ReferenceEquals(_f,value))
					return;
				
				SpriteFont? old = _f;
				_f = value;
				MessageDimensions = _f.MeasureString(Text);

				if(old is not null)
					FontChanged(this,_f,old);

				return;
			}
		}

		protected SpriteFont? _f;

		/// <summary>
		/// The message to display.
		/// </summary>
		public string Text
		{
			get => _tx;

			set
			{
				if(_tx == value)
					return;

				string old = _tx;
				_tx = value;
				MessageDimensions = Font is null ? Vector2.Zero : Font.MeasureString(Text);

				TextChanged(this,old,_tx);
				return;
			}
		}

		protected string _tx;

		/// <summary>
		/// Obtains the dimensions of the message with the current font.
		/// If Font has not yet been initialized, this returns (0,0).
		/// </summary>
		public Vector2 MessageDimensions
		{get; protected set;}

		/// <summary>
		/// This is the resource name of this game object's font if we need to load one.
		/// </summary>
		protected string? Resource;

		/// <summary>
		/// Called when this TextGameObject changes its text after the initial assignment.
		/// </summary>
		public event OnTextChange TextChanged;

		/// <summary>
		/// Called when this TextGameObject changes its font after the initial assignment.
		/// </summary>
		public event OnFontChange FontChanged;

		/// <summary>
		/// The width of the button (sans transformations).
		/// </summary>
		public override int Width => Font is null ? 0 : (int)MathF.Ceiling(MessageDimensions.X);

		/// <summary>
		/// The height of the button (sans transformations).
		/// </summary>
		public override int Height => Font is null ? 0 : (int)MathF.Ceiling(MessageDimensions.Y);
	}

	/// <summary>
	/// Called when the text of a TextGameObject is changed after the initial assignment.
	/// </summary>
	/// <param name="sender">The TextGameObject whose text has changed.</param>
	/// <param name="otext">The old text.</param>
	/// <param name="ntext">The new text.</param>
	public delegate void OnTextChange(TextGameObject sender, string otext, string ntext);

	/// <summary>
	/// Called when the font of a TextGameObject is changed after the initial assignment.
	/// </summary>
	/// <param name="sender">The TextGameObject whose font has changed.</param>
	/// <param name="nfont">The new font.</param>
	/// <param name="ofont">The old font.</param>
	public delegate void OnFontChange(TextGameObject sender, SpriteFont nfont, SpriteFont ofont);
}
