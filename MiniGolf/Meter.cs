using GameEngine.GameObjects;
using GameEngine.Sprites;
using GameEngine.Texture;
using Microsoft.Xna.Framework;
using System;

namespace MiniGolf
{
	/// <summary>
	/// Makes a meter that goes up and down.
	/// </summary>
	public class Meter : RectangleGameObject
	{
		/// <summary>
		/// Creates a meter with a solid color.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="c">The color to draw with.</param>
		public Meter(SpriteRenderer? renderer, int w, int h, Color c) : this(renderer,w,h,ColorFunctions.SolidColor(c))
		{return;}
		
		/// <summary>
		/// Creates a meter with a textured generated according to <paramref name="func"/>.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="func">The color generating function.</param>
		public Meter(SpriteRenderer? renderer, int w, int h, ColorFunction func) : base(renderer,w,h,func) // The meter is always full initially, so we can ignore it
		{
			MeterPercentage = 1.0f;
			Function = (x,y) => MeterPercentage >= (float)x / Width ? func(x,y) : Color.Transparent;
			StaleTexture = false;

			return;
		}

		public override void Update(GameTime gameTime)
		{
			if(StaleTexture)
			{
				Color[] data = new Color[Width * Height];
			
				for(int x = 0;x < Width;x++)
					for(int y = 0;y < Height;y++)
						data[y * Width + x] = Function(x,y);
				
				Source!.SetData(data);
				StaleTexture = false;
			}

			base.Update(gameTime);
			return;
		}

		/// <summary>
		/// The amount of the meter that is filled.
		/// This value is clamped between 0 and 1.
		/// </summary>
		public float MeterPercentage
		{
			get => _mp;

			set
			{
				float _lmp = _mp;
				_mp = Math.Clamp(value,0f,1f);

				StaleTexture = _mp != _lmp;
				return;
			}
		}

		protected float _mp;
		
		/// <summary>
		/// How we color our texture.
		/// </summary>
		protected ColorFunction Function
		{get; set;}

		/// <summary>
		/// If true, then the texture is stale and needs updating
		/// </summary>
		protected bool StaleTexture
		{get; set;}
	}
}
