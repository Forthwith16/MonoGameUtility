using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using GameEnginePipeline.Readers.Sprites;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// Contains the base raw asset data of an Animation2D.
	/// </summary>
	[JsonConverter(typeof(JsonAnimation2DAssetConverter))]
	public class Animation2DAsset : BaseAnimationAsset<Animation2DAsset>
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public override void Serialize(string path) => this.SerializeJson(path);

		/// <summary>
		/// Creates a Animation2D with garbage values.
		/// </summary>
		public Animation2DAsset() : base(AnimationType.Animation2D)
		{return;}

		/// <summary>
		/// The source sprite sheet for the animation.
		/// </summary>
		public string? Source;

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
				ret.Source = (string?)otemp;

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

			if(value.Source is not null)
				writer.WriteString("Source",value.Source);
			
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
