using GameEngine.DataStructures.Geometry;
using GameEngine.Physics.Collision.Colliders;
using System.Collections;

namespace GameEngine.Physics.Collision
{
	/// <summary>
	/// A quadtree.
	/// </summary>
	public class Quadtree : IEnumerable<ICollider2D>
	{
		/// <summary>
		/// Creates a new quadtree.
		/// </summary>
		/// <param name="left">The initial left bound of the quadtree. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The initial right bound of the quadtree. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The initial bottom bound of the quadtree. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The initial top bound of the quadtree. This should be strictly greater than <paramref name="bottom"/>.</param>
		public Quadtree(float left, float right, float bottom, float top) : this(new FRectangle(left,bottom,right - left,top - bottom))
		{return;}

		/// <summary>
		/// Creates a new quadtree.
		/// </summary>
		/// <param name="initial_boundary">The initial boundary of the quadtree.</param>
		public Quadtree(FRectangle initial_boundary)
		{
			Root = new Quad(initial_boundary);
			return;
		}

		/// <summary>
		/// Queries this tree to determine what colliders in it collide with <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to check for collisions.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="c"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider2D> Query(ICollider2D c)
		{
			if(!TreeBoundary.Intersects(c.Boundary))
				return Enumerable.Empty<ICollider2D>();

			return Root.Query(c);
		}

		/// <summary>
		/// Queries this tree to determine what colliders in it collide with <paramref name="region"/>.
		/// </summary>
		/// <param name="region">The region to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="region"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider2D> Query(FRectangle region)
		{
			if(!TreeBoundary.Intersects(region))
				return Enumerable.Empty<ICollider2D>();

			return Root.Query(region);
		}

		/// <summary>
		/// Adds <paramref name="c"/> to this tree.
		/// If <paramref name="c"/> is out of bounds, then the tree will grow to accommodate it.
		/// </summary>
		/// <param name="c">The collider to add.</param>
		public void Add(ICollider2D c)
		{
			// Compute once
			FRectangle b = TreeBoundary;

			while(!b.Contains(c.Boundary))
			{
				// Determine which direction we need to grow to contain c
				// Our new collider can literally have our boundary but inflated, so we need to just pick a direction to grow in greedily
				if(c.LeftBound < b.Left)
					if(c.BottomBound < b.Bottom) // Too far left and too far down
						Root = new Quad(Root,QuadRegion.TopRight);
					else // Too far left and too far up or we're too far left but within the vertical bounds, so just pick a direction to grow the vertical bound
						Root = new Quad(Root,QuadRegion.BottomRight);
				else  // Too far right or within reason for the horizontal bounds, in which case just pick a direction to grow the horizontal bound
					if(c.BottomBound < b.Bottom) // Too far right and too far down
						Root = new Quad(Root,QuadRegion.TopLeft);
					else // Too far right and too far up or we're too far right but within the vertical bounds, so just pick a direction to grow the vertical bound
						Root = new Quad(Root,QuadRegion.BottomLeft);

				// Update our bounds
				b = TreeBoundary;
			}

			Root.Add(c);
			return;
		}

		/// <summary>
		/// Removes the first instance of <paramref name="c"/> found from this tree.
		/// </summary>
		/// <param name="c">The collider to remove.</param>
		/// <returns>Returns true if <paramref name="c"/> is removed and false otherwise.</returns>
		public bool Remove(ICollider2D c, bool use_current_boundary = true) => IsEmpty ? false : Root.Remove(c);

		/// <summary>
		/// Determines if this tree contains <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to look for.</param>
		/// <returns>Returns true if this tree contains <paramref name="c"/> and false otherwise.</returns>
		public bool Contains(ICollider2D c) => IsEmpty ? false : Root.Contains(c);

