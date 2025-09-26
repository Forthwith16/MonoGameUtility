using GameEngine.Events;
using GameEngine.Maths;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A game object that draws one of a selection of animations.
	/// </summary>
	public class AnimationLibraryGameObject : ImageGameObject, IObserver<TimeEvent>, IObservable<TimeEvent>
	{
		/// <summary>
		/// Creates an animated game object from a not yet loaded animation collection.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="resource">The path to the animations for this game object.</param>
		public AnimationLibraryGameObject(SpriteBatch? renderer, string resource) : base(renderer,resource)
		{
			// We'll load these in LoadContent
			Animations = null;
			Animation = null;

			return;
		}

		/// <summary>
		/// Creates an animated game object from an already loaded animation collection.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="animations">The animations for this game object.</param>
		public AnimationLibraryGameObject(SpriteBatch? renderer, Animation2DCollection animations) : base(renderer)
		{
			Animations = animations;

			Subscription = Animations.Subscribe(this);
			Animations.AnimationSwapped += (sender,old_name,new_name) => 
			{
				Animation = Animations.ActiveAnimation;
				Source = Animation.SourceTexture;

				UpdateCurrentAnimationData();
				return;
			};

			Animation = Animations.ActiveAnimation;
			Source = Animation.Source.Source;

			UpdateCurrentAnimationData();
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
		public AnimationLibraryGameObject(AnimationLibraryGameObject other) : base(other)
		{
			if(other.Animations is null)
			{
				Animations = null;
				Animation = null;
			}
			else
			{
				Animations = new Animation2DCollection(other.Animations);

				Subscription = Animations.Subscribe(this);
				Animations.AnimationSwapped += (sender,old_name,new_name) => 
				{
					Animation = Animations.ActiveAnimation;
					Source = Animation.SourceTexture;

					UpdateCurrentAnimationData();
					return;
				};

				Animation = Animations.ActiveAnimation;
				Source = Animation.Source.Source;

				UpdateCurrentAnimationData();
			}

			return;
		}

		protected override void LoadContent()
		{
			// If we have a resource, then we need to load our animations
			if(Resource is not null)
			{
				Animations = Game!.Content.Load<Animation2DCollection>(Resource);

				Subscription = Animations.Subscribe(this);
				Animations.AnimationSwapped += (sender,old_name,new_name) => 
				{
					Animation = Animations.ActiveAnimation;
					Source = Animation.SourceTexture;

					UpdateCurrentAnimationData();
					return;
				};

				Animation = Animations.ActiveAnimation;
				Source = Animation.Source.Source;

				UpdateCurrentAnimationData();
			}

			// We always do this here regardless of if we needed to load the animations, which is why it's not in Initialize
			// We also can't get here without setting Animations at some point
			Animations?.Initialize();

			return;
		}

		public override void Update(GameTime delta)
		{
			Animations?.Update(delta); // All other updates happen through our subscriptions
			return;
		}
		
		/// <summary>
		/// Draws this animation library.
		/// </summary>
		/// <param name="delta">The elapsed time since the last draw.</param>
		public override void Draw(GameTime delta)
		{
			if(Renderer is null)
				return;
			
			// First grab the transformation game object we'll need to draw this with.
			(World * AnimationTransformation).Decompose(out Vector2 t,out float r,out Vector2 s);

			// Now we can draw!
			Renderer.Draw(Source,t,SourceRect,Tint,-r,Vector2.Zero,s,Effect,LayerDepth);
			
			return;
		}

		public void OnCompleted()
		{return;} // The worst that happens by not unsubscribing or not setting Subscription to null is that we get more events later to process

		public void OnError(Exception error)
		{return;}

		public void OnNext(TimeEvent value)
		{
			UpdateCurrentAnimationData();
			return;
		}

		/// <summary>
		/// Updates the local current animation data to match the current animation's.
		/// </summary>
		protected void UpdateCurrentAnimationData()
		{
			// We can only get here if Animation is set
			SourceRect = Animation!.CurrentSource; 
			AnimationTransformation = Animation[Animation.CurrentFrame].Transformation;

			return;
		}

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException">Thrown if no animation is loaded yet.</exception>
		public IDisposable Subscribe(IObserver<TimeEvent> observer)
		{return Animations?.Subscribe(observer) ?? throw new InvalidOperationException();}

		/// <summary>
		/// The active animation name.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string ActiveAnimationName
		{
			get => Animations?.ActiveAnimationName ?? throw new KeyNotFoundException();
			set
			{
				if(Animations is null)
					throw new KeyNotFoundException();

				Animations.ActiveAnimationName = value;
				return;
			}
		}

		/// <summary>
		/// The active animation.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if there is no active animation loaded.</exception>
		public Animation2D ActiveAnimation => Animations?.ActiveAnimation ?? throw new InvalidOperationException();

		/// <summary>
		/// The idle animation name of this collection.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string IdleAnimationName => Animations?.IdleAnimationName ?? throw new KeyNotFoundException();

		/// <summary>
		/// The idle animation.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if there is no idle animation loaded.</exception>
		public Animation2D IdleAnimation => Animations?.IdleAnimation ?? throw new InvalidOperationException();

		/// <summary>
		/// Obtains the animation named <paramref name="name"/> of this game object.
		/// </summary>
		/// <param name="name">The name of the animation.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not a valid animation name.</exception>
		public Animation2D this[string name] => Animations?[name] ?? throw new KeyNotFoundException();

		/// <summary>
		/// The animation names of this game object in no particular order.
		/// </summary>
		public IEnumerable<string> AnimationNames => Animations?.AnimationNames ?? [];

		/// <summary>
		/// The number of animations in this game object.
		/// </summary>
		public int Count => Animations?.Count ?? 0;

		/// <summary>
		/// An event called when this game object changes its active animation.
		/// </summary>
		public event AnimationSwap AnimationSwapped
		{
			add
			{
				if(Animations is null)
					return;

				Animations.AnimationSwapped += value;
				return;
			}

			remove
			{
				if(Animations is null)
					return;

				Animations.AnimationSwapped -= value;
				return;
			}
		}

		/// <summary>
		/// The animations of this game object.
		/// </summary>
		protected Animation2DCollection? Animations
		{get; set;}

		/// <summary>
		/// The current animation of this game object.
		/// </summary>
		protected Animation2D? Animation
		{get; set;}

		/// <summary>
		/// The additional animation transformation to apply to this game object.
		/// </summary>
		protected Matrix2D AnimationTransformation
		{get; set;}

		/// <summary>
		/// This is how this game object unsubscribes from the animation it is currently observing.
		/// </summary>
		protected IDisposable? Subscription
		{get; set;}
	}
}
