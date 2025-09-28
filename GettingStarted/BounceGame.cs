using GameEngine.Framework;
using GameEngine.Maths;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GettingStarted
{
	public class BounceGame : RenderTargetFriendlyGame
	{
		public BounceGame() : base()
		{
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			
			_left = 0;
			_right = 1600;
			_top = 0;
			_bottom = 1200;

			Random rand = new Random();
			velocity = 300.0f * new Vector2(rand.NextSingle(),rand.NextSingle());

			angle = 0.0f;
			a_velocity = 0.5f;

			Graphics.PreferredBackBufferWidth = _right;
			Graphics.PreferredBackBufferHeight = _bottom;

			return;
		}

		protected override void PreInitialize()
		{
			_image = Content.Load<Texture2D>("sword");
			position = new Vector2(_right - _image.Width,_bottom - _image.Height) / 2.0f;

			return;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteRenderer(this);
			return;
		}

		protected override void Update(GameTime delta)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			
			position += velocity * (float)delta.ElapsedGameTime.TotalSeconds;
			angle += a_velocity * (float)delta.ElapsedGameTime.TotalSeconds;

			transform = Matrix2D.Translation(position) * Matrix2D.Rotation(angle,new Vector2(_image!.Width,_image.Height) / 2.0f);

			CheckPoint(Vector2.Zero);
			CheckPoint(new Vector2(0.0f,_image.Height));
			CheckPoint(new Vector2(_image.Width,0.0f));
			CheckPoint(new Vector2(_image.Width,_image.Height));

			base.Update(delta);
			return;
		}

		protected void CheckPoint(Vector2 v)
		{
			v = transform * v;

			if(v.X < _left)
			{
				velocity.X = -velocity.X;
				a_velocity = -a_velocity;
			}
			
			if(v.X >= _right)
			{
				velocity.X = -velocity.X;
				a_velocity = -a_velocity;
			}

			if(v.Y < _top)
			{
				velocity.Y = -velocity.Y;
				a_velocity = -a_velocity;
			}

			if(v.Y >= _bottom)
			{
				velocity.Y = -velocity.Y;
				a_velocity = -a_velocity;
			}

			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch?.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearClamp,null,null,null,null);

			transform.Decompose(out Vector2 t,out float r,out Vector2 s);
			_spriteBatch?.Draw(_image!,t,null,Color.White,-r,Vector2.Zero,s,SpriteEffects.None,1.0f);

			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			_spriteBatch?.End();
			return;
		}

		private SpriteRenderer? _spriteBatch;
		private Texture2D? _image;
		private Matrix2D transform;

		private Vector2 position;
		private Vector2 velocity;
		private float angle;
		private float a_velocity;

		private int _left;
		private int _right;
		private int _top;
		private int _bottom;
	}
}
