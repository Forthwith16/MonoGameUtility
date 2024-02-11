using GameEngine.DataStructures.Geometry;
using GameEngine.Physics.Collision.Colliders;

namespace GameEngine.Physics.Collision
{
	/// <summary>
	/// A collision engine for 2D applications.
	/// </summary>
	/// <remarks>
	/// Note that this class is not a GameObject.
	/// It is intended for use by other systems to detect collisions.
	/// It should be updated after all GameObjects have updated but before these systems (such as a physics system) utilizes it.
	/// </remarks>
	public class CollisionEngine2D
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
		public CollisionEngine2D(FRectangle world_bounds)
		{
			Statics = new Quadtree(world_bounds);
			
			Kinetics = new Dictionary<uint,ColliderWraper>();
			KineticXAxis = new CollisionLinkedList<AxisColliderWrapper>();
			KineticYAxis = new CollisionLinkedList<AxisColliderWrapper>();

			MarkedX = new HashSet<(uint,uint)>();
			MarkedXY = new HashSet<(uint,uint)>();

			Collisions = new CollisionLinkedList<(ICollider2D,ICollider2D)>();
			return;
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
			c.OnStaticMovement += RefreshStaticCollider;

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
			ColliderWraper cw = new ColliderWraper(c);
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
				KineticXAxis.QuickSort((a,b) => a.Value.CompareTo(b.Value));
				KineticYAxis.QuickSort((a,b) => a.Value.CompareTo(b.Value));
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
		/// Updates <paramref name="c"/>'s position in this collision engine's static collider tree.
		/// </summary>
		private void RefreshStaticCollider(ICollider2D c)
		{
			// We only operate on static colliders
			if(c.IsKinetic)
				return;

			// Moving static colliders should be rare, so we don't need to worry too much about efficiency so long as we remain O(log n)
			RemoveCollider(c);
			AddCollider(c);

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
			c.OnStaticMovement -= RefreshStaticCollider;

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
			ColliderWraper cw = Kinetics[c.ID];

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
		/// Determines what is colliding this frame.
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
			KineticXAxis.InsertionSort((a,b) => a.Value.CompareTo(b.Value));
			KineticYAxis.InsertionSort((a,b) => a.Value.CompareTo(b.Value));

			// Since we have two dimensions, we need to remember which things we've seen overlapping in the first when we get to the second
			MarkedX.Clear();

			// Now we can iterate over the axes and mark what kinetic objects need additional inspection
			// X axis first
			CollisionLinkedListNode<AxisColliderWrapper> i = KineticXAxis.Head!;
			
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
				CollisionLinkedListNode<AxisColliderWrapper> j = i.Next!;

				// Loop to i's terminating point
				while(!(j.Value.Terminating && j.Value.Owner.Collider.ID == i.Value.Owner.Collider.ID))
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

				CollisionLinkedListNode<AxisColliderWrapper> j = i.Next!;

				while(!(j.Value.Terminating && j.Value.Owner.Collider.ID == i.Value.Owner.Collider.ID))
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
		protected Dictionary<uint,ColliderWraper> Kinetics;

		/// <summary>
		/// The x axis list of colliders for the sweep and prune section of the collision engine.
		/// </summary>
		protected CollisionLinkedList<AxisColliderWrapper> KineticXAxis;

		/// <summary>
		/// The y axis list of colliders for the sweep and prune section of the collision engine.
		/// </summary>
		protected CollisionLinkedList<AxisColliderWrapper> KineticYAxis;

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
	public class ColliderWraper
	{
		/// <summary>
		/// Creates a wrapper around a collider.
		/// </summary>
		/// <param name="c">The collider to wrap around.</param>
		public ColliderWraper(ICollider2D c)
		{
			Collider = c;

			Left = new AxisColliderWrapper(this,false,() => Collider.LeftBound);
			Right = new AxisColliderWrapper(this,true,() => Collider.RightBound);
			Bottom = new AxisColliderWrapper(this,false,() => Collider.BottomBound);
			Top = new AxisColliderWrapper(this,true,() => Collider.TopBound);
			
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
		public AxisColliderWrapper Left
		{get;}

		/// <summary>
		/// The node containing our left bound.
		/// <para/>
		/// This must be manually set once added to the x axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper>? LeftNode
		{get; set;}

		/// <summary>
		/// The wrapper for our right bound.
		/// </summary>
		public AxisColliderWrapper Right
		{get;}

		/// <summary>
		/// The node containing our right bound.
		/// <para/>
		/// This must be manually set once added to the x axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper>? RightNode
		{get; set;}

		/// <summary>
		/// The wrapper for our bottom bound.
		/// </summary>
		public AxisColliderWrapper Bottom
		{get;}

		/// <summary>
		/// The node containing our bottom bound.
		/// <para/>
		/// This must be manually set once added to the y axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper>? BottomNode
		{get; set;}

		/// <summary>
		/// The wrapper for our top bound.
		/// </summary>
		public AxisColliderWrapper Top
		{get;}

		/// <summary>
		/// The node containing our top bound.
		/// <para/>
		/// This must be manually set once added to the y axis.
		/// </summary>
		public CollisionLinkedListNode<AxisColliderWrapper>? TopNode
		{get; set;}
	}

	/// <summary>
	/// A wrapper for an entry of a collider wrapper into a linked list for a sweep and prune algorithm.
	/// </summary>
	public readonly struct AxisColliderWrapper
	{
		/// <summary>
		/// Creates a new wrapper.
		/// </summary>
		/// <param name="w">The wrapper we are wrapping around.</param>
		/// <param name="terminal">If true, then this is the end of the axis interval. If false, it is the beginning.</param>
		/// <param name="value_func">The means by which we obtain our axis value.</param>
		public AxisColliderWrapper(ColliderWraper w, bool terminal, FetchAxisValue value_func)
		{
			Owner = w;
			Terminating = terminal;
			ValueFetcher = value_func;

			return;
		}

		public override string ToString() => "(value: " + Value.ToString() + " terminating: " + Terminating + " ID: " + Owner.Collider.ID + ")";

		/// <summary>
		/// The wrapper that owns this.
		/// </summary>
		public ColliderWraper Owner
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
		private FetchAxisValue ValueFetcher
		{get;}
	}

	/// <summary>
	/// Returns a position along an axis.
	/// </summary>
	public delegate float FetchAxisValue();
}
