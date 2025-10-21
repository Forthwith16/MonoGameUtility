using GameEngine.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.Framework
{
	/// <summary>
	/// Contains the raw asset data of a DrawableAffineObject.
	/// </summary>
	public abstract partial class DrawableAffineObjectAsset
	{
		/// <summary>
		/// Instantiates <paramref name="instance"/> by assigning the variables this asset is responsible for.
		/// </summary>
		/// <param name="instance">The instance to instantiate.</param>
		/// <param name="g">The graphics device used for instantiation.</param>
		protected void InstantiateDrawableAffineObject(DrawableAffineObject instance, GraphicsDevice g)
		{
			InstantiateAffineObject(instance,g);

			instance.Visible = Visible;
			instance.DrawOrder = DrawOrder;

			instance.Tint = Tint;
			instance.Effect = Effect;
			instance.LayerDepth = LayerDepth;

			return;
		}
	}

	public abstract partial class DrawableAffineObjectAsset : AffineObjectAsset
	{
		/// <summary>
		/// Creates an asset version of a drawable affine object with all default values.
		/// </summary>
		protected DrawableAffineObjectAsset() : base()
		{
			Visible = true;
			DrawOrder = 0;

			Tint = Color.White;
			Effect = SpriteEffects.None;
			LayerDepth = 0.0f;

			return;
		}

		/// <summary>
		/// Creates an asset version of <paramref name="obj"/>.
		/// </summary>
		protected DrawableAffineObjectAsset(DrawableAffineObject obj) : base(obj)
		{
			Visible = obj.Visible;
			DrawOrder = obj.DrawOrder;

			Tint = obj.Tint;
			Effect = obj.Effect;
			LayerDepth = obj.LayerDepth;

			return;
		}

		/// <summary>
		/// The visibility of this drawable affine object.
		/// </summary>
		public bool Visible
		{get; set;}

		/// <summary>
		/// The draw order of this drawable affine object.
		/// </summary>
		public int DrawOrder
		{get; set;}
		
		/// <summary>
		/// The tint/color to apply to this drawable affine object.
		/// </summary>
		public Color Tint
		{get; set;}

		/// <summary>
		/// The sprite effect of this drawable affine object.
		/// </summary>
		public SpriteEffects Effect
		{get; set;}

		/// <summary>
		/// The drawing layer of this drawable affine object.
		/// This value must be within [0,1].
		/// </summary>
		public float LayerDepth
		{get; set;}
	}
}
