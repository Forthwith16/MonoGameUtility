using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for GameObjectID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converter's read type below as well
using GameObjectIDType = uint;

namespace GameEngine.Framework
{
	/// <summary>
	/// A struct wrapper for game object IDs.
	/// </summary>
	[JsonConverter(typeof(JsonGameObjectIDConverter))]
	public readonly struct GameObjectID : IComparable<GameObjectID>, IEquatable<GameObjectID>, IDisposable
	{
		/// <summary>
		/// Constructs the null ID.
		/// </summary>
		public GameObjectID()
		{
			ID = NullValue;
			return;
		}

		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		/// <remarks>
		///	The intended way to obtain new IDs is to call <see cref="GetFreshID(GameObject)"/>.
		///	This is used for creating dummy IDs to search for things or store information.
		/// </remarks>
		public GameObjectID(GameObjectIDType id)
		{
			ID = id;
			return;
		}

		/// <summary>
		/// Releases this ID to be reused elsewhere.
		/// </summary>
		public void Dispose()
		{
			ReleaseID(ID);
			return;
		}

		public static implicit operator GameObjectID(GameObjectIDType id) => new GameObjectID(id);
		public static explicit operator GameObjectIDType(GameObjectID id) => id.ID;

		/// <summary>
		/// Obtains the game object with this ID.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if there is no game object with this ID.</exception>
		public static implicit operator GameObject(GameObjectID id) => Get(id);

		/// <summary>
		/// Trys to fetch the game object associated with this ID.
		/// </summary>
		/// <param name="output">The game object with this ID or null when this returns false.</param>
		/// <returns>Returns true if a game object with this ID was found and false otherwise.</returns>
		public bool TryGetGameObject([MaybeNullWhen(false)] out GameObject output) => TryGet(this,out output);

		public static bool operator ==(GameObjectID a, GameObjectID b) => a.ID == b.ID;
		public static bool operator !=(GameObjectID a, GameObjectID b) => a.ID != b.ID;

		public bool Equals(GameObjectID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is GameObjectID id && this == id;

		public static bool operator <(GameObjectID a, GameObjectID b) => a.ID < b.ID;
		public static bool operator <=(GameObjectID a, GameObjectID b) => a.ID <= b.ID;
		public static bool operator >(GameObjectID a, GameObjectID b) => a.ID > b.ID;
		public static bool operator >=(GameObjectID a, GameObjectID b) => a.ID >= b.ID;

		public int CompareTo(GameObjectID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly GameObjectIDType ID;

		/// <summary>
		/// The null ID never used by game objects.
		/// </summary>
		public static readonly GameObjectID NULL = new GameObjectID();

		/// <summary>
		/// The null ID value.
		/// </summary>
		private const GameObjectIDType NullValue = 0;

		#region Static System
		/// <summary>
		/// Obtains a fresh ID that is not in use.
		/// </summary>
		/// <param name="obj">The game object this ID will be assigned to.</param>
		/// <returns>Returns the new ID.</returns>
		public static GameObjectID GetFreshID(GameObject obj)
		{
			GameObjectID ret = new GameObjectID(NextID);
			_lut[ret] = new WeakReference(obj,false);

			return ret;
		}

		/// <summary>
		/// Gets the game object with the given ID.
		/// </summary>
		/// <param name="id">The ID of the game object to obtain.</param>
		/// <returns>Returns the game object with ID <paramref name="id"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="id"/> is not the ID of a game object.</exception>
		public static GameObject Get(GameObjectID id)
		{
			WeakReference ptr = _lut[id];
			
			if(ptr.Target is null)
			{
				ReleaseID(id.ID);
				throw new KeyNotFoundException();
			}
			
			return (GameObject)ptr.Target;
		}

		/// <summary>
		/// Attempts to get a game object by its ID.
		/// </summary>
		/// <param name="id">The ID of the game object to obtain.</param>
		/// <param name="ret">The discovered game object or null when this returns false.</param>
		/// <returns>Returns true if there is a game object with the given ID and false otherwise.</returns>
		public static bool TryGet(GameObjectID id, [MaybeNullWhen(false)] out GameObject ret)
		{
			if(!_lut.TryGetValue(id,out WeakReference? ptr))
			{
				ret = null;
				return false;
			}

			if(ptr.Target is null)
			{
				ReleaseID(id.ID);

				ret = null;
				return false;
			}

			ret = (GameObject)ptr.Target;
			return true;
		}

		/// <summary>
		/// Releases a game object ID for future use.
		/// </summary>
		private static void ReleaseID(GameObjectIDType id)
		{
			if(_lut.TryRemove(id,out _))
				_ndid.Push(id);
			
			return;
		}

		/// <summary>
		/// Obtains the next available ID for a game object.
		/// This will recycle discarded game object IDs.
		/// </summary>
		/// <remarks>The 0 ID is reserved for error results.</remarks>
		private static GameObjectIDType NextID
		{
			get
			{
				if(_ndid.Count == 0)
					lock(_ndid_lock)
						return _nid++;

				if(_ndid.TryPop(out GameObjectIDType ret))
					return ret;

				lock(_ndid_lock)
					return _nid++;
			}
		}

		private static ConcurrentStack<GameObjectIDType> _ndid = new ConcurrentStack<GameObjectIDType>();
		private static GameObjectIDType _nid = 1;
		private static object _ndid_lock = new object();

		private static ConcurrentDictionary<GameObjectID,WeakReference> _lut = new ConcurrentDictionary<GameObjectID,WeakReference>();
		#endregion
	}

	/// <summary>
	/// Converts a GameObjectID to/from a JSON format.
	/// </summary>
	file class JsonGameObjectIDConverter : JsonBaseConverter<GameObjectID>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "ID":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetUInt32();
			default:
				throw new JsonException();
			}
		}

		protected override GameObjectID ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 1)
				throw new JsonException();

			return new GameObjectID((GameObjectIDType)properties["ID"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, GameObjectID value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("ID",value.ID);
			return;
		}
	}
}
