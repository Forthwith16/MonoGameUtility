using GameEngine.DataStructures.Sets;
using GameEngine.Maths;

namespace GameEngine.Framework
{
	/// <summary>
	/// The base requirements for a game object to be affine.
	/// </summary>
	public abstract class AffineObject : GameObject
	{
		/// <summary>
		/// Initializes this component to have the identity transformation and no parent.
		/// </summary>
		/// <param name="name">The resource name.</param>
		/// <param name="game">The game this component will belong to.</param>
		protected AffineObject(string name) : base(name)
		{
			_p = null;
			_children = new AVLSet<AffineObject>(Comparer<AffineObject>.Create((a,b) => a.ID.CompareTo(b.ID)));

			WorldRevision = RevisionID.Initial;
			InverseWorldRevision = RevisionID.Initial;
			
			Transform = Matrix2D.Identity; // This assignment will flag everything else as stale (except World, but it defaults to the identity matrix, so that's fine to leave alone)
			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will have a fresh ID.</item>
		///	<item>This will have the same parent as <paramref name="other"/>, but it will leave Children unpopulated.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		protected AffineObject(AffineObject other) : base(other)
		{
			_p = null; // We do this just until we're ready to properly set it
			_children = new AVLSet<AffineObject>(Comparer<AffineObject>.Create((a,b) => a.ID.CompareTo(b.ID)));

			WorldRevision = RevisionID.Initial;
			InverseWorldRevision = RevisionID.Initial;
			
			Transform = other.Transform;
			Parent = other._p;
			
			return;
		}

		/// <summary>
		/// Applies <paramref name="f"/> to every affine object of an affine object hierarchy.
		/// </summary>
		/// <param name="f">The function to apply to the hierarchy.</param>
		/// <param name="bfs">
		/// If true, this will apply <paramref name="f"/> in a breadth first search order.
		/// If false, it will apply <paramref name="f"/> in a depth first search order, visiting affine objects on the way down the hierarchy rather than on the way back up.
		/// </param>
		/// <remarks>This will not branch off into affine objects not part of the parent/child hiearchy.</remarks>
		public void ApplyToAffineHierarchy(ModifyAffineObject f, bool bfs = true)
		{
			if(bfs)
				ApplyBFS(f);
			else
				ApplyDFS(f);

			return;
		}

		/// <summary>
		/// Applies <paramref name="f"/> to every affine object of this hiearchy in BFS order.
		/// </summary>
		private void ApplyBFS(ModifyAffineObject f)
		{
			Queue<AffineObject> q = new Queue<AffineObject>();
			q.Enqueue(this);

			while(q.Count > 0)
			{
				AffineObject obj = q.Dequeue();

				f(obj);

				foreach(AffineObject child in obj.Children)
					q.Enqueue(child);
			}

			return;
		}

		/// <summary>
		/// Applies <paramref name="f"/> to every affine object of this hiearchy in DFS order.
		/// </summary>
		private void ApplyDFS(ModifyAffineObject f)
		{
			Stack<AffineObject> q = new Stack<AffineObject>();
			q.Push(this);

			while(q.Count > 0)
			{
				AffineObject obj = q.Pop();

				f(obj);

				foreach(AffineObject child in obj.Children)
					q.Push(child);
			}

			return;
		}

		/// <summary>
		/// The transformation matrix of this component absent its parent transform (if it has one).
		/// <para/>
		/// For the transformation matrix containing the parent (if present), use World instead.
		/// </summary>
		public virtual Matrix2D Transform
		{
			get => _t;

			set
			{
				// It's not worth checking if _t == value, so we'll just blindly mark these as stale
				StaleInverse = true;

				// Instead of keeping a stale boolean, we double our value in using the revision number to mark these as stale
				ParentWorldRevision = RevisionID.Stale;
				ParentInverseWorldRevision = RevisionID.Stale;
				
				_t = value;
				return;
			}
		}

		protected Matrix2D _t;

		/// <summary>
		/// If true, then the transform itself is stale and needs to be updated.
		/// </summary>
		/// <remarks>
		///	This value is always false in its initial implementation.
		///	Override this in derived classes if Transform depends on values other than itself.
		///	You should also override Transform's get so that Transform is properly set before any call to its get returns.
		/// </remarks>
		protected virtual bool StaleTransform => false;

		/// <summary>
		/// The transformation matrix of this component.
		/// If this component has a parent, this includes the parent transform.
		/// The parent transform is applied second when it exists with this component's transform being applied first (i.e. Transform is left multiplied by Parent.Transform).
		/// <para/>
		/// For the raw transformation matrix, use Transform instead.
		/// </summary>
		public Matrix2D World
		{
			get
			{
				if(StaleWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentWorldRevision = RevisionID.NoParentInfo; // A 1 value means our world is up to date when we have no parent
						_w = Transform;
					}
					else
					{
						ParentWorldRevision = Parent.WorldRevision; // Our parent's revision number means our world is up to date when we have a parent
						_w = Parent.World * Transform;
					}

					++WorldRevision;
				}

				return _w;
			}
		}