		/// <summary>
		/// Updates the boundary of <paramref name="c"/> in this tree.
		/// If <paramref name="c"/> does not belong to this tree, then this does nothing.
		/// </summary>
		/// <param name="c">The collider whose boundary needs updating.</param>
		/// <returns>Returns true if <paramref name="c"/>'s boundary was changed and false otherwise.</returns>
		/// <remarks><paramref name="c"/>'s membership is determined via the Remove method, which uses Query to locate <paramref name="c"/>.</remarks>
		public bool UpdateBoundary(ICollider2D c)
		{
               if(!Root.RemoveByPreviousBoundary(c))
				return false;

			Add(c);
			return true;
          }

		/// <summary>
		/// Clears this tree.
		/// </summary>
		/// <remarks>This does not reset the tree's boundary to its initial condition but retains whatever boundary it grew to.</remarks>
		public void Clear()
		{
			Root.Clear(); // We want to keep the root around since it has our boundary.
			return;
		}

		public IEnumerator<ICollider2D> GetEnumerator() => Root.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => TreeBoundary.ToString();

		/// <summary>
		/// The number of items in this tree.
		/// </summary>
		public int Count => Root.Count;

		/// <summary>
		/// If true, the this tree is empty.
		/// Otherwise, this tree has at least one item.
		/// </summary>
		public bool IsEmpty => Root.IsEmpty;

		/// <summary>
		/// If true, then this tree is not empty.
		/// If false, it is.
		/// </summary>
		public bool IsNotEmpty => !IsEmpty;

		/// <summary>
		/// The current global boundary of this tree.
		/// </summary>
		public FRectangle TreeBoundary => Root.Bounds;

		/// <summary>
		/// The root quad of this tree.
		/// </summary>
		protected Quad Root
		{get; set;}
	}


	/// <summary>
	/// A quad of a quadtree.
	/// </summary>
	public class Quad : IEnumerable<ICollider2D>
	{
		/// <summary>
		/// Creates an empty root quad.
		/// </summary>
		/// <param name="left">The left bound of the quad. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The right bound of the quad. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The bottom bound of the quad. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The top bound of the quad. This should be strictly greater than <paramref name="bottom"/>.</param>
		public Quad(float left, float right, float bottom, float top) : this(new FRectangle(left,bottom,right - left,top - bottom),null)
		{return;}

		/// <summary>
		/// Creates an empty quad.
		/// </summary>
		/// <param name="left">The left bound of the quad. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The right bound of the quad. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The bottom bound of the quad. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The top bound of the quad. This should be strictly greater than <paramref name="bottom"/>.</param>
		/// <param name="parent">The parent of the quad.</param>
		protected Quad(float left, float right, float bottom, float top, Quad parent) : this(new FRectangle(left,bottom,right - left,top - bottom),parent)
		{return;}

		/// <summary>
		/// Creates an empty root quad.
		/// </summary>
		/// <param name="bounds">The boundary of the quad.</param>
		public Quad(FRectangle bounds) : this(bounds,null)
		{return;}

		/// <summary>
		/// Creates an empty quad.
		/// </summary>
		/// <param name="bounds">The boundary of the quad.</param>
		/// <param name="parent">The parent of the quad. If this is null, then this will be a root quad.</param>
		protected Quad(FRectangle bounds, Quad? parent)
		{
			BigColliders = new AABBTree<ICollider2D,FRectangle>(c => c.Boundary,c => c.PreviousBoundary);
			
			SmallTopRightColliders = new CollisionLinkedList<ICollider2D>();
			SmallTopLeftColliders = new CollisionLinkedList<ICollider2D>();
			SmallBottomRightColliders = new CollisionLinkedList<ICollider2D>();
			SmallBottomLeftColliders = new CollisionLinkedList<ICollider2D>();

			Bounds = bounds;

			HalfX = bounds.Left + bounds.Width / 2.0f;
			HalfY = bounds.Bottom + bounds.Height / 2.0f;

			Parent = parent;
			
			TopRight = null;
			TopLeft = null;
			BottomRight = null;
			BottomLeft = null;

			ChildCount = 0;
			return;
		}

