using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// Contains the raw asset data of an sprite sheet.
	/// </summary>
	public class SpriteSheetAsset : IAsset
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public void Serialize(string path)
		{
			this.SerializeXml(path);
			return;
		}

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static SpriteSheetAsset? Deserialize(string path)
		{return path.DeserializeXmlFile<SpriteSheetAsset>();}

		/// <summary>
		/// The source texture of the sprites.
		/// </summary>
		public string? Source
		{get; set;}
		
		public bool ShouldSerializeSource()
		{return true;}

		/// <summary>
		/// Specifies that this asset fills rows before creating new columns.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public bool TileFillRowFirst
		{get; set;}

		public bool ShouldSerializeTileFillRowFirst()
		{return true;}

		/// <summary>
		/// Specifies that this asset fills columns before creating new rows.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		[JsonIgnore]
		public bool TileFillColumnFirst
		{
			get => !TileFillRowFirst;
			set => TileFillRowFirst = !value;
		}

		public bool ShouldSerializeTileFillColumnFirst()
		{return false;}

		/// <summary>
		/// The the number of sprites per row if specified via a tile system.
		/// If this is specified by not TileVCount, then sprite sheet rows are filled before a new column is created.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileHCount
		{get; set;}

		public bool ShouldSerializeTileHCount()
		{return TileHCount.HasValue;}

		/// <summary>
		/// The the number of sprites per column if specified via a tile system.
		/// If this is specified by not TileVCount, then sprite sheet columns are filled before a new row is created.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileVCount
		{get; set;}

		public bool ShouldSerializeTileVCount()
		{return TileVCount.HasValue;}

		/// <summary>
		/// The number of sprites occuring vertically if specified via a tile system.
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileCount
		{get; set;}

		public bool ShouldSerializeTileCount()
		{return TileCount.HasValue;}

		/// <summary>
		/// The width of a sprite if specified via a tile system.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileWidth
		{get; set;}

		public bool ShouldSerializeTileWidth()
		{return TileWidth.HasValue;}

		/// <summary>
		/// The height of a sprite if specified via a tile system.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileHeight
		{get; set;}

		public bool ShouldSerializeTileHeight()
		{return TileHeight.HasValue;}

		/// <summary>
		/// The sprite source rectangles.
		/// <para/>
		/// If these are not present, use the tile related data to calculate them.
		/// </summary>
		public Rectangle[]? Sprites
		{get; set;}

		public bool ShouldSerializeSprites()
		{return Sprites is not null;}
	}
}
