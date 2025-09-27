using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Texture;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MiniGolf
{
	public class MiniGolfGame : RenderTargetFriendlyGame
	{
		public MiniGolfGame()
		{
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1000;
			Graphics.PreferredBackBufferHeight = 1000;

			Colliders = new List<CollisionBox>();
			captured = false;
			considering = false;
			clap = false;
			delay = 0.0;
			strokes = 0;

			Input = new InputManager();
			return;
		}

		protected override void LoadContent()
		{
			// Set up input
			Components.Add(Input);
			Input.AddKeyInput("Esc",Keys.Escape);
			Input.AddMouseButtonInput("Left",MouseButton.Left);
			Input.AddMouseAxisInput("MX",true,false);
			Input.AddMouseAxisInput("MY",false,false);

			// Set up the renderer
			Renderer = new SpriteBatch(GraphicsDevice);
			 
			// Load the sound effects
			Hit = Content.Load<SoundEffect>("hit");
			Sink = Content.Load<SoundEffect>("hole");
			Clap = Content.Load<SoundEffect>("clap");
			Bounce = Content.Load<SoundEffect>("bounce");

			// Add the course behind everything
			Components.Add(new ImageGameObject(Renderer,"hole1"));

			// Add in the stoke counter
			Components.Add(Strokes = new TextGameObject(Renderer,Content.Load<SpriteFont>("Times New Roman"),"Strokes: " + strokes,Color.Black));
			Strokes.Translate(new Vector2(1000.0f,1000.0f) - Strokes.MessageDimensions);

			// One temp variable for all
			CollisionBox box;

			// Wall colliders
			// Leftmost wall
			Colliders.Add(box = new CollisionBox(Renderer,10,800,Color.Yellow));
			box.Translate(100.0f,100.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(110.0f + g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);

				Bounce.Play();
				return;
			};
			
			// Middle left lower wall
			Colliders.Add(box = new CollisionBox(Renderer,10,348,Color.Yellow));
			box.Translate(400.0f,552.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(400.0f - g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);

				Bounce.Play();
				return;
			};

			// Middle left upper wall
			Colliders.Add(box = new CollisionBox(Renderer,10,171,Color.Yellow));
			box.Translate(400.0f,327.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(400.0f - g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);

				Bounce.Play();
				return;
			};
			
			// Middle lowest wall
			Colliders.Add(box = new CollisionBox(Renderer,206,10,Color.Yellow));
			box.Translate(402.0f,550.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,550.0f - g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Middle lower wall
			Colliders.Add(box = new CollisionBox(Renderer,206,10,Color.Yellow));
			box.Translate(402.0f,490.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,500.0f + g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Middle upper wall
			Colliders.Add(box = new CollisionBox(Renderer,206,10,Color.Yellow));
			box.Translate(402.0f,325.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,325.0f - g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Middle highest wall
			Colliders.Add(box = new CollisionBox(Renderer,780,10,Color.Yellow));
			box.Translate(110.0f,100.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,110.0f + g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Middle right lower wall
			Colliders.Add(box = new CollisionBox(Renderer,10,148,Color.Yellow));
			box.Translate(600.0f,552.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(610.0f + g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Middle right upper wall
			Colliders.Add(box = new CollisionBox(Renderer,10,171,Color.Yellow));
			box.Translate(600.0f,327.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(610.0f + g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Rightmost wall
			Colliders.Add(box = new CollisionBox(Renderer,10,600,Color.Yellow));
			box.Translate(890.0f,100.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(890.0f - g.Radius - g.Position.X,0.0f);
				g.Velocity = elasticity * new Vector2(-g.Velocity.X,g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Left bottom wall
			Colliders.Add(box = new CollisionBox(Renderer,310,10,Color.Yellow));
			box.Translate(100.0f,900.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,900.0f - g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Right bottom wall
			Colliders.Add(box = new CollisionBox(Renderer,300,10,Color.Yellow));
			box.Translate(600.0f,700.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Translate(0.0f,700.0f - g.Radius - g.Position.Y);
				g.Velocity = elasticity * new Vector2(g.Velocity.X,-g.Velocity.Y);
				
				Bounce.Play();
				return;
			};

			// Green colliders
			// Left green
			Colliders.Add(box = new CollisionBox(Renderer,290,790,Color.Green));
			box.Translate(110.0f,110.0f);
			box.KineticFriction = kf;
			box.Resolve += (me,g,delta) =>
			{
				float energy = 0.5f * g.Velocity.LengthSquared();
				float friction_loss = (float)delta.ElapsedGameTime.TotalSeconds * g.Velocity.Length() * me.KineticFriction;

				if(energy < friction_loss)
				{
					g.Velocity = Vector2.Zero;
					return;
				}

				float new_speed = MathF.Sqrt(2.0f * (energy - friction_loss));
				
				g.Velocity = g.Velocity.Normalized() * new_speed;
				return;
			};

			// Right green
			Colliders.Add(box = new CollisionBox(Renderer,280,590,Color.Green));
			box.Translate(610.0f,110.0f);
			box.KineticFriction = kf;
			box.Resolve += (me,g,delta) =>
			{
				float energy = 0.5f * g.Velocity.LengthSquared();
				float friction_loss = (float)delta.ElapsedGameTime.TotalSeconds * g.Velocity.Length() * me.KineticFriction;

				if(energy < friction_loss)
				{
					g.Velocity = Vector2.Zero;
					return;
				}

				float new_speed = MathF.Sqrt(2.0f * (energy - friction_loss));
				
				g.Velocity = g.Velocity.Normalized() * new_speed;
				return;
			};

			// Slope colliders
			// Upper right downhill
			Colliders.Add(box = new CollisionBox(Renderer,210,215,Color.Red));
			box.Translate(400.0f,110.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Velocity += new Vector2(100.0f,0.0f) * (float)delta.ElapsedGameTime.TotalSeconds; // Slopes right
				return;
			};

			// Lower right downhill
			Colliders.Add(box = new CollisionBox(Renderer,210,50,Color.Red));
			box.Translate(400.0f,500.0f);
			box.Resolve += (me,g,delta) =>
			{
				g.Velocity += new Vector2(100.0f,0.0f) * (float)delta.ElapsedGameTime.TotalSeconds; // Slopes right
				return;
			};

			// If we're debugging, add all the colliders to our components
			#if DEBUG
			foreach(CollisionBox collider in Colliders)
				Components.Add(collider);
			#endif

			// Create the hole
			Components.Add(Goal = new Hole(this,Renderer,30));
			Goal.Translate(730.0f,510.0f);
			Goal.Resolve += (me,g,delta) =>
			{
				// If the hole contains the ball's center, we need to decide what happens
				if(me.ContainsPoint(g.Position))
					if(g.Velocity.LengthSquared() <= max_capture_speed) // If we're going slow enough, we can capture the ball
						Goal.Capture(Goal,g);
					else
						g.Velocity *= 0.99f; // We'll simulate falling and hitting the edge by decreasing speed the longer we're over the hole
				else // Otherwise, we're on the lip of the hole and we should increase the velocity toward the center of the hole
					g.Velocity += (Goal.Position - g.Position).Normalized() * 4.0f;

				return;
			};
			Goal.Capture += (me,ball) =>
			{
				Components.Remove(ball);

				// Play capture sound
				Sink.Play();

				// Play golf clap
				clap = true;
				delay = Sink.Duration.TotalSeconds;

				captured = true;
				return;
			};

			// Create the power meter (add it before the ball so that it ends up below
			Components.Add(PowerMeter = new Meter(Renderer,power_meter_length,golf_ball_diameter << 1,(x,y) =>
			{
				float x_percentage = x / (float)power_meter_length;
				float y_percentage = MathF.Abs(y - golf_ball_diameter) / golf_ball_diameter;

				// The easy case is the inner rectangle of a trapezoid
				if(y_percentage <= 0.25f)
					return Color.Lerp(Color.LightGreen,Color.Red,x_percentage);

				// The hard case is the outer triangles, but we've reduced this to only the positive case
				y_percentage = (y_percentage - 0.25f) * 2.0f;

				return y_percentage > x_percentage ? Color.Transparent : Color.Lerp(Color.LightGreen,Color.Red,x_percentage);
			}));
			PowerMeter.Translate(golf_ball_diameter >> 1,-golf_ball_diameter >> 1);
			PowerMeter.MeterPercentage = 0.0f;
			PowerMeter.Visible = false;
			
			// Create the golf ball
			Components.Add(Ball = new GolfBall(Renderer,ColorFunctions.GenerateTexture(this,golf_ball_diameter,golf_ball_diameter,ColorFunctions.Ellipse(golf_ball_diameter,golf_ball_diameter,Color.White))));
			Ball.Translate(250.0f,850.0f);

			// Make sure the power meter is always centered on the golf ball
			PowerMeter.Parent = Ball;

			return;
		}

		protected override void Update(GameTime delta)
		{
			// Update everything first
			base.Update(delta);

			// If we intend to clap, get ready
			if(clap)
			{
				delay -= delta.ElapsedGameTime.TotalSeconds;

				if(delay <= 0.0)
				{
					clap = false;
					Clap!.Play();
				}
			}

			// If we want to quit, do so
			if(Input["Esc"].CurrentDigitalValue)
				Exit();
			
			// If the ball has been captured, we're done
			if(captured)
				return;

			// If the ball is still, we need to hit it
			if(Ball!.Still)
			{
				// If we're already considering how we want to hit the ball, consider if we should keep considering
				if(considering)
				{
					// We are still considering, so update the power meter
					if(Input["Left"].CurrentDigitalValue)
					{
						// Make the meter visible if it's not already
						// We do this here so that it has a chance to update the stale meter before drawing it
						PowerMeter!.Visible = true;

						// First get the direction of the mouse from the ball center
						Vector2 power_vector = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue) - Ball.Position;
						
						// We rotate the power meter to the mouse's direction and then translate it
						PowerMeter.Transform = Matrix2D.Translation(golf_ball_diameter >> 1,-golf_ball_diameter >> 1) * Matrix2D.Rotation(MathF.Atan2(-power_vector.Y,power_vector.X),0.0f,golf_ball_diameter);

						// Draw back the power meter
						PowerMeter.MeterPercentage = power_vector.Length() / power_meter_length;
					}
					else // If we let go of the mouse, we hit the ball
					{
						// Hit the ball
						Vector2 power_vector = -(new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue) - Ball.Position);
						float power = Math.Clamp(power_vector.Length() / power_meter_length,0.0f,1.0f);
						
						Ball.Velocity = power_vector.Normalized() * power * max_power;

						// Update the strokes
						Strokes!.Text = "Strokes: " + ++strokes;
						Strokes.Transform = Matrix2D.Translation(new Vector2(1000.0f,1000.0f) - Strokes.MessageDimensions);

						// Play the sound
						Hit!.Play();

						considering = false;
						PowerMeter!.Visible = false;
					}
				}
				else if(Input["Left"].CurrentDigitalValue && Ball.ContainsPoint(new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue)))
				{
					considering = true;
					PowerMeter!.MeterPercentage = 0.0f;
				}

				return;
			}

			// Otherwise, update the ball according to its velocity
			Ball!.Translate(Ball.Velocity * (float)delta.ElapsedGameTime.TotalSeconds);
			
			foreach(ICollider collider in Colliders)
				if(collider.IntersectsCircle(Ball.Position,Ball.Radius))
					collider.Resolve(collider,Ball,delta);

			if(Goal!.IntersectsCircle(Ball.Position,Ball.Radius))
				Goal.Resolve(Goal,Ball,delta);

			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			Renderer!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearWrap,null,null,null,null);
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			Renderer!.End();
			return;
		}
		
		protected InputManager Input;
		protected SpriteBatch? Renderer;
		protected List<CollisionBox> Colliders;
		protected GolfBall? Ball;
		protected Meter? PowerMeter;
		protected Hole? Goal;
		protected TextGameObject? Strokes;
		
		protected SoundEffect? Hit;
		protected SoundEffect? Sink;
		protected SoundEffect? Clap;
		protected SoundEffect? Bounce;

		protected bool captured;
		protected bool considering;
		protected bool clap;
		protected double delay;

		protected int strokes;

		public const float kf = 20.0f;
		public const float elasticity = 0.8f;
		public const int golf_ball_diameter = 16;
		public const int power_meter_length = 200;
		public const float max_power = 500.0f;
		public const float max_capture_speed = 40000.0f;
	}
}
