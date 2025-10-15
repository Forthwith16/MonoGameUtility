using GameEngine.Assets.Serialization;
using GameEngine.DataStructures.Collections;
using GameEngine.Exceptions;
using GameEngine.Framework;
using GameEngine.Maths;

namespace GameEngine.Assets.Framework
{
	/// <summary>
	/// Contains the raw asset data of an AffineObject.
	/// </summary>
	/// <remarks>
	/// This class hierarchy has no default JSON serializer provided.
	/// Serializing/deserializing such an object requires using the <see cref="Serialize(string)"/> and <see cref="Deserialize(string)"/> methods (or some custom user defined converter).
	/// </summary>
	public abstract partial class AffineObjectAsset
	{


		/// <param name="args">
		/// This should contain, as the first argument at index 0, a Dictionary&lt;InternalAssetID,AssetBase&gt; that maps IDs to assets loaded as part of a hierarchy of game objects.
		/// For example, this object itself should appear in the dictionary with <c>InternalID</c> as its key.
		/// Its children, too, must appear in the dictionary, each with a key equal to their <c>InternalID</c>.
		/// </param>
		/// <inheritdoc/>
		protected override void LinkAssets(params object?[] args)
		{
			if(args[0] is not Dictionary<InternalAssetID,AssetBase> map)
				throw new LinkException("AffineObjectAssets require a dictionary of loaded assets during linking.");

			foreach(InternalAssetID id in UnloadedChildren)
				if(map.TryGetValue(id,out AssetBase? asset) && asset is AffineObjectAsset child)
					AddChild(child);
				else
					throw new LinkException("Attempted to add or failed to find a non-AffineObjectAsset child.");

			return;
		}
	}

	public abstract partial class AffineObjectAsset : GameObjectAsset
	{
		/// <summary>
		/// Creates an affine object asset with all default values.
		/// </summary>
		protected AffineObjectAsset() : base()
		{
			Transform = Matrix2D.Identity;
			Parent = null;

			Children = new IndexedQueue<AffineObjectAsset>();
			UnloadedChildren = new IndexedQueue<InternalAssetID>();

			return;
		}


		protected AffineObjectAsset(AffineObject obj) : base(obj)
		{
			Transform = obj.Transform;
			
			Children = new IndexedQueue<AffineObjectAsset>();
			UnloadedChildren = new IndexedQueue<InternalAssetID>();

			// It is okay to create children in this manner, because the parent hierarchy is always a tree
			foreach(AffineObject child in obj.Children)
				;//Asset.CreateAsset(child,);

			return;
		}

		/// <summary>
		/// Adds a child affine object asset to this.
		/// </summary>
		/// <param name="child">The child asset to add.</param>
		public virtual void AddChild(AffineObjectAsset child)
		{
			Children.Add(child);
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
		/// This value is provided as a courtesy and is never serialized (and is only indirectly deserialized).
		/// </summary>
		/// <remarks>
		/// If assigning the parent of a deriving class requires additional logic to occur, override this property to hook into the event.
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
		public IndexedQueue<AffineObjectAsset> Children
		{get;}

		/// <summary>
		/// This is the set of unloaded children of this affine object asset using the root game object's internal asset ID system.
		/// This must be transformed into proper AffineObjectAsset children in <see cref="Children"/> during <c>Link</c> by overriding the <c>LinkAssets</c> method.
		/// </summary>
		public IndexedQueue<InternalAssetID> UnloadedChildren
		{get;}
	}
}
