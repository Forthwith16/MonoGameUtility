using GameEngine.Assets.Serialization;
using GameEngine.DataStructures.Collections;
using GameEngine.Framework;
using GameEngine.Maths;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.Framework
{
	/// <summary>
	/// Contains the raw asset data of an AffineObject.
	/// </summary>
	public abstract partial class AffineObjectAsset
	{
		/// <summary>
		/// Instantiates <paramref name="instance"/> by assigning the variables this asset is responsible for.
		/// </summary>
		/// <param name="instance">The instance to instantiate.</param>
		/// <param name="g">The graphics device used for instantiation.</param>
		protected void InstantiateAffineObject(AffineObject instance, GraphicsDevice g)
		{
			InstantiateGameObject(instance);

			instance.Transform = Transform;

			foreach(AffineObjectAsset? child in Children.Select(src => src.LocalAsset))
				if(child is not null && Asset.Instantiate<AffineObjectAsset,AffineObject>(child,g,out AffineObject? output)) // child should never be null, but just in case
					output.Parent = instance;

			return;
		}

		/// <param name="args">
		/// This should contain, as the first argument at index 0, a Dictionary&lt;InternalAssetID,AssetBase&gt; that maps IDs to assets loaded as part of a hierarchy of game objects.
		/// For example, this object itself should appear in the dictionary with <c>InternalID</c> as its key.
		/// Its children, too, must appear in the dictionary, each with a key equal to their <c>InternalID</c>.
		/// </param>
		/// <inheritdoc/>
		protected override void LinkAssets(params object?[] args)
		{
			/*if(args[0] is not Dictionary<InternalAssetID,AssetBase> map)
				throw new LinkException("AffineObjectAssets require a dictionary of loaded assets during linking.");

			foreach(InternalAssetID id in UnloadedChildren)
				if(map.TryGetValue(id,out AssetBase? asset) && asset is AffineObjectAsset child)
					AddChild(child);
				else
					throw new LinkException("Attempted to add or failed to find a non-AffineObjectAsset child.");

			return;*/
		}
	}

	public abstract partial class AffineObjectAsset : GameObjectAsset
	{
		/// <summary>
		/// Creates an asset version of an affine object with all default values.
		/// </summary>
		protected AffineObjectAsset() : base()
		{
			Transform = Matrix2D.Identity;
			Parent = null;

			Children = new IndexedQueue<AssetSource<AffineObjectAsset,AffineObject>>();
			return;
		}

		/// <summary>
		/// Creates an asset version of <paramref name="obj"/>.
		/// </summary>
		protected AffineObjectAsset(AffineObject obj) : base(obj)
		{
			Transform = obj.Transform;
			Parent = null; // Parentage is ONLY assign via AddChild, so if this has a parent, then it will be added as a child somewhere

			Children = new IndexedQueue<AssetSource<AffineObjectAsset,AffineObject>>();

			// The parent hierarchy is always a tree, so we can always blindly create the children
			foreach(AffineObject child in obj.Children)
				AddChild(child);
			
			return;
		}

		/// <summary>
		/// Adds a child to this.
		/// </summary>
		public virtual void AddChild(AffineObject child)
		{
			AssetSource<AffineObjectAsset,AffineObject> src = new AssetSource<AffineObjectAsset,AffineObject>(child);
			
			if(src.CreateAsset())
				src.LocalAsset!.Parent = this;

			Children.Add(src);
			return;
		}

		/// <summary>
		/// Adds a child to this.
		/// </summary>
		public virtual void AddChild(AffineObjectAsset child)
		{
			Children.Add(new AssetSource<AffineObjectAsset,AffineObject>(child));
			child.Parent = this;

			return;
		}

		/// <summary>
		/// The transform of this affine object.
		/// <para/>
		/// This defaults to the identity matrix and is not required to be present in any serialization.
		/// </summary>
		public Matrix2D Transform;

		/// <summary>
		/// This is the parent affine asset of this affine object (if any).
		/// </summary>
		/// <remarks>
		/// If assigning the parent of a deriving class requires additional logic to occur, override this property to hook into the event.
		/// <para/>
		/// This value should never be set outside of <see cref="AddChild(AffineObjectAsset)"/> or maybe serialization.
		/// </remarks>
		public virtual AffineObjectAsset? Parent
		{get; set;}

		/// <summary>
		/// These are the children of this asset (if any) that are loaded in.
		/// Note that this asset may have unloaded children in <see cref="UnloadedChildren"/>.
		/// </summary>
		/// <remarks>
		/// To add to this, it is best to utilize <see cref="AddChild(AffineObjectAsset)"/>.
		/// This will perform courtesy data assignments, such as assigning the child's <see cref="Parent"/> value..
		/// </remarks>
		public IndexedQueue<AssetSource<AffineObjectAsset,AffineObject>> Children
		{get;}
	}
}
