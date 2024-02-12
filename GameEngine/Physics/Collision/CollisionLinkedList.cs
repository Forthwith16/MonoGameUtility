using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameEngine.Physics.Collision
{
	/// <summary>
	/// A specialized linked list class that collision engines utilize.
	/// </summary>
	/// <typeparam name="T">The type to store in the list.</typeparam>
	public class CollisionLinkedList<T> : IEnumerable<T>
	{
		/// <summary>
		/// Creates an empty list.
		/// </summary>
		public CollisionLinkedList()
		{
			Head = null;
			Tail = null;

			Count = 0;
			return;
		}

		/// <summary>
		/// Adds a value to the front of the linked list.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>Returns the node created.</returns>
		public CollisionLinkedListNode<T> AddFirst(T value)
		{
			Head = new CollisionLinkedListNode<T>(value,null,Head);

			if(IsEmpty)
				Tail = Head;

			Count++;
			return Head;
		}

		/// <summary>
		/// Adds all items in <paramref name="items"/> to the front of this list.
		/// It maintains the order they are provided in <paramref name="items"/> so that the front of this list begins with <paramref name="items"/>.
		/// </summary>
		public void AddAllFirst(IEnumerable<T> items)
		{
			IEnumerator<T> itr = items.GetEnumerator();

			// If we have no items, do nothing
			if(!itr.MoveNext())
				return;

			// Add our first element
			AddFirst(itr.Current);

			// Now we can create a pointer to just add to new items after
			CollisionLinkedListNode<T> ptr = Head!;

			while(itr.MoveNext())
			{
				ptr = new CollisionLinkedListNode<T>(itr.Current,ptr,ptr.Next);
				Count++;
			}

			if(ptr.IsTail)
				Tail = ptr;

			return;
		}

		/// <summary>
		/// Adds <paramref name="value"/> to this list before <paramref name="n"/>.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <param name="n">The node to add this before. It is assumed that this node belongs to this list. If not, the behavior is undefined.</param>
		/// <returns>Returns the node created.</returns>
		public CollisionLinkedListNode<T> AddBefore(T value, CollisionLinkedListNode<T> n)
		{
			CollisionLinkedListNode<T> ret;

			if(n.IsHead)
				ret = Head = new CollisionLinkedListNode<T>(value,null,n);
			else
				ret = new CollisionLinkedListNode<T>(value,n.Previous,n);

			Count++;
			return ret;
		}

		/// <summary>
		/// Adds all items in <paramref name="items"/> to this list before <paramref name="n"/>.
		/// It maintains the order the items appear in <paramref name="items"/> before <paramref name="n"/>.
		/// </summary>
		public void AddAllBefore(IEnumerable<T> items, CollisionLinkedListNode<T> n)
		{
			foreach(T item in items)
				AddBefore(item,n);

			return;
		}

		/// <summary>
		/// Adds <paramref name="value"/> to this list after <paramref name="n"/>.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <param name="n">The node to add this after. It is assumed that this node belongs to this list. If not, the behavior is undefined.</param>
		/// <returns>Returns the node created.</returns>
		public CollisionLinkedListNode<T> AddAfter(T value, CollisionLinkedListNode<T> n)
		{
			CollisionLinkedListNode<T> ret;

			if(n.IsTail)
				ret = Tail = new CollisionLinkedListNode<T>(value,n,null);
			else
				ret = new CollisionLinkedListNode<T>(value,n,n.Next);

			Count++;
			return ret;
		}

		/// <summary>
		/// Adds all items in <paramref name="items"/> to this list after <paramref name="n"/>.
		/// It maintains the order the items appear in <paramref name="items"/> after <paramref name="n"/>.
		/// </summary>
		public void AddAllAfter(IEnumerable<T> items, CollisionLinkedListNode<T> n)
		{
			foreach(T item in items)
			{
				AddAfter(item,n);
				n = n.Next!;
			}

			return;
		}

		/// <summary>
		/// Adds a value to the end of the linked list.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>Returns the node created.</returns>
		public CollisionLinkedListNode<T> AddLast(T value)
		{
			Tail = new CollisionLinkedListNode<T>(value,Tail,null);

			if(IsEmpty)
				Head = Tail;

			Count++;
			return Tail;
		}

		/// <summary>
		/// Adds all items in <paramref name="items"/> to the end of this list.
		/// It maintains the order they are provided in <paramref name="items"/> so that the end of this list ends with <paramref name="items"/>.
		/// </summary>
		public void AddAllLast(IEnumerable<T> items)
		{
			foreach(T item in items)
				AddLast(item);

			return;
		}

		/// <summary>
		/// Adds a value into this list in its sorted position.
		/// This requires that the array is initially sorted before this method is invoked.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <param name="cmp">The means by which <typeparamref name="T"/> types are compared.</param>
		/// <returns>Returns the node created.</returns>
		public CollisionLinkedListNode<T> AddSorted(T value, Comparison<T> cmp)
		{
			CollisionLinkedListNode<T> n = AddLast(value);
			CollisionLinkedListNode<T>? nt = n.Previous;

			while(!n.IsHead && cmp(n.Previous!.Value,n.Value) > 0)
				n.MoveLeft();

			// If n is the head now, we update the head
			if(n.IsHead)
				Head = n;

			// If we moved n at all, then we update the tail
			if(!n.IsTail)
				Tail = nt;

			return n;
		}

		/// <summary>
		/// Removes the first instance of <paramref name="item"/> from this list.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>Returns true if the item was removed and false otherwise, such as if it does not exist in this list.</returns>
		public bool Remove(T item)
		{
			CollisionLinkedListNode<T>? ptr = Find(item);

			if(ptr is null)
				return false;

			if(ptr.IsHead)
				Head = ptr.Next;

			if(ptr.IsTail)
				Tail = ptr.Previous;

			ptr.Extirpate();
			Count--;

			return true;
		}

		/// <summary>
		/// Removes the first instance of <paramref name="item"/> after <paramref name="after"/> from this list.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <param name="after">The node to start searching after. It is assumed it belongs to this list. If not, then this method's behavior is undefined.</param>
		/// <returns>Returns true if the item was removed and false otherwise, such as if it does not exist in this list.</returns>
		public bool Remove(T item, CollisionLinkedListNode<T> after)
		{
			CollisionLinkedListNode<T>? ptr = Find(item,after);

			if(ptr is null)
				return false;

			if(ptr.IsHead)
				Head = ptr.Next;

			if(ptr.IsTail)
				Tail = ptr.Previous;

			ptr.Extirpate();
			Count--;

			return true;
		}

		/// <summary>
		/// Removes the last instance of <paramref name="item"/> from this list.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>Returns true if the item was removed and false otherwise, such as if it does not exist in this list.</returns>
		public bool RemoveLast(T item)
		{
			CollisionLinkedListNode<T>? ptr = FindLast(item);

			if(ptr is null)
				return false;

			if(ptr.IsHead)
				Head = ptr.Next;

			if(ptr.IsTail)
				Tail = ptr.Previous;

			ptr.Extirpate();
			Count--;

			return true;
		}

		/// <summary>
		/// Removes the last instance of <paramref name="item"/> after <paramref name="after"/> from this list.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <param name="after">The node to start searching before. It is assumed it belongs to this list. If not, then this method's behavior is undefined.</param>
		/// <returns>Returns true if the item was removed and false otherwise, such as if it does not exist in this list.</returns>
		public bool RemoveLast(T item, CollisionLinkedListNode<T> after)
		{
			CollisionLinkedListNode<T>? ptr = FindLast(item,after);

			if(ptr is null)
				return false;

			if(ptr.IsHead)
				Head = ptr.Next;

			if(ptr.IsTail)
				Tail = ptr.Previous;

			ptr.Extirpate();
			Count--;

			return true;
		}

		/// <summary>
		/// Removes the node <paramref name="n"/> from this list.
		/// </summary>
		/// <param name="n">The node to remove. It is assumed the node belongs to this list. If not, then this method's behavior is undefined.</param>
		public void Remove(CollisionLinkedListNode<T> n)
		{
			if(n == Head)
				Head = n.Next;

			if(n == Tail)
				Tail = n.Previous;

			n.Extirpate();
			Count--;

			return;
		}

		/// <summary>
		/// Finds the first node with the value given.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <returns>Returns the first node containing <paramref name="value"/> or null if no such node exists.</returns>
		public CollisionLinkedListNode<T>? Find(T value)
		{
			if(IsEmpty)
				return null;

			CollisionLinkedListNode<T>? ptr = Head;

			while(ptr is not null && ptr.Value is not null && !ptr.Value.Equals(value))
				ptr = ptr.Next;

			return ptr;
		}

		/// <summary>
		/// Finds the first node with the value given that occurs after <paramref name="after"/>.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <param name="after">The node to start searching after. It is assumed this node belongs to this list. If not, then the behavior is undefined.</param>
		/// <returns>Returns the first node containing <paramref name="value"/> after <paramref name="after"/> or null if no such node exists.</returns>
		public CollisionLinkedListNode<T>? Find(T value, CollisionLinkedListNode<T> after)
		{
			CollisionLinkedListNode<T>? ptr = after.Next;

			while(ptr is not null && ptr.Value is not null && !ptr.Value.Equals(value))
				ptr = ptr.Next;

			return ptr;
		}

		/// <summary>
		/// Finds the last node with the value given.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <returns>Returns the last node containing <paramref name="value"/> or null if no such node exists.</returns>
		public CollisionLinkedListNode<T>? FindLast(T value)
		{
			if(IsEmpty)
				return null;

			CollisionLinkedListNode<T>? ptr = Tail;

			while(ptr is not null && ptr.Value is not null && !ptr.Value.Equals(value))
				ptr = ptr.Previous;

			return ptr;
		}

		/// <summary>
		/// Finds the first node with the value given that occurs before <paramref name="before"/>.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <param name="before">The node to start searching before. It is assumed this node belongs to this list. If not, then the behavior is undefined.</param>
		/// <returns>Returns the first node containing <paramref name="value"/> before <paramref name="before"/> or null if no such node exists.</returns>
		public CollisionLinkedListNode<T>? FindLast(T value, CollisionLinkedListNode<T> before)
		{
			CollisionLinkedListNode<T>? ptr = before.Previous;

			while(ptr is not null && ptr.Value is not null && !ptr.Value.Equals(value))
				ptr = ptr.Previous;

			return ptr;
		}

		/// <summary>
		/// Determines if this list contains the value <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <returns>Returns true if this list contains <paramref name="value"/> and false otherwise.</returns>
		public bool Contains(T value) => Find(value) is not null;

		/// <summary>
		/// Clears this list.
		/// </summary>
		public void Clear()
		{
			Head = Tail = null;

			Count = 0;
			return;
		}

		/// <summary>
		/// Performs an insertion sort on this list.
		/// </summary>
		/// <param name="cmp">The means by which we compare <typeparamref name="T"/> types.</param>
		/// <remarks>Use this sort when this list is expected to already be mostly sorted.</remarks>
		public void InsertionSort(Comparison<T> cmp)
		{
			if(IsEmpty)
				return;

			// Grab the second element (if it exists)
			CollisionLinkedListNode<T>? i = Head!.Next;

			// Loop until we go past the end of the list
			while(i is not null)
			{
				CollisionLinkedListNode<T> j = i;

				// Special case the first swap so we maintain the correct i
				if(cmp(j.Value,j.Previous!.Value) < 0) // j always has a previous at this point
				{
					j.MoveLeft();
					i = i.Next;
				}
				else
				{
					i = i.Next;
					continue; // We don't need to perform any further swaps
				}

				// Now we know that i will be okay, so we can just swap normally
				while(!j.IsHead && cmp(j.Value,j.Previous!.Value) < 0)
					j.MoveLeft();

				i = i!.Next;
			}

			RepairHeadTail();
			return;
		}

		/// <summary>
		/// Repairs the Head and Tail values if they were lost somehow.
		/// </summary>
		public void RepairHeadTail()
		{
			// Restore the head (we could do this inline, but we save no time and this is easier
			while(!Head!.IsHead)
				Head = Head.Previous;

			// Restore the tail (we could do this inline, but we save no time and this is easier
			while(!Tail!.IsTail)
				Tail = Tail.Next;

			return;
		}

		/// <summary>
		/// Performs a quick sort on this list.
		/// </summary>
		/// <param name="cmp">The means by which we compare <typeparamref name="T"/> types.</param>
		/// <remarks>Use this sort when the number of comparisons required to sort this list is unknown.</remarks>
		public void QuickSort(Comparison<T> cmp)
		{
			if(Count < 2)
				return;

			// Due to how properties work, we need these
			CollisionLinkedListNode<T> h = Head!;
			CollisionLinkedListNode<T> t = Tail!;

			// Perform the quicksort
			QuickSort(cmp,ref h,ref t);

			// And lastly assign the head and tail to wherever they end up
			Head = h;
			Tail = t;

			return;
		}

		/// <summary>
		/// Performs a quick sort from <paramref name="from"/> to <paramref name="to"/>, both inclusive.
		/// </summary>
		/// <param name="cmp">The means by which we compare <typeparamref name="T"/> types.</param>
		/// <param name="from">The place to start quick sorting from (inclusive).</param>
		/// <param name="to">The place to quick sort to (inclusive).</param>
		protected void QuickSort(Comparison<T> cmp, ref CollisionLinkedListNode<T> from, ref CollisionLinkedListNode<T> to)
		{
			// If we have only one element or we crossed from past to, we do nothing
			if(from == null || to == null || from == to || to.Next == from)
				return;

			// Otherwise, do the usual quicksort stuff
			CollisionLinkedListNode<T> p = Partition(cmp,ref from,ref to);

			CollisionLinkedListNode<T> pp = p.Previous!;
			QuickSort(cmp,ref from,ref pp);

			CollisionLinkedListNode<T> pn = p.Next!;
			QuickSort(cmp,ref pn,ref to);

			return;
		}

		/// <summary>
		/// Paritions a sublist.
		/// </summary>
		/// <param name="cmp">The means by which <typeparamref name="T"/> types are compared.</param>
		/// <param name="from">The left pointer to the sublist (inclusive).</param>
		/// <param name="to">The right pointer to the sublist (inclusive).</param>
		/// <returns>Returns the pivot node.</returns>
		protected CollisionLinkedListNode<T> Partition(Comparison<T> cmp, ref CollisionLinkedListNode<T> from, ref CollisionLinkedListNode<T> to)
		{
			// Just assign the pivot to be the last element
			// We could get away with not having this, but it will make the final swaps easier
			CollisionLinkedListNode<T> pivot = to;

			// Make a left pointer
			CollisionLinkedListNode<T> left = from;

			// Move to left one to ignore the pivot
			CollisionLinkedListNode<T> right = to.Previous!; // We only get here if from is strictly before to

			// Check if we only had one element
			if(left == right)
			{
				// Swap left with the pivot if we need to
				if(cmp(left.Value,pivot.Value) > 0)
				{
					left.Swap(pivot);

					from = pivot;
					to = left;
				}

				return pivot;
			}

			// Loop until left and right meet
			while(true)
			{
				// Move left right until we get something bigger than the pivot or we meet the right pointer
				while(left != right && cmp(left.Value,pivot.Value) <= 0)
					left = left.Next!;

				// Move right left until we get something smaller than the pivot or we meet the left pointer
				while(right != left && cmp(right.Value,pivot.Value) >= 0)
					right = right.Previous!;

				// If left met right, then we will either swap left with the pivot or left's right with the pivot
				if(left == right)
					if(cmp(left.Value,pivot.Value) < 0) // If the left value is smaller than the pivot, then it NEEDS to stay on the left
					{
						to = left.Next!; // Do this before swapping so we don't lose track of it
						left.Next!.Swap(pivot);

						break;
					}
					else // Otherwise, we can just swap with the pivot and be done
					{
						// If we haven't moved left, we'll have to update from
						if(left == from)
							from = pivot;

						left.Swap(pivot);
						to = left;  // We swapped pivot out of to, so we need to update to

						break;
					}
				else
				{
					// Maintain the from variable (to never changes since we have the pivot there)
					if(left == from)
						from = right;

					// Now do the actual swap
					left.Swap(right);

					CollisionLinkedListNode<T> n = left;
					left = right;
					right = n;
				}
			}

			return pivot;
		}

		public IEnumerator<T> GetEnumerator() => new CollisionLinkedListEnumerator<T>(this);
		IEnumerator IEnumerable.GetEnumerator() => new CollisionLinkedListEnumerator<T>(this);

		public override string ToString()
		{
			StringBuilder ret = new StringBuilder();
			ret.Append('(');

			foreach(T value in this)
				ret.Append(value + ",");

			ret.Remove(ret.Length - 1,1);
			ret.Append(')');

			return ret.ToString();
		}

		public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

		/// <summary>
		/// The head node of the linked list.
		/// This node either contains real data or is null when Count is zero.
		/// </summary>
		public CollisionLinkedListNode<T>? Head
		{get; protected set;}

		/// <summary>
		/// The first item of the list.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if no such item exists.</exception>
		public T First => Head is null ? throw new InvalidOperationException() : Head.Value;

		/// <summary>
		/// The tail node of the linked list.
		/// This node either contains real data or is null when Count is zero.
		/// </summary>
		public CollisionLinkedListNode<T>? Tail
		{get; protected set;}

		/// <summary>
		/// The last item of the list.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if no such item exists.</exception>
		public T Last => Tail is null ? throw new InvalidOperationException() : Tail.Value;

		/// <summary>
		/// The number of nodes in this list.
		/// </summary>
		public int Count
		{get; protected set;}

		/// <summary>
		/// If true, this list is empty.
		/// Otherwise, this is false and Count is at least one.
		/// </summary>
		public bool IsEmpty => Count == 0;

		/// <summary>
		/// If true, this list is not empty and Count is at least one.
		/// Otherwise, this is false and the list is empty.
		/// </summary>
		public bool IsNotEmpty => Count > 0;
	}

	/// <summary>
	/// A specialized linked list node for collision engines.
	/// </summary>
	/// <typeparam name="T">The type to store in the node.</typeparam>
	public class CollisionLinkedListNode<T>
	{
		/// <summary>
		/// Creates a new node with no adjacent nodes.
		/// </summary>
		/// <param name="value">The value stored in the node.</param>
		public CollisionLinkedListNode(T value) : this(value,null,null)
		{return;}

		/// <summary>
		/// Creates a new node.
		/// </summary>
		/// <param name="value">The value stored in the node.</param>
		/// <param name="prev">The previous node.</param>
		/// <param name="next">The next node.</param>
		public CollisionLinkedListNode(T value,CollisionLinkedListNode<T>? prev, CollisionLinkedListNode<T>? next)
		{
			Value = value;
			StitchTogether(prev,next);

			return;
		}

		/// <summary>
		/// Stitches this node in-between <paramref name="prev"/> and <paramref name="next"/>.
		/// If this node was previous between two nodes, then those nodes are stitched together first.
		/// </summary>
		/// <param name="prev">The new previous node.</param>
		/// <param name="next">The new next node.</param>
		public void StitchTogether(CollisionLinkedListNode<T>? prev, CollisionLinkedListNode<T>? next)
		{
			// Stitch the old previous and next together
			if(Previous is not null)
				Previous.Next = Next;

			if(Next is not null)
				Next.Previous = Previous;

			// Stitch the new previous to this
			Previous = prev;

			if(Previous is not null)
				Previous.Next = this;

			// Stitch the new next to this
			Next = next;

			if(Next is not null)
				Next.Previous = this;

			return;
		}

		/// <summary>
		/// Swaps this node with <paramref name="n"/>.
		/// This swaps the whole node, not the data within the nodes.
		/// </summary>
		/// <param name="n">The node to swap with. It is assumed it exists within the same list as this. If not, this method's behavior is undefined.</param>
		public void Swap(CollisionLinkedListNode<T> n)
		{
			if(n == this)
				return;

			// If we're swapping foward, just move right
			if(n == Next)
			{
				MoveRight();
				return;
			}

			// If we're swapping backward, just move left
			if(n == Previous)
			{
				MoveLeft();
				return;
			}

			// Otherwise, perform a general swap
			CollisionLinkedListNode<T>? np = n.Previous;
			CollisionLinkedListNode<T>? nn = n.Next;

			n.StitchTogether(Previous,Next);

			Previous = null;
			Next = null;
			StitchTogether(np,nn);

			return;
		}

		/// <summary>
		/// Removes this node by stitching its Next and Previous together.
		/// This method does not null this node's Next or Previous upon completion.
		/// </summary>
		public void Extirpate()
		{
			if(Previous is not null)
				Previous.Next = Next;

			if(Next is not null)
				Next.Previous = Previous;

			return;
		}

		/// <summary>
		/// Moves this node left in its list.
		/// </summary>
		/// <returns>Returns true if it moved left and false otherwise, such as when it is already the head.</returns>
		public bool MoveLeft()
		{
			if(IsHead)
				return false;

			StitchTogether(Previous!.Previous,Previous);
			return true;
		}

		/// <summary>
		/// Moves this node right in its list.
		/// </summary>
		/// <returns>Returns true if it moved right and false otherwise, such as when it is already the tail.</returns>
		public bool MoveRight()
		{
			if(IsTail)
				return false;

			StitchTogether(Next,Next!.Next);
			return true;
		}

		public override string ToString() => "(" + (IsHead ? "null" : Previous!.Value) + "," + Value + "," + (IsTail ? "null" : Next!.Value) + ")";

		/// <summary>
		/// The value of this node.
		/// </summary>
		public T Value
		{get; set;}

		/// <summary>
		/// The previous node in the list.
		/// </summary>
		public CollisionLinkedListNode<T>? Previous
		{get; protected set;}

		/// <summary>
		/// The next node in the list.
		/// </summary>
		public CollisionLinkedListNode<T>? Next
		{get; protected set;}

		/// <summary>
		/// True if this is the head of the linked list, which occurs when Previous is null.
		/// </summary>
		public bool IsHead => Previous is null;

		/// <summary>
		/// True if this is the tail of the linked list, which occurs when Next is null.
		/// </summary>
		public bool IsTail => Next is null;
	}

	/// <summary>
	/// A specialize enumerator for the collision engine's linked list class.
	/// </summary>
	/// <typeparam name="T">The type being enumerated.</typeparam>
	public class CollisionLinkedListEnumerator<T> : IEnumerator<T>
	{
		/// <summary>
		/// Creates a new enumerator that enumerates all of <paramref name="l"/>.
		/// </summary>
		public CollisionLinkedListEnumerator(CollisionLinkedList<T> l)
		{
			Start = l.Head;
			End = l.Tail;

			Ptr = null;
			return;
		}

		/// <summary>
		/// Enumerates from <paramref name="start"/> to <paramref name="end"/>, both inclusive.
		/// </summary>
		/// <param name="start">The first piece of data enumerated.</param>
		/// <param name="end">The last piece of data enumerated.</param>
		public CollisionLinkedListEnumerator(CollisionLinkedListNode<T> start, CollisionLinkedListNode<T> end)
		{
			Start = start;
			End = end;

			Ptr = null;
			return;
		}

		public bool MoveNext()
		{
			if(Ptr is null)
				if(Start is null)
					return false; // We won't set Ptr to null because that would reset us and is unncessary
				else
					Ptr = Start;
			else // Ptr is not null
				if(Ptr == End)
					return false; // We won't set Ptr to null because that would reset us and is unncessary
				else // We are not done yet
					Ptr = Ptr.Next;

			return true;
		}

		public void Reset()
		{
			Ptr = null;
			return;
		}

		public void Dispose()
		{return;} // We have no resources held, so we do nothing here

		public T Current => Ptr is null ? throw new InvalidOperationException() : Ptr.Value;

		object? IEnumerator.Current => Current;

		/// <summary>
		/// The starting node of this enumeration.
		/// </summary>
		protected CollisionLinkedListNode<T>? Start
		{get;}

		/// <summary>
		/// The current node of the enumeration.
		/// </summary>
		protected CollisionLinkedListNode<T>? Ptr
		{get; set;}

		/// <summary>
		/// The ending node of this enumeration.
		/// </summary>
		protected CollisionLinkedListNode<T>? End
		{get;}
	}
}
