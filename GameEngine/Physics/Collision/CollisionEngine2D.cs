using GameEngine.DataStructures.Geometry;
using GameEngine.GameComponents;
using GameEngine.Physics.Collision.Colliders;
using Microsoft.Xna.Framework;

namespace GameEngine.Physics.Collision
{
	/// <summary>
	/// A collision engine for 2D applications.
	/// </summary>
	/// <remarks>
	/// Note that this while this class is a GameComponent, it is a system.
	/// It does not interact with any of the standard features, such as Initialize, Dispose, etc.
	/// If you add it to a game, you <b>can</b> set its update order if desired.
	/// It also maintains the state information expected of a GameComponent, even if it doesn't use it internally.
	/// </remarks>
	public class CollisionEngine2D : BareGameComponent
	{
		/// <summary>
		/// Creates a new 2D collision engine.
		/// </summary>
		/// <param name="left">The initial left bound of the world. This should be strictly less than <paramref name="right"/>.</param>
		/// <param name="right">The initial right bound of the world. This should be strictly greater than <paramref name="left"/>.</param>
		/// <param name="bottom">The initial bottom bound of the world. This should be strictly less than <paramref name="top"/>.</param>
		/// <param name="top">The initial top bound of the world. This should be strictly greater than <paramref name="bottom"/>.</param>
		public CollisionEngine2D(float left, float right, float bottom, float top) : this(new FRectangle(left,bottom,right - left,top - bottom))
		{return;}

		/// <summary>
		/// Creates a new 2D collision engine.
		/// </summary>
		/// <param name="world_bounds">The initial boundary of the world. This value will expand if necessary.</param>
		public CollisionEngine2D(FRectangle world_bounds) : base()
		{
			Statics = new Quadtree(world_bounds);
			
			Kinetics = new Dictionary<uint,ColliderWrapper2D>();
			KineticXAxis = new CollisionLinkedList<AxisColliderWrapper2D>();
			KineticYAxis = new CollisionLinkedList<AxisColliderWrapper2D>();

			MarkedX = new HashSet<(uint,uint)>();
			MarkedXY = new HashSet<(uint,uint)>();

			Collisions = new CollisionLinkedList<(ICollider2D,ICollider2D)>();
			return;
		}

		/// <summary>
		/// Determines what colliders lie at least partially within <paramref name="region"/>.
		/// This check is determined via each collider's bounding box intersecting with <paramref name="region"/>, not via its fully collision check method.
		/// </summary>
		/// <param name="region">The region to query.</param>
		/// <returns>Returns the list of colliders which at least partially lie within <paramref name="region"/>. If there are none, an empty list is returned.</returns>
		public IEnumerable<ICollider2D> Query(FRectangle region, bool query_statics = true, bool query_kinetics = true)
		{
			CollisionLinkedList<ICollider2D> ret = new CollisionLinkedList<ICollider2D>();

			// If we have nothing, we're done
			if(IsEmpty)
				return ret;

			// Grab every static we collide with
			if(query_statics)
				ret.AddAllLast(Statics.Query(region));

			// If we don't want kinetics, we're done
			if(!query_kinetics)
				return ret;

			// Now we have the more complicated task of querying the kinetics
			// To do this, we have little better option than to go through them all
			// We could scan across the axes, but we have no way to binary search, so that's linear time regardless
			// Even though we would often be able to end early at region's upper bounds, we would still have to scan multiple lists and interact with hashsets, so it's probably just as efficient (or better) to just scan Kinetics
			foreach(ICollider2D c in Kinetics.Values.Select(k => k.Collider))
				if(c.Boundary.Intersects(region))
					ret.AddLast(c);

			return ret;
		}

