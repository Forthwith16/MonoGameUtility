using GameEngine.DataStructures.Geometry;
using GameEngine.Physics.Collision.Colliders;
using System.Collections;

namespace GameEngine.Physics.Collision
{
	/// <summary>
	/// An octree.
	/// </summary>
	public class Octree : IEnumerable<ICollider3D>
	{
		/// <summary>
		/// Creates a new quad tree.
		/// </summary>
		/// <param name="left">The initial left bound of the octree. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The initial right bound of the octree. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The initial bottom bound of the octree. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The initial top bound of the octree. This should be strictly greater than <paramref name="bottom"/>.</param>
		/// <param name="near">The initial near bound of the octree. This should be strictly less than <paramref name="far"/>.</param>
		/// <param name="far">The initial far bound of the octree. This should be strictly greater than <paramref name="near"/>.</param>
		public Octree(float left, float right, float bottom, float top, float near, float far) : this(new FPrism(left,bottom,near,right - left,top - bottom,far - near))
		{return;}

		/// <summary>
		/// Creates a new octree.
		/// </summary>
		/// <param name="initial_boundary">The initial boundary of the octree.</param>
		public Octree(FPrism initial_boundary)
		{
			Root = new Oct(initial_boundary);
			return;
		}

		/// <summary>
		/// Queries this tree to determine what colliders in it collide with <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to check for collisions.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="c"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider3D> Query(ICollider3D c)
		{
			if(!TreeBoundary.Intersects(c.Boundary))
				return Enumerable.Empty<ICollider3D>();

			return Root.Query(c);
		}

		/// <summary>
		/// Queries this tree to determine what colliders in it collide with <paramref name="region"/>.
		/// </summary>
		/// <param name="region">The region to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="region"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider3D> Query(FPrism region)
		{
			if(!TreeBoundary.Intersects(region))
				return Enumerable.Empty<ICollider3D>();

			return Root.Query(region);
		}

