using GameEngine.Sprites;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using GameEnginePipeline.Serialization;
using Microsoft.Xna.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// Contains the raw asset data of an sprite sheet.
	/// </summary>
	[Asset(typeof(SpriteSheet))]
	[JsonConverter(typeof(JsonSpriteSheetAssetConverter))]
	public class SpriteSheetAsset : AssetBase
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
		/// The default constructor for a sprite sheet asset.
		/// It sets every value to its default.
		/// </summary>
		public SpriteSheetAsset() : base()
		{return;} // We can just leave everything with default values; we just need this constructor to exist

		/// <summary>
		/// Creates a sprite sheet asset from <paramref name="ss"/>.
		/// </summary>
		/// <param name="ss">The sprite sheet to turn into its asset form.</param>
		/// <remarks>
		/// This will attemtp to determine if the source sprites are a tiling rather than custom sprite locations.
		/// To do so, it checks that each sprite is the same size and that they are in row or column major order with each tightly packed.
		/// </remarks>
		public SpriteSheetAsset(SpriteSheet ss) : base()
		{
			Source = ss.SourceSource;

			// We need to check if the source sprites are a tiling rather than custom locations
			// This is relatively straightforward but tedious
			// First, it's useful to get the Sprites once so we don't keep tossing (sorta) big primitives around
			Sprites = new Rectangle[ss.Count];

			for(int i = 0;i < ss.Count;i++)
				Sprites[i] = ss[i];

			// The first sprite has to be at (0,0)
			// If this is not the case, then we are not a tiling
			if(Sprites[0].Location != Point.Zero)
				return;

			// Next, if we only have 1 sprite, then we can just call it a tiling and move on
			if(ss.Count == 1)
			{
				TileFillRowFirst = true;
				TileHCount = 1;
				TileCount = 1;

				TileWidth = ss[0].Width;
				TileHeight = ss[0].Height;

				Sprites = null;
				return;
			}

			// Now check that every sprite is of the same dimension
			for(int i = 0;i < Sprites.Length - 1;i++)
				if(Sprites[i].Size != Sprites[i + 1].Size)
					return;

			// If every sprite is of the same size, then they must be contiguous in either row or column major order to be a tiling
			// We can distinguish between row and column major by checking which direction the first and second sprite are from each other
			if(Sprites[0].Right == Sprites[1].Left) // Row major (left to right, top to bottom)
			{
				// We now need to figure out how far we tile left to right before breaking the row
				int tiling_length = 1;

				for(;tiling_length < Sprites.Length;tiling_length++)
					if(Sprites[tiling_length - 1].TopRight() != Sprites[tiling_length].TopLeft())
						break;

				// Now that we know the tiling length we just need to loop over every tile past the first row (which is already fully verified as a tiling) to finish up determining that this is a proper tiling
				for(int i = tiling_length;i < Sprites.Length;i++)
					if(Sprites[i - tiling_length].BottomLeft() != Sprites[i].TopLeft()) // We know every sprite has the same size, so it suffices to check that the sprites are lining up with the one directly above them
						return;

				TileFillRowFirst = true;
				TileHCount = tiling_length;
				TileCount = Sprites.Length;
			}
			else if(Sprites[0].Bottom == Sprites[1].Top) // Column major (top to bottom, left to right)
			{
				// We now need to figure out how far we tile top to bottom before breaking the column
				int tiling_length = 1;

				for(;tiling_length < Sprites.Length;tiling_length++)
					if(Sprites[tiling_length - 1].BottomLeft() != Sprites[tiling_length].TopLeft())
						break;

				// Now that we know the tiling length we just need to loop over every tile past the first column (which is already fully verified as a tiling) to finish up determining that this is a proper tiling
				for(int i = tiling_length;i < Sprites.Length;i++)
					if(Sprites[i - tiling_length].TopRight() != Sprites[i].TopLeft()) // We know every sprite has the same size, so it suffices to check that the sprites are lining up with the one directly left of them
						return;

				TileFillRowFirst = false;
				TileVCount = tiling_length;
				TileCount = Sprites.Length;
			}
			else // Not contiguous, so we're not a tiling (or at least not a packed one)
				return;

			// We're a tiling, so set the common tiling parameters
			TileWidth = ss[0].Width;
			TileHeight = ss[0].Height;
			Sprites = null;

			return;
		}

		/// <summary>
		/// The source texture of the sprites.
		/// This path must be relative to the sprite sheet file.
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
		public bool TileFillColumnFirst => !TileFillRowFirst;

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
