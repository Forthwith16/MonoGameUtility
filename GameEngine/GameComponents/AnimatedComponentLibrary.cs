using GameEngine.Events;
using GameEngine.Maths;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A component that draws one of a selection of animations.
	/// </summary>
	public class AnimatedComponentLibrary : ImageComponent, IObserver<TimeEvent>, IObservable<TimeEvent>
	{
		/// <summary>
		/// Creates an animated component from a not yet loaded animation collection.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="resource">The path to the animations for this component.</param>
		public AnimatedComponentLibrary(Game game, SpriteBatch? renderer, string resource) : base(game,renderer,resource)
		{
			// We'll load these in LoadContent
			Animations = null!;
			Animation = null!;

			return;
		}

		/// <summary>
		/// Creates an animated component from an already loaded animation collection.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="animations">The animations for this component.</param>
		public AnimatedComponentLibrary(Game game, SpriteBatch? renderer, Animation2DCollection animations) : base(game,renderer)
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

		protected override void LoadContent()
		{
			// If we have a resource, then we need to load our animations
			if(Resource is not null)
			{
				Animations = Game.Content.Load<Animation2DCollection>(Resource);

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

			Animations.Initialize(); // We always do this regardless of if we needed to load the animations, which is why it's not in Initialize
			return;
		}

		public override void Update(GameTime delta)
		{
			Animations.Update(delta); // All other updates happen through our subscriptions
			return;
		}
		
		/// <summary>
		/// Draws this image component.
		/// </summary>
		/// <param name="delta">The elapsed time since the last draw.</param>
		public override void Draw(GameTime delta)
		{
			if(Renderer is null)
				return;
			
			// First grab the transformation components we'll need to draw this with.
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
			SourceRect = Animation.CurrentSource; 
			AnimationTransformation = Animation[Animation.CurrentFrame].Transformation;

			return;
		}

		public IDisposable Subscribe(IObserver<TimeEvent> observer)
		{return Animations.Subscribe(observer);}

		/// <summary>
		/// The active animation name.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string ActiveAnimationName
		{
			get => Animations.ActiveAnimationName;
			set => Animations.ActiveAnimationName = value;
		}

		/// <summary>
		/// The active animation.
		/// </summary>
		public Animation2D ActiveAnimation => Animations.ActiveAnimation;

		/// <summary>
		/// The idle animation name of this collection.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if the set value is not a valid animation name.</exception>
		public string IdleAnimationName => Animations.IdleAnimationName;

		/// <summary>
		/// The idle animation.
		/// </summary>
		public Animation2D IdleAnimation => Animations.IdleAnimation;

		/// <summary>
		/// Obtains the animation named <paramref name="name"/> of this collection.
		/// </summary>
		/// <param name="name">The name of the animation.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not a valid animation name.</exception>
		public Animation2D this[string name] => Animations[name];

		/// <summary>
		/// The animation names of this collection in no particular order.
		/// </summary>
		public IEnumerable<string> AnimationNames => Animations.AnimationNames;

		/// <summary>
		/// The number of animations in this collection.
		/// </summary>
		public int Count => Animations.Count;

		/// <summary>
		/// An event called when this collection changes its active animation.
		/// </summary>
		public event AnimationSwap AnimationSwapped
		{
			add
			{
				Animations.AnimationSwapped += value;
				return;
			}

			remove
			{
				Animations.AnimationSwapped -= value;
				return;
			}
		}

		/// <summary>
		/// The animations of this component.
		/// </summary>
		protected Animation2DCollection Animations
		{get; set;}

		/// <summary>
		/// The current animation of this component.
		/// </summary>
		protected Animation2D Animation
		{get; set;}

		/// <summary>
		/// The additional animation transformation to apply to this component.
		/// </summary>
		protected Matrix2D AnimationTransformation
		{get; set;}

		/// <summary>
		/// This is how this component unsubscribes from the animation it is currently observing.
		/// </summary>
		protected IDisposable? Subscription
		{get; set;}
	}
}
