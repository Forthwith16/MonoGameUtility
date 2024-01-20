using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A component that draws an image selected from a sprite sheet.
	/// </summary>
	public class MultiImageComponent : ImageComponent
	{
		/// <summary>
		/// Creates a multi image component by loading a sprite sheet via <paramref name="game"/>'s content manager with the name <paramref name="resource"/>.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="resource">The name of the sprite sheet resource to load.</param>
		/// <param name="selection">The initially selected image.</param>
		public MultiImageComponent(Game game, SpriteBatch? renderer, string resource, int selection = 0) : base(game,renderer,resource)
		{
			Sprites = null;
			_si = selection;
			
			SelectionChanged += (a,b,c) => {}; // This way we don't have to think about null checks
			return;
		}

		/// <summary>
		/// Creates an image component from the provided texture.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="textures">The texture sources of this image.</param>
		/// <param name="selection">The initially selected image.</param>
		public MultiImageComponent(Game game, SpriteBatch? renderer, SpriteSheet textures, int selection = 0) : base(game,renderer,textures.Source)
		{
			Sprites = textures;
			_si = selection;

			SelectionChanged += (a,b,c) => {}; // This way we don't have to think about null checks
			return;
		}

		protected override void LoadContent()
		{
			if(Source is null && Resource is not null)
			{
				Sprites = Game.Content.Load<SpriteSheet>(Resource);
				Source = Sprites.Source;

				if(SelectedIndex < 0)
					SelectedIndex = 0;
				else
					SourceRect = Sprites[_si];
			}
			
			return;
		}

		/// <summary>
		/// The source texture information for this multi image component.
		/// </summary>
		protected SpriteSheet? Sprites
		{get; set;}

		/// <summary>
		/// The selected image to draw of this multi image component.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the value is set to less than 0 or at least Count.</exception>
		public int SelectedIndex
		{
			get => _si;

			set
			{
				if(_si == value)
					return;

				if(value < 0 || value >= Count)
					throw new ArgumentOutOfRangeException("MultiImageComponent's SelectedIndex must be nonnegative and less than Count");

				int old = _si;
				_si = value;

				if(Sprites is not null)
					SourceRect = Sprites[_si];

				SelectionChanged(this,old,_si);
				return;
			}
		}

		protected int _si;

		/// <summary>
		/// The number of images in this multi image component.
		/// </summary>
		public int Count => Sprites is null ? 0 : Sprites.Count;

		/// <summary>
		/// An event called after the selected index changes.
		/// </summary>
		public event OnSelectionChange SelectionChanged;
	}

	/// <summary>
	/// Called after a MultiImageComponent's selected index changes.
	/// </summary>
	/// <param name="sender">The MultiImageComponent sending the event.</param>
	/// <param name="old_index">The old selected index.</param>
	/// <param name="new_index">The new selected index.</param>
	public delegate void OnSelectionChange(MultiImageComponent sender, int old_index, int new_index);
}
