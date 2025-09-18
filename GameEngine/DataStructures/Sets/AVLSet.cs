using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GameEngine.DataStructures.Sets
{
	/// <summary>
	/// An AVL set.
	/// This data structure allows only unique entries and keeps them in an AVL tree.
	/// When enumerated, the elements are produced in ascending order (unless otherwise specified).
	/// </summary>
	/// <typeparam name="T">The type stored in the tree.</typeparam>
	public class AVLSet<T> : IEnumerable<T>, ICollection<T>
	{
		/// <summary>
		/// Creates an empty set.
		/// </summary>
		/// <param name="cmp">The means by which items are compared.</param>
		public AVLSet(IComparer<T> cmp)
		{
			Root = null;
			Count = 0;

			Scale = cmp;
			return;
		}

		/// <summary>
		/// Creates a set initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed values to add to the set.</param>
		/// <param name="cmp">The means by which items are compared.</param>
		public AVLSet(IEnumerable<T> seed, IComparer<T> cmp) : this(cmp)
		{
			foreach(T t in seed)
				Add(t);

			return;
		}

		/// <summary>
		/// Adds <paramref name="item"/> to this set.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>
		///	Returns true if the item was added and false otherwise.
		///	The latter will only happen if it already exists in this set.
		/// </returns>
		public bool Add(T item)
		{
			// If we have no root, then adding is easy
			if(Root is null)
			{
				Root = new AVLArrayNode(item);
				Count = 1;

				return true;
			}

			// If we're not adding the root, then we need to crawl down to the node we want to attach the new item to
			AVLArrayNode n = Root;

			while(true)
			{
				// Weight the new item against where we are in the tree to decide which way to go further down
				int cmp = Scale.Compare(n.Value,item);

				if(cmp == 0)
					return false; // We do not permit duplicates
				else if(cmp < 0)
				{
					// We are bigger than the current location in the tree, so we go right
					if(n.Right is null)
					{
						n.Right = new AVLArrayNode(item,n);
						break; // We will update n's height in the rebalance
					}

					n = n.Right;
				}
				else if(cmp > 0)
				{
					// We are smaller than the current location in the tree, so we go left
					if(n.Left is null)
					{
						n.Left = new AVLArrayNode(item,n);
						break; // We will update n's height in the rebalance
					}

					n = n.Left;
				}
			}

			// We gained an item
			Count++;

			// Now we need to rebalance
			AddRebalance(n);

			return true;
		}

		/// <summary>
		/// Performs a tree rebalance upward starting from <paramref name="n"/>.
		/// </summary>
		protected void AddRebalance(AVLArrayNode n)
		{
			// Remember if we're the root
			bool root = n.IsRoot;

			// Make sure our height is correct
			n.UpdateHeight();

			switch(n.Balance)
			{
			case BalanceFactor.LeftHeavy:
				// We are looking to shift n's weight right
				// To do so, we need n.Left to be Balanced or LeftUnsteady
				if(n.Left!.Balance == BalanceFactor.RightUnsteady) // To be left heavy, n.Left cannot be null
					n.Left.RotateLeft(); // Rotating n.Left left shifts its weight left

				// Rotating n right will now shift its weight right
				n.RotateRight();

				break;
			case BalanceFactor.RightHeavy:
				// We are looking to shift n's weight left
				// To do so, we need n.Right to be Balanced or RightUnsteady
				if(n.Right!.Balance == BalanceFactor.LeftUnsteady) // To be right heavy, n.Right cannot be null
					n.Right.RotateRight(); // Rotating n.Right right shifts its weight right

				// Rotating n left will now shift its weight left
				n.RotateLeft();

				break;
			default:
				break;
			}
			
			// If we are the root, then we're done (and may need to reassign the root)
			if(root)
				Root = n.Parent; // n's parent is always the root, even if that's just n itself
			else
				AddRebalance(n.Parent); // If we're not the root, we need to carry on up the tree until we get there

			return;
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
			return;
		}

		/// <summary>
		/// Removes <paramref name="item"/> from this set.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>Returns true if the item was present in the set and removed and false otherwise.</returns>
		public bool Remove(T item)
		{
			// We first find the item in the tree
			AVLArrayNode? n = Root;

			while(n is not null)
			{
				int cmp = Scale.Compare(n.Value,item);

				if(cmp == 0) // Found it
					break;
				else if(cmp < 0) // We are bigger than the current tree node
					n = n.Right;
				else if(cmp > 0) // We are smaller than the current tree node
					n = n.Left;
			}

			// If we found nothing, then fail
			if(n is null)
				return false;

			// We will for sure lose something now
			Count--;

			// Switch based on how many children we have
			if(n.Left is null)
				if(n.Right is null)
					n.Sever(null); // No children
				else
					n.Sever(n.Right); // Just a right child
			else if(n.Right is null)
				n.Sever(n.Left); // Just a left child
			else
			{
				// n has two children, so we need to move its successor up (it has two children, so it must have a successor)
				AVLArrayNode swap = GetNext(n)!;
				
				// We don't need to swap, only clobber, as we're deleting n's value regardless
				n.Value = swap.Value;

				// Now behave as normal but with the confidence that we have at most one child
				n = swap;

				if(n.Left is null)
					if(n.Right is null)
						n.Sever(null);
					else
						n.Sever(n.Right);
				else
					n.Sever(n.Left);
			}

			// If n was the root, then we need to reassign the root
			if(n.IsRoot)
			{
				if(n.Left is not null)
					Root = n.Left.Parent = n.Left;
				else if(n.Right is not null)
					Root = n.Right.Parent = n.Right;
				else
					Root = null;

				// Removing the root is ONLY possible when there are 1 or 2 nodes in the tree, otherwise the root MUST have 2 children to be balanced
				// As such, we're done early
				return true;
			}

			// And now we need to rebalance the tree starting from n's parent, as any child n had must have been balanced
			RemoveRebalance(n.Parent);

			return true;
		}

		/// <summary>
		/// Obtains the node that follows <paramref name="n"/>.
		/// </summary>
		/// <param name="n">The node.</param>
		/// <returns>Returns the next node after <paramref name="n"/> or null if <paramref name="n"/> is the last node in the set.</returns>
		protected AVLArrayNode? GetNext(AVLArrayNode n)
		{
			// We find the next node by going right one and then left as far as possible
			if(n.Right is not null)
			{
				n = n.Right;

				while(n.Left is not null)
					n = n.Left;

				return n;
			}

			// Or, if going right is not an option, then we go up until we are a left child and then jump to the parent
			while(!n.IsRoot && n.IsRightChild)
				n = n.Parent;

			// If we ended at the root, we're done
			if(n.IsRoot)
				return null;

			return n.Parent;
		}

		/// <summary>
		/// Performs a tree rebalance upward starting from <paramref name="n"/>.
		/// </summary>
		protected void RemoveRebalance(AVLArrayNode n)
		{
			// Remember if we're the root
			bool root = n.IsRoot;

			// Make sure our height is correct
			n.UpdateHeight();

			switch(n.Balance)
			{
			case BalanceFactor.LeftHeavy:
				// We are looking to shift n's weight right
				// To do so, we need n.Left to be Balanced or LeftUnsteady
				if(n.Left!.Balance == BalanceFactor.RightUnsteady) // To be left heavy, n.Left cannot be null
					n.Left.RotateLeft(); // Rotating n.Left left shifts its weight left

				// Rotating n right will now shift its weight right
				n.RotateRight();

				break;
			case BalanceFactor.RightHeavy:
				// We are looking to shift n's weight left
				// To do so, we need n.Right to be Balanced or RightUnsteady
				if(n.Right!.Balance == BalanceFactor.LeftUnsteady) // To be right heavy, n.Right cannot be null
					n.Right.RotateRight(); // Rotating n.Right right shifts its weight right

				// Rotating n left will now shift its weight left
				n.RotateLeft();

				break;
			default:
				break;
			}
			
			// If we are the root, then we're done (and may need to reassign the root)
			if(root)
				Root = n.Parent; // n's parent is always the root, even if that's just n itself
			else
				RemoveRebalance(n.Parent); // If we're not the root, we need to carry on up the tree until we get there

			return;
		}

		/// <summary>
		/// Determines if this set contains <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <returns>Returns true if this set contains <paramref name="item"/> and false otherwise.</returns>
		public bool Contains(T item)
		{
			AVLArrayNode? n = Root;

			while(n is not null)
			{
				int cmp = Scale.Compare(n.Value,item);

				if(cmp == 0) // Found it
					return true;
				else if(cmp < 0) // We are bigger than the current tree node
					n = n.Right;
				else if(cmp > 0) // We are smaller than the current tree node
					n = n.Left;
			}

			return false;
		}

		/// <summary>
		/// Gets an item in this set.
		/// </summary>
		/// <param name="item">A representative value considered equal to the item desired when weighed on the Scale.</param>
		/// <param name="output">The output item. This is null when no matching item is in this set.</param>
		/// <returns>Returns true if an item was found and false otherwise.</returns>
		public bool Get(T item, [MaybeNullWhen(false)] out T? output)
		{
			AVLArrayNode? n = Root;

			while(n is not null)
			{
				int cmp = Scale.Compare(n.Value,item);

				if(cmp == 0) // Found it
				{
					output = n.Value;
					return true;
				}
				else if(cmp < 0) // We are bigger than the current tree node
					n = n.Right;
				else if(cmp > 0) // We are smaller than the current tree node
					n = n.Left;
			}

			output = default(T?);
			return false;
		}

		/// <summary>
		/// Clears this set of all items.
		/// </summary>
		public void Clear()
		{
			Root = null;
			Count = 0;

			return;
		}

		/// <summary>
		/// Copies this into an array.
		/// </summary>
		/// <param name="array">The array to copy into.</param>
		/// <param name="offset">The offset into <paramref name="array"/> to start copying to.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown if there is not enough room in <paramref name="array"/> to copy this set starting at index <paramref name="offset"/>.</exception>
		public void CopyTo(T[] array, int offset)
		{
			if(offset < 0)
				throw new ArgumentOutOfRangeException();

			if(array.Length < offset + Count)
				throw new ArgumentException();

			int i = 0;

			foreach(T t in this)
				array[offset + i++] = t;

			return;
		}

		public IEnumerator<T> GetEnumerator() => new AVLSetEnumerator(this);
		public IEnumerator<T> GetReverseEnumerator() => new AVLSetEnumerator(this,true);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString()
		{
			if(IsEmpty)
				return "{}";

			StringBuilder ret = new StringBuilder("{");

			foreach(T t in this)
				ret.Append(t + ", ");

			ret.Remove(ret.Length - 2,2);
			ret.Append("}");

			return ret.ToString();
		}

		/// <summary>
		/// The number of items in this set.
		/// </summary>
		public int Count
		{get; protected set;}

		/// <summary>
		/// If true, then this set is empty.
		/// </summary>
		public bool IsEmpty => Count == 0;

		/// <summary>
		/// If true, then this set is not empty.
		/// </summary>
		public bool IsNotEmpty => Count != 0;

		/// <summary>
		/// The root node of this tree set.
		/// </summary>
		protected AVLArrayNode? Root
		{get; set;}

		/// <summary>
		/// The means by which we compare <typeparamref name="T"/> types.
		/// </summary>
		private IComparer<T> Scale
		{get;}
		
		public bool IsReadOnly => false;

		/// <summary>
		/// Represents a node in an AVL tree.
		/// </summary>
		protected class AVLArrayNode
		{
			/// <summary>
			/// Creates a new root node with the given value.
			/// </summary>
			public AVLArrayNode(T value)
			{
				Value = value;
				Height = 1;
				
				Parent = this; // Can't pass this to another constructor
				Left = Right = null;

				return;
			}

			/// <summary>
			/// Creates a new leaf node with the given value, height, and parent.
			/// </summary>
			public AVLArrayNode(T value, AVLArrayNode parent) : this(value,1,parent,null,null)
			{return;}

			/// <summary>
			/// Creates a new node with the given value, height, parent, left child, and right child.
			/// </summary>
			public AVLArrayNode(T value, int height, AVLArrayNode parent, AVLArrayNode? left, AVLArrayNode? right)
			{
				Value = value;
				Height = height;
				
				Parent = parent;
				Left = left;
				Right = right;

				return;
			}

			/// <summary>
			/// Rotates this node left in the tree.
			/// </summary>
			/// <remarks>
			///	This method assumes Right and Right.left are not null.
			///	<para/>
			///	This does not update heights recursively up the tree.
			/// </remarks>
			public void RotateLeft()
			{
				// We need to remember our parent
				AVLArrayNode over_node = Parent;
				
				bool root = IsRoot;
				bool left = IsLeftChild;

				// This node moves down to the left
				Parent = Right!;
				
				// The right child's left subtree becomes this node's right child
				Right = Parent.Left;
				
				if(Right is not null)
					Right.Parent = this;

				// It's now safe to finish stitching us into our new parent
				Parent.Left = this;
				
				// The right child moves up
				if(root)
					Parent.Parent = Parent;
				else
				{
					Parent.Parent = over_node;
				
					if(left)
						over_node.Left = Parent;
					else
						over_node.Right = Parent;
				}

				// Update every height that might have changed
				UpdateHeight();
				Parent.Right?.UpdateHeight();
				Parent.UpdateHeight();

				return;
			}

			/// <summary>
			/// Rotates this node right in the tree.
			/// </summary>
			/// <remarks>
			///	This method assumes Left and Left.Right are not null.
			///	<para/>
			///	This does not update heights recursively up the tree.
			/// </remarks>
			public void RotateRight()
			{
				// We need to remember our parent
				AVLArrayNode over_node = Parent;

				bool root = IsRoot;
				bool left = IsLeftChild;

				// This node moves down to the right
				Parent = Left!;
				
				// The left child's right subtree becomes this node's left child
				Left = Parent.Right;
				
				if(Left is not null)
					Left.Parent = this;

				// It's now safe to finish stitching us into our new parent
				Parent.Right = this;
				
				// The left child moves up
				if(root)
					Parent.Parent = Parent;
				else
				{
					Parent.Parent = over_node;
				
					if(left)
						over_node.Left = Parent;
					else
						over_node.Right = Parent;
				}

				// Update every height that might have changed
				UpdateHeight();
				Parent.Left?.UpdateHeight();
				Parent.UpdateHeight();

				return;
			}

			/// <summary>
			/// Stitches this node together with its parent and children.
			/// To do so, it sets the appropriate link in each to this.
			/// </summary>
			/// <param name="left">
			///	If true, then this should be the left child of its parent.
			///	If false, then it should be the right child of its parent.
			///	If this is a root node, then this value is ignored.
			/// </param>
			public void Stitch(bool left)
			{
				if(!IsRoot)
					if(left)
						Parent.Left = this;
					else
						Parent.Right = this;

				if(Left is not null)
					Left.Parent = this;

				if(Right is not null)
					Right.Parent = this;
				
				return;
			}

			/// <summary>
			/// Severs this node's parent from this (but not this from its parent).
			/// It is replaced with <paramref name="replacement"/> in the same child slot.
			/// This will also assign <paramref name="replacement"/>'s parent to be Parent if <paramref name="replacement"/> is not null.
			/// </summary>
			public void Sever(AVLArrayNode? replacement)
			{
				if(Parent.Left == this) // Faster since we don't check for rootage
					Parent.Left = replacement;
				else if(Parent.Right == this)
					Parent.Right = replacement;

				if(replacement is not null)
					replacement.Parent = Parent;

				return;
			}

			/// <summary>
			/// Updates the height of this node based on its children's height.
			/// </summary>
			/// <returns>Returns true if the height changed and false otherwise.</returns>
			public bool UpdateHeight()
			{
				int old = Height;
				Height = 1 + Math.Max(Left is null ? 0 : Left.Height,Right is null ? 0 : Right.Height);

				return old != Height;
			}

			public override string ToString() => "(" + Value + "," + Height + "," + Balance + ")";

 			/// <summary>
			/// The value stored in this node.
			/// </summary>
			public T Value;

			/// <summary>
			/// The height of the node.
			/// </summary>
			public int Height;

			/// <summary>
			/// The balance factor of this node.
			/// </summary>
			public BalanceFactor Balance
			{
				get
				{
					int bf = (Right is null ? 0 : Right.Height) - (Left is null ? 0 : Left.Height);

					if(bf < -1)
						return BalanceFactor.LeftHeavy;
					else if(bf > 1)
						return BalanceFactor.RightHeavy;
					else if(bf == -1)
						return BalanceFactor.LeftUnsteady;
					else if(bf == 1)
						return BalanceFactor.RightUnsteady;

					return BalanceFactor.Balanced;
				}
			}

			/// <summary>
			/// The parent of this node.
			/// </summary>
			public AVLArrayNode Parent;

			/// <summary>
			/// The left child of this node.
			/// </summary>
			public AVLArrayNode? Left;

			/// <summary>
			/// The right child of this node.
			/// </summary>
			public AVLArrayNode? Right;

			/// <summary>
			/// Iff true, then this is a root node. 
			/// </summary>
			public bool IsRoot => Parent == this;

			/// <summary>
			/// Determines if this is a left child.
			/// </summary>
			public bool IsLeftChild => !IsRoot && Parent.Left == this;

			/// <summary>
			/// Determines if this is a right child.
			/// </summary>
			public bool IsRightChild => !IsRoot && Parent.Right == this;

			/// <summary>
			/// Iff true, then this is a leaf node.
			/// </summary>
			public bool IsLeaf => Left is null && Right is null;
		}

		/// <summary>
		/// Enumerates an AVL Set in ascending order.
		/// </summary>
		private class AVLSetEnumerator : IEnumerator<T>
		{
			/// <summary>
			/// Creates an enumerator of an AVL set.
			/// </summary>
			public AVLSetEnumerator(AVLSet<T> set, bool reverse = false)
			{
				Set = set;

				Next = null;
				Done = false;

				Reversed = reverse;
				return;
			}

			public void Dispose()
			{return;}

			public bool MoveNext()
			{
				if(Done)
					return false;

				// If we've not started, we just go all the way left (or right in reverse)
				if(Next is null)
				{
					Next = Set.Root;

					// Unless we have nothing, in which case we're immediately done
					if(Next is null)
					{
						Done = true;
						return false;
					}

					if(Reversed)
						while(Next.Right is not null)
							Next = Next.Right;
					else
						while(Next.Left is not null)
							Next = Next.Left;

					return true;
				}

				// If we have started, then we need to go to the next node
				return Reversed ? GoPrev() : GoNext();
			}

			/// <summary>
			/// Goes to the next node.
			/// </summary>
			/// <returns>Returns false if there was no next node and true otherwise.</returns>
			protected bool GoNext()
			{
				// This is done by going right one and then left as far as possible
				if(Next!.Right is not null)
				{
					Next = Next.Right;

					while(Next.Left is not null)
						Next = Next.Left;

					return true;
				}

				// Or, if going right is not an option, then we go up until we are a left child and then jump to the parent
				while(!Next.IsRoot && Next.IsRightChild)
					Next = Next.Parent;

				// If we ended at the root, we're done
				if(Next.IsRoot)
				{
					Done = true;
					return false;
				}

				Next = Next.Parent;
				return true;
			}

			/// <summary>
			/// Goes to the previous node.
			/// </summary>
			/// <returns>Returns false if there was no previous node and true otherwise.</returns>
			protected bool GoPrev()
			{
				// This is done by going left one and then right as far as possible
				if(Next!.Left is not null)
				{
					Next = Next.Left;

					while(Next.Right is not null)
						Next = Next.Right;

					return true;
				}

				// Or, if going left is not an option, then we go up until we are a right child and then jump to the parent
				while(!Next.IsRoot && Next.IsLeftChild)
					Next = Next.Parent;

				// If we ended at the root, we're done
				if(Next.IsRoot)
				{
					Done = true;
					return false;
				}

				Next = Next.Parent;
				return true;
			}

			public void Reset()
			{
				Next = null;
				Done = false;

				return;
			}

			public T Current => Next!.Value; // Current is undefined if we're past the end of the set
			object? IEnumerator.Current => Current;

			/// <summary>
			/// The set we enumerate.
			/// </summary>
			private AVLSet<T> Set
			{get;}

			/// <summary>
			/// The next node.
			/// Or rather the current node.
			/// </summary>
			private AVLArrayNode? Next;

			/// <summary>
			/// Iff true, we are enumerating in reverse.
			/// </summary>
			public bool Reversed
			{get;}

			/// <summary>
			/// If true, then we're done.
			/// </summary>
			public bool Done
			{get; protected set;}
		}

		/// <summary>
		/// Represents how a tree node is balanced.
		/// </summary>
		protected enum BalanceFactor
		{
			LeftHeavy,
			LeftUnsteady,
			Balanced,
			RightUnsteady,
			RightHeavy
		}
	}
}
