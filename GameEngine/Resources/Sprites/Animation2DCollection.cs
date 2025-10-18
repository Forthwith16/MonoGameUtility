using GameEngine.Assets;
using GameEngine.Assets.Sprites;
using GameEngine.DataStructures.Absorbing;
using GameEngine.Events;
using GameEngine.Exceptions;
using GameEngine.Framework;
using Microsoft.Xna.Framework;

namespace GameEngine.Resources.Sprites
{
	/// <summary>
	/// An Animation2DCollection groups a fixed set of animations together, each under a unique name (although the animations underneath each name may be identical).
	/// At most one animation of the collection is active at any time but may be swapped out to any other animation in the collection.
	/// <para/>
	/// For more information, see Animation2D and Animation2DController.
	/// </summary>
	/// <remarks>
	/// Note that neither this nor anything it controls ever needs to know what Game it belongs to.
	/// </remarks>
	public class Animation2DCollection : GameObject, IResource, IObservable<TimeEvent>
	{
		/// <summary>
		/// Creates a new collection of Animation2Ds.
		/// </summary>
		/// <param name="name">The asset name.</param>
		/// <param name="animations">The animations to add to this collection.</param>
		/// <param name="names">The names of the animations to add to this collection.</param>
		/// <param name="idle_animation">The name of the idle animation (or the otherwise default animation).</param>
		/// <exception cref="AnimationFormatException">Thrown if there are more animations than names or vice versa or if the animations are improperly formatted.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="idle_animation"/> is not an animation in this collection.</exception>
		public Animation2DCollection(string name, IEnumerable<Animation> animations, IEnumerable<string> names, string idle_animation) : this(name,animations.Select(a => new Animation2D(a)),names,idle_animation)
		{return;}

		/// <summary>
		/// Creates a new collection of Animation2Ds.
		/// </summary>
		/// <param name="name">The asset name.</param>
		/// <param name="animations">The animations to add to this collection.</param>
		/// <param name="names">The names of the animations to add to this collection.</param>
		/// <param name="idle_animation">The name of the idle animation (or the otherwise default animation).</param>
		/// <exception cref="AnimationFormatException">Thrown if there are more animations than names or vice versa.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="idle_animation"/> is not an animation in this collection.</exception>
		public Animation2DCollection(string name, IEnumerable<Animation2D> animations, IEnumerable<string> names, string idle_animation) : base()
		{
			ResourceName = name;

			Animations = new AbsorbingDictionary<string,Animation2D>();

			IEnumerator<Animation2D> _ia = animations.GetEnumerator();
			IEnumerator<string> _in = names.GetEnumerator();

			while(_ia.MoveNext() && _in.MoveNext())
				Animations[_in.Current] = _ia.Current;

			if(_ia.MoveNext() || _in.MoveNext())
				throw new AnimationFormatException("The number of animations did not match the number of animation names.");
			
			_ian = idle_animation;
			_aan = idle_animation;

			IdleAnimation = this[IdleAnimationName];
			ActiveAnimation = this[ActiveAnimationName];
			
			AnimationSwapped = (a,b,c) =>
			{
				if(ResetOnSwitch)
					ActiveAnimation.Reset();
			};
			
			ResetOnSwitch = true;
			return;
		}

		/// <summary>
		/// Creates a sufficiently deep copy of <paramref name="c"/> to operate independently.
		/// Events and subscriptions will not be copied.
		/// </summary>
		/// <param name="c">The animation collection to copy.</param>
		public Animation2DCollection(Animation2DCollection c) : base(c)
		{
			ResourceName = c.ResourceName;

			Animations = new AbsorbingDictionary<string,Animation2D>(c.Animations.Select(p => new KeyValuePair<string,Animation2D>(p.Key,new Animation2D(p.Value))));

			_ian = c._ian;
			_aan = c._aan;

			IdleAnimation = this[IdleAnimationName];
			ActiveAnimation = this[ActiveAnimationName];

			AnimationSwapped += (a,b,c) =>
			{
				if(ResetOnSwitch)
					ActiveAnimation.Reset();
			};
			
			ResetOnSwitch = c.ResetOnSwitch;
			return;
		}