		/// <summary>
		/// Determines what colliders <paramref name="c"/> is colliding with.
		/// This will take roughly O(1 + log n) time on average, as it bypasses the (here considered stale) current list of collisions stored in CurrentCollisions.
		/// </summary>
		/// <param name="c">
		/// The collider to check for collision.
		/// It must be the case that <paramref name="c"/>'s boundary information is not stale here (see RefreshKineticCollider/Update).
		/// Everything kinetic that it does or does not collide with must also not be stale to obtain correct results.
		/// </param>
		/// <param name="query_statics">If true, then the static list will be queried.</param>
		/// <param name="query_kinetics">If true, then the kinetic list will be queried.</param>
		/// <returns>Returns a list of colliders <paramref name="c"/> is colliding with (if any). An empty list is returned if there are no collisions.</returns>
		public IEnumerable<ICollider2D> Query(ICollider2D c, bool query_statics = true, bool query_kinetics = true)
		{
			CollisionLinkedList<ICollider2D> ret = new CollisionLinkedList<ICollider2D>();
			
			// If we have nothing, we're done
			if(IsEmpty)
				return ret;

			// The static colliders are easy to grab
			if(query_statics)
				ret.AddAllLast(Statics.Query(c));

			// If we don't want kinetics, we're done
			if(!query_kinetics)
				return ret;

			// The kinetics are more difficult to get
			// We will run a mini sweep and prune algorithm on just c
			ColliderWrapper2D cw = Kinetics[c.ID];

			// Sweep the x axis first
			HashSet<uint> x_mark = new HashSet<uint>();

			// The left node cannot be the tail
			// n.Next is also never null
			for(CollisionLinkedListNode<AxisColliderWrapper2D> n = cw.LeftNode!.Next!;n.Value.Owner.Collider.ID != c.ID;n = n.Next!)
				x_mark.Add(n.Value.Owner.Collider.ID);

			// Now sweep the y axis
			HashSet<uint> y_mark = new HashSet<uint>();

			for(CollisionLinkedListNode<AxisColliderWrapper2D> n = cw.BottomNode!.Next!;n.Value.Owner.Collider.ID != c.ID;n = n.Next!)
				if(x_mark.Contains(n.Value.Owner.Collider.ID))
					y_mark.Add(n.Value.Owner.Collider.ID);
			
			// Now that we have all of our collisions (and no duplicates), add them all to ret
			ret.AddAllLast(y_mark.Where(ID => ID != c.ID).Select(ID => Kinetics[ID].Collider));

			return ret;
		}

		/// <summary>
		/// Determines what colliders <paramref name="c"/> is colliding with.
		/// This will search the current list of collisions stored in CurrentCollisions for the result.
		/// </summary>
		/// <param name="c">The collider to check for collision.</param>
		/// <param name="query_statics">If true, then statics collisions with <paramref name="c"/> will be obtained..</param>
		/// <param name="query_kinetics">If true, then kinetic collisions with <paramref name="c"/> will be obtained.</param>
		/// <returns>Returns a list of colliders <paramref name="c"/> is colliding with (if any). An empty list is returned if there are no collisions.</returns>
		public IEnumerable<ICollider2D> QueryCache(ICollider2D c, bool query_statics = true, bool query_kinetics = true)
		{
			CollisionLinkedList<ICollider2D> ret = new CollisionLinkedList<ICollider2D>();

			// If we have nothing, we're done
			if(Collisions.IsEmpty)
				return ret;

			// Check the cached colliders for c
			foreach((ICollider2D,ICollider2D) cc in Collisions)
				if(cc.Item1.ID == c.ID)
				{
					if(query_statics && cc.Item2.IsStatic || query_kinetics && cc.Item2.IsKinetic)
						ret.AddLast(cc.Item2);
				}
				else if(cc.Item2.ID == c.ID)
					if(query_statics && cc.Item1.IsStatic || query_kinetics && cc.Item1.IsKinetic)
						ret.AddLast(cc.Item1);

			return ret;
		}

		/// <summary>
		/// Adds a collider to the collision engine.
		/// </summary>
		/// <param name="c">The collider to add. If this is already in the collision engine, no change will occur.</param>
		/// <returns>Returns true if <paramref name="c"/> is added and false otherwise.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="c"/> already belongs to this collision engine.</exception>
		public bool AddCollider(ICollider2D c)
		{
			// If we already contain c, we're done
			// Contains is fast, so we don't need to worry about inlining it
			if(ContainsCollider(c))
				return false;

			if(c.IsStatic)
				AddToStaticSystem(c);
			else
				AddToKineticSystem(c);

			// Lastly, sign up for c's events
			// It's easiest to just sign up for all of them, never unsubscribe, and add guards to the subscriptions to make sure we only actually execute them when we actually need to
			// In the static movement event, for example, we just check to make sure that our collider is static in the event body
			c.OnStaticStateChanged += SwapStaticKinetic;
			c.OnStaticMovement += RefreshCollider;

			return true;
		}