		/// <summary>
		/// Creates an empty root quad, already split, with one or more quads provided.
		/// </summary>
		/// <param name="q">The extant quad to build this larger quad around.</param>
		/// <param name="region">The region <paramref name="q"/> will belong it. This defines the geometry of this larger quad.</param>
		public Quad(Quad q, QuadRegion region) : this(new FRectangle(region == QuadRegion.TopRight || region == QuadRegion.BottomRight ? q.Bounds.Left - q.Bounds.Width : q.Bounds.Left,region == QuadRegion.TopRight || region == QuadRegion.TopLeft ? q.Bounds.Bottom - q.Bounds.Height : q.Bounds.Bottom,2.0f * q.Bounds.Width,2.0f * q.Bounds.Height),null)
		{
			TopRight = region == QuadRegion.TopRight ? q : null;
			TopLeft = region == QuadRegion.TopLeft ? q : null;
			BottomRight = region == QuadRegion.BottomRight ? q : null;
			BottomLeft = region == QuadRegion.BottomLeft ? q : null;

			ChildCount = q.Count;
			return;
		}

		/// <summary>
		/// Queries this quad to determine what colliders in it (or its children) collide with <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="c"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider2D> Query(ICollider2D c)
		{
			CollisionLinkedList<ICollider2D> ret = new CollisionLinkedList<ICollider2D>();

			// We always need to check the big colliders, since they can cover any arbitrary area of this quad
			foreach(ICollider2D c2 in BigColliders)
				if(c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2))
					ret.AddLast(c2);

			// Now we need to check all of the children that c overlaps with (or the appropriate small colliders lists if this is a leaf)
			// Top right
			if(c.RightBound >= HalfX && c.TopBound >= HalfY)
				if(IsTopRightLeaf)
					ret.AddAllLast(SmallTopRightColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopRight!.Query(c));
			
			// Top left
			if(c.LeftBound <= HalfX && c.TopBound >= HalfY)
				if(IsTopLeftLeaf)
					ret.AddAllLast(SmallTopLeftColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopLeft!.Query(c));

			// Bottom right
			if(c.RightBound >= HalfX && c.BottomBound <= HalfY)
				if(IsBottomRightLeaf)
					ret.AddAllLast(SmallBottomRightColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomRight!.Query(c));

			// Bottom left
			if(c.LeftBound <= HalfX && c.BottomBound <= HalfY)
				if(IsBottomLeftLeaf)
					ret.AddAllLast(SmallBottomLeftColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomLeft!.Query(c));

			return ret;
		}

		/// <summary>
		/// Queries this quad to determine what colliders in it (or its children) collide with <paramref name="region"/>.
		/// </summary>
		/// <param name="region">The region to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="region"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider2D> Query(FRectangle region)
		{
			CollisionLinkedList<ICollider2D> ret = new CollisionLinkedList<ICollider2D>();

			// We always need to check the big colliders, since they can cover any arbitrary area of this quad
			foreach(ICollider2D c2 in BigColliders)
				if(region.Intersects(c2.Boundary))
					ret.AddLast(c2);

			// Now we need to check all of the children that region overlaps with (or the appropriate small colliders lists if this is a leaf)
			// Top right
			if(region.Right >= HalfX && region.Top >= HalfY)
				if(IsTopRightLeaf)
					ret.AddAllLast(SmallTopRightColliders.Where(c => region.Intersects(c.Boundary)));
				else
					ret.AddAllLast(TopRight!.Query(region));
			
			// Top left
			if(region.Left <= HalfX && region.Top >= HalfY)
				if(IsTopLeftLeaf)
					ret.AddAllLast(SmallTopLeftColliders.Where(c => region.Intersects(c.Boundary)));
				else
					ret.AddAllLast(TopLeft!.Query(region));

			// Bottom right
			if(region.Right >= HalfX && region.Bottom <= HalfY)
				if(IsBottomRightLeaf)
					ret.AddAllLast(SmallBottomRightColliders.Where(c => region.Intersects(c.Boundary)));
				else
					ret.AddAllLast(BottomRight!.Query(region));

			// Bottom left
			if(region.Left <= HalfX && region.Bottom <= HalfY)
				if(IsBottomLeftLeaf)
					ret.AddAllLast(SmallBottomLeftColliders.Where(c => region.Intersects(c.Boundary)));
				else
					ret.AddAllLast(BottomLeft!.Query(region));

			return ret;
		}