		AssetBase? IResource.ToAsset() => new Animation2DCollectionAsset(this);

		protected override void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			foreach(Animation2D a in Animations.Values)
				a.Dispose();

			base.Dispose(disposing);
			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			foreach(Animation2D a in Animations.Values)
				a.Initialize();

			base.Initialize();
			return;
		}

		public override void Update(GameTime delta)
		{
			ActiveAnimation.Update(delta);
			return;
		}

		public IDisposable Subscribe(IObserver<TimeEvent> observer)
		{
			LinkedList<IDisposable> subscriptions = new LinkedList<IDisposable>();

			foreach(Animation2D a in Animations.Values)
				subscriptions.AddLast(a.Subscribe(observer));
			
			return new Unsubscriber(subscriptions);
		}

		/// <summary>
		/// Obtains the animation named <paramref name="name"/> of this collection.
		/// </summary>
		/// <param name="name">The name of the animation.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not a valid animation name.</exception>
		public Animation2D this[string name] => Animations[name];

		/// <summary>
		/// If true, then the active animation will be restarted from time 0 after being changed.
		/// If false, then the animation states are fully preserved in between usage.
		/// <para/>
		/// By default, this is true.
		/// </summary>
		public bool ResetOnSwitch
		{get; set;}

		/// <summary>
		/// The active animation name.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string ActiveAnimationName
		{
			get => _aan;

			set
			{
				// If we're asked to do nothing, then do nothing
				if(_aan == value)
					return;

				string old = _aan;

				_aan = value;
				ActiveAnimation = this[_aan];

				AnimationSwapped(this,old,_aan);
				return;
			}
		}

		protected string _aan;

		/// <summary>
		/// The active animation.
		/// </summary>
		public Animation2D ActiveAnimation
		{get; protected set;}

		/// <summary>
		/// The idle animation name of this collection.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string IdleAnimationName
		{
			get => _ian;

			protected set
			{
				_ian = value;
				IdleAnimation = this[_ian];

				return;
			}
		}

		protected string _ian;

		/// <summary>
		/// The idle animation.
		/// </summary>
		public Animation2D IdleAnimation
		{get; protected set;}

		/// <summary>
		/// The animation names of this collection in no particular order.
		/// </summary>
		public IEnumerable<string> AnimationNames => Animations.Keys;

		/// <summary>
		/// The number of animations in this collection.
		/// </summary>
		public int Count => Animations.Count;

		/// <summary>
		/// The animations in this collection.
		/// </summary>
		protected AbsorbingDictionary<string,Animation2D> Animations
		{get; init;}

		/// <summary>
		/// An event called when this collection changes its active animation.
		/// </summary>
		public event AnimationSwap AnimationSwapped;

		public string ResourceName
		{get;}

		/// <summary>
		/// Allows subscribers to unsubscribe.
		/// </summary>
		protected class Unsubscriber : IDisposable
		{
			public Unsubscriber(IEnumerable<IDisposable> subscriptions)
			{
				Subscriptions = subscriptions;
				return;
			}

			public void Dispose()
			{
				foreach(IDisposable subscription in Subscriptions)
					subscription.Dispose();

				return;
			}

			private IEnumerable<IDisposable> Subscriptions
			{get; set;}
		}
	}

	/// <summary>
	/// A function called when an Animation2DCollection changes its active animation.
	/// </summary>
	/// <param name="sender">The Animation2DCollection which changed its active animation.</param>
	/// <param name="old_name">The old active animation.</param>
	/// <param name="new_name">The new active animation.</param>
	public delegate void AnimationSwap(Animation2DCollection sender, string old_name, string new_name);
}