		/// <summary>
		/// Adds <paramref name="c"/> to the static system of this collision engine.
		/// </summary>
		/// <remarks>It is assumed that <paramref name="c"/> does not belong to this collision engine already.</remarks>
		protected void AddToStaticSystem(ICollider2D c)
		{
			Statics.Add(c);
			return;
		}

		/// <summary>
		/// Adds <paramref name="c"/> to the kinetic system of this collision engine.
		/// </summary>
		/// <remarks>It is assumed that <paramref name="c"/> does not belong to this collision engine already.</remarks>
		protected void AddToKineticSystem(ICollider2D c)
		{
			// Create a new wrapper and add it to our dictionary linked to the collider's ID
			ColliderWrapper2D cw = new ColliderWrapper2D(c);
			Kinetics.Add(c.ID,cw);
			
			// Add the x axis left and right wrappers
			cw.LeftNode = KineticXAxis.AddLast(cw.Left);
			cw.RightNode = KineticXAxis.AddLast(cw.Right);

			// Add the y axis bottom and top wrappers
			cw.BottomNode = KineticYAxis.AddLast(cw.Bottom);
			cw.TopNode = KineticYAxis.AddLast(cw.Top);

			return;
		}

		/// <summary>
		/// Adds every collider in <paramref name="colliders"/>.
		/// This method is more efficient for adding colliders in batch, as it optimizes the sweep and prune algorithm which requires 'mostly sorted' data to be efficient.
		/// </summary>
		/// <param name="colliders">The colliders to add.</param>
		public void AddAllColliders(IEnumerable<ICollider2D> colliders)
		{
			int nk = 0;

			// Add every collider lazily and keep track of how many of them were kinetic
			foreach(ICollider2D c in colliders)
				if(AddCollider(c))
					if(c.IsKinetic)
						nk++;

			// If we added a nontrivial number of kinetic colliders, we should use a quick sort now to sort them rather than leaving them for insertion sort to deal with later
			if(nk > 8)
			{
				KineticXAxis.QuickSort((a,b) => a.CompareTo(b));
				KineticYAxis.QuickSort((a,b) => a.CompareTo(b));
			}

			return;
		}

		/// <summary>
		/// Swaps a collider between a static state and kinetic state or vice versa.
		/// </summary>
		/// <param name="c">The collider whose static state changed.</param>
		/// <param name="static_state">The current static state of <paramref name="c"/>.</param>
		private void SwapStaticKinetic(ICollider2D c, bool static_state)
		{
			if(static_state)
			{
				// Pull c out of the kinetic system first since we WERE kinetic but are now static
				RemoveFromKineticSystem(c);

				// Then add it into the static system
				AddToStaticSystem(c);
			}
			else
			{
				// Pull c out of the static system first since we WERE static but are now kinetic
				RemoveFromStaticSystem(c);

				// Then add it into the kinetic system
				AddToKineticSystem(c);
			}

			return;
		}

		/// <summary>
		/// Updates <paramref name="c"/>'s position in this collision engine.
		/// </summary>
		/// <remarks>
		/// This method works on both static and kinetic colliders.
		/// The former runs in O(log n) time while the latter typically runs in O(1) time but can be as bad as O(n).
		/// In either case, the current list of collisions is <b><u>NOT</u></b> updated.
		/// For that, run the Update method.
		/// </remarks>
		private void RefreshCollider(ICollider2D c)
		{
			// We handle an individual update for a static and kinetic collider VERY differently
			if(c.IsStatic)
			{
				// Moving static colliders should be rare, so we don't need to worry too much about efficiency so long as we remain O(log n)
				Statics.UpdateBoundary(c);
				return;
			}

			// We have a kinetic collider, so we need to update it's axis positions
			// We will do this here because this is a weird special case we really don't want to put into the linked list itself
			// First grab the wrapper with c's node information
			if(!Kinetics.TryGetValue(c.ID,out ColliderWrapper2D? cw))
				return;

			// Update each axis
			UpdateKineticBoundPointer(KineticXAxis,cw.LeftNode!,cw.RightNode!);
			UpdateKineticBoundPointer(KineticYAxis,cw.BottomNode!,cw.TopNode!);

			return;
		}

