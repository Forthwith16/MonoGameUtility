using GameEngine.Assets.Framework;
using GameEngine.GameObjects;
using GameEngine.Resources;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.GameObjects
{
	/// <summary>
	/// Contains the raw asset data of a DummyAffineGameObjectAsset.
	/// </summary>
	public partial class DummyAffineGameObjectAsset
	{
		

		protected override IResource? Instantiate(GraphicsDevice g)
		{
			DummyAffineGameObject ret = new DummyAffineGameObject();
			InstantiateDummyAffineGameObject(ret,g);
			
			return ret;
		}

		/// <summary>
		/// Instantiates <paramref name="instance"/> by assigning the variables this asset is responsible for.
		/// </summary>
		/// <param name="instance">The instance to instantiate.</param>
		/// <param name="g">The graphics device used for instantiation.</param>
		protected void InstantiateDummyAffineGameObject(DummyAffineGameObject instance, GraphicsDevice g)
		{
			InstantiateDrawableAffineObject(instance,g);

			instance.DummyWidth = DummyWidth;
			instance.DummyHeight = DummyHeight;

			return;
		}
	}

	public partial class DummyAffineGameObjectAsset : DrawableAffineObjectAsset
	{
		/// <summary>
		/// Creates an asset version of a dummy affine object with all default values.
		/// </summary>
		public DummyAffineGameObjectAsset() : base()
		{return;}

		/// <summary>
		/// Creates an asset version of <paramref name="obj"/>.
		/// </summary>
		protected internal DummyAffineGameObjectAsset(DummyAffineGameObject obj) : base(obj)
		{
			DummyWidth = obj.DummyWidth;
			DummyHeight = obj.DummyHeight;

			return;
		}

		/// <summary>
		/// The dummy width of this dummy asset.
		/// </summary>
		public int DummyWidth;

		/// <summary>
		/// The dummy height of this dummy asset.
		/// </summary>
		public int DummyHeight;
	}
}
