using GameEngine.Assets.Serialization;
using GameEngine.Resources;
using GameEngine.Resources.Sprites;
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
	/// Contains the raw asset data of an sprite sheet.
	/// </summary>
	[AssetLoader(typeof(SpriteSheetAsset),nameof(FromFile))]
	public partial class SpriteSheetAsset
	{
		protected override void Serialize(string path, string root, bool overwrite_dependencies = false)
		{
			// If our source texture doesn't exist where we expect it to, write it out to that location (if we have it)
			if(StandardShouldSerializeCheck(Source,Path.GetDirectoryName(path) ?? "",overwrite_dependencies,out string? dst))
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(dst) ?? "");

					using(FileStream fout = new FileStream(dst,FileMode.OpenOrCreate))
						Source.Resource!.SaveAsPng(fout,Source.Resource.Width,Source.Resource.Height);
				}
				catch
				{} // If something goes wrong, we don't want to crash horribly

			// Now we can serialize our sprite sheet proper
			this.SerializeJson(path);
			
			return;
		}

		protected override void AdjustFilePaths(string path, string root)
		{
			StandardAssetSourcePathAdjustment(Source,path,Source.Unnamed ? GenerateFreeFile(Path.Combine(root,path),".png") : "");
			return;
		}

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		protected static SpriteSheetAsset? FromFile(string path) => path.DeserializeJsonFile<SpriteSheetAsset>();

		protected override IResource? Instantiate(Linker link)
		{
			// If we don't have our resource loaded, then we can't do anything useful
			// We don't have a great way to get our hands on the resource either, so we'll just not bother
			if(Source.Resource is null)
				return null;
			
			// If we already have our sprites, we're good to go
			if(Sprites is not null && Sprites.Length > 0)
				return new SpriteSheet("",Source.Resource,Sprites); // Assets are not necessarily loaded from disc, so we do not give a name to the asset

			// We have a tiling system, so we MUST have positive tile widths and heights
			if(TileWidth is null || TileWidth < 1 || TileHeight is null || TileHeight < 1)
				return null;

			// We gotta get our tiling dimensions
			int width;
			int height;
			int len;

			// We need EXACTLY two out of three of the remaining positive tile parameters OR they can all be in agreement
			if(TileVCount is null)
				if(TileHCount is null)
					return null;
				else
					if(TileCount is null || TileHCount < 1 || TileCount < 1)
						return null;
					else
					{
						width = TileHCount.Value;
						height = ((TileCount + TileHCount - 1) / TileHCount).Value;
						len = TileCount.Value;
					}
			else
				if(TileHCount is null)
					if(TileCount is null || TileVCount < 1 || TileCount < 1)
						return null;
					else
					{
						width = ((TileCount + TileVCount - 1) / TileVCount).Value;
						height = TileVCount.Value;
						len = TileCount.Value;
					}
				else
					if(TileHCount < 1 || TileVCount < 1)
						return null;
					else if(TileCount is null)
					{
						width = TileHCount.Value;
						height = TileVCount.Value;
						len = (TileVCount * TileHCount).Value;
					}
					else if(TileCount != TileVCount * TileHCount)
						return null;
					else
					{
						width = TileHCount.Value;
						height = TileVCount.Value;
						len = TileCount.Value;
					}

			// We need to create Sprites temporarily
			Rectangle[] sprites = new Rectangle[len];

			if(TileFillRowFirst)
				for(int y = 0,c = 0;c < len;y++)
					for(int x = 0;x < width && c < len;x++,c++)
						sprites[c] = new Rectangle(x * width,y * height,width,height);
			else
				for(int x = 0,c = 0;c < len;x++)
					for(int y = 0;y < height && c < len;y++,c++)
						sprites[c] = new Rectangle(x * width,y * height,width,height);

			return new SpriteSheet("",Source.Resource,sprites); // Assets are not necessarily loaded from disc, so we do not give a name to the asset
		}
	}

	[JsonConverter(typeof(JsonSpriteSheetAssetConverter))]
	public partial class SpriteSheetAsset : AssetBase
	{
		/// <summary>
		/// The default constructor.
		/// It sets every value to its default.
		/// </summary>
		public SpriteSheetAsset() : base()
		{
			Source = new AssetSource<Texture2D>();
			return;
		}

		/// <summary>
		/// Creates a sprite sheet asset from <paramref name="ss"/>.
		/// </summary>
		/// <param name="ss">The sprite sheet to turn into its asset form.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="ss"/> does not have a source image for its texture.</exception>
		/// <remarks>
		/// This will attempt to determine if the source sprites are a tiling rather than custom sprite locations.
		/// To do so, it checks that each sprite is the same size and that they are in row or column major order with each tightly packed.
		/// </remarks>
		protected internal SpriteSheetAsset(SpriteSheet ss) : base()
		{
			// Create the source texture info
			Source = new AssetSource<Texture2D>(ss.Source);

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
		/// </summary>
		public AssetSource<Texture2D> Source;
		
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
		/// <para/>
		/// Only two of TileHCount, TileVCount, or TileCount need be specified.
		/// If all three are, it <i>must</i> be the case that TileCount = TileHCount * TileVCount.
		/// <para/>
		/// If Sprites is defined, this value is ignored.
		/// </summary>
		public int? TileHCount;

		/// <summary>
		/// The the number of sprites per column if specified via a tile system.
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
		/// If these are not present, use tiling data to calculate them.
		/// These do not count as present if their length is 0.
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
				ret.Source.AssignRelativePath((string?)otemp);

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
			if(value.Source.RelativePath is not null)
				writer.WriteString("Source",value.Source.RelativePath);

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