		/// <summary>
		/// Updates the pointers <paramref name="low"/> and <paramref name="high"/> in <paramref name="l"/> via 2-3 partial insertion sorts, requiring at most O(n) time but usually only taking O(1) time.
		/// </summary>
		/// <param name="l">The axis we are updating these nodes in.</param>
		/// <param name="low">The lower bound node.</param>
		/// <param name="high">The upper bound node.</param>
		protected void UpdateKineticBoundPointer(CollisionLinkedList<AxisColliderWrapper2D> l, CollisionLinkedListNode<AxisColliderWrapper2D> low, CollisionLinkedListNode<AxisColliderWrapper2D> high)
		{
			// We need to move nodes around in l, which can be a little awkward when low gets stuck on high instead of passing over high or vice versa
			// However, we know that this can only happen when we move low right and high right since if high moves left, it must be the case that left won't cross its old position
			bool low_left = low.IsHead ? false : low.Value.Value < low.Previous!.Value.Value; // low can never be the tail
			bool high_right = high.IsTail ? false : high.Value.Value > high.Next!.Value.Value; // high can never be the head

			// Now that we know which was we're moving each node, let's get to it
			if(low_left)
			{
				// Move low left until it's done
				while(!low.IsHead && low.Value.Value < low.Previous!.Value.Value)
					low.MoveLeft();

				// Move high either left or right
				// It can't cross low, which is in its correct position now, so this is fine
				if(high_right)
					while(!high.IsTail && high.Value.Value > high.Next!.Value.Value)
						high.MoveRight();
				else
					while(!high.IsHead && high.Value.Value < high.Previous!.Value.Value)
						high.MoveLeft();
			}
			else if(high_right)
			{
				// Move high right until its done
				while(!high.IsTail && high.Value.Value > high.Next!.Value.Value)
					high.MoveRight();

				// Move low right until it's done
				// This is after high is already in its new correct position, so this is fine
				while(!low.IsTail && low.Value.Value > low.Next!.Value.Value)
					low.MoveRight();
			}
			else // low moves right and high moves left
			{
				// low and high are moving toward each other, so they won't cross
				while(!low.IsTail && low.Value.Value > low.Next!.Value.Value)
					low.MoveRight();

				while(!high.IsHead && high.Value.Value < high.Previous!.Value.Value)
					high.MoveLeft();
			}

			l.RepairHeadTail();
			return;
		}

		/// <summary>
		/// Removes a collider from the collision engine.
		/// </summary>
		/// <param name="c">The collider to remove. If this was not in the collision engine, no change will occur.</param>
		/// <returns>Returns true if <paramref name="c"/> was removed and false otherwise.</returns>
		public bool RemoveCollider(ICollider2D c)
		{
			// If we don't contain the collider, do nothing
			// This is fast, so don't worry about not inlining this check
			if(!ContainsCollider(c))
				return false;

			if(c.IsStatic)
				RemoveFromStaticSystem(c);
			else
				RemoveFromKineticSystem(c);

			// We unsubscribe from c's events
			c.OnStaticStateChanged -= SwapStaticKinetic;
			c.OnStaticMovement -= RefreshCollider;

			return true;
		}

		/// <summary>
		/// Removes <paramref name="c"/> from the static system of this collision engine.
		/// </summary>
		/// <remarks>It is assumed that <paramref name="c"/> belongs to this collision engine.</remarks>
		protected void RemoveFromStaticSystem(ICollider2D c)
		{
			Statics.Remove(c);
			return;
		}

		/// <summary>
		/// Removes <paramref name="c"/> from the kinetic system of this collision engine.
		/// </summary>
		/// <remarks>It is assumed that <paramref name="c"/> belongs to this collision engine.</remarks>
		protected void RemoveFromKineticSystem(ICollider2D c)
		{
			// Grab the wrapper
			ColliderWrapper2D cw = Kinetics[c.ID];

			// We no longer need to be in Kinetics
			Kinetics.Remove(c.ID);
			
			// And now we can remove all of the nodes from the axes
			KineticXAxis.Remove(cw.LeftNode!);
			KineticXAxis.Remove(cw.RightNode!);

			KineticYAxis.Remove(cw.BottomNode!);
			KineticYAxis.Remove(cw.TopNode!);

			return;
		}

