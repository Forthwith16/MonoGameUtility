using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Framework
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
	/// For a GameObject to serialize/deserialize properly, it must be entirely self-contained.
	/// For example, suppose you have GameObjects A and B.
	/// A has a reference to B.
	/// For these game objects to deserialize correctly (and serialize as a consequence), they must deserialize together.
	/// To achieve this, simply make every top-level game object the child of some variety of dummy GameObject class.
	/// Then serialize/deserialize the dummy, which can be discarded afterward.
	/// </remarks>
	public class GameObjectJsonConverter : JsonBaseTypeConverter<GameObject>
	{
		/// <summary>
		/// Initializes this converter.
		/// </summary>
		public GameObjectJsonConverter() : base()
		{
			IDConverter = null;
			return;
		}

		public override GameObject Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
		{
			// If we don't have our ID converter yet, get it
			if(IDConverter is null)
				IDConverter = (JsonConverter<GameObjectID>)ops.GetConverter(typeof(GameObjectID));

			// We start with the object opening
			if(!reader.HasNextObjectStart())
				throw new JsonException();
			
			reader.Read();

			// We'll need to track what properties we've already done
			HashSet<string> processed = new HashSet<string>();

			// We need a place to store the values we read
			GameObjectID old_id = GameObjectID.NULL;
			GameObject? ret = null;
			
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
					old_id = IDConverter.Read(ref reader,typeof(GameObjectID),ops);
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

			// Now that we're sure we have the correct return values, log them and be done
			OldIDToNewID[old_id] = ret!.ID;

			return ret;
		}

		public override void Write(Utf8JsonWriter writer, GameObject value, JsonSerializerOptions ops)
		{
			// If we don't have our ID converter yet, get it
			if(IDConverter is null)
				IDConverter = (JsonConverter<GameObjectID>)ops.GetConverter(typeof(GameObjectID));

			// Now we need to write out our old ID in a way that we can always get to it
			// We don't require that derived classes write out their own ID, and they probably won't since we do it for them
			// After, we can just write out the object as per normal
			writer.WriteStartObject();

			writer.WritePropertyName("ID");
			IDConverter.Write(writer,value.ID,ops);

			writer.WritePropertyName("Object");
			base.Write(writer,value,ops);

			writer.WriteEndObject();

			return;
		}

		/// <summary>
		/// This converts GameObjectIDs to/from JSON.
		/// </summary>
		private JsonConverter<GameObjectID>? IDConverter;

		#region Static System
		/// <summary>
		/// Obtains the new ID of <paramref name="old_id"/> after deserialization.
		/// </summary>
		/// <param name="old_id">The old ID prior to deserialization.</param>
		/// <param name="new_id">The new ID for <paramref name="old_id"/> when this returns true. When this returns false, this is the null ID.</param>
		/// <returns>Returns true if there is a deserialized game object whose old ID was <paramref name="old_id"/>.</returns>
		/// <remarks>
		/// It is entirely possible to deserialize several game objects with the same old ID.
		/// To combat this, one should generally keep their asset IDs unique upon deserialization.
		/// At the very least, no two game objects in a single deserialization (attained via parent/child paired deserialization) should have the same old ID.
		/// <para/>
		/// However, it is not always possible to have all old IDs be unique.
		/// In this case, know that the lifespan of a mapping from old ID to new ID is limited.
		/// Use it quickly, and use it safely.
		/// </remarks>
		public static bool GetNewID(GameObjectID old_id, out GameObjectID new_id)
		{
			if(OldIDToNewID.TryGetValue(old_id,out new_id))
				return true;

			new_id = GameObjectID.NULL;
			return false;
		}

		/// <summary>
		/// Obtains the deserialized object whose old ID was <paramref name="old_id"/>.
		/// </summary>
		/// <param name="old_id">The old ID of the object prior to deserialization.</param>
		/// <param name="output">The object whose old ID was <paramref name="old_id"/> when this returns true or null when this returns false.</param>
		/// <returns>Returns true if there is a deserialized object whose ID was <paramref name="old_id"/> and false otherwise.</returns>
		/// <remarks>
		/// It is entirely possible to deserialize several game objects with the same old ID.
		/// To combat this, one should generally keep their asset IDs unique upon deserialization.
		/// At the very least, no two game objects in a single deserialization (attained via parent/child paired deserialization) should have the same old ID.
		/// <para/>
		/// However, it is not always possible to have all old IDs be unique.
		/// In this case, know that the lifespan of a mapping from old ID to new ID is limited.
		/// Use it quickly, and use it safely.
		/// </remarks>
		public static bool GetObject(GameObjectID old_id, [MaybeNullWhen(false)] out GameObject output)
		{
			if(GetNewID(old_id,out GameObjectID new_id))
			{
				output = new_id;
				return true;
			}

			output = null;
			return false;
		}

		private static ConcurrentDictionary<GameObjectID,GameObjectID> OldIDToNewID = new ConcurrentDictionary<GameObjectID,GameObjectID>();
		#endregion
	}
}
