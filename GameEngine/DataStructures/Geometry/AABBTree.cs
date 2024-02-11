using System.Collections;

namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// An AABB tree built on top of an AVL tree.
	/// </summary>
	/// <typeparam name="B">The type of bounding box used for the tree.</typeparam>
	/// <typeparam name="V">The type of values stored in the tree.</typeparam>
	public class AABBTree<V,B> : IEnumerable<V> where B : struct, IBoundingBox<B>
	{
		/// <summary>
		/// Creates an empty AABB tree.
		/// </summary>
		/// <param name="extractor">The means by which we extract bounds from <typeparamref name="V"/> types.</param>
		/// <param name="updater">The means by which we update bounds in <typeparamref name="V"/> types.</param>
		public AABBTree(BoundaryExtractor<V,B> extractor, BoundaryUpdator<V,B> updater)
		{
			Root = null;
			Count = 0;

			Extractor = extractor;
			Updater = updater;

			return;
		}

		/// <summary>
		/// Determines which items in this tree intersect with <paramref name="query"/>.
		/// </summary>
		/// <param name="query">The item to investigate.</param>
		/// <returns>Returns an enumeration of the items which intersect with <paramref name="query"/>.</returns>
		public IEnumerable<V> Query(V query)
		{return Query(Extractor(query));}

		/// <summary>
		/// Determines which items in this tree intersect with <paramref name="query"/>.
		/// </summary>
		/// <param name="query">The query region to investigate.</param>
		/// <returns>Returns an enumeration of the items which intersect with <paramref name="query"/>.</returns>
		public IEnumerable<V> Query(B query)
		{return IsEmpty ? Enumerable.Empty<V>() : Root!.Query(query);}

		/// <summary>
		/// Adds <paramref name="v"/> to this tree.
		/// </summary>
		/// <param name="v">The item to add to this tree.</param>
		/// <remarks>This tree does not prevent double adding items. It is up to the adding class to so restrain itself or accept responsibility therein.</remarks>
		public void Add(V v)
		{
			// Get the bounds once (this may be an expensive operation in general, though it shouldn't be)
			B bound = Extractor(v);

			// If the tree is empty, it's easy to add
			if(IsEmpty)
			{
				Root = new AABBTreeNode<V,B>(v,bound);
				Count++;

				return;
			}

			// The tree is not empty, so we need to find the best leaf node to pair v with
			// Then once we know what leaf we're pairing with, expand the leaf
			new AABBTreeNode<V,B>(v,bound,PickBestLeaf(Root!,bound)); // This will perform the add by the mere act of its creation
			Count++;

			// Lastly, we may have rotated the root away from being the root, so restore Root to the actual root
			RestoreRoot();

			return;
		}

		/// <summary>
		/// Finds the best leaf below <paramref name="root"/> to place a new item with bounds given by <paramref name="new_bounds"/>.
		/// </summary>
		/// <param name="root">The root of the tree whose leaves we wish to consider.</param>
		/// <param name="new_bounds">The new boundary.</param>
		/// <returns>Returns the leaf node that would pair best with <paramref name="new_bounds"/>. If <paramref name="root"/> is a leaf, <paramref name="root"/> is returned.</returns>
		protected AABBTreeNode<V,B> PickBestLeaf(AABBTreeNode<V,B> root, B new_bounds)
		{
			// If we ARE a leaf, there's nothing to do
			if(root.IsLeaf)
				return root;

			// Otherwise, we're looking for the child that expands the bounding boxes with new_bounds the least (which we then recurse upon)
			// However, if both options completely contain new_bounds, then we need to get both of their best answers and then pick between the final results of each branch
			// This last case should hopefully be infrequent
			// Also, note that we always have 0 or 2 children, so we don't need to worry about Left/Right checking
			if(root.Left!.Bounds.Contains(new_bounds))
				if(root.Right!.Bounds.Contains(new_bounds))
				{
					// Both contain new_bounds, so we need to pick the better of two sub options
					AABBTreeNode<V,B> bl = PickBestLeaf(root.Left,new_bounds);
					AABBTreeNode<V,B> br = PickBestLeaf(root.Right,new_bounds);

					// Pick the one that expands the sub bounding box the least
					// Note that union is associative and commutative, so the union of all three is the same no matter where we store new_bounds
					B lu = new_bounds.Union(bl.Bounds);
					B ru = new_bounds.Union(br.Bounds);

					// We care more about absolute space than percentage space increase because the odds of a query hitting something depends on its absolute space
					if(lu.BoxSpace <= ru.BoxSpace) // If they happen to be equal, just go left ig
						return PickBestLeaf(root.Left,new_bounds);
					
					return PickBestLeaf(root.Right,new_bounds);
				}
				else
					return PickBestLeaf(root.Left,new_bounds);
			else // Left does not contain new_bounds
				if(root.Right!.Bounds.Contains(new_bounds))
					return PickBestLeaf(root.Right,new_bounds);
				else // Neither contains new_bounds
				{
					// Since neither contains new_bounds, just pick the one that expands the sub bounding box the least
					// Note that union is associative and commutative, so the union of all three is the same no matter where we store new_bounds
					B lu = new_bounds.Union(root.Left.Bounds);
					B ru = new_bounds.Union(root.Right.Bounds);
					
					// We care more about absolute space than percentage space increase because the odds of a query hitting something depends on its absolute space
					if(lu.BoxSpace <= ru.BoxSpace) // If they happen to be equal, just go left ig
						return PickBestLeaf(root.Left,new_bounds);
					
					return PickBestLeaf(root.Right,new_bounds);
				}
			
			// We shouldn't be able to get here, but just in case, just go left ig and hope for the best
			return PickBestLeaf(root.Left,new_bounds);
		}

		/// <summary>
		/// Restores Root to be the actual root of the tree.
		/// This detached root phenomenon can occur when the root is rotated away from the root or the root is replaced after a remove.
		/// In theory, this function should never require more than a single execution of Root = Root.Parent.
		/// </summary>
		protected void RestoreRoot()
		{
			if(IsEmpty)
				return;

			// We do not use a do while loop here to avoid potentially checking the condition twice because we may not need to execute it at all
			while(!Root!.IsRoot)
				Root = Root.Parent;

			return;
		}

		/// <summary>
		/// Updates the boundary of <paramref name="v"/> in this tree.
		/// If <paramref name="v"/> does not belong to this tree, then this does nothing.
		/// </summary>
		/// <param name="v">The value whose boundary needs updating.</param>
		/// <param name="new_boundary">The new boundary for <paramref name="v"/>.</param>
		/// <returns>Returns true if <paramref name="v"/>'s boundary was changed and false otherwise.</returns>
		/// <remarks><paramref name="v"/>'s membership is determined via the Remove method, which uses Query to locate <paramref name="v"/>.</remarks>
		public bool UpdateBoundary(V v, B new_boundary)
		{
			// If we fail to remove v, then we don't have v and can't update it
			if(!Remove(v))
				return false;

			// Now we need to update v
			bool ret = Updater(v,new_boundary);

			// And lastly, we add v back in (regardless of if its boundary was successfully changed)
			Add(v);

			return ret;
		}

		/// <summary>
		/// Removes the first <paramref name="v"/> found from this tree (if any exist).
		/// </summary>
		/// <param name="v">The item to remove.</param>
		/// <returns>Returns true if <paramref name="v"/> was removed and false otherwise.</returns>
		public bool Remove(V v)
		{
			// If we have nothing, we're done
			if(IsEmpty)
				return false;

			AABBTreeNode<V,B>? remove_me = null;

			// Find v
			foreach(AABBTreeNode<V,B> n in Root!.Find(Extractor(v)))
				if(v is null ? n.Value is null : v.Equals(n.Value))
				{
					remove_me = n;
					break; // If there's duplicates, we only remove the first found
				}

			// If we didn't find v, we're done
			if(remove_me is null)
				return false;

			// Remove our node
			Root = remove_me.Remove(Root);
			Count--;
			
			return true;
		}

		/// <summary>
		/// Determines if this tree contains <paramref name="v"/> by performing an ordinary Query.
		/// </summary>
		/// <param name="v">The value to look for.</param>
		/// <returns>Returns true if this tree contains <paramref name="v"/> and false otherwise.</returns>
		public bool Contains(V v)
		{
			if(IsEmpty)
				return false;

			if(v is null)
				foreach(V q in Query(v))
				{
					if(q is null)
						return true;
				}
			else
				foreach(V q in Query(v))
					if(v.Equals(q))
						return true;

			return false;
		}

		/// <summary>
		/// Determines if this tree contains <paramref name="v"/> by search every single one of its leaves for <paramref name="v"/>.
		/// Use this is the boundary for <paramref name="v"/> has changed without notice to this tree (note that this is undesired user behavior).
		/// </summary>
		/// <param name="v">The value to search for.</param>
		/// <returns>Returns true if this tree contains <paramref name="v"/> and false otherwise.</returns>
		public bool ContainsQueryless(V v) => IsNotEmpty && ContainsQueryless(v,Root!);

		/// <summary>
		/// A helper function for ContainsQueryless that performs the containment query without using Query.
		/// </summary>
		/// <param name="v">The value to look for.</param>
		/// <param name="root">The tree to look for <paramref name="v"/> in.</param>
		/// <returns>Returns true if <paramref name="v"/> was found and false otherwise.</returns>
		protected bool ContainsQueryless(V v, AABBTreeNode<V,B> root)
		{
			// If we have a value, we are a leaf and we are done
			if(root.HasValue)
				if(root.Value is null)
					return v is null;
				else
					return root.Value.Equals(v);

			// Otherwise, we must have 2 children, so query both of them
			return ContainsQueryless(v,root.Left!) || ContainsQueryless(v,root.Right!);
		}

		/// <summary>
		/// Empties this tree.
		/// </summary>
		public void Clear()
		{
			Root = null;
			Count = 0;

			return;
		}

		public IEnumerator<V> GetEnumerator() => Query(TreeBoundary).GetEnumerator(); // Inefficient, yes, but you shouldn't be enumerating an AABB tree anyway except maybe when debugging
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// The number of items in this tree.
		/// </summary>
		public int Count
		{get; protected set;}

		/// <summary>
		/// If true, then this tree is empty.
		/// If false, it is not.
		/// </summary>
		public bool IsEmpty => Root is null;

		/// <summary>
		/// If true, then this tree is not empty.
		/// If false, it is.
		/// </summary>
		public bool IsNotEmpty => Root is not null;

		/// <summary>
		/// Obtains the boundary of the entire tree.
		/// </summary>
		public B TreeBoundary => IsEmpty ? default(B) : Root!.Bounds;

		/// <summary>
		/// The root of the tree.
		/// </summary>
		protected AABBTreeNode<V,B>? Root
		{get; set;}

		/// <summary>
		/// The means by which we extract bounding boxes from <typeparamref name="V"/> types.
		/// </summary>
		protected BoundaryExtractor<V,B> Extractor
		{get; set;}

		/// <summary>
		/// The means by which we update a boundary in a <typeparamref name="V"/> type.
		/// </summary>
		protected BoundaryUpdator<V,B> Updater
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

			Parent = null;
			Left = null;
			Right = null;

			BalanceFactor = 0;
			return;
		}

		/// <summary>
		/// Creates a new AABB tree node.
		/// It creates another new node to replace <paramref name="parent"/> as a parent while <paramref name="parent"/> becomes this node's new sibling.
		/// </summary>
		/// <param name="v">The value to add.</param>
		/// <param name="b">The bounds.</param>
		/// <param name="parent">The parent to make our new sibling.</param>
		public AABBTreeNode(V v, B b, AABBTreeNode<V,B> parent)
		{
			Value = v;
			Bounds = b;

			Left = null;
			Right = null;

			BalanceFactor = 0;

			if(parent.IsLeftChild)
				Parent = new AABBTreeNode<V,B>(parent.Parent,parent,this,true); // The assignment is unncessary, but nullability analysis doesn't realize that
			else // Right child or root
				Parent = new AABBTreeNode<V,B>(parent.Parent,this,parent,false);

			return;
		}

		/// <summary>
		/// Creates an internal (or root) node.
		/// </summary>
		/// <param name="parent">The parent node. If this is null, this will be a root node.</param>
		/// <param name="left">The left node.</param>
		/// <param name="right">The right node.</param>
		/// <param name="left_child">If true, then this will be the left child of <paramref name="parent"/> (if this is not the root). Otherwise, it will be a right child.</param>
		protected AABBTreeNode(AABBTreeNode<V,B>? parent, AABBTreeNode<V,B> left, AABBTreeNode<V,B> right, bool left_child)
		{
			Parent = parent;

			if(!IsRoot)
			{
				if(left_child)
					Parent!.Left = this;
				else
					Parent!.Right = this;
			}

			Left = left;
			Left.Parent = this;

			Right = right;
			Right.Parent = this;

			BalanceFactor = 0;
			AABBTreeNode<V,B>? n = PropagateAddBalanceChange(this);

			// Check if we need to rotate; if so, pick the correct rotation
			if(n is not null)
				if(n.BalanceFactor < 0)
					n = n.RotateRight();
				else
					n = n.RotateLeft();

			// If we rotated, then we updated our bounds all the way up to n, so we can jump up one
			if(n is not null)
				n = n.Parent;

			// Now that we've rotated, we need to propogate the bounds changes upward (if we're not already the root; the rotation fixed n's bounds)
			while(n is not null)
			{
				// No AABBTree should ever be more than, say, 20 tall, so let's just blindly assign bounds and go upward
				// It should be, if not faster than performing an equality check between the old and new bounds, then not much slower
				n.UpdateBounds();
				n = n.Parent;
			}

			return;
		}
		
		/// <summary>
		/// Determines which leaf descendants of this node intersect with <paramref name="query"/>.
		/// </summary>
		/// <param name="query">The query region to investigate.</param>
		/// <returns>Returns an enumeration of the objects which intersect with <paramref name="query"/>.</returns>
		public IEnumerable<V> Query(B query)
		{
			// If our bounds don't intersect with the query, then we have nothing to do
			if(!query.Intersects(Bounds))
				return Enumerable.Empty<V>();

			// If we are a leaf, we just return our value
			if(IsLeaf)
				return new V[] {Value!};
			
			// Otherwise, we are intersecting, so perform two recursive queries
			return Left!.Query(query).Concat(Right!.Query(query)); // We always have 0 or 2 children, so this is fine
		}

		/// <summary>
		/// Determines which leaf descendants of this node intersect with <paramref name="query"/>.
		/// </summary>
		/// <param name="query">The query region to investigate.</param>
		/// <returns>Returns an enumeration of the objects which intersect with <paramref name="query"/>.</returns>
		public IEnumerable<AABBTreeNode<V,B>> Find(B query)
		{
			// If our bounds don't intersect with the query, then we have nothing to do
			if(!query.Intersects(Bounds))
				return Enumerable.Empty<AABBTreeNode<V,B>>();

			// If we are a leaf, we just return our value
			if(IsLeaf)
				return new AABBTreeNode<V,B>[] {this};
			
			// Otherwise, we are intersecting, so perform two recursive queries
			return Left!.Find(query).Concat(Right!.Find(query)); // We always have 0 or 2 children, so this is fine
		}

		/// <summary>
		/// Propagates a balance change upward.
		/// </summary>
		/// <param name="n">The tree node to start the balance change at (its BalanceFactor will not change).</param>
		/// <returns>Returns the node (if any) that requires rotation (AVL trees on insert only ever require one rotation). If no rotations are required, then this returns null.</returns>
		protected AABBTreeNode<V,B>? PropagateAddBalanceChange(AABBTreeNode<V,B> n)
		{
			// Update the initial bounds
			n.UpdateBounds();

			while(!n.IsRoot)
			{
				// Update the bounds first to get it out of the way
				n.Parent!.UpdateBounds();

				if(n.IsLeftChild) // n is a left child, so we decreased the balance factor by 1
					if(n.Parent.BalanceFactor <= 0)
					{
						if(--n.Parent.BalanceFactor < -1)
							return n.Parent; // After a rotation, the tree will be back to the way it was before, so we don't need to update anything more
					}
					else
					{
						// +1 and -1 annihilate each other and we end propogation
						n.Parent.BalanceFactor = 0;
						return null;
					}
				else // n is a right child, so we increased the balance factor by 1
					if(n.Parent.BalanceFactor >= 0)
					{
						if(++n.Parent.BalanceFactor > 1)
							return n.Parent;
					}
					else
					{
						// -1 and +1 annihilate each other and we end propogation
						n.Parent.BalanceFactor = 0;
						return null;
					}

				n = n.Parent;
			}

			return null;
		}

		/// <summary>
		/// Updates the bounds of this node.
		/// </summary>
		public void UpdateBounds()
		{
			if(IsLeaf)
				return;

			Bounds = Left!.Bounds.Union(Right!.Bounds); // We always have 0 or 2 children, so if we're not a leaf, both Left and Right must exist
			return;
		}

		/// <summary>
		/// Removes this leaf node from its tree.
		/// If this is not a leaf, this does nothing and returns <paramref name="root"/>.
		/// </summary>
		/// <param name="root">The old root of the tree. This is a convenience variable to return in case the Remove method finishes early.</param>
		/// <returns>Returns the new root of the tree. If there is nothing left in the tree, returns null.</returns>
		public AABBTreeNode<V,B>? Remove(AABBTreeNode<V,B> root)
		{
			// If we're a leaf, get out of here
			if(!IsLeaf)
				return root;

			// If we're the root, we're done
			if(IsRoot)
				return null;

			// We have at least three nodes in the tree, so our sibling and parent must exist
			// Grab our sibling
			AABBTreeNode<V,B> sibling = Sibling!;

			// Now disconnect this (and its parent) and move our sibling up
			// Since it's its own tree doing its own thing, it's balance factor is already correct for its subtree
			sibling.Parent = Parent!.Parent;

			if(sibling.IsRoot)
				return sibling; // sibling is the root and there's nothing left to do
			else // silbing is not the root and we need to do some further propagation
				if(Parent.IsLeftChild)
					sibling.Parent!.Left = sibling;
				else // Parent isn't the root, so it must be a right child
					sibling.Parent!.Right = sibling;

			// Now we need to propagate this balance change up the tree and repair the boundaries upward
			AABBTreeNode<V,B> processed = sibling;
			
			// Once we make a balance factor +1 or -1, then we change it away from 0
			// In this case, the height of the subtree remains unchanged
			// This is because it was 0 and balanced, but now one subsubtree is taller and maintaining the height, which means upper balance factors remain unchanged
			bool not_satisfied = true;

			// We'll loop until we've repaired our boundaries all the way up to the root (and performed any required rotations along the way)
			// Since AABB trees are typically short and our runtime is log n for the initial query anyway, there's no harm in just going all the way back up and blindly assigning boundaries
			do
			{
				// Going into this, we know that our parent is not null
				AABBTreeNode<V,B> cur = processed.Parent;

				// Repair our boundary first
				cur.RepairBoundary();

				// If we've hit a +1 or -1, we no longer need to think about balance factors or rotations (see declaration above)
				if(not_satisfied)
				{
					// Our balance factor changes depending on if we came from the left or right
					// We do know that processed is for sure not the root though, so we can skip that check
					if(processed.IsLeftChild)
					{
						if(++cur.BalanceFactor == 1) // ++ because the left side got smaller by 1 (i.e. -(-1))
							not_satisfied = false;
					}
					else // We were a right child
						if(--cur.BalanceFactor == -1) // -- because the right side got smaller by 1 (i.e. -(+1))
							not_satisfied = false;

					// If our balance factor is out of control, we'll just have to fix it
					// After a rotation, we need to inspect and modify our new cur's balance factor
					// It increases by 1 with a right rotation and decreases by 1 with a left rotation
					// A rotation will end our balance factor concerns if it changes the balance factor to +1 or -1
					// However, if it changes to 0, then we'll have lost 1 height from the subtree and need to keep going up
					if(cur.BalanceFactor == 2)
					{
						cur = cur.RotateLeft(false);
						cur.BalanceFactor--;

						if(cur.BalanceFactor == -1)
							not_satisfied = false;
					}
					else if(cur.BalanceFactor == -2)
					{
						cur = cur.RotateRight(false);
						cur.BalanceFactor++;

						if(cur.BalanceFactor == 1)
							not_satisfied = false;
					}
				}

				// Climb!
				processed = cur;
			}
			while(processed.Parent is not null);

			// We are now done and can just return our new root node
			return processed;
		}

		/// <summary>
		/// Repairs this node's boundary by unioning its children (this does not occur recursively in either direction).
		/// If this node has no children, this does nothing.
		/// </summary>
		public void RepairBoundary()
		{
			if(IsLeaf)
				return;

			Bounds = Left!.Bounds.Union(Right!.Bounds);
			return;
		}

		/// <summary>
		/// Rotates left with the rotation rooted at this node's current position.
		/// </summary>
		/// <param name="add">If true, this is an add rotation. If false, this is a remove rotation. A remove rotation will leave the balance factor of the promoted sibling node along for inspection and modification.</param>
		/// <returns>Returns the new node that is now where this node was in the tree. This may be the new root of the tree.</returns>
		/// <remarks>
		/// It is assumed that this, Left, and Right are not leaves.
		/// Similarly, the BalanceFactor for this must be 2 and Right must be 1 in magnitude.
		/// If this is not the case, then this method's behavior is undefined.
		/// </remarks>
		public AABBTreeNode<V,B> RotateLeft(bool add = true)
		{
			// Give names to these things for convenience
			AABBTreeNode<V,B>? parent = Parent;
			
			// The children cannot be null if we are attempting to make a valid rotation
			AABBTreeNode<V,B> r_child = Right!;
			
			// The right niblings cannot be null if we are attempting to make a valid left rotation (we always have 0 or 2 children)
			AABBTreeNode<V,B> retained_right = r_child.BalanceFactor > 0 ? r_child.Right! : r_child.Left!;
			AABBTreeNode<V,B> new_right = r_child.BalanceFactor > 0 ? r_child.Left! : r_child.Right!;

			// Now assign everything
			// Parent stitching
			if(IsLeftChild)
			{
				Parent!.Left = r_child;
				r_child.Parent = parent;
			}
			else if(IsRightChild)
			{
				Parent!.Right = r_child;
				r_child.Parent = parent;
			}
			else
				r_child.Parent = null;

			// Right stitching
			r_child.Left = this;
			Parent = r_child;
			
			// Nibling stitching
			Right = new_right;
			new_right.Parent = this;

			// The following two nibling stitchings do nothing 1/2 the time, but we'll just put them in blindly because who cares
			r_child.Right = retained_right;
			retained_right.Parent = r_child;

			// The balance factors change for this and its old right child
			BalanceFactor = 0;

			// If this is an add rotation, then we'll set this to 0 as it should be
			// Otherwise, we'll preserve the balance factor for remove to look at and modify as appropriate
			if(add)
				r_child.BalanceFactor = 0;

			// Now update the bounds
			UpdateBounds();
			r_child.UpdateBounds();

			// Return the old right child
			return r_child;
		}

		/// <summary>
		/// Rotates right with the rotation rooted at this node's current position.
		/// </summary>
		/// <param name="add">If true, this is an add rotation. If false, this is a remove rotation. A remove rotation will leave the balance factor of the promoted sibling node along for inspection and modification.</param>
		/// <returns>Returns the new node that is now where this node was in the tree. This may be the new root of the tree.</returns>
		/// <remarks>
		/// It is assumed that this, Left, and Right are not leaves.
		/// Similarly, the BalanceFactor for this must be -2 and Left must be 1 in magnitude.
		/// If this is not the case, then this method's behavior is undefined.
		/// </remarks>
		public AABBTreeNode<V,B> RotateRight(bool add = true)
		{
			// Give names to these things for convenience
			AABBTreeNode<V,B>? parent = Parent;
			
			// The children cannot be null if we are attempting to make a valid rotation
			AABBTreeNode<V,B> l_child = Left!;
			
			// The left niblings cannot be null if we are attempting to make a valid right rotation (we always have 0 or 2 children)
			AABBTreeNode<V,B> retained_left = l_child.BalanceFactor < 0 ? l_child.Left! : l_child.Right!;
			AABBTreeNode<V,B> new_left = l_child.BalanceFactor < 0 ? l_child.Right! : l_child.Left!;

			// Now assign everything
			// Parent stitching
			if(IsLeftChild)
			{
				Parent!.Left = l_child;
				l_child.Parent = parent;
			}
			else if(IsRightChild)
			{
				Parent!.Right = l_child;
				l_child.Parent = parent;
			}
			else
				l_child.Parent = null;

			// Right stitching
			l_child.Right = this;
			Parent = l_child;
			
			// Nibling stitching
			Left = new_left;
			new_left.Parent = this;

			// The following two nibling stitchings do nothing 1/2 the time, but we'll just put them in blindly because who cares
			l_child.Left = retained_left;
			retained_left.Parent = l_child;

			// The balance factors change for this and its old left child
			BalanceFactor = 0;

			// If this is an add rotation, then we'll set this to 0 as it should be
			// Otherwise, we'll preserve the balance factor for remove to look at and modify as appropriate
			if(add)
				l_child.BalanceFactor = 0;

			// Now update the bounds
			UpdateBounds();
			l_child.UpdateBounds();
			
			// Return the old left child
			return l_child;
		}

		public override string? ToString() => HasValue ? Value!.ToString() : "No Value: " + Bounds;

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
		public AABBTreeNode<V,B>? Parent
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
		/// The sibling of this node or null if it has no sibling.
		/// </summary>
		public AABBTreeNode<V,B>? Sibling => IsRoot ? null : (IsLeftChild ? Parent!.Right : Parent!.Left);

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
		public bool IsRoot => Parent is null;

		/// <summary>
		/// If true, then this is an internal node.
		/// Otherwise, this is a leaf node.
		/// </summary>
		public bool IsInternal => !IsLeaf;

		/// <summary>
		/// If true, then this is a leaf node.
		/// Otherwise, this is an internal node.
		/// </summary>
		public bool IsLeaf => Left is null; // The way we construct our AABB tree, every node has either 0 or 2 children, never 1

		/// <summary>
		/// If true, then this is a left child of its parent.
		/// If false, then this is either a root node or a right child of its parent.
		/// </summary>
		public bool IsLeftChild => !IsRoot && Parent!.Left == this;

		/// <summary>
		/// If true, then this is a right child of its parent.
		/// If false, then this is either a root node or a left child of its parent.
		/// </summary>
		public bool IsRightChild => !IsRoot && Parent!.Right == this;
	}

	/// <summary>
	/// Extracts a bounding box from <paramref name="value"/>.
	/// </summary>
	/// <typeparam name="V">The type which has a bounding box.</typeparam>
	/// <typeparam name="B">The type of the bounding box.</typeparam>
	/// <param name="value">The value to extract a bounding box from.</param>
	/// <returns>Returns the bounding box of <paramref name="value"/>.</returns>
	public delegate B BoundaryExtractor<V,B>(V value) where B : struct, IBoundingBox<B>;

	/// <summary>
	/// Updates the boundary of <paramref name="value"/> to be <paramref name="new_boundary"/>.
	/// </summary>
	/// <typeparam name="V">The type which has a bounding box.</typeparam>
	/// <typeparam name="B">The type of the bounding box.</typeparam>
	/// <param name="value">The value to place a new boundary into.</param>
	/// <param name="new_boundary">The new boundary to place.</param>
	/// <returns>Returns true if the boundary was successfully updated and false otherwise.</returns>
	public delegate bool BoundaryUpdator<V,B>(V value, B new_boundary) where B : struct, IBoundingBox<B>;
}
