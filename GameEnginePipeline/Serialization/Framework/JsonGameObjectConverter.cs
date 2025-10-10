using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using GameEnginePipeline.Assets;
using GameEnginePipeline.Assets.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEnginePipeline.Serialization.Framework
{
	/// <summary>
	/// Converts game objects to/from JSON.
	/// <para/>
	/// This class is of pivitol importance for deserializing game objects of all varieties.
	/// In fact, all game objects should be serialized/deserialized via this (its abstract child classes redirect to this).
	/// <para/>
	/// This converter tracks deserialized game objects, mapping their old IDs to their new IDs.
	/// These deserialized game objects may, of course, then be retrieved via their new ID.
	/// </summary>
	/// <remarks>
	/// For this converter to work correctly, it must serialize/deserialize a single game object.
	/// It can be reset with the Clean method if desired, but it is probably usually better to just construct a new instance instead.
	/// 
	/// 
	/// 
	/// 
	/// For a GameObject to serialize/deserialize properly, it must be entirely self-contained.
	/// For example, suppose you have GameObjects A and B.
	/// A has a reference to B.
	/// For these game objects to deserialize correctly (and serialize as a consequence), they must deserialize together.
	/// To achieve this, simply make every top-level game object the child of some variety of dummy GameObject class.
	/// Then serialize/deserialize the dummy, which can be discarded afterward.
	/// </remarks>
	public class JsonGameObjectConverter : JsonBaseTypeConverter<GameObjectAsset>
	{
		/// <summary>
		/// Creates a new game object converter.
		/// </summary>
		public JsonGameObjectConverter()
		{
			// We need an internal map from internal asset IDs to asset IDs on read
			// On write, we can more directly map objects to internal asset IDs, I think

			return;
		}

		public override GameObjectAsset Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
		{
			// If we don't have our ID converter yet, get it
			if(IDConverter is null)
				IDConverter = (JsonConverter<AssetID>)ops.GetConverter(typeof(AssetID));

			// We start with the object opening
			if(!reader.HasNextObjectStart())
				throw new JsonException();
			
			reader.Read();

			// We'll need to track what properties we've already done
			HashSet<string> processed = new HashSet<string>();

			// We need a place to store the values we read
			AssetID old_id = AssetID.NULL;
			GameObjectAsset? ret = null;
			
			// Loop until we reach the end of the object
			while(!reader.HasNextObjectEnd())
			{
				// We better have a property before a value
				if(!reader.HasNextProperty())
					throw new JsonException();

				// We have a property, so grab it and move on
				string property_name = reader.GetString()!;
				reader.Read();

				// If we've already read this property, complain
				if(processed.Contains(property_name))
					throw new JsonException();

				// Switch on the property requested
				switch(property_name)
				{
				case "ID":
					old_id = IDConverter.Read(ref reader,typeof(AssetID),ops);
					break;
				case "Object":
					ret = base.Read(ref reader,type_to_convert,ops);
					break;
				default:
					throw new JsonException();
				}

				// We finished with this property, so move on
				processed.Add(property_name);
				reader.Read();
			}

			// Make sure we got both properties
			if(processed.Count != 2)
				throw new JsonException();



			return ret!;
		}

		public override void Write(Utf8JsonWriter writer, GameObjectAsset value, JsonSerializerOptions ops)
		{
			// If we don't have our ID converter yet, get it
			if(IDConverter is null)
				IDConverter = (JsonConverter<AssetID>)ops.GetConverter(typeof(AssetID));

			// Now we need to write out our old ID in a way that we can always get to it
			// We don't require that derived classes write out their own ID, and they probably won't since we do it for them
			// After, we can just write out the object as per normal
			writer.WriteStartObject();

			writer.WritePropertyName("ID");
			IDConverter.Write(writer,value.ContentID,ops);

			writer.WritePropertyName("Object");
			base.Write(writer,value,ops);

			writer.WriteEndObject();

			return;
		}

		/// <summary>
		/// Prepares this converter to convert a new game object, starting from scratch.
		/// </summary>
		public void Clean()
		{


			return;
		}

		/// <summary>
		/// This converts GameObjectIDs to/from JSON.
		/// </summary>
		private JsonConverter<AssetID>? IDConverter = null;
	}
}