		/// <summary>
		/// Determines if the collider <paramref name="c"/> is in this collision engine.
		/// </summary>
		/// <param name="c">The collider to look for.</param>
		/// <returns>Returns true if <paramref name="c"/> belongs to this collision engine and false otherwise.</returns>
		/// <remarks>Containment is checked by comparing unique collider IDs.</remarks>
		public bool ContainsCollider(ICollider2D c) => c.IsStatic ? Statics.Contains(c) : Kinetics.ContainsKey(c.ID);

		/// <summary>
		/// Determines what is currently colliding.
		/// <para/>
		/// If allowing the Game's Components to update the collision engine automatically, let it call Update(GameTime) instead and omit a call to this function.
		/// However, if objects move after Update(GameTime) is called, one should call Update() to obtain the new list of collisions if they are needed before the next call to Update(GameTime).
		/// </summary>
		public void Update()
		{
			// Find all of the kinetic-static collisions
			Collisions.Clear();
			
			foreach(ICollider2D k in Kinetics.Values.Select(cw => cw.Collider))
				Collisions.AddAllLast(Statics.Query(k).Select(s => (k,s)));

			// Now grab all of the kinetic-kinetic collisions
			Collisions.AddAllLast(SweepAndPrune().Select(kk => (Kinetics[kk.Item1].Collider,Kinetics[kk.Item2].Collider)));

			return;
		}

		/// <summary>
		/// Determines what is currently colliding.
		/// </summary>
		/// <param name="delta">The elapsed game time. It will be ignored.</param>
		public override void Update(GameTime delta) => Update();

		/// <summary>
		/// Runs a sweep and prune algorithm to determine which kinetic colliders collide with each other.
		/// </summary>
		/// <returns>Returns the set of pairwise kinetic collisions in the simulation. Each collision (a,b) will satisfy a < b (where a and b are collider IDs located in Kinetics), so no symmetric collisions will be included.</returns>
		protected IEnumerable<(uint,uint)> SweepAndPrune()
		{
			// If we have no kinetic objects and somehow got here, we're done
			// Also, if we only have one kinetic object, we're done too, since we need at least two objects to have a collision
			if(KineticCount < 2)
				return new List<(uint,uint)>();

			// Sort the kinetics along their axes
			// They should be mostly sorted already due to temporal coherence, so this should be fast
			KineticXAxis.InsertionSort((a,b) => a.CompareTo(b));
			KineticYAxis.InsertionSort((a,b) => a.CompareTo(b));

			// Since we have two dimensions, we need to remember which things we've seen overlapping in the first when we get to the second
			MarkedX.Clear();

			// Now we can iterate over the axes and mark what kinetic objects need additional inspection
			// X axis first
			CollisionLinkedListNode<AxisColliderWrapper2D> i = KineticXAxis.Head!;
			
			// Loop until we get to the tail, which cannot possibly interact with anything
			while(!i.IsTail)
			{
				// If this is a terminating point, ignore it
				if(i.Value.Terminating || i.Value.Owner.Collider.Disabled)
				{
					i = i.Next!; // i wasn't the tail, so it has a next
					continue;
				}

				// i is a beginning point, so we need to loop forward until we get to the terminating point
				// We do, of course, assume that the terminating point is always after the beginning point in the list
				CollisionLinkedListNode<AxisColliderWrapper2D> j = i.Next!;

				// Loop to i's terminating point (i.e. loop while j is not i's termination point)
				while(j.Value.Owner.Collider.ID != i.Value.Owner.Collider.ID)
				{
					// We mark j's owner for further consideration
					// We force the smaller ID to be added first to avoid the duplicate symmetric collisions
					// If we try to double add later, it will fail because marked is a set
					if(j.Value.Owner.Collider.Enabled)
						if(i.Value.Owner.Collider.ID < j.Value.Owner.Collider.ID)
							MarkedX.Add((i.Value.Owner.Collider.ID,j.Value.Owner.Collider.ID));
						else
							MarkedX.Add((j.Value.Owner.Collider.ID,i.Value.Owner.Collider.ID));

					// Increment j
					j = j.Next!; // The next must exist
				}

				// Advance i
				i = i.Next!;
			}
			
			// If we have nothing marked, we're done
			if(MarkedX.Count == 0)
				return Enumerable.Empty<(uint,uint)>();

			// Now we have everything that COULD intersect along the x axis
			// Now we do it again for the y axis nearly identically
			MarkedXY.Clear();
			i = KineticYAxis.Head!;

			while(!i.IsTail)
			{
				if(i.Value.Terminating || i.Value.Owner.Collider.Disabled)
				{
					i = i.Next!;
					continue;
				}

				CollisionLinkedListNode<AxisColliderWrapper2D> j = i.Next!;

				while(j.Value.Owner.Collider.ID != i.Value.Owner.Collider.ID)
				{
					// Because we only have two dimensions, we CAN immediately resolve this collision
					// Instead we will leave this marked or 'unmark' it if we no longer think it's fit for collision
					// This allows us to collect everything that will collide into one independent list
					// Then if a collision removes things from the simulation, we won't have to worry about our enumerators' behavior being called into question
					if(j.Value.Owner.Collider.Enabled)
						if(i.Value.Owner.Collider.ID < j.Value.Owner.Collider.ID)
						{
							if(MarkedX.Contains((i.Value.Owner.Collider.ID,j.Value.Owner.Collider.ID)))
								MarkedXY.Add((i.Value.Owner.Collider.ID,j.Value.Owner.Collider.ID));
						}
						else
						{
							if(MarkedX.Contains((j.Value.Owner.Collider.ID,i.Value.Owner.Collider.ID)))
								MarkedXY.Add((j.Value.Owner.Collider.ID,i.Value.Owner.Collider.ID));
						}
					
					j = j.Next!;
				}

				// Advance i
				i = i.Next!;
			}

			// double_marked is independent of the kinetic and static lists, so we don't need to worry if those change as a result of collision resolution
			return MarkedXY.Where(pair => Kinetics[pair.Item1].Collider.CollidesWith(Kinetics[pair.Item2].Collider));
		}