		/// <summary>
		/// Adds a collider to this quad or one of its children.
		/// </summary>
		/// <param name="c">The collider to add. It is assumed that it can fit inside of this quad. If this is not the case, then this method's behavior is undefined.</param>
		public void Add(ICollider2D c)
		{
			// Now determine if we can fit in a child quad
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
				{
					if(IsBottomLeftLeaf)
					{
						SmallBottomLeftColliders.AddLast(c);

						if(SmallBottomLeftColliders.Count > MaxSmallQuadSizeBeforeSplit)
							BottomLeft = Split(new FRectangle(Bounds.X,Bounds.Y,Bounds.Width / 2.0f,Bounds.Height / 2.0f),SmallBottomLeftColliders);
					}
					else
					{
						BottomLeft!.Add(c);
						ChildCount++;
					}

					return;
				}
				else if(c.BottomBound >= HalfY) // Top left
				{
					if(IsTopLeftLeaf)
					{
						SmallTopLeftColliders.AddLast(c);

						if(SmallTopLeftColliders.Count > MaxSmallQuadSizeBeforeSplit)
							TopLeft = Split(new FRectangle(Bounds.X,HalfY,Bounds.Width / 2.0f,Bounds.Height / 2.0f),SmallTopLeftColliders);
					}
					else
					{
						TopLeft!.Add(c);
						ChildCount++;
					}

					return;
				}
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
				{
					if(IsBottomRightLeaf)
					{
						SmallBottomRightColliders.AddLast(c);

						if(SmallBottomRightColliders.Count > MaxSmallQuadSizeBeforeSplit)
							BottomRight = Split(new FRectangle(HalfX,Bounds.Y,Bounds.Width / 2.0f,Bounds.Height / 2.0f),SmallBottomRightColliders);
					}
					else
					{
						BottomRight!.Add(c);
						ChildCount++;
					}
					
					return;
				}
				else if(c.BottomBound >= HalfY) // Top right
				{
					if(IsTopRightLeaf)
					{
						SmallTopRightColliders.AddLast(c);

						if(SmallTopRightColliders.Count > MaxSmallQuadSizeBeforeSplit)
							TopRight = Split(new FRectangle(HalfX,HalfY,Bounds.Width / 2.0f,Bounds.Height / 2.0f),SmallTopRightColliders);
					}
					else
					{
						TopRight!.Add(c);
						ChildCount++;
					}

					return;
				}
			}

			// Since we haven't returned yet, we can't be in a smaller quad, so store it here and be done
			BigColliders.Add(c);

