using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// Contains the raw asset data of an animation collection.
	/// </summary>
	[JsonConverter(typeof(JsonAnimation2DCollectionAssetConverter))]
	public class Animation2DCollectionAsset : IAsset
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public void Serialize(string path) => this.SerializeJson(path);

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static Animation2DCollectionAsset? Deserialize(string path) => path.DeserializeJsonFile<Animation2DCollectionAsset>();

		/// <summary>
		/// The animations of this collection.
		/// </summary>
		public NamedAnimation2D[] Animations = [];

		/// <summary>
		/// The name of the idle animation.
		/// </summary>
		public string? IdleAnimation;

		/// <summary>
		/// If true, then the collection resets the active animation after swapping in a new one.
		/// </summary>
		public bool ResetOnSwap = true; // This defaults to true, so let's be consistent
	}

	/// <summary>
	/// Encapsulates an animation paired with a name for it.
	/// </summary>
	public struct NamedAnimation2D
	{
		/// <summary>
		/// The animation name.
		/// </summary>
		public string? Name;

		/// <summary>
		/// The source animation file.
		/// This path must be relative to the animation collection source file.
		/// </summary>
		public string? Source;
	}

	/// <summary>
	/// Converts Animation2DCollectionAssets to/from JSON.
	/// </summary>
	file class JsonAnimation2DCollectionAssetConverter : JsonBaseConverter<Animation2DCollectionAsset>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "ResetOnSwap":
				if(!reader.HasNextBool())
					throw new JsonException();

				return reader.GetBoolean();
			case "IdleAnimation":
				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			case "Animations":
				AnimationConverter ??= (JsonConverter<NamedAnimation2D[]>)ops.GetConverter(typeof(NamedAnimation2D[]));
				return AnimationConverter.Read(ref reader,typeof(NamedAnimation2D[]),ops) ?? throw new JsonException();
			default:
				throw new JsonException();
			}
		}

		protected override Animation2DCollectionAsset ConstructT(Dictionary<string,object?> properties)
		{
			Animation2DCollectionAsset ret = new Animation2DCollectionAsset();

			if(properties.TryGetValue("ResetOnSwap",out object? otemp))
				ret.ResetOnSwap = (bool)otemp!;
			
			if(properties.TryGetValue("IdleAnimation",out otemp))
				ret.IdleAnimation = (string)otemp!;

			if(properties.TryGetValue("Animations",out otemp))
				ret.Animations = (NamedAnimation2D[])otemp!;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, Animation2DCollectionAsset value, JsonSerializerOptions ops)
		{
			writer.WriteBoolean("ResetOnSwap",value.ResetOnSwap);
			writer.WriteString("IdleAnimation",value.IdleAnimation);

			AnimationConverter ??= (JsonConverter<NamedAnimation2D[]>)ops.GetConverter(typeof(NamedAnimation2D[]));
			
			writer.WritePropertyName("Animations");
			AnimationConverter.Write(writer,value.Animations,ops);
		}

		public JsonConverter<NamedAnimation2D[]>? AnimationConverter;
	}
}