		/// <summary>
		/// If true, then this collision engine has no colliders in it.
		/// If false, then it has at least one.
		/// </summary>
		public bool IsEmpty => Count == 0;

		/// <summary>
		/// If true, then this collision engine has at least one collider in it.
		/// If false, then it has none.
		/// </summary>
		public bool IsNotEmpty => !IsEmpty;

		/// <summary>
		/// The total number of colliders in this collision engine.
		/// </summary>
		public int Count => Statics.Count + Kinetics.Count;

		/// <summary>
		/// The total number of static colliders in this collision engine.
		/// </summary>
		public int StaticCount => Statics.Count;

		/// <summary>
		/// The total number of kinetic colliders in this collision engine.
		/// </summary>
		public int KineticCount => Kinetics.Count;

		/// <summary>
		/// The static colliders in the collision engine.
		/// </summary>
		protected Quadtree Statics;

		/// <summary>
		/// The kinetic colliders in the collision engine.
		/// </summary>
		protected Dictionary<uint,ColliderWrapper2D> Kinetics;

		/// <summary>
		/// The x axis list of colliders for the sweep and prune section of the collision engine.
		/// </summary>
		protected CollisionLinkedList<AxisColliderWrapper2D> KineticXAxis;

		/// <summary>
		/// The y axis list of colliders for the sweep and prune section of the collision engine.
		/// </summary>
		protected CollisionLinkedList<AxisColliderWrapper2D> KineticYAxis;

		/// <summary>
		/// The kinetic x axis for the sweep and prune.
		/// This value is kept between runs and cleared to prevent the heap from being filled with garbage.
		/// </summary>
		protected HashSet<(uint,uint)> MarkedX
		{get; set;}

		/// <summary>
		/// The kinetic y axis for the sweep and prune.
		/// This value is kept between runs and cleared to prevent the heap from being filled with garbage.
		/// </summary>
		protected HashSet<(uint,uint)> MarkedXY
		{get; set;}

		/// <summary>
		/// The set of collisions calculated for the current frame.
		/// The first collider is always kinetic.
		/// The second collider may be kinetic or static.
		/// <para/>
		/// These pairs may be stale this frame if the collisions have already been resolved.
		/// </summary>
		public IEnumerable<(ICollider2D,ICollider2D)> CurrentCollisions => Collisions;

		/// <summary>
		/// The current set of collisions.
		/// </summary>
		protected CollisionLinkedList<(ICollider2D,ICollider2D)> Collisions
		{get; set;}
	}

