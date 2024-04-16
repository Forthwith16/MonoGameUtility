using GameEngine.GameComponents;
using GameEngine.Input;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExampleAStar
{
	public class ExampleAStar : Game
	{
		public ExampleAStar()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			Width = 50;
			Height = 50;
			TileWidth = 20;

			Graphics.PreferredBackBufferWidth = ((Width << 1) + 2) * TileWidth;
			Graphics.PreferredBackBufferHeight = Height * TileWidth;

			Rocks = new bool[Width,Height];
			Grass = new List<Point>(Width * Height);
			RockThreshold = 0.35f;

			Search = 0;
			
			ATerrain = new RectangleComponent[Width,Height];
			AQ = new PriorityQueue<FDistancePoint,float>(Comparer<float>.Create((a,b) => a.CompareTo(b)));
			APrev = new Dictionary<Point,FDistancePoint>();
			ADone = true;

			DTerrain = new RectangleComponent[Width,Height];
			DQ = new PriorityQueue<DistancePoint,int>(Comparer<int>.Create((a,b) => a.CompareTo(b)));
			DPrev = new Dictionary<Point,DistancePoint>();
			DDone = true;

			return;
		}

		protected override void LoadContent()
		{
			// Title will not persist if set in the constructor, so we'll set it here
			Window.Title = "A* vs. Dijkstra";

			Renderer = new SpriteBatch(GraphicsDevice);
			Input = new InputManager();
			
			Input.AddKeyInput("Escape",Keys.Escape);
			Input.AddEdgeTriggeredInput("Exit","Escape",true);

			Input.AddKeyInput("Space",Keys.Space);
			Input.AddEdgeTriggeredInput("Step","Space",true);

			Input.AddKeyInput("R",Keys.R);
			Input.AddEdgeTriggeredInput("Run","R",true);

			Input.AddKeyInput("F5",Keys.F5);
			Input.AddEdgeTriggeredInput("Reset","F5",true);

			GenerateRun();
			return;
		}

		private void GenerateRun()
		{
			Components.Clear();
			Components.Add(Input);

			GenerateTerrain();
			SetCaveToRocks();
			SelectStartEnd();

			InitializeAStar();
			InitializeDijkstra();
			
			return;
		}

		private void GenerateTerrain()
		{
			Random rand = new Random();

			for(int i = 0;i < Width;i++)
				for(int j = 0;j < Height;j++)
				{
					Rocks[i,j] = rand.NextSingle() <= RockThreshold;

					Components.Add(ATerrain[i,j] = new RectangleComponent(this,Renderer,TileWidth,TileWidth,Color.White));
					ATerrain[i,j].Translate(new Vector2(TileWidth * i,TileWidth * j));

					Components.Add(DTerrain[i,j] = new RectangleComponent(this,Renderer,TileWidth,TileWidth,Color.White));
					DTerrain[i,j].Translate(new Vector2(TileWidth * (Width + 2 + i),TileWidth * j));
				}

			return;
		}

		private void SetCaveToRocks()
		{
			Grass.Clear();
			
			for(int i = 0;i < Width;i++)
				for(int j = 0;j < Height;j++)
					if(Rocks[i,j])
					{
						ATerrain[i,j].Tint = Color.Black;
						DTerrain[i,j].Tint = Color.Black;
					}
					else
					{
						Grass.Add(new Point(i,j));

						ATerrain[i,j].Tint = Color.Green;
						DTerrain[i,j].Tint = Color.Green;
					}

			return;
		}

		private void SelectStartEnd()
		{
			if(Grass.Count < 2)
				throw new Exception("What are you doing?");

			Random rand = new Random();
			int temp;

			Source = Grass[temp = rand.Next(Grass.Count)];

			// Move the random choice already made to the front to get it out of the way
			Point ptemp = Grass[temp];
			Grass[temp] = Grass[0];
			Grass[0] = ptemp;

			Destination = Grass[1 + rand.Next(Grass.Count - 1)];

			ATerrain[Source.X,Source.Y].Tint = Color.Blue;
			ATerrain[Destination.X,Destination.Y].Tint = Color.Red;

			DTerrain[Source.X,Source.Y].Tint = Color.Blue;
			DTerrain[Destination.X,Destination.Y].Tint = Color.Red;

			return;
		}

		private void InitializeAStar()
		{
			AQ.Clear();
			APrev.Clear();

			AQ.Enqueue(new FDistancePoint(Source,0.0f),Source.Distance(Destination));
			APrev[Source] = new FDistancePoint(null,float.NaN);

			ADone = false;
			return;
		}

		private void InitializeDijkstra()
		{
			DQ.Clear();
			DPrev.Clear();

			DQ.Enqueue(new DistancePoint(Source,0),0);
			DPrev[Source] = new DistancePoint(null,int.MinValue);

			DDone = false;
			return;
		}

		protected override void Update(GameTime delta)
		{
			// Quitting always gets priority
			if(Input!["Exit"].CurrentDigitalValue)
				Exit();

			if(Input["Reset"].CurrentDigitalValue)
			{
				GenerateRun();

				// We'll eat any other inputs
				base.Update(delta);
				return;
			}

			if(Input["Step"].CurrentDigitalValue)
				Search = 1;
			
			if(Input["Run"].CurrentDigitalValue)
				if(Search > 0)
					Search = 0;
				else
					Search = int.MaxValue;

			if(Search > 0)
			{
				StepSearch();
				Search--;
			}

			base.Update(delta);
			return;
		}

		private void StepSearch()
		{
			if(!ADone)
				StepAStar();

			if(!DDone)
				StepDijkstra();

			return;
		}

		private void StepAStar()
		{
			FDistancePoint next;

			if(AQ.Count == 0 || (next = AQ.Dequeue()).Location!.Value == Destination)
			{
				MarkAPath();

				ADone = true;
				return;
			}

			// Skip visited vertices but don't count that as a step
			// Also note that next.Location is never null
			if(APrev.TryGetValue(next.Location!.Value,out FDistancePoint me) && me.Visited)
			{
				StepDijkstra();
				return;
			}

			// Mark ourselves as visited (structs are passed by value, so we need to assign it like so)
			me.Visited = true;
			APrev[next.Location.Value] = me;

			// Special case source coloring (can't get to destination here)
			if(next.Location.Value != Source)
				ATerrain[next.Location.Value.X,next.Location.Value.Y].Tint = Color.Cyan;

			Point[] neighbors = new Point[] {next.Location.Value + new Point(0,1),next.Location.Value + new Point(0,-1),next.Location.Value + new Point(1,0),next.Location.Value + new Point(-1,0)};
			
			foreach(Point p in neighbors)
			{
				// Grab the distance to p
				float pdistance = p.Distance(Destination);

				// If we are out of bounds or if we've already found a shorter path to this neighbor, skip it
				// If the neighbor has no predecessor, it is Source and we should skip it since you can't get to Source faster than starting there (this case should never come up)
				// We also skip rocks
				if(p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height || Rocks[p.X,p.Y] || APrev.TryGetValue(p,out FDistancePoint n) && (n.Visited || n.Distance <= pdistance))
					continue;

				AQ.Enqueue(new FDistancePoint(p,pdistance),pdistance);
				APrev[p] = new FDistancePoint(next.Location,pdistance);

				// We shouldn't overwrite visited colors here since we skip visited vertices
				if(p == Destination)
					ATerrain[p.X,p.Y].Tint = Color.PaleVioletRed;
				else
					ATerrain[p.X,p.Y].Tint = Color.Purple;
			}

			return;
		}

		private void MarkAPath()
		{
			ATerrain[Destination.X,Destination.Y].Tint = Color.Red;

			if(!APrev.ContainsKey(Destination))
				return;

			FDistancePoint p = APrev[Destination];

			while(p.Location.HasValue && p.Location.Value != Source)
			{
				ATerrain[p.Location.Value.X,p.Location.Value.Y].Tint = Color.Yellow;
				p = APrev[p.Location.Value];
			}

			return;
		}

		private void StepDijkstra()
		{
			DistancePoint next;

			if(DQ.Count == 0 || (next = DQ.Dequeue()).Location!.Value == Destination)
			{
				MarkDijkstraPath();

				DDone = true;
				return;
			}

			// Skip visited vertices but don't count that as a step
			// Also note that next.Location is never null
			if(DPrev.TryGetValue(next.Location!.Value,out DistancePoint me) && me.Visited)
			{
				StepDijkstra();
				return;
			}

			// Mark ourselves as visited (structs are passed by value, so we need to assign it like so)
			me.Visited = true;
			DPrev[next.Location.Value] = me;

			// Special case source coloring (can't get to destination here)
			if(next.Location.Value != Source)
				DTerrain[next.Location.Value.X,next.Location.Value.Y].Tint = Color.Cyan;

			Point[] neighbors = new Point[] {next.Location.Value + new Point(0,1),next.Location.Value + new Point(0,-1),next.Location.Value + new Point(1,0),next.Location.Value + new Point(-1,0)};
			
			foreach(Point p in neighbors)
			{
				// If we are out of bounds or if we've already found a shorter path to this neighbor, skip it
				// If the neighbor has no predecessor, it is Source and we should skip it since you can't get to Source faster than starting there (this case should never come up)
				// We also skip rocks
				if(p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height || Rocks[p.X,p.Y] || DPrev.TryGetValue(p,out DistancePoint n) && (n.Visited || n.Distance <= next.Distance + 1))
					continue;

				DQ.Enqueue(new DistancePoint(p,next.Distance + 1),next.Distance + 1);
				DPrev[p] = new DistancePoint(next.Location,next.Distance + 1);

				// We shouldn't overwrite visited colors here since we skip visited vertices
				if(p == Destination)
					DTerrain[p.X,p.Y].Tint = Color.PaleVioletRed;
				else
					DTerrain[p.X,p.Y].Tint = Color.Purple;
			}

			return;
		}

		private void MarkDijkstraPath()
		{
			DTerrain[Destination.X,Destination.Y].Tint = Color.Red;

			if(!DPrev.ContainsKey(Destination))
				return;

			DistancePoint p = DPrev[Destination];

			while(p.Location.HasValue && p.Location.Value != Source)
			{
				DTerrain[p.Location.Value.X,p.Location.Value.Y].Tint = Color.Yellow;
				p = DPrev[p.Location.Value];
			}

			return;
		}

		protected override void Draw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			Renderer!.Begin(SpriteSortMode.BackToFront);
			base.Draw(delta);
			Renderer.End();

			return;
		}

		private GraphicsDeviceManager Graphics;
		private SpriteBatch? Renderer;
		private InputManager? Input;

		private Point Source;
		private Point Destination;

		private int Search;

		private readonly bool[,] Rocks;
		private readonly List<Point> Grass;
		private readonly float RockThreshold;

		private readonly RectangleComponent[,] ATerrain;
		private readonly PriorityQueue<FDistancePoint,float> AQ;
		private readonly Dictionary<Point,FDistancePoint> APrev;
		private bool ADone;

		private readonly RectangleComponent[,] DTerrain;
		private readonly PriorityQueue<DistancePoint,int> DQ;
		private readonly Dictionary<Point,DistancePoint> DPrev;
		private bool DDone;

		private readonly int Width;
		private readonly int Height;
		private readonly int TileWidth;
	}

	public struct DistancePoint : IEquatable<DistancePoint>
	{
		public DistancePoint(Point? loc, int dist)
		{
			Location = loc;
			Distance = dist;
			Visited = false;

			return;
		}

		public static bool operator ==(DistancePoint a, DistancePoint b) => a.Location == b.Location;
		public static bool operator !=(DistancePoint a, DistancePoint b) => !(a == b);
		public bool Equals(DistancePoint other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is DistancePoint p && this == p;
		public override int GetHashCode() => HashCode.Combine(Location.GetHashCode(),Distance.GetHashCode());

		public override string? ToString() => Location is null ? "NaL" : Location.ToString();

		public Point? Location;
		public int Distance;
		public bool Visited;
	}

	public struct FDistancePoint : IEquatable<FDistancePoint>
	{
		public FDistancePoint(Point? loc, float dist)
		{
			Location = loc;
			Distance = dist;
			Visited = false;

			return;
		}

		public static bool operator ==(FDistancePoint a, FDistancePoint b) => a.Location == b.Location;
		public static bool operator !=(FDistancePoint a, FDistancePoint b) => !(a == b);
		public bool Equals(FDistancePoint other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is FDistancePoint p && this == p;
		public override int GetHashCode() => HashCode.Combine(Location.GetHashCode(),Distance.GetHashCode());

		public override string? ToString() => Location is null ? "NaL" : Location.ToString();

		public Point? Location;
		public float Distance;
		public bool Visited;
	}
}