		protected Matrix2D _w;

		/// <summary>
		/// This is the revision number of the World matrix.
		/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
		/// This number should thus always start from at least 2.
		/// </summary>
		/// <remarks>This value can be useful for determining if this object has moved, though this is not a guarantee if the World matrix is stale.</remarks>
		public RevisionID WorldRevision
		{get; protected set;}
		
		/// <summary>
		/// The currently known revision of Parent.World.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected RevisionID ParentWorldRevision
		{get; set;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		protected bool StaleWorld => StaleTransform || (Parent is null ? ParentWorldRevision != RevisionID.NoParentInfo : ParentWorldRevision != Parent.WorldRevision || Parent.StaleWorld);

		/// <summary>
		/// The inverse transform of this component's Transform absent its parent transform (if it has one).
		/// <para/>
		/// For the inverse transformation matrix containing the parent (if present), use InverseWorld instead.
		/// </summary>
		public Matrix2D InverseTransform
		{
			get
			{
				if(StaleInverse)
					InverseTransform = Transform.Invert();

				return _it;
			}
			
			private set
			{
				_it = value;
				StaleInverse = false;

				return;
			}
		}

		protected Matrix2D _it;

		/// <summary>
		/// If true, then our inverse is stale and needs to be recalculated.
		/// If false, then our inverse is up to date.
		/// <para/>
		/// Setting this will set _si.
		/// </summary>
		protected bool StaleInverse
		{
			get => StaleTransform || _si;
			set => _si = value;
		}

		private bool _si;

		/// <summary>
		/// The inverse transformation matrix of this component.
		/// If this component has a parent, this includes the parent's inverse transformation.
		/// The parent transform is applied first in the inverse when it exists with this component's transform being applied last (i.e. InverseTransform is right multiplied by Parent.InverseTransform).
		/// <para/>
		/// For the raw inverse transformation matrix, use InverseTransform instead.
		/// </summary>
		public Matrix2D InverseWorld
		{
			get
			{
				if(StaleInverseWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentInverseWorldRevision = RevisionID.NoParentInfo; // A 1 value means our inverse world is up to date when we have no parent
						_iw = InverseTransform;
					}
					else
					{
						ParentInverseWorldRevision = Parent.InverseWorldRevision; // Our parent's revision number means our inverse world is up to date when we have a parent
						_iw = InverseTransform * Parent.InverseWorld;
					}

					++InverseWorldRevision;
				}

				return _iw;
			}
		}

		protected Matrix2D _iw;

		/// <summary>
		/// This is the revision number of the InverseWorld matrix.
		/// <para/>
		/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
		/// This number should thus always start from at least 2.
		/// </summary>
		protected RevisionID InverseWorldRevision
		{get; set;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		protected bool StaleInverseWorld => StaleInverse || (Parent is null ? ParentInverseWorldRevision != RevisionID.NoParentInfo : ParentInverseWorldRevision != Parent.InverseWorldRevision || Parent.StaleInverseWorld);

		/// <summary>
		/// The currently known revision of Parent.InverseWorld.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected RevisionID ParentInverseWorldRevision
		{get; set;}

		/// <summary>
		/// The parent of this component.
		/// Transforming the parent will transform this object in turn.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown on set if the set would create a cycle.</exception>
		public virtual AffineObject? Parent
		{
			get => _p;
			
			set
			{
				// If we are asked to do nothing, then do nothing
				if(ReferenceEquals(_p,value))
					return;

				// If this set would create a cycle, complain hard
				AffineObject? obj = value;

				while(obj != null) // We have to find something without a parent eventually, otherwise we would ALREADY have a cycle, which is assumed not to be the case
					if(obj == this) // Cycles are created by going to value and then coming back to this
						throw new ArgumentException();
					else
						obj = obj.Parent;

				// We're leaving a parent, so we need to no longer be a child of it
				_p?._children.Remove(this);

				// Otherwise, we can go ahead and mark that our matrices are stale
				ParentWorldRevision = RevisionID.Stale;
				ParentInverseWorldRevision = RevisionID.Stale;

				// Set the parent (and take their Game)
				_p = value;
				Game = _p?.Game;

				// And make sure we are a child as well
				_p?._children.Add(this);
				
				return;
			}
		}

		protected AffineObject? _p;

		/// <summary>
		/// This is the set of children of this component.
		/// </summary>
		public IEnumerable<AffineObject> Children => _children;

		/// <summary>
		/// The concrete children set.
		/// </summary>
		protected AVLSet<AffineObject> _children;
	}

	/// <summary>
	/// Represents a function that applies some effect to an affine object.
	/// </summary>
	/// <param name="current">The current affine object to apply the effect to.</param>
	public delegate void ModifyAffineObject(AffineObject current);
}