	/// <summary>
	/// A wrapper around colliders for additional collision engine information.
	/// </summary>
	public class ColliderWrapper2D
	{
		/// <summary>
		/// Creates a wrapper around a collider.
		/// </summary>
		/// <param name="c">The collider to wrap around.</param>
		public ColliderWrapper2D(ICollider2D c)
		{
			Collider = c;

			Left = new AxisColliderWrapper2D(this,false,() => Collider.LeftBound);
			Right = new AxisColliderWrapper2D(this,true,() => Collider.RightBound);
			Bottom = new AxisColliderWrapper2D(this,false,() => Collider.BottomBound);
			Top = new AxisColliderWrapper2D(this,true,() => Collider.TopBound);
			
			LeftNode = null;
			RightNode = null;
			BottomNode = null;
			TopNode = null;

			return;
		}

		/// <summary>
		/// The collider.
		/// </summary>
		public ICollider2D Collider
		{get;}

		/// <summary>
		/// The wrapper for our left bound.
		/// </summary>
		public AxisColliderWrapper2D Left
		{get;}

		/// <summary>
		/// The node containing our left bound.
		/// <para/>
		/// This must be manually set once added to the x axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper2D>? LeftNode
		{get; set;}

		/// <summary>
		/// The wrapper for our right bound.
		/// </summary>
		public AxisColliderWrapper2D Right
		{get;}

		/// <summary>
		/// The node containing our right bound.
		/// <para/>
		/// This must be manually set once added to the x axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper2D>? RightNode
		{get; set;}

		/// <summary>
		/// The wrapper for our bottom bound.
		/// </summary>
		public AxisColliderWrapper2D Bottom
		{get;}

		/// <summary>
		/// The node containing our bottom bound.
		/// <para/>
		/// This must be manually set once added to the y axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper2D>? BottomNode
		{get; set;}

		/// <summary>
		/// The wrapper for our top bound.
		/// </summary>
		public AxisColliderWrapper2D Top
		{get;}

		/// <summary>
		/// The node containing our top bound.
		/// <para/>
		/// This must be manually set once added to the y axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper2D>? TopNode
		{get; set;}
	}

	/// <summary>
	/// A wrapper for an entry of a collider wrapper into a linked list for a sweep and prune algorithm.
	/// </summary>
	public readonly struct AxisColliderWrapper2D : IComparable<AxisColliderWrapper2D>
	{
		/// <summary>
		/// Creates a new wrapper.
		/// </summary>
		/// <param name="w">The wrapper we are wrapping around.</param>
		/// <param name="terminal">If true, then this is the end of the axis interval. If false, it is the beginning.</param>
		/// <param name="value_func">The means by which we obtain our axis value.</param>
		public AxisColliderWrapper2D(ColliderWrapper2D w, bool terminal, FetchAxisValue2D value_func)
		{
			Owner = w;
			Terminating = terminal;
			ValueFetcher = value_func;

			return;
		}

		public override string ToString() => "(value: " + Value.ToString() + " terminating: " + Terminating + " ID: " + Owner.Collider.ID + ")";

		public int CompareTo(AxisColliderWrapper2D other)
		{
			int ret = Value.CompareTo(other.Value);

			// If our floats are equal, then we need to sort terminating things before non terminating ones
			if(ret == 0)
				if(Terminating)
					if(other.Terminating)
						return 0; // Both terminating; equal weight
					else
						return -1; // We're terminating, so we must come before non terminating values
				else // We're not terminating
					if(other.Terminating)
						return 1; // We're not terminating, so we must come after terminals
					else
						return 0; // Both not terminating; equal weight

			return ret;
		}

		/// <summary>
		/// The wrapper that owns this.
		/// </summary>
		public ColliderWrapper2D Owner
		{get;}

		/// <summary>
		/// If true, then this is the terminating point of an interval on an axis.
		/// If false, then this is the beginning point.
		/// </summary>
		public bool Terminating
		{get;}

		/// <summary>
		/// If true, then this is the beginning point of an interval on an axis.
		/// If false, then this is the terminating point.
		/// </summary>
		public bool Initiating => !Terminating;

		/// <summary>
		/// The value this wrapper has within an axis.
		/// </summary>
		public float Value => ValueFetcher();

		/// <summary>
		/// How we obtain our value.
		/// </summary>
		private FetchAxisValue2D ValueFetcher
		{get;}
	}

	/// <summary>
	/// Returns a position along an axis.
	/// </summary>
	public delegate float FetchAxisValue2D();
}
