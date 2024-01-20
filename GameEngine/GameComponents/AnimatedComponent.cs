using GameEngine.Events;
using GameEngine.Maths;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A component that draws an animated image.
	/// </summary>
	public class AnimatedComponent : ImageComponent, IObserver<TimeEvent>
	{
		/// <summary>
		/// Creates an animated component from a not yet loaded animation.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="resource">The path to the animation for this component. The animation can be changed later.</param>
		public AnimatedComponent(Game game, SpriteBatch? renderer, string resource) : base(game,renderer,resource)
		{return;}

		/// <summary>
		/// Creates an animated component from an already loaded animation.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="animation">The animation for this component (this can be changed later).</param>
		public AnimatedComponent(Game game, SpriteBatch? renderer, Animation2D animation) : base(game,renderer,animation.Source.Source)
		{
			Animation = animation;
			return;
		}

		protected override void LoadContent()
		{
			// If we have a resource, then we need to load an animation
			if(Resource is not null)
				Animation = new Animation2D(Game.Content.Load<Animation>(Resource));

			return;
		}

		public override void Update(GameTime delta)
		{
			Animation.Update(delta); // We don't need to update the source rectangle since that happens through our subscription
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
			// Regardless of the event, we'll just assign these and be done with it
			SourceRect = Animation.CurrentSource; 
			AnimationTransformation = Animation[Animation.CurrentFrame].Transformation;

			return;
		}

		/// <summary>
		/// The animation of this component.
		/// </summary>
		/// <remarks>Setting the animation here will call its Initialize if it has not been initialized yet, but this will not call Dispose on old animations.</remarks>
		public Animation2D Animation
		{
			get => _a!; // This can only ever be null before it's assigned in the constructor
			
			set
			{
				// If this is a self-assignment, do nothing
				if(ReferenceEquals(_a,value))
					return;

				// Unsubscribe
				if(Subscription is not null)
					Subscription.Dispose();

				// The user is responsible for disposing of old animations early if they are meant to be disposed, so we can just make this assignment
				_a = value;
				
				if(!_a.Initialized)
					_a.Initialize();

				// Subscribe
				Subscription = _a.Subscribe(this);

				// Set up the new source information
				Source = _a.SourceTexture;
				SourceRect = _a.CurrentSource; // We'll do this here in case we managed to catch ourselves between an Update and a Draw call
				AnimationTransformation = Animation[Animation.CurrentFrame].Transformation;

				return;
			}
		}

		protected Animation2D? _a;

		/// <summary>
		/// This is how this component unsubscribes from the animation it is currently observing.
		/// </summary>
		protected IDisposable? Subscription
		{get; set;}

		/// <summary>
		/// The additional animation transformation to apply to this component.
		/// </summary>
		protected Matrix2D AnimationTransformation
		{get; set;}
	}
}
