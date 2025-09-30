using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input;
using GameEngine.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ExampleCaveGeneration
{
	public class ExampleCaveGenerationGame : RenderTargetFriendlyGame
	{
		public ExampleCaveGenerationGame() : base()
		{
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			Width = 50;
			Height = 50;
			TileWidth = 20;

			Graphics.PreferredBackBufferWidth = Width * TileWidth;
			Graphics.PreferredBackBufferHeight = Height * TileWidth;

			PreviousRocks = new bool[Width,Height];
			Rocks = new bool[Width,Height];
			Cave = new RectangleGameObject[Width,Height];

			RockOdds = 0.5f;
			RockSurvivalThreshold = 5;

			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteRenderer(GraphicsDevice);
			Components.Add(Input = new InputManager());
			
			Input.AddKeyInput("Escape",Keys.Escape);
			Input.AddEdgeTriggeredInput("Exit","Escape",true);

			Input.AddKeyInput("Space",Keys.Space);
			Input.AddEdgeTriggeredInput("Step","Space",true);

			Random rand = new Random();

			for(int i = 0;i < Width;i++)
				for(int j = 0;j < Height;j++)
				{
					PreviousRocks[i,j] = Rocks[i,j] = rand.NextDouble() <= RockOdds;

					Components.Add(Cave[i,j] = new RectangleGameObject(Renderer,TileWidth,TileWidth,Color.White));
					Cave[i,j].Translate(new Vector2(TileWidth * i,TileWidth * j));
				}

			SetCaveToRocks();
			return;
		}

		private void SetCaveToRocks()
		{
			for(int i = 0;i < Width;i++)
				for(int j = 0;j < Height;j++)
					Cave[i,j].Tint = Rocks[i,j] ? Color.DarkGray : Color.Green;

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Input!["Exit"].CurrentDigitalValue)
				Exit();

			if(Input["Step"].CurrentDigitalValue)
				StepSimulation();

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			Renderer!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearClamp,null,null,null,null);

			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			Renderer!.End();
			return;
		}

		private void StepSimulation()
		{
			for(int i = 0;i < Width;i++)
				for(int j = 0; j < Height;j++)
					Rocks[i,j] = Rockify(i,j);

			for(int i = 0;i < Width;i++)
				for(int j = 0; j < Height;j++)
					PreviousRocks[i,j] = Rocks[i,j];

			SetCaveToRocks();
			return;
		}

		private bool Rockify(int i, int j)
		{
			int count = 0;

			for(int x = -1;x <= 1;x++)
				for(int y = -1;y <= 1;y++)
					if(WasRock(i + x,j + y))
						count++;

			return count >= RockSurvivalThreshold;
		}

		private bool WasRock(int i, int j) => i < 0 || i >= Width || j < 0 || j >= Height || PreviousRocks[i,j];

		private SpriteRenderer? Renderer;
		private InputManager? Input;

		private bool[,] PreviousRocks;
		private bool[,] Rocks;
		private RectangleGameObject[,] Cave;

		private readonly int Width;
		private readonly int Height;
		private readonly int TileWidth;

		private readonly float RockOdds;
		private readonly int RockSurvivalThreshold;
	}
}