			return;
		}

		/// <summary>
		/// Removes the collider <paramref name="c"/> from this quad (or its children).
		/// </summary>
		/// <param name="c">The collider to remove.</param>
		/// <returns>Returns true if <paramref name="c"/> was removed and false if it was not.</returns>
		public bool Remove(ICollider2D c)
		{
			// Check if c belongs to a smaller quad
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
				{
					bool ret;
					
					if(IsBottomLeftLeaf)
						ret = SmallBottomLeftColliders.Remove(c);
					else
					{
						ret = BottomLeft!.Remove(c);

						if(ret)
						{
							ChildCount--;

							if(BottomLeft.IsEmpty)
								BottomLeft = null;
						}
					}

					return ret;
				}
				else if(c.BottomBound >= HalfY) // Top left
				{
					bool ret;
					
					if(IsTopLeftLeaf)
						ret = SmallTopLeftColliders.Remove(c);
					else
					{
						ret = TopLeft!.Remove(c);

						if(ret)
						{
							ChildCount--;

							if(TopLeft.IsEmpty)
								TopLeft = null;
						}
					}

					return ret;
				}
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
				{
					bool ret;
					
					if(IsBottomRightLeaf)
						ret = SmallBottomRightColliders.Remove(c);
					else
					{
						ret = BottomRight!.Remove(c);

						if(ret)
						{
							ChildCount--;

							if(BottomRight.IsEmpty)
								BottomRight = null;
						}
					}

					return ret;
				}
				else if(c.BottomBound >= HalfY) // Top right
				{
					bool ret;
					
					if(IsTopRightLeaf)
						ret = SmallTopRightColliders.Remove(c);
					else
					{
						ret = TopRight!.Remove(c);

						if(ret)
						{
							ChildCount--;

							if(TopRight.IsEmpty)
								TopRight = null;
						}
					}

					return ret;
				}
			}

			// Since we haven't returned yet, c must belong to this quad, so look for it here
			return BigColliders.Remove(c);
		}

		/// <summary>
		/// Removes <paramref name="c"/> from this quad.
		/// If <paramref name="c"/> does not belong to this quad, then this does nothing.
		/// </summary>
		/// <param name="c">The collider to remove.</param>
		/// <returns>Returns true if <paramref name="c"/> was removed and false otherwise.</returns>
		public bool RemoveByPreviousBoundary(ICollider2D c)
		{
			// Check if c belongs to a smaller quad
			if(c.PreviousRightBound <= HalfX)
			{
				if(c.PreviousTopBound <= HalfY) // Bottom left
				{
					bool ret;
					
					if(IsBottomLeftLeaf)
						ret = SmallBottomLeftColliders.Remove(c);
					else
					{
						ret = BottomLeft!.RemoveByPreviousBoundary(c);

						if(ret)
						{
							ChildCount--;

							if(BottomLeft.IsEmpty)
								BottomLeft = null;
						}
					}

					return ret;
				}
				else if(c.PreviousBottomBound >= HalfY) // Top left
				{
					bool ret;
					
					if(IsTopLeftLeaf)
						ret = SmallTopLeftColliders.Remove(c);
					else
					{
						ret = TopLeft!.RemoveByPreviousBoundary(c);

						if(ret)
						{
							ChildCount--;

							if(TopLeft.IsEmpty)
								TopLeft = null;
						}
					}

					return ret;
				}
			}
			else if(c.PreviousLeftBound >= HalfX)
			{
				if(c.PreviousTopBound <= HalfY) // Bottom right
				{
					bool ret;
					
					if(IsBottomRightLeaf)
						ret = SmallBottomRightColliders.Remove(c);
					else
					{
						ret = BottomRight!.RemoveByPreviousBoundary(c);

						if(ret)
						{
							ChildCount--;

							if(BottomRight.IsEmpty)
								BottomRight = null;
						}
					}

					return ret;
				}
				else if(c.PreviousBottomBound >= HalfY) // Top right
				{
					bool ret;
					
					if(IsTopRightLeaf)
						ret = SmallTopRightColliders.Remove(c);
					else
					{
						ret = TopRight!.RemoveByPreviousBoundary(c);

						if(ret)
						{
							ChildCount--;

							if(TopRight.IsEmpty)
								TopRight = null;
						}
					}

					return ret;
				}
			}

			// Since we haven't returned yet, c must belong to this quad, so look for it here
			return BigColliders.RemoveByPreviousBoundary(c);
		}

		/// <summary>
		/// Determines if this quad (or its children) contains <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to look for.</param>
		/// <returns>Returns true if this quad (or its children) contains <paramref name="c"/>.</returns>
		public bool Contains(ICollider2D c)
		{
			// Check if c belongs to a smaller quad
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
					return IsBottomLeftLeaf ? SmallBottomLeftColliders.Contains(c) : BottomLeft!.Contains(c);
				else if(c.BottomBound >= HalfY) // Top left
					return IsTopLeftLeaf ? SmallTopLeftColliders.Contains(c) : TopLeft!.Contains(c);
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
					return IsBottomRightLeaf ? SmallBottomRightColliders.Contains(c) : BottomRight!.Contains(c);
				else if(c.BottomBound >= HalfY) // Top right
					return IsTopRightLeaf ? SmallTopRightColliders.Contains(c) : TopRight!.Contains(c);
			}

			// Since we haven't returned yet, c must belong to this quad, so look for it here
			return BigColliders.Contains(c);
		}

		/// <summary>
		/// Creates a new quad containing <paramref name="colliders"/> with the boundary given by <paramref name="bounds"/>.
		/// </summary>
		/// <param name="bounds">The boundary of the new quad.</param>
		/// <param name="colliders">The colliders to place into the quad.</param>
		/// <returns>Returns the quad created.</returns>
		protected Quad Split(FRectangle bounds, CollisionLinkedList<ICollider2D> colliders)
		{
			Quad ret = new Quad(bounds,this);

			foreach(ICollider2D c in colliders)
				ret.Add(c);

			ChildCount += colliders.Count;
			colliders.Clear();

			return ret;
		}

		/// <summary>
		/// Clears this quad and its children.
		/// </summary>
		/// <param name="contract">If true, then empty children will be deleted.</param>
		public void Clear(bool contract = true)
		{
			BigColliders.Clear();

			if(IsTopRightLeaf)
				SmallTopRightColliders.Clear();
			else
				if(contract)
					TopRight = null;
				else
					TopRight!.Clear();

			if(IsTopLeftLeaf)
				SmallTopLeftColliders.Clear();
			else
				if(contract)
					TopLeft = null;
				else
					TopLeft!.Clear();

			if(IsBottomRightLeaf)
				SmallBottomRightColliders.Clear();
			else
				if(contract)
					BottomRight = null;
				else
					BottomRight!.Clear();

			if(IsBottomLeftLeaf)
				SmallBottomLeftColliders.Clear();
			else
				if(contract)
					BottomLeft = null;
				else
					BottomLeft!.Clear();

			ChildCount = 0;
			return;
		}

		public IEnumerator<ICollider2D> GetEnumerator()
		{
			IEnumerable<ICollider2D> ret = BigColliders;

			if(IsTopRightLeaf)
				ret = ret.Concat(SmallTopRightColliders);
			else
				ret = ret.Concat(TopRight!);

			if(IsTopLeftLeaf)
				ret = ret.Concat(SmallTopLeftColliders);
			else
				ret = ret.Concat(TopLeft!);

			if(IsBottomRightLeaf)
				ret = ret.Concat(SmallBottomRightColliders);
			else
				ret = ret.Concat(BottomRight!);

			if(IsBottomLeftLeaf)
				ret = ret.Concat(SmallBottomLeftColliders);
			else
				ret = ret.Concat(BottomLeft!);

			return ret.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => "{Bounds: " + Bounds.ToString() + " IsLeaf: " + IsLeaf + "}";

		/// <summary>
		/// Determines (quickly) if this quad is empty.
		/// </summary>
		public bool IsEmpty => BigColliders.IsEmpty && IsLeaf && SmallTopRightColliders.IsEmpty && SmallTopLeftColliders.IsEmpty && SmallBottomRightColliders.IsEmpty && SmallBottomLeftColliders.IsEmpty;

		/// <summary>
		/// Determines (quickly) if this quad is not empty.
		/// </summary>
		public bool IsNotEmpty => !IsEmpty;

		/// <summary>
		/// The total number of colliders stored in this quad (or its children).
		/// </summary>
		public int Count => InternalCount + ChildCount;

		/// <summary>
		/// The total number of colliders stored in this quad (but not its children).
		/// </summary>
		public int InternalCount => BigCount + SmallCount;

		/// <summary>
		/// The total number of colliders stored in this quad that cannot go into a smaller quad.
		/// </summary>
		public int BigCount => BigColliders.Count;

		/// <summary>
		/// The total number of colliders stored in this quad that would go into a smaller quad were it to split.
		/// </summary>
		public int SmallCount => SmallTopRightColliders.Count + SmallTopLeftColliders.Count + SmallBottomRightColliders.Count + SmallBottomLeftColliders.Count;

		/// <summary>
		/// The number of colliders in this quad's children.
		/// </summary>
		public int ChildCount
		{get; protected set;}

		/// <summary>
		/// The colliders belonging to this quad that cannot fit in any of its children.
		/// </summary>
		/// <remarks>Typically, quads are very large relative to the size of objects, so there is likely little overlap between objects that straddle the boundary, so an AABB tree will actually be more useful than a slow linked list.</remarks>
		protected AABBTree<ICollider2D,FRectangle> BigColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top right quad if only this were not a leaf quad.
		/// If this is not a leaf quad, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider2D> SmallTopRightColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top left quad if only this were not a leaf quad.
		/// If this is not a leaf quad, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider2D> SmallTopLeftColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bottom right quad if only this were not a leaf quad.
		/// If this is not a leaf quad, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider2D> SmallBottomRightColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bototm left quad if only this were not a leaf quad.
		/// If this is not a leaf quad, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider2D> SmallBottomLeftColliders
		{get; set;}

		/// <summary>
		/// The boundary of this quad.
		/// </summary>
		public FRectangle Bounds
		{get;}

		/// <summary>
		/// The half x position dividing the quad in half.
		/// </summary>
		public float HalfX
		{get;}

		/// <summary>
		/// The half y position dividing the quad in half.
		/// </summary>
		public float HalfY
		{get;}

		/// <summary>
		/// The parent quad of this quad.
		/// If this is the root quad, then this value is null.
		/// </summary>
		public Quad? Parent
		{get; protected set;}

		/// <summary>
		/// The top right child quad of this quad.
		/// If this is a leaf quad, then this value is null.
		/// </summary>
		public Quad? TopRight
		{get; protected set;}

		/// <summary>
		/// The top left child quad of this quad.
		/// If this is a leaf quad, then this value is null.
		/// </summary>
		public Quad? TopLeft
		{get; protected set;}

		/// <summary>
		/// The bottom right child quad of this quad.
		/// If this is a leaf quad, then this value is null.
		/// </summary>
		public Quad? BottomRight
		{get; protected set;}

		/// <summary>
		/// The bottom left child quad of this quad.
		/// If this is a leaf quad, then this value is null.
		/// </summary>
		public Quad? BottomLeft
		{get; protected set;}

		/// <summary>
		/// If true, then this is a root quad.
		/// If false, then it is not.
		/// </summary>
		public bool IsRoot => Parent is null;

		/// <summary>
		/// If true, then this is not a root or leaf quad.
		/// If false, then this ia a root or a leaf.
		/// </summary>
		public bool IsInternal => !IsRoot && !IsLeaf;

		/// <summary>
		/// If true, then this is a terminal leaf quad.
		/// If false, then this is not.
		/// </summary>
		public bool IsLeaf => IsTopRightLeaf && IsTopLeftLeaf && IsBottomRightLeaf && IsBottomLeftLeaf;

		/// <summary>
		/// If true, then this is a terminal leaf quad as far as its top right quad is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopRightLeaf => TopRight is null;

		/// <summary>
		/// If true, then this is a terminal leaf quad as far as its top left quad is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopLeftLeaf => TopLeft is null;

		/// <summary>
		/// If true, then this is a terminal leaf quad as far as its bottom right quad is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomRightLeaf => BottomRight is null;

		/// <summary>
		/// If true, then this is a terminal leaf quad as far as its bottom left quad is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomLeftLeaf => BottomLeft is null;

		/// <summary>
		/// This is the maximum number of colliders allowed to be stored in a smaller quad list in a leaf before the quad splits into four smaller quads.
		/// </summary>
		public const int MaxSmallQuadSizeBeforeSplit = 4;
	}

	/// <summary>
	/// Represents what region a quad lies in.
	/// </summary>
	public enum QuadRegion
	{
		TopRight,
		TopLeft,
		BottomRight,
		BottomLeft,
		Root
	}
}
