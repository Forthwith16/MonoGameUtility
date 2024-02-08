namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// An AABB tree built on top of an AVL tree.
	/// </summary>
	/// <typeparam name="B">The type of bounding box used for the tree.</typeparam>
	/// <typeparam name="V">The type of values stored in the tree.</typeparam>
	public class AABBTree<V,B> where B : struct, IBoundingBox<B>
	{
		/// <summary>
		/// Creates an empty AABB tree.
		/// </summary>
		/// <param name="extractor">The means by which we extract bounds from <typeparamref name="V"/> types.</param>
		public AABBTree(BoundsExtractor<V,B> extractor)
		{


			Extractor = extractor;
			return;
		}




		/// <summary>
		/// The means by which we extract bounding boxes from <typeparamref name="V"/> types.
		/// </summary>
		protected BoundsExtractor<V,B> Extractor
		{get; set;}
	}

	/// <summary>
	/// The node of an AABB tree backed by an AVL tree.
	/// </summary>
	/// <typeparam name="T">The type of bounding box used for the tree.</typeparam>
	/// <typeparam name="B">The type of values stored in the tree.</typeparam>
	public class AABBTreeNode<V,B> where B : struct, IBoundingBox<B>
	{
		/// <summary>
		/// Creates a new root node with the value <paramref name="v"/> and bounds <paramref name="b"/>.
		/// </summary>
		public AABBTreeNode(V v, B b)
		{
			Value = v;
			Bounds = b;

			Parent = this;
			Left = null;
			Right = null;

			BalanceFactor = 0;
			return;
		}



		/// <summary>
		/// Stitches four nodes together.
		/// </summary>
		/// <param name="me">The center of the world.</param>
		/// <param name="left">The left child.</param>
		/// <param name="right">The right child.</param>
		/// <param name="parent">The parent node. If this is null, <paramref name="me"/> will be made its own parent.</param>
		/// <param name="left_child">If true, then this is a left child of its parent (assuming this is not its own parent). If false, it will be the right child instead.</param>
		public static void StitchTogether(AABBTreeNode<V,B> me, AABBTreeNode<V,B>? left, AABBTreeNode<V,B>? right, AABBTreeNode<V,B>? parent = null, bool left_child = false)
		{
			// Stitch the parent and this together
			if(parent is null)
				me.Parent = me;
			else
			{
				me.Parent = parent;
				
				if(left_child)
					parent.Left = me;
				else
					parent.Right = me;
			}

			// Stitch this and the left child together
			me.Left = left;

			if(left is not null)
				left.Parent = me;

			// Stitch this and the right child together
			me.Right = right;

			if(right is not null)
				right.Parent = me;

			return;
		}

		/// <summary>
		/// Rotates left with the rotation rooted at this node's current position.
		/// </summary>
		/// <remarks>It is assumed that this, Left, and Right are not leaves. If this is not the case, then this method's behavior is undefined.</remarks>
		public void RotateLeft()
		{


			return;
		}

		/// <summary>
		/// Rotates right with the rotation rooted at this node's current position.
		/// </summary>
		/// <remarks>It is assumed that this, Left, and Right are not leaves. If this is not the case, then this method's behavior is undefined.</remarks>
		public void RotateRight()
		{
			// Give names to these things for convenience
			AABBTreeNode<V,B> parent = Parent;
			AABBTreeNode<V,B> me = this;

			// The children cannot be null if we are attempting to make a valid rotation
			AABBTreeNode<V,B> l_child = Left!;
			AABBTreeNode<V,B> r_child = Right!;

			// The left niblings cannot be null if we are attempting to make a valid right rotation
			AABBTreeNode<V,B> l_nibling = l_child.Left!;
			AABBTreeNode<V,B> r_nilbing = l_child.Right!;

			// Now assign everything
			// We don't behave like a binary search tree, so we can perform our rotation a little differently to keep our bounding boxes minimal
			

			return;
		}

		/// <summary>
		/// The value this node holds if HasValue is true.
		/// Otherwise, this value is null and has no meaning.
		/// </summary>
		public V? Value
		{get;}

		/// <summary>
		/// If true, then this node has a value and represents the bounding box of Value.
		/// If false, then this is a dummy node used for efficient querying.
		/// </summary>
		public bool HasValue => IsLeaf;

		/// <summary>
		/// The bounds of this node.
		/// </summary>
		public B Bounds
		{get; protected set;}

		/// <summary>
		/// The parent of this node.
		/// If this node is the root, then it is its own parent.
		/// </summary>
		public AABBTreeNode<V,B> Parent
		{get; protected set;}

		/// <summary>
		/// The left child of this node.
		/// </summary>
		/// <remarks>This and Right are always null together or not null together.</remarks>
		public AABBTreeNode<V,B>? Left
		{get; protected set;}

		/// <summary>
		/// The right child of this node.
		/// </summary>
		/// <remarks>This and Left are always null together or not null together.</remarks>
		public AABBTreeNode<V,B>? Right
		{get; protected set;}

		/// <summary>
		/// The balance factor of this node.
		/// This is always equal to Right's height - Left's height.
		/// </summary>
		public sbyte BalanceFactor
		{get; protected set;}

		/// <summary>
		/// If true, then this is a root node.
		/// If false, this is not the root.
		/// </summary>
		public bool IsRoot => Parent == this;

		/// <summary>
		/// If true, then this is an internal node.
		/// Otherwise, this is a leaf node.
		/// </summary>
		public bool IsInternal => !IsLeaf;

		/// <summary>
		/// If true, then this is a leaf node.
		/// Otherwise, this is an internal node.
		/// </summary>
		public bool IsLeaf => Left is not null; // The way we construct our AABB tree, every node has either 0 or 2 children, never 1
	}

	/// <summary>
	/// Extracts a bounding box from <paramref name="value"/>.
	/// </summary>
	/// <typeparam name="V">The type which has a bounding box.</typeparam>
	/// <typeparam name="B">The type of the bounding box.</typeparam>
	/// <param name="value">The value to extract a bounding box from.</param>
	/// <returns>Returns the bounding box of <paramref name="value"/>.</returns>
	public delegate B BoundsExtractor<V,B>(V value) where B : struct, IBoundingBox<B>;
}