		/// <summary>
		/// Adds <paramref name="c"/> to this tree.
		/// If <paramref name="c"/> is out of bounds, then the tree will grow to accommodate it.
		/// </summary>
		/// <param name="c">The collider to add.</param>
		public void Add(ICollider3D c)
		{
			// Compute once
			FPrism b = TreeBoundary;

			while(!b.Contains(c.Boundary))
			{
				// Determine which direction we need to grow to contain c
				// Our new collider can literally have our boundary but inflated, so we need to just pick a direction to grow in greedily
				if(c.LeftBound < b.Left)
					if(c.BottomBound < b.Bottom) // Too far left and too far down
						if(c.NearBound < b.Near) // Also too near
							Root = new Oct(Root,OctRegion.TopRightFar);
						else // We're either too far or just fine, in which case we just go with the same answer
							Root = new Oct(Root,OctRegion.TopRightNear);
					else // Too far left and too far up or we're too far left but within the vertical bounds, so just pick a direction to grow the vertical bound
						if(c.NearBound < b.Near) // Also too near
							Root = new Oct(Root,OctRegion.BottomRightFar);
						else // We're either too far or just fine, in which case we just go with the same answer
							Root = new Oct(Root,OctRegion.BottomRightNear);
				else  // Too far right or within reason for the horizontal bounds, in which case just pick a direction to grow the horizontal bound
					if(c.BottomBound < b.Bottom) // Too far right and too far down
						if(c.NearBound < b.Near) // Also too near
							Root = new Oct(Root,OctRegion.TopLeftFar);
						else // We're either too far or just fine, in which case we just go with the same answer
							Root = new Oct(Root,OctRegion.TopLeftNear);
					else // Too far right and too far up or we're too far right but within the vertical bounds, so just pick a direction to grow the vertical bound
						if(c.NearBound < b.Near) // Also too near
							Root = new Oct(Root,OctRegion.BottomLeftFar);
						else // We're either too far or just fine, in which case we just go with the same answer
							Root = new Oct(Root,OctRegion.BottomLeftNear);

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
		public bool Remove(ICollider3D c) => IsEmpty ? false : Root.Remove(c);

		/// <summary>
		/// Determines if this tree contains <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to look for.</param>
		/// <returns>Returns true if this tree contains <paramref name="c"/> and false otherwise.</returns>
		public bool Contains(ICollider3D c) => IsEmpty ? false : Root.Contains(c);

		/// <summary>
		/// Updates the boundary of <paramref name="c"/> in this tree.
		/// If <paramref name="c"/> does not belong to this tree, then this does nothing.
		/// </summary>
		/// <param name="c">The collider whose boundary needs updating.</param>
		/// <param name="new_boundary">The new boundary for <paramref name="c"/>.</param>
		/// <returns>Returns true if <paramref name="c"/>'s boundary was changed and false otherwise.</returns>
		/// <remarks><paramref name="c"/>'s membership is determined via the Remove method, which uses Query to locate <paramref name="c"/>.</remarks>
		public bool UpdateBoundary(ICollider3D c, FPrism new_boundary) => Root.UpdateBoundary(c,new_boundary);

		/// <summary>
		/// Clears this tree.
		/// </summary>
		/// <remarks>This does not reset the tree's boundary to its initial condition but retains whatever boundary it grew to.</remarks>
		public void Clear()
		{
			Root.Clear(); // We want to keep the root around since it has our boundary.
			return;
		}

		public IEnumerator<ICollider3D> GetEnumerator() => Root.GetEnumerator();
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
		public FPrism TreeBoundary => Root.Bounds;

		/// <summary>
		/// The root oct of this tree.
		/// </summary>
		protected Oct Root
		{get; set;}
	}

	/// <summary>
	/// An oct of an octree.
	/// </summary>
	public class Oct : IEnumerable<ICollider3D>
	{
		/// <summary>
		/// Creates an empty root oct.
		/// </summary>
		/// <param name="left">The left bound of the oct. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The right bound of the oct. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The bottom bound of the oct. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The top bound of the oct. This should be strictly greater than <paramref name="bottom"/>.</param>
		/// <param name="near">The near bound of the oct. This should be strictly less than <paramref name="far"/>.</param>
		/// <param name="far">The far bound of the oct. This should be strictly greater than <paramref name="near"/>.</param>
		public Oct(float left, float right, float bottom, float top, float near, float far) : this(new FPrism(left,bottom,near,right - left,top - bottom,far - near),null)
		{return;}

		/// <summary>
		/// Creates an empty quad.
		/// </summary>
		/// <param name="left">The left bound of the quad. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The right bound of the quad. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The bottom bound of the quad. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The top bound of the quad. This should be strictly greater than <paramref name="bottom"/>.</param>
		/// <param name="near">The near bound of the oct. This should be strictly less than <paramref name="far"/>.</param>
		/// <param name="far">The far bound of the oct. This should be strictly greater than <paramref name="near"/>.</param>
		/// <param name="parent">The parent of the oct.</param>
		protected Oct(float left, float right, float bottom, float top, float near, float far, Oct parent) : this(new FPrism(left,bottom,near,right - left,top - bottom,far - near),parent)
		{return;}

		/// <summary>
		/// Creates an empty root oct.
		/// </summary>
		/// <param name="bounds">The boundary of the oct.</param>
		public Oct(FPrism bounds) : this(bounds,null)
		{return;}

		/// <summary>
		/// Creates an empty oct.
		/// </summary>
		/// <param name="bounds">The boundary of the oct.</param>
		/// <param name="parent">The parent of the oct. If this is null, then this will be a root oct.</param>
		protected Oct(FPrism bounds, Oct? parent)
		{
			BigColliders = new AABBTree<ICollider3D,FPrism>(c => c.Boundary,(c,nb) => c.ChangeBoundary(nb));
			
			SmallTopRightNearColliders = new CollisionLinkedList<ICollider3D>();
			SmallTopLeftNearColliders = new CollisionLinkedList<ICollider3D>();
			SmallBottomRightNearColliders = new CollisionLinkedList<ICollider3D>();
			SmallBottomLeftNearColliders = new CollisionLinkedList<ICollider3D>();
			SmallTopRightFarColliders = new CollisionLinkedList<ICollider3D>();
			SmallTopLeftFarColliders = new CollisionLinkedList<ICollider3D>();
			SmallBottomRightFarColliders = new CollisionLinkedList<ICollider3D>();
			SmallBottomLeftFarColliders = new CollisionLinkedList<ICollider3D>();

			Bounds = bounds;

			HalfX = bounds.Left + bounds.Width / 2.0f;
			HalfY = bounds.Bottom + bounds.Height / 2.0f;
			HalfZ = bounds.Near + bounds.Depth / 2.0f;

			Parent = parent;

			TopRightNear = null;
			TopLeftNear = null;
			BottomRightNear = null;
			BottomLeftNear = null;
			TopRightFar = null;
			TopLeftFar = null;
			BottomRightFar = null;
			BottomLeftFar = null;

			ChildCount = 0;
			return;
		}

		/// <summary>
		/// Creates an empty root oct, already split, with one or more oct provided.
		/// </summary>
		/// <param name="o">The extant oct to build this larger oct around.</param>
		/// <param name="region">The region <paramref name="o"/> will belong it. This defines the geometry of this larger oct.</param>
		public Oct(Oct o, OctRegion region) : this(new FPrism((region & OctRegion.Right) == OctRegion.Right ? o.Bounds.Left - o.Bounds.Width : o.Bounds.Left,
												    (region & OctRegion.Top) == OctRegion.Top ? o.Bounds.Bottom - o.Bounds.Height : o.Bounds.Bottom,
												    (region & OctRegion.Far) == OctRegion.Far ? o.Bounds.Near - o.Bounds.Depth : o.Bounds.Near,
												    2.0f * o.Bounds.Width,2.0f * o.Bounds.Height,2.0f * o.Bounds.Depth),null)
		{
			TopRightNear = region == OctRegion.TopRightNear ? o : null;
			TopLeftNear = region == OctRegion.TopLeftNear ? o : null;
			BottomRightNear = region == OctRegion.BottomRightNear ? o : null;
			BottomLeftNear = region == OctRegion.BottomLeftNear ? o : null;
			TopRightFar = region == OctRegion.TopRightFar ? o : null;
			TopLeftFar = region == OctRegion.TopLeftFar ? o : null;
			BottomRightFar = region == OctRegion.BottomRightFar ? o : null;
			BottomLeftFar = region == OctRegion.BottomLeftFar ? o : null;

			ChildCount = o.Count;
			return;
		}

		/// <summary>
		/// Queries this oct to determine what colliders in it (or its children) collide with <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="c"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider3D> Query(ICollider3D c)
		{
			CollisionLinkedList<ICollider3D> ret = new CollisionLinkedList<ICollider3D>();

			// We always need to check the big colliders, since they can cover any arbitrary area of this oct
			foreach(ICollider3D c2 in BigColliders)
				if(c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2))
					ret.AddLast(c2);

			// Now we need to check all of the children that c overlaps with (or the appropriate small colliders lists if this is a leaf)
			bool left = c.LeftBound <= HalfX;
			bool right = c.RightBound >= HalfX;
			bool bottom = c.BottomBound <= HalfY;
			bool top = c.TopBound >= HalfY;
			bool near = c.NearBound <= HalfZ;
			bool far = c.FarBound >= HalfZ;

			// Top right far
			if(right && top && far)
				if(IsTopRightFarLeaf)
					ret.AddAllLast(SmallTopRightFarColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopRightFar!.Query(c));
			
			// Top right near
			if(right && top && near)
				if(IsTopRightNearLeaf)
					ret.AddAllLast(SmallTopRightNearColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopRightNear!.Query(c));
			
			// Top left far
			if(left && top && far)
				if(IsTopLeftFarLeaf)
					ret.AddAllLast(SmallTopLeftFarColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopLeftFar!.Query(c));
			
			// Top left near
			if(left && top && near)
				if(IsTopLeftNearLeaf)
					ret.AddAllLast(SmallTopLeftNearColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(TopLeftNear!.Query(c));

			// Bottom right far
			if(bottom && right && far)
				if(IsBottomRightFarLeaf)
					ret.AddAllLast(SmallBottomRightFarColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomRightFar!.Query(c));

			// Bottom right near
			if(bottom && right && near)
				if(IsBottomRightNearLeaf)
					ret.AddAllLast(SmallBottomRightNearColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomRightNear!.Query(c));

			// Bottom left far
			if(bottom && left && far)
				if(IsBottomLeftFarLeaf)
					ret.AddAllLast(SmallBottomLeftFarColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomLeftFar!.Query(c));

			// Bottom left near
			if(bottom && left && near)
				if(IsBottomLeftNearLeaf)
					ret.AddAllLast(SmallBottomLeftNearColliders.Where(c2 => c.Boundary.Intersects(c2.Boundary) && c.CollidesWith(c2)));
				else
					ret.AddAllLast(BottomLeftNear!.Query(c));

			return ret;
		}

		/// <summary>
		/// Queries this oct to determine what colliders in it (or its children) collide with <paramref name="region"/>.
		/// </summary>
		/// <param name="region">The region to check for possible collision.</param>
		/// <returns>Returns an enumeration of colliders that collide with <paramref name="region"/>. If there are none, then an empty list is returned.</returns>
		public IEnumerable<ICollider3D> Query(FPrism region)
		{
			CollisionLinkedList<ICollider3D> ret = new CollisionLinkedList<ICollider3D>();

			// We always need to check the big colliders, since they can cover any arbitrary area of this oct
			foreach(ICollider3D c2 in BigColliders)
				if(region.Intersects(c2.Boundary))
					ret.AddLast(c2);

			// Now we need to check all of the children that region overlaps with (or the appropriate small colliders lists if this is a leaf)
			bool left = region.Left <= HalfX;
			bool right = region.Right >= HalfX;
			bool bottom = region.Bottom <= HalfY;
			bool top = region.Top >= HalfY;
			bool near = region.Near <= HalfZ;
			bool far = region.Far >= HalfZ;

			// Top right far
			if(right && top && far)
				if(IsTopRightFarLeaf)
					ret.AddAllLast(SmallTopRightFarColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(TopRightFar!.Query(region));
			
			// Top right near
			if(right && top && near)
				if(IsTopRightNearLeaf)
					ret.AddAllLast(SmallTopRightNearColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(TopRightNear!.Query(region));
			
			// Top left far
			if(left && top && far)
				if(IsTopLeftFarLeaf)
					ret.AddAllLast(SmallTopLeftFarColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(TopLeftFar!.Query(region));
			
			// Top left near
			if(left && top && near)
				if(IsTopLeftNearLeaf)
					ret.AddAllLast(SmallTopLeftNearColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(TopLeftNear!.Query(region));

			// Bottom right far
			if(bottom && right && far)
				if(IsBottomRightFarLeaf)
					ret.AddAllLast(SmallBottomRightFarColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(BottomRightFar!.Query(region));

			// Bottom right near
			if(bottom && right && near)
				if(IsBottomRightNearLeaf)
					ret.AddAllLast(SmallBottomRightNearColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(BottomRightNear!.Query(region));

			// Bottom left far
			if(bottom && left && far)
				if(IsBottomLeftFarLeaf)
					ret.AddAllLast(SmallBottomLeftFarColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(BottomLeftFar!.Query(region));

			// Bottom left near
			if(bottom && left && near)
				if(IsBottomLeftNearLeaf)
					ret.AddAllLast(SmallBottomLeftNearColliders.Where(c2 => region.Intersects(c2.Boundary)));
				else
					ret.AddAllLast(BottomLeftNear!.Query(region));

			return ret;
		}

		/// <summary>
		/// Adds a collider to this oct or one of its children.
		/// </summary>
		/// <param name="c">The collider to add. It is assumed that it can fit inside of this oct. If this is not the case, then this method's behavior is undefined.</param>
		public void Add(ICollider3D c)
		{
			// Now determine if we can fit in a child oct
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
				{
					if(c.FarBound <= HalfZ) // Bottom left near
					{
						if(IsBottomLeftNearLeaf)
						{
							SmallBottomLeftNearColliders.AddLast(c);

							if(SmallBottomLeftNearColliders.Count > MaxSmallOctSizeBeforeSplit)
								BottomLeftNear = Split(new FPrism(Bounds.X,Bounds.Y,Bounds.Z,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallBottomLeftNearColliders);
						}
						else
						{
							BottomLeftNear!.Add(c);
							ChildCount++;
						}

						return;
					}
					else if(c.NearBound >= HalfZ) // Bottom left far
					{
						if(IsBottomLeftFarLeaf)
						{
							SmallBottomLeftFarColliders.AddLast(c);

							if(SmallBottomLeftFarColliders.Count > MaxSmallOctSizeBeforeSplit)
								BottomLeftFar = Split(new FPrism(Bounds.X,Bounds.Y,HalfZ,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallBottomLeftFarColliders);
						}
						else
						{
							BottomLeftFar!.Add(c);
							ChildCount++;
						}

						return;
					}
				}
				else if(c.BottomBound >= HalfY) // Top left
				{
					if(c.FarBound <= HalfZ) // Top left near
					{
						if(IsBottomLeftNearLeaf)
						{
							SmallTopLeftNearColliders.AddLast(c);

							if(SmallTopLeftNearColliders.Count > MaxSmallOctSizeBeforeSplit)
								TopLeftNear = Split(new FPrism(Bounds.X,Bounds.Y,Bounds.Z,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallTopLeftNearColliders);
						}
						else
						{
							TopLeftNear!.Add(c);
							ChildCount++;
						}

						return;
					}
					else if(c.NearBound >= HalfZ) // Top left far
					{
						if(IsTopLeftFarLeaf)
						{
							SmallTopLeftFarColliders.AddLast(c);

							if(SmallTopLeftFarColliders.Count > MaxSmallOctSizeBeforeSplit)
								TopLeftFar = Split(new FPrism(Bounds.X,Bounds.Y,HalfZ,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallTopLeftFarColliders);
						}
						else
						{
							TopLeftFar!.Add(c);
							ChildCount++;
						}

						return;
					}
				}
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
				{
					if(c.FarBound <= HalfZ) // Bottom right near
					{
						if(IsBottomRightNearLeaf)
						{
							SmallBottomRightNearColliders.AddLast(c);

							if(SmallBottomRightNearColliders.Count > MaxSmallOctSizeBeforeSplit)
								BottomRightNear = Split(new FPrism(Bounds.X,Bounds.Y,Bounds.Z,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallBottomRightNearColliders);
						}
						else
						{
							BottomRightNear!.Add(c);
							ChildCount++;
						}

						return;
					}
					else if(c.NearBound >= HalfZ) // Bottom right far
					{
						if(IsBottomRightFarLeaf)
						{
							SmallBottomRightFarColliders.AddLast(c);

							if(SmallBottomRightFarColliders.Count > MaxSmallOctSizeBeforeSplit)
								BottomRightFar = Split(new FPrism(Bounds.X,Bounds.Y,HalfZ,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallBottomRightFarColliders);
						}
						else
						{
							BottomRightFar!.Add(c);
							ChildCount++;
						}

						return;
					}
				}
				else if(c.BottomBound >= HalfY) // Top right
				{
					if(c.FarBound <= HalfZ) // Top right near
					{
						if(IsTopRightNearLeaf)
						{
							SmallTopRightNearColliders.AddLast(c);

							if(SmallTopRightNearColliders.Count > MaxSmallOctSizeBeforeSplit)
								TopRightNear = Split(new FPrism(Bounds.X,Bounds.Y,Bounds.Z,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallTopRightNearColliders);
						}
						else
						{
							TopRightNear!.Add(c);
							ChildCount++;
						}

						return;
					}
					else if(c.NearBound >= HalfZ) // Top right far
					{
						if(IsTopRightFarLeaf)
						{
							SmallTopRightFarColliders.AddLast(c);

							if(SmallTopRightFarColliders.Count > MaxSmallOctSizeBeforeSplit)
								TopRightFar = Split(new FPrism(Bounds.X,Bounds.Y,HalfZ,Bounds.Width / 2.0f,Bounds.Height / 2.0f,Bounds.Depth / 2.0f),SmallTopRightFarColliders);
						}
						else
						{
							TopRightFar!.Add(c);
							ChildCount++;
						}

						return;
					}
				}
			}

			// Since we haven't returned yet, we can't be in a smaller oct, so store it here and be done
			BigColliders.Add(c);

			return;
		}

		/// <summary>
		/// Removes the collider <paramref name="c"/> from this oct (or its children).
		/// </summary>
		/// <param name="c">The collider to remove.</param>
		/// <returns>Returns true if <paramref name="c"/> was removed and false if it was not.</returns>
		public bool Remove(ICollider3D c)
		{
			bool ret;
			
			// Check if c belongs to a smaller oct
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
				{
					if(c.FarBound <= HalfZ) // Bottom left near
					{
						if(IsBottomLeftNearLeaf)
							ret = SmallBottomLeftNearColliders.Remove(c);
						else
						{
							ret = BottomLeftNear!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(BottomLeftNear.IsEmpty)
									BottomLeftNear = null;
							}
						}

						return ret;
					}
					else if(c.NearBound >= HalfZ) // Bottom left far
					{
						if(IsBottomLeftFarLeaf)
							ret = SmallBottomLeftFarColliders.Remove(c);
						else
						{
							ret = BottomLeftFar!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(BottomLeftFar.IsEmpty)
									BottomLeftFar = null;
							}
						}

						return ret;
					}
				}
				else if(c.BottomBound >= HalfY) // Top left
				{
					if(c.FarBound <= HalfZ) // Top left near
					{
						if(IsTopLeftNearLeaf)
							ret = SmallTopLeftNearColliders.Remove(c);
						else
						{
							ret = TopLeftNear!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(TopLeftNear.IsEmpty)
									TopLeftNear = null;
							}
						}

						return ret;
					}
					else if(c.NearBound >= HalfZ) // Top left far
					{
						if(IsTopLeftFarLeaf)
							ret = SmallTopLeftFarColliders.Remove(c);
						else
						{
							ret = TopLeftFar!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(TopLeftFar.IsEmpty)
									TopLeftFar = null;
							}
						}

						return ret;
					}
				}
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
				{
					if(c.FarBound <= HalfZ) // Bottom right near
					{
						if(IsBottomRightNearLeaf)
							ret = SmallBottomRightNearColliders.Remove(c);
						else
						{
							ret = BottomRightNear!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(BottomRightNear.IsEmpty)
									BottomRightNear = null;
							}
						}

						return ret;
					}
					else if(c.NearBound >= HalfZ) // Bottom right far
					{
						if(IsBottomRightFarLeaf)
							ret = SmallBottomRightFarColliders.Remove(c);
						else
						{
							ret = BottomRightFar!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(BottomRightFar.IsEmpty)
									BottomRightFar = null;
							}
						}

						return ret;
					}
				}
				else if(c.BottomBound >= HalfY) // Top right
				{
					if(c.FarBound <= HalfZ) // Top right near
					{
						if(IsTopRightNearLeaf)
							ret = SmallTopRightNearColliders.Remove(c);
						else
						{
							ret = TopRightNear!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(TopRightNear.IsEmpty)
									TopRightNear = null;
							}
						}

						return ret;
					}
					else if(c.NearBound >= HalfZ) // Top right far
					{
						if(IsTopRightFarLeaf)
							ret = SmallTopRightFarColliders.Remove(c);
						else
						{
							ret = TopRightFar!.Remove(c);

							if(ret)
							{
								ChildCount--;

								if(TopRightFar.IsEmpty)
									TopRightFar = null;
							}
						}

						return ret;
					}
				}
			}

			// Since we haven't returned yet, c must belong to this oct, so look for it here
			return BigColliders.Remove(c);
		}

		/// <summary>
		/// Determines if this oct (or its children) contains <paramref name="c"/>.
		/// </summary>
		/// <param name="c">The collider to look for.</param>
		/// <returns>Returns true if this oct (or its children) contains <paramref name="c"/>.</returns>
		public bool Contains(ICollider3D c)
		{
			// Check if c belongs to a smaller oct
			if(c.RightBound <= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom left
				{
					if(c.FarBound <= HalfZ) // Bottom left near
						return IsBottomLeftNearLeaf ? SmallBottomLeftNearColliders.Contains(c) : BottomLeftNear!.Contains(c);
					else if(c.NearBound >= HalfZ) // Bottom left far
						return IsBottomLeftFarLeaf ? SmallBottomLeftFarColliders.Contains(c) : BottomLeftFar!.Contains(c);
				}
				else if(c.BottomBound >= HalfY) // Top left
				{
					if(c.FarBound <= HalfZ) // Top left near
						return IsTopLeftNearLeaf ? SmallTopLeftNearColliders.Contains(c) : TopLeftNear!.Contains(c);
					else if(c.NearBound >= HalfZ) // Top left far
						return IsTopLeftFarLeaf ? SmallTopLeftFarColliders.Contains(c) : TopLeftFar!.Contains(c);
				}
			}
			else if(c.LeftBound >= HalfX)
			{
				if(c.TopBound <= HalfY) // Bottom right
				{
					if(c.FarBound <= HalfZ) // Bottom right near
						return IsBottomRightNearLeaf ? SmallBottomRightNearColliders.Contains(c) : BottomRightNear!.Contains(c);
					else if(c.NearBound >= HalfZ) // Bottom right far
						return IsBottomRightFarLeaf ? SmallBottomRightFarColliders.Contains(c) : BottomRightFar!.Contains(c);
				}
				else if(c.BottomBound >= HalfY) // Top right
				{
					if(c.FarBound <= HalfZ) // Top right near
						return IsTopRightNearLeaf ? SmallTopRightNearColliders.Contains(c) : TopRightNear!.Contains(c);
					else if(c.NearBound >= HalfZ) // Top right far
						return IsTopRightFarLeaf ? SmallTopRightFarColliders.Contains(c) : TopRightFar!.Contains(c);
				}
			}

			// Since we haven't returned yet, c must belong to this oct, so look for it here
			return BigColliders.Contains(c);
		}

		/// <summary>
		/// Updates the boundary of <paramref name="c"/> in this oct.
		/// If <paramref name="c"/> does not belong to this oct, then this does nothing.
		/// </summary>
		/// <param name="c">The collider whose boundary needs updating.</param>
		/// <param name="new_boundary">The new boundary for <paramref name="c"/>.</param>
		/// <returns>Returns true if <paramref name="c"/>'s boundary was changed and false otherwise.</returns>
		/// <remarks><paramref name="c"/>'s membership is determined via the Remove method, which uses Query to locate <paramref name="c"/>.</remarks>
		public bool UpdateBoundary(ICollider3D c, FPrism new_boundary)
		{
			// If we fail to remove c, then we don't have c and can't update it
			if(!Remove(c))
				return false;

			// Now we need to update c
			bool ret = c.ChangeBoundary(new_boundary);

			// And lastly, we add c back in (regardless of if its boundary was successfully changed)
			Add(c);

			return ret;
		}

		/// <summary>
		/// Creates a new oct containing <paramref name="colliders"/> with the boundary given by <paramref name="bounds"/>.
		/// </summary>
		/// <param name="bounds">The boundary of the new oct.</param>
		/// <param name="colliders">The colliders to place into the oct.</param>
		/// <returns>Returns the oct created.</returns>
		protected Oct Split(FPrism bounds, CollisionLinkedList<ICollider3D> colliders)
		{
			Oct ret = new Oct(bounds,this);

			foreach(ICollider3D c in colliders)
				ret.Add(c);

			ChildCount += colliders.Count;
			colliders.Clear();

			return ret;
		}

		/// <summary>
		/// Clears this oct and its children.
		/// </summary>
		/// <param name="contract">If true, then empty children will be deleted.</param>
		public void Clear(bool contract = true)
		{
			BigColliders.Clear();

			if(IsTopRightNearLeaf)
				SmallTopRightNearColliders.Clear();
			else
				if(contract)
					TopRightNear = null;
				else
					TopRightNear!.Clear();

			if(IsTopLeftNearLeaf)
				SmallTopLeftNearColliders.Clear();
			else
				if(contract)
					TopLeftNear = null;
				else
					TopLeftNear!.Clear();

			if(IsBottomRightNearLeaf)
				SmallBottomRightNearColliders.Clear();
			else
				if(contract)
					BottomRightNear = null;
				else
					BottomRightNear!.Clear();

			if(IsBottomLeftNearLeaf)
				SmallBottomLeftNearColliders.Clear();
			else
				if(contract)
					BottomLeftNear = null;
				else
					BottomLeftNear!.Clear();

			if(IsTopRightFarLeaf)
				SmallTopRightFarColliders.Clear();
			else
				if(contract)
					TopRightFar = null;
				else
					TopRightFar!.Clear();

			if(IsTopLeftFarLeaf)
				SmallTopLeftFarColliders.Clear();
			else
				if(contract)
					TopLeftFar = null;
				else
					TopLeftFar!.Clear();

			if(IsBottomRightFarLeaf)
				SmallBottomRightFarColliders.Clear();
			else
				if(contract)
					BottomRightFar = null;
				else
					BottomRightFar!.Clear();

			if(IsBottomLeftFarLeaf)
				SmallBottomLeftFarColliders.Clear();
			else
				if(contract)
					BottomLeftFar = null;
				else
					BottomLeftFar!.Clear();

			ChildCount = 0;
			return;
		}

		public IEnumerator<ICollider3D> GetEnumerator()
		{
			IEnumerable<ICollider3D> ret = BigColliders;

			if(IsTopRightNearLeaf)
				ret = ret.Concat(SmallTopRightNearColliders);
			else
				ret = ret.Concat(TopRightNear!);

			if(IsTopLeftNearLeaf)
				ret = ret.Concat(SmallTopLeftNearColliders);
			else
				ret = ret.Concat(TopLeftNear!);

			if(IsBottomRightNearLeaf)
				ret = ret.Concat(SmallBottomRightNearColliders);
			else
				ret = ret.Concat(BottomRightNear!);

			if(IsBottomLeftNearLeaf)
				ret = ret.Concat(SmallBottomLeftNearColliders);
			else
				ret = ret.Concat(BottomLeftNear!);

			if(IsTopRightFarLeaf)
				ret = ret.Concat(SmallTopRightFarColliders);
			else
				ret = ret.Concat(TopRightFar!);

			if(IsTopLeftFarLeaf)
				ret = ret.Concat(SmallTopLeftFarColliders);
			else
				ret = ret.Concat(TopLeftFar!);

			if(IsBottomRightFarLeaf)
				ret = ret.Concat(SmallBottomRightFarColliders);
			else
				ret = ret.Concat(BottomRightFar!);

			if(IsBottomLeftFarLeaf)
				ret = ret.Concat(SmallBottomLeftFarColliders);
			else
				ret = ret.Concat(BottomLeftFar!);

			return ret.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => "{Bounds: " + Bounds.ToString() + " IsLeaf: " + IsLeaf + "}";

		/// <summary>
		/// Determines (quickly) if this oct is empty.
		/// </summary>
		public bool IsEmpty => BigColliders.IsEmpty && IsLeaf && SmallTopRightNearColliders.IsEmpty && SmallTopLeftNearColliders.IsEmpty && SmallBottomRightNearColliders.IsEmpty && SmallBottomLeftNearColliders.IsEmpty && SmallTopRightFarColliders.IsEmpty && SmallTopLeftFarColliders.IsEmpty && SmallBottomRightFarColliders.IsEmpty && SmallBottomLeftFarColliders.IsEmpty;

		/// <summary>
		/// Determines (quickly) if this oct is not empty.
		/// </summary>
		public bool IsNotEmpty => !IsEmpty;

		/// <summary>
		/// The total number of colliders stored in this oct (or its children).
		/// </summary>
		public int Count => InternalCount + ChildCount;

		/// <summary>
		/// The total number of colliders stored in this oct (but not its children).
		/// </summary>
		public int InternalCount => BigCount + SmallCount;

		/// <summary>
		/// The total number of colliders stored in this oct that cannot go into a smaller oct.
		/// </summary>
		public int BigCount => BigColliders.Count;

		/// <summary>
		/// The total number of colliders stored in this oct that would go into a smaller oct were it to split.
		/// </summary>
		public int SmallCount => SmallTopRightNearColliders.Count + SmallTopLeftNearColliders.Count + SmallBottomRightNearColliders.Count + SmallBottomLeftNearColliders.Count + SmallTopRightFarColliders.Count + SmallTopLeftFarColliders.Count + SmallBottomRightFarColliders.Count + SmallBottomLeftFarColliders.Count;

		/// <summary>
		/// The number of colliders in this octs's children.
		/// </summary>
		public int ChildCount
		{get; protected set;}

		/// <summary>
		/// The colliders belonging to this oct that cannot fit in any of its children.
		/// </summary>
		/// <remarks>Typically, octs are very large relative to the size of objects, so there is likely little overlap between objects that straddle the boundary, so an AABB tree will actually be more useful than a slow linked list.</remarks>
		protected AABBTree<ICollider3D,FPrism> BigColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top right near oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallTopRightNearColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top left near oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallTopLeftNearColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bottom right near oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallBottomRightNearColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bototm left near oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallBottomLeftNearColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top right far oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallTopRightFarColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the top left far oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallTopLeftFarColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bottom right far oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallBottomRightFarColliders
		{get; set;}

		/// <summary>
		/// The colliders that could go into the bototm left far oct if only this were not a leaf oct.
		/// If this is not a leaf oct, then this is always empty.
		/// </summary>
		protected CollisionLinkedList<ICollider3D> SmallBottomLeftFarColliders
		{get; set;}

		/// <summary>
		/// The boundary of this quad.
		/// </summary>
		public FPrism Bounds
		{get;}

		/// <summary>
		/// The half x position dividing the oct in half.
		/// </summary>
		public float HalfX
		{get;}

		/// <summary>
		/// The half y position dividing the oct in half.
		/// </summary>
		public float HalfY
		{get;}

		/// <summary>
		/// The half z position dividing the oct in half.
		/// </summary>
		public float HalfZ
		{get;}

		/// <summary>
		/// The parent oct of this oct.
		/// If this is the root oct, then this value is null.
		/// </summary>
		public Oct? Parent
		{get; protected set;}

		/// <summary>
		/// The top right child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? TopRightNear
		{get; protected set;}

		/// <summary>
		/// The top left child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? TopLeftNear
		{get; protected set;}

		/// <summary>
		/// The bottom right child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? BottomRightNear
		{get; protected set;}

		/// <summary>
		/// The bottom left child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? BottomLeftNear
		{get; protected set;}

		/// <summary>
		/// The top right child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? TopRightFar
		{get; protected set;}

		/// <summary>
		/// The top left child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? TopLeftFar
		{get; protected set;}

		/// <summary>
		/// The bottom right child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? BottomRightFar
		{get; protected set;}

		/// <summary>
		/// The bottom left child oct of this oct.
		/// If this is a leaf oct, then this value is null.
		/// </summary>
		public Oct? BottomLeftFar
		{get; protected set;}

		/// <summary>
		/// If true, then this is a root oct.
		/// If false, then it is not.
		/// </summary>
		public bool IsRoot => Parent is null;

		/// <summary>
		/// If true, then this is not a root or leaf oct.
		/// If false, then this ia a root or a leaf.
		/// </summary>
		public bool IsInternal => !IsRoot && !IsLeaf;

		/// <summary>
		/// If true, then this is a terminal leaf oct.
		/// If false, then this is not.
		/// </summary>
		public bool IsLeaf => IsTopRightNearLeaf && IsTopLeftNearLeaf && IsBottomRightNearLeaf && IsBottomLeftNearLeaf && IsTopRightFarLeaf && IsTopLeftFarLeaf && IsBottomRightFarLeaf && IsBottomLeftFarLeaf;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its top right near oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopRightNearLeaf => TopRightNear is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its top left near oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopLeftNearLeaf => TopLeftNear is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its bottom right near oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomRightNearLeaf => BottomRightNear is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its bottom left near oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomLeftNearLeaf => BottomLeftNear is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its top right far oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopRightFarLeaf => TopRightFar is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its top left far oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsTopLeftFarLeaf => TopLeftFar is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its bottom right far oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomRightFarLeaf => BottomRightFar is null;

		/// <summary>
		/// If true, then this is a terminal leaf oct as far as its bottom left far oct is concerned.
		/// If false, then this is not.
		/// </summary>
		public bool IsBottomLeftFarLeaf => BottomLeftFar is null;

		/// <summary>
		/// This is the maximum number of colliders allowed to be stored in a smaller oct list in a leaf before the oct splits into eight smaller octs.
		/// </summary>
		public const int MaxSmallOctSizeBeforeSplit = 4;
	}

	/// <summary>
	/// Represents what region an oct lies in.
	/// </summary>
	[Flags]
	public enum OctRegion
	{
		Left = 0b000,
		Bottom = 0b000,
		Near = 0b000,
		Right = 0b100,
		Top = 0b010,
		Far = 0b001,
		TopRightNear = Top | Right | Near,
		TopLeftNear = Top | Left | Near,
		BottomRightNear = Bottom | Right | Near,
		BottomLeftNear = Bottom | Left | Near,
		TopRightFar = Top | Right | Far,
		TopLeftFar = Top | Left | Far,
		BottomRightFar = Bottom | Right | Far,
		BottomLeftFar = Bottom | Left | Far,
		Root = 0b1000
	}
}
