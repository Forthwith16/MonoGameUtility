using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// Contains the raw asset data of an sprite sheet.
	/// </summary>
	[JsonConverter(typeof(JsonSpriteSheetAssetConverter))]
	public class SpriteSheetAsset : IAsset
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
		public static SpriteSheetAsset? Deserialize(string path) => path.DeserializeJsonFile<SpriteSheetAsset>();
		
		/// <summary>
		/// The source texture of the sprites.
		/// </summary>
		public string? Source;
		
		/// <summary>
		/// Specifies that this asset fills rows before creating new columns.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public bool TileFillRowFirst;

		/// <summary>
		/// Specifies that this asset fills columns before creating new rows.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public bool TileFillColumnFirst
		{
			get => !TileFillRowFirst;
			set => TileFillRowFirst = !value;
		}

		/// <summary>
		/// The the number of sprites per row if specified via a tile system.
		/// If this is specified by not TileVCount, then sprite sheet rows are filled before a new column is created.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileHCount;

		/// <summary>
		/// The the number of sprites per column if specified via a tile system.
		/// If this is specified by not TileVCount, then sprite sheet columns are filled before a new row is created.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileVCount;

		/// <summary>
		/// The number of sprites occuring vertically if specified via a tile system.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileCount;

		/// <summary>
		/// The width of a sprite if specified via a tile system.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileWidth;

		/// <summary>
		/// The height of a sprite if specified via a tile system.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileHeight;

		/// <summary>
		/// The sprite source rectangles.
		/// <para/>
		/// If these are not present, use the tile related data to calculate them.
		/// </summary>
		public Rectangle[]? Sprites;
	}

	/// <summary>
	/// Converts SpriteSheetAssets to/from JSON.
	/// </summary>
	file class JsonSpriteSheetAssetConverter : JsonBaseConverter<SpriteSheetAsset>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Source":
				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			case "TileFillRowFirst":
				if(!reader.HasNextBool())
					throw new JsonException();

				return reader.GetBoolean();
			case "TileHCount":
			case "TileVCount":
			case "TileCount":
			case "TileWidth":
			case "TileHeight":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetInt32();
			case "Sprites":
				RectConverter ??= (JsonConverter<Rectangle[]>)ops.GetConverter(typeof(Rectangle[]));

				return RectConverter.Read(ref reader,typeof(Rectangle[]),ops);
			default:
				throw new JsonException();
			}
		}

		protected override SpriteSheetAsset ConstructT(Dictionary<string,object?> properties)
		{
			// Error checking asset values happens later, so we need not care here
			SpriteSheetAsset ret = new SpriteSheetAsset();

			if(properties.TryGetValue("Source",out object? otemp))
				ret.Source = (string?)otemp;

			if(properties.TryGetValue("Sprites",out otemp))
				ret.Sprites = (Rectangle[]?)otemp;

			if(properties.TryGetValue("TileFillRowFirst",out otemp))
				ret.TileFillRowFirst = (bool)otemp!;

			if(properties.TryGetValue("TileHCount",out otemp))
				ret.TileHCount = (int?)otemp;

			if(properties.TryGetValue("TileVCount",out otemp))
				ret.TileVCount = (int?)otemp;

			if(properties.TryGetValue("TileCount",out otemp))
				ret.TileCount = (int?)otemp;

			if(properties.TryGetValue("TileWidth",out otemp))
				ret.TileWidth = (int?)otemp;

			if(properties.TryGetValue("TileHeight",out otemp))
				ret.TileHeight = (int?)otemp;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, SpriteSheetAsset value, JsonSerializerOptions ops)
		{
			if(value.Source is not null)
				writer.WriteString("Source",value.Source);

			writer.WriteBoolean("TileFillRowFirst",value.TileFillRowFirst);

			if(value.TileHCount.HasValue)
				writer.WriteNumber("TileHCount",value.TileHCount.Value);

			if(value.TileVCount.HasValue)
				writer.WriteNumber("TileVCount",value.TileVCount.Value);
			
			if(value.TileCount.HasValue)
				writer.WriteNumber("TileCount",value.TileCount.Value);

			if(value.TileWidth.HasValue)
				writer.WriteNumber("TileWidth",value.TileWidth.Value);

			if(value.TileHeight.HasValue)
				writer.WriteNumber("TileHeight",value.TileHeight.Value);

			if(value.Sprites is not null)
			{
				RectConverter ??= (JsonConverter<Rectangle[]>)ops.GetConverter(typeof(Rectangle[]));

				writer.WritePropertyName("Sprites");
				RectConverter.Write(writer,value.Sprites,ops);
			}

			return;
		}

		/// <summary>
		/// Converts rectangle arrays to/from JSON.
		/// </summary>
		private JsonConverter<Rectangle[]>? RectConverter;
	}
}
