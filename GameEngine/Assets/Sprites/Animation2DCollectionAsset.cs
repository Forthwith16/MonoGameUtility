using GameEngine.Assets.Serialization;
using GameEngine.Resources;
using GameEngine.Resources.Sprites;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEngine.Utility.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Assets.Sprites
{
	/// <summary>
	/// Contains the raw asset data of an animation collection.
	/// </summary>
	[AssetLoader(typeof(Animation2DCollection),nameof(FromFile))]
	public partial class Animation2DCollectionAsset
	{
		protected override void Serialize(string path, string root, bool overwrite_dependencies = false)
		{
			// If our source animtions don't exist where we expect them to, write them out to those locations (if we have them)
			for(int i = 0;i < Animations.Length;i++)
				if(StandardShouldSerializeCheck(Animations[i].Source,Path.GetDirectoryName(path) ?? "",overwrite_dependencies,out string? dst))
					if(Animations[i].Source.Resource is null)
						throw new IOException("Failed to save an animation collection to disc due to an absent animation source");
					else if(Asset.CreateAssetFromResource<SpriteSheetAsset>(Animations[i].Source.Resource!,out SpriteSheetAsset? asset))
						Asset.SaveAsset(asset,Path.GetRelativePath(root,dst),root,overwrite_dependencies);

			// Now we can serialize our sprite sheet proper
			this.SerializeJson(path);

			return;
		}

		protected override void AdjustFilePaths(string path, string root)
		{
			for(int i = 0;i < Animations.Length;i++)
				StandardAssetSourcePathAdjustment(Animations[i].Source,path,Animations[i].Source.Unnamed ? GenerateFreeFile(Path.Combine(root,path),".a2d") : "");

			return;
		}

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		protected static Animation2DCollectionAsset? FromFile(string path) => path.DeserializeJsonFile<Animation2DCollectionAsset>();

		protected override IResource? Instantiate(Linker link)
		{
			// We need to ensure we have everything we need before we can instantiate all willy nilly
			HashSet<string> names = new HashSet<string>();
			
			foreach(NamedAnimation2D a in Animations)
				if(a.Name is null || !names.Add(a.Name) || a.Source.Resource is null)
					return null;

			if(IdleAnimation is null || !names.Contains(IdleAnimation))
				return null;

			return new Animation2DCollection("",Animations.Select(a => a.Source.Resource!),Animations.Select(a => a.Name!),IdleAnimation);
		}
	}
	
	[JsonConverter(typeof(JsonAnimation2DCollectionAssetConverter))]
	public partial class Animation2DCollectionAsset : AssetBase
	{
		/// <summary>
		/// The default constructor.
		/// It sets every value to its default.
		/// </summary>
		public Animation2DCollectionAsset() : base()
		{
			Animations = [];
			IdleAnimation = null;
			ResetOnSwap = true;

			return;
		}

		/// <summary>
		/// Creates an animation collection asset from <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">The animation collection to turn into its asset form.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="collection"/> is missing an animation source.</exception>
		protected internal Animation2DCollectionAsset(Animation2DCollection collection) : base()
		{
			IdleAnimation = collection.IdleAnimationName;
			ResetOnSwap = collection.ResetOnSwitch;

			Animations = new NamedAnimation2D[collection.Count];
			int i = 0;

			foreach(string a_name in collection.AnimationNames)
			{
				NamedAnimation2D a;
				Animation2D a2d = collection[a_name];

				a.Name = a_name;
				a.Source = new AssetSource<Animation>(a2d.SourceData);

				Animations[i++] = a;
			}

			return;
		}

		/// <summary>
		/// The animations of this collection.
		/// </summary>
		public NamedAnimation2D[] Animations;

		/// <summary>
		/// The name of the idle animation.
		/// </summary>
		public string? IdleAnimation;

		/// <summary>
		/// If true, then the collection resets the active animation after swapping in a new one.
		/// </summary>
		public bool ResetOnSwap;
	}

	/// <summary>
	/// Encapsulates an animation paired with a name for it.
	/// </summary>
	[JsonConverter(typeof(JsonNamedAnimation2DConverter))]
	public struct NamedAnimation2D
	{
		/// <summary>
		/// Constructs a named animation with all default values.
		/// </summary>
		public NamedAnimation2D()
		{
			Name = null;
			Source = new AssetSource<Animation>();

			return;
		}

		/// <summary>
		/// The animation name.
		/// </summary>
		public string? Name;

		/// <summary>
		/// The source animation file.
		/// This path must be relative to the animation collection source file.
		/// </summary>
		public AssetSource<Animation> Source;
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

	/// <summary>
	/// Converts NamedAnimation2Da to/from JSON.
	/// </summary>
	file class JsonNamedAnimation2DConverter : JsonBaseConverter<NamedAnimation2D>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Name":
				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			case "Source":
				if(!reader.HasNextString())
					throw new JsonException();

				return reader.GetString();
			default:
				throw new JsonException();
			}
		}

		protected override NamedAnimation2D ConstructT(Dictionary<string,object?> properties)
		{
			NamedAnimation2D ret = new NamedAnimation2D();
			
			if(properties.TryGetValue("Name",out object? otemp))
				ret.Name = (string?)otemp;

			if(properties.TryGetValue("Source",out otemp))
				ret.Source.AssignRelativePath((string?)otemp);

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, NamedAnimation2D value, JsonSerializerOptions ops)
		{
			writer.WriteString("Name",value.Name);
			writer.WriteString("Source",value.Source.RelativePath);

			return;
		}
	}
}
