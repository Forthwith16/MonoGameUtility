using GameEngine.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A game object that draws an image selected from a sprite sheet.
	/// </summary>
	public class MultiImageGameObject : ImageGameObject
	{
		/// <summary>
		/// Creates a multi image game object by loading a sprite sheet via <paramref name="game"/>'s content manager with the name <paramref name="resource"/>.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="resource">The name of the sprite sheet resource to load.</param>
		/// <param name="selection">The initially selected image.</param>
		public MultiImageGameObject(SpriteBatch? renderer, string resource, int selection = 0) : base(renderer,resource)
		{
			Sprites = null;
			_si = selection;
			
			SelectionChanged += (a,b,c) => {}; // This way we don't have to think about null checks
			return;
		}

		/// <summary>
		/// Creates a multi image game object from the provided sprite sheet.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (this can be changed later).</param>
		/// <param name="textures">The texture sources of this image.</param>
		/// <param name="selection">The initially selected image.</param>
		public MultiImageGameObject(SpriteBatch? renderer, SpriteSheet textures, int selection = 0) : base(renderer,textures.Source)
		{
			Sprites = textures;
			_si = selection;

			SelectionChanged += (a,b,c) => {}; // This way we don't have to think about null checks
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
		public MultiImageGameObject(MultiImageGameObject other) : base(other)
		{
			Sprites = other.Sprites;
			_si = other._si;

			SelectionChanged += (a,b,c) => {};
			return;
		}

		protected override void LoadContent()
		{
			if(Source is null && Resource is not null)
			{
				Sprites = Game!.Content.Load<SpriteSheet>(Resource);
				Source = Sprites.Source;

				if(SelectedIndex < 0)
					SelectedIndex = 0;
				else
					SourceRect = Sprites[_si];
			}
			
			return;
		}

		/// <summary>
		/// The source texture information for this multi image game object.
		/// </summary>
		protected SpriteSheet? Sprites
		{get; set;}

		/// <summary>
		/// The selected image to draw of this multi image game object.
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
					throw new ArgumentOutOfRangeException("MultiImageGameObject's SelectedIndex must be nonnegative and less than Count");

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
		/// The number of images in this multi image game object.
		/// </summary>
		public int Count => Sprites is null ? 0 : Sprites.Count;

		/// <summary>
		/// An event called after the selected index changes.
		/// </summary>
		public event OnSelectionChange SelectionChanged;
	}

	/// <summary>
	/// Called after a MultiImageGameObject's selected index changes.
	/// </summary>
	/// <param name="sender">The MultiImageGameObject sending the event.</param>
	/// <param name="old_index">The old selected index.</param>
	/// <param name="new_index">The new selected index.</param>
	public delegate void OnSelectionChange(MultiImageGameObject sender, int old_index, int new_index);
}
