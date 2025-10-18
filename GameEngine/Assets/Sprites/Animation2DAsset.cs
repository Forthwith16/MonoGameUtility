using GameEngine.Assets.Serialization;
using GameEngine.Framework;
using GameEngine.Maths;
using GameEngine.Resources;
using GameEngine.Resources.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Assets.Sprites
{
	/// <summary>
	/// Contains the base raw asset data of an Animation2D.
	/// </summary>
	[AssetLoader(typeof(BaseAnimationAsset<Animation2DAsset>),nameof(FromFile))]
	public partial class Animation2DAsset
	{
		protected override void Serialize(string path, string root, bool overwrite_dependencies = false)
		{
			// If our source sprite sheet doesn't exist where we expect it to, write it out to that location (if we have it)
			if(StandardShouldSerializeCheck(Source,Path.GetDirectoryName(path) ?? "",overwrite_dependencies,out string? dst))
				try
				{Asset.CreateAsset<SpriteSheetAsset>(Source.Resource!)?.SaveToDisc(Path.GetRelativePath(root,dst),root,overwrite_dependencies);}
				catch
				{} // If something goes wrong, we don't want to crash horribly
			
			// Now we can serialize our sprite sheet proper
			this.SerializeJson(path);
			
			return;
		}

		protected override void AdjustFilePaths(string path, string root)
		{
			StandardAssetSourcePathAdjustment(Source,path,Source.Unnamed ? GenerateFreeFile(Path.Combine(root,path),".ss") : "");
			return;
		}

		protected override IResource? Instantiate(GraphicsDevice? g)
		{
			// We must have a sprite sheet to instantiate an animation
			// Similarly, we must have the frame data
			if(Source.Resource is null || Frames is null || Frames.Length == 0)
				return null;

			// We will have to trust that the textures all exist in the sprite sheet, but we can check that the durations are nontrivial while we construct some timing info
			float[] segs = new float[Frames.Length + 1];
			segs[0] = 0.0f;

			int i = 0;

			foreach(float d in Frames.Select(f => f.Duration))
			{
				segs[++i] = segs[i - 1] + d;

				if(d <= 0.0f)
					return null;
			}

			// We need to calculate some timing information to send off to the animation constructor
			float start;
			
			// Assign the start time
			if(StartTime is null)
				if(StartFrame is null)
					start = 0.0f; // If we have neither start time, then we default to 0
				else
					start = segs[StartFrame.Value];
			else
				start = StartTime.Value;

			// If we have the wrong loop data, we'll have to fix that
			float loop_start;
			
			if(LoopStart is null)
				if(LoopFrameStart is null)
					loop_start = 0.0f;
				else
					if(LoopFrameStart < 0 || LoopFrameStart >= Frames.Length)
						loop_start = -1.0f; // This is straight up not allowed, so give it a value that will cause problems
					else
						loop_start = segs[LoopFrameStart.Value];
			else
				loop_start = LoopStart.Value;

			float loop_end;
			
			if(LoopEnd is null)
				if(LoopFrameEnd is null)
					loop_end = segs[Frames.Length];
				else
					if(LoopFrameEnd < 0 || LoopFrameEnd >= Frames.Length)
						loop_end = -2.0f; // This is straight up not allowed, so give it a value that will cause problems
					else
						loop_end = segs[LoopFrameEnd.Value + 1];
			else
				loop_end = LoopEnd.Value;
			
			// All that we have left to do is check that the start time and loop data makes sense
			if(start < 0.0f || start > segs[Frames.Length] || loop_end <= loop_start || loop_start < 0.0f || loop_end > segs[Frames.Length])
				return null;

			// We return an Animation with the understanding that there is an implicit converstion to Animation2D if that is what we're asking for
			// If we're just asking for an Animation (which is the ideal scenario, since that's the reusable form), then we have no need to fret
			return new Animation("",Source.Resource,Frames.Select(f => f.Duration),Frames.Select(f => f.Sprite),Frames.Select(f => Matrix2D.FromPositionRotationScaleOrigin(f.Translation,f.Rotation,f.Scale,f.Origin)),start,Loops,loop_start,loop_end);
		}
	}
	
	[JsonConverter(typeof(JsonAnimation2DAssetConverter))]
	public partial class Animation2DAsset : BaseAnimationAsset<Animation2DAsset>
	{
		/// <summary>
		/// Creates a Animation2D with garbage values.
		/// </summary>
		public Animation2DAsset() : base(AnimationType.Animation2D)
		{
			Source = new AssetSource<SpriteSheet>();
			return;
		}

		/// <summary>
		/// Creates an animation asset from <paramref name="a"/>.
		/// </summary>
		/// <param name="a">The animation to turn into its asset form.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="a"/> is missing a sprite sheet source.</exception>
		protected internal Animation2DAsset(Animation2D a) : this(a.SourceData)
		{return;}

		/// <summary>
		/// Creates an animation asset from <paramref name="a"/>.
		/// </summary>
		/// <param name="a">The animation to turn into its asset form.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="a"/> does not represent an 2D animation or is missing a sprite sheet source.</exception>
		protected internal Animation2DAsset(Animation a) : base(AnimationType.Animation2D)
		{
			// We must be a 2D animation
			if(!a.IsAnimation2D)
				throw new ArgumentException("Animation2DAssets can only be created out of 2D animations.");

			// We have to have a source sprite sheet to make an asset out of an animation
			if(a.Source is null)
				throw new ArgumentException("Animations2DAssets must have a source sprite sheet.");
			else
				Source = new AssetSource<SpriteSheet>(a.Source);

			// Copy the frames now
			// Note that a is only allowed to exist if there is at least one frame and everything is defined
			Frames = new Animation2DKeyFrame[a.FrameCount];
			float start = 0.0f;
			
			for(int i = 0;i < Frames.Length;i++)
			{
				Animation2DFrame f = a.Get2DFrame(i);

				// We try to align frame info with times and, if we fail to do so, assign nonframe info in its place later
				if(MathF.Abs(start - a.Start) < GlobalConstants.EPSILON)
					StartFrame = i; // Start frames go by the start of a frame, and start is currently the start of the current frame

				if(MathF.Abs(start - a.LoopStart) < GlobalConstants.EPSILON)
					LoopFrameStart = i; // Start frames go by the start of a frame, and start is currently the start of the current frame

				// Now we assign the ith frame data
				Frames[i] = new Animation2DKeyFrame();

				start += Frames[i].Duration = f.Duration;
				Frames[i].Sprite = f.SpriteIndex;

				// We need to decompose the matrix into something that is equivalent to what it started as
				if(!f.Transformation.Decompose(out Vector2 t,out float r,out Vector2 s))
					throw new ArgumentException("A frame transform could not be decomposed into its component parts.");

				Frames[i].Translation = t;
				Frames[i].Rotation = r;
				Frames[i].Scale = s;
				Frames[i].Origin = Vector2.Zero;

				// We try to align frame info with times and, if we fail to do so, assign nonframe info in its place later
				if(MathF.Abs(start - a.LoopEnd) < GlobalConstants.EPSILON)
					LoopFrameEnd = i; // The loop frame end goes by the end time of the frame, not the beginning, and start is at the end now
			}

			// We now need to assign timing info that didn't get assigned in the loop
			if(StartFrame is null)
				StartTime = a.Start;

			if(LoopFrameStart is null)
				LoopStart = a.LoopStart;

			if(LoopFrameEnd is null)
				LoopEnd = a.LoopEnd;

			return;
		}

		/// <summary>
		/// The source sprite sheet for the animation.
		/// </summary>
		public AssetSource<SpriteSheet> Source;

		/// <summary>
		/// The keyframes of this animation.
		/// </summary>
		public Animation2DKeyFrame[]? Frames;

		/// <summary>
		/// The time this animation starts from.
		/// <para/>
		/// If neither this nor StartFrame are defined, the start time will default to 0.
		/// </summary>
		public float? StartTime;

		/// <summary>
		/// The frame to start this animation from.
		/// This will be convereted into a time rather than remain as a frame.
		/// <para/>
		/// This value is ignored if StartTime is defined.
		/// </summary>
		public int? StartFrame;

		/// <summary>
		/// If true, the animation loops.
		/// If false, the animation does not loop.
		/// </summary>
		/// <remarks>Since booleans default to false, we'll just use that default value as the default value for looping and not bother with letting this be nullable.</remarks>
		public bool Loops;

		/// <summary>
		/// The time when the loop start should be set to regardless of whether or not the animation loops.
		/// <para/>
		/// If neither this nor LoopFrameStart are defined, the loop start will default to 0.
		/// </summary>
		public float? LoopStart;

		/// <summary>
		/// The frame when the loop start should be set to regardless of whether or not the animation loops.
		/// This will be converted into a time rather than remain as a frame.
		/// <para/>
		/// This value is ignored if LoopStart is defined.
		/// </summary>
		public int? LoopFrameStart;

		/// <summary>
		/// The time when the loop start should be set to regardless of whether or not the animation loops.
		/// <para/>
		/// If neither this nor LoopFrameEnd are defined, the loop end will default to the end of the animation.
		/// </summary>
		public float? LoopEnd;

		/// <summary>
		/// The frame when the loop end should be set to regardless of whether or not the animation loops.
		/// This will be converted into a time rather than remain as a frame.
		/// Note that this uses the frame's ending time.
		/// So for example, a loop from frame 0 to frame 0 loops across the entire 0th frame.
		/// <para/>
		/// This value is ignored if LoopStart is defined.
		/// </summary>
		public int? LoopFrameEnd;
	}

	/// <summary>
	/// The information we need for an animation keyframe.
	/// </summary>
	[JsonConverter(typeof(JsonAnimation2DKeyFrameConverter))]
	public class Animation2DKeyFrame
	{
		/// <summary>
		/// The index of the sprite for this animation frame.
		/// </summary>
		public int Sprite;

		/// <summary>
		/// The duration of this keyframe.
		/// </summary>
		public float Duration;

		/// <summary>
		/// The translation component of this frame's transformation matrix.
		/// </summary>
		public Vector2 Translation;

		/// <summary>
		/// The scale component of this frame's transformation matrix.
		/// </summary>
		public Vector2 Scale = Vector2.One;

		/// <summary>
		/// The rotation component of this frame's transformation matrix.
		/// </summary>
		public float Rotation;

		/// <summary>
		/// The origin component of this frame's transformation matrix.
		/// </summary>
		public Vector2 Origin;
	}

	/// <summary>
	/// Converts Animation2DAsset to/from JSON.
	/// </summary>
	file class JsonAnimation2DAssetConverter : JsonBaseConverter<Animation2DAsset>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Type":
				if(!reader.HasNextEnum())
					throw new JsonException();

				return reader.ReadEnum<AnimationType>();
			case "Source":
				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			case "Frames":
				FrameConverter ??= (JsonConverter<Animation2DKeyFrame[]>)ops.GetConverter(typeof(Animation2DKeyFrame[]));

				return FrameConverter.Read(ref reader,typeof(Animation2DKeyFrame[]),ops);
			case "Loops":
				if(!reader.HasNextBool())
					throw new JsonException();

				return reader.GetBoolean();
			case "StartTime":
			case "LoopStart":
			case "LoopEnd":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetSingle();
			case "StartFrame":
			case "LoopFrameStart":
			case "LoopFrameEnd":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetInt32();
			default:
				throw new JsonException();
			}
		}

		protected override Animation2DAsset ConstructT(Dictionary<string,object?> properties)
		{
			// We error check later with the exception of Type
			if(!properties.TryGetValue("Type",out object? otemp) || AnimationType.Animation2D != (AnimationType)otemp!)
				throw new JsonException();

			Animation2DAsset ret = new Animation2DAsset();

			if(properties.TryGetValue("Source",out otemp))
				ret.Source.AssignRelativePath((string?)otemp);

			if(properties.TryGetValue("Frames",out otemp))
				ret.Frames = (Animation2DKeyFrame[]?)otemp;

			if(properties.TryGetValue("StartTime",out otemp))
				ret.StartTime = (float?)otemp;

			if(properties.TryGetValue("StartFrame",out otemp))
				ret.StartFrame = (int?)otemp;

			if(properties.TryGetValue("Loops",out otemp))
				ret.Loops = (bool)otemp!;

			if(properties.TryGetValue("LoopStart",out otemp))
				ret.LoopStart = (float?)otemp;

			if(properties.TryGetValue("LoopFrameStart",out otemp))
				ret.LoopFrameStart = (int?)otemp;

			if(properties.TryGetValue("LoopEnd",out otemp))
				ret.LoopEnd = (float?)otemp;

			if(properties.TryGetValue("LoopFrameEnd",out otemp))
				ret.LoopFrameEnd = (int?)otemp;
			
			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, Animation2DAsset value, JsonSerializerOptions ops)
		{
			writer.WriteEnum("Type",value.Type);

			if(value.Source.RelativePath is not null)
				writer.WriteString("Source",value.Source.RelativePath);
			
			if(value.Frames is not null)
			{
				FrameConverter ??= (JsonConverter<Animation2DKeyFrame[]>)ops.GetConverter(typeof(Animation2DKeyFrame[]));
				
				writer.WritePropertyName("Frames");
				FrameConverter.Write(writer,value.Frames,ops);
			}

			if(value.StartTime is not null)
				writer.WriteNumber("StartTime",value.StartTime.Value);

			if(value.StartFrame is not null)
				writer.WriteNumber("StartFrame",value.StartFrame.Value);

			writer.WriteBoolean("Loops",value.Loops);

			if(value.LoopStart is not null)
				writer.WriteNumber("LoopStart",value.LoopStart.Value);

			if(value.LoopFrameStart is not null)
				writer.WriteNumber("LoopFrameStart",value.LoopFrameStart.Value);

			if(value.LoopEnd is not null)
				writer.WriteNumber("LoopEnd",value.LoopEnd.Value);

			if(value.LoopFrameEnd is not null)
				writer.WriteNumber("LoopFrameEnd",value.LoopFrameEnd.Value);

			return;
		}

		/// <summary>
		/// Converts keyframe arrays to/from JSON.
		/// </summary>
		private JsonConverter<Animation2DKeyFrame[]>? FrameConverter;
	}

	/// <summary>
	/// Converts Animation2DKeyFrame to/from JSON.
	/// </summary>
	file class JsonAnimation2DKeyFrameConverter : JsonBaseConverter<Animation2DKeyFrame>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Sprite":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetInt32();
			case "Duration":
			case "Rotation":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetSingle();
			case "Translation":
			case "Scale":
			case "Origin":
				VectorConverter ??= (JsonConverter<Vector2>)ops.GetConverter(typeof(Vector2));

				return VectorConverter.Read(ref reader,typeof(Vector2),ops);
			default:
				throw new JsonException();
			}
		}

		protected override Animation2DKeyFrame ConstructT(Dictionary<string,object?> properties)
		{
			Animation2DKeyFrame ret = new Animation2DKeyFrame();

			if(properties.TryGetValue("Sprite",out object? otemp))
				ret.Sprite = (int)otemp!;

			if(properties.TryGetValue("Duration",out otemp))
				ret.Duration = (float)otemp!;

			if(properties.TryGetValue("Translation",out otemp))
				ret.Translation = (Vector2)otemp!;

			if(properties.TryGetValue("Rotation",out otemp))
				ret.Rotation = (float)otemp!;

			if(properties.TryGetValue("Scale",out otemp))
				ret.Scale = (Vector2)otemp!;

			if(properties.TryGetValue("Origin",out otemp))
				ret.Origin = (Vector2)otemp!;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, Animation2DKeyFrame value, JsonSerializerOptions ops)
		{
			VectorConverter ??= (JsonConverter<Vector2>)ops.GetConverter(typeof(Vector2));

			writer.WriteNumber("Sprite",value.Sprite);
			writer.WriteNumber("Duration",value.Duration);

			if(value.Translation != Vector2.Zero)
			{
				writer.WritePropertyName("Translation");
				VectorConverter.Write(writer,value.Translation,ops);
			}

			if(value.Rotation != 0.0f)
				writer.WriteNumber("Rotation",value.Rotation);

			if(value.Scale != Vector2.One)
			{
				writer.WritePropertyName("Scale");
				VectorConverter.Write(writer,value.Scale,ops);
			}

			if(value.Origin != Vector2.Zero)
			{
				writer.WritePropertyName("Origin");
				VectorConverter.Write(writer,value.Origin,ops);
			}

			return;
		}

		/// <summary>
		/// Converts vectors to/from JSON.
		/// </summary>
		private JsonConverter<Vector2>? VectorConverter;
	}
}
