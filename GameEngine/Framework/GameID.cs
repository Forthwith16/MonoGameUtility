using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for GameID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converter's read type below as well
using GameIDType = uint;

namespace GameEngine.Framework
{
	/// <summary>
	/// A struct wrapper for game IDs.
	/// </summary>
	[JsonConverter(typeof(JsonGameIDConverter))]
	public readonly struct GameID : IComparable<GameID>, IEquatable<GameID>, IDisposable
	{
		/// <summary>
		/// Constructs the null ID.
		/// </summary>
		public GameID()
		{
			ID = NullValue;
			return;
		}

		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		/// <remarks>
		///	The intended way to obtain new IDs is to call <see cref="GetFreshID(Game)"/>.
		///	This is used for creating dummy IDs to search for things or store information.
		/// </remarks>
		public GameID(GameIDType id)
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

		public static implicit operator GameID(GameIDType id) => new GameID(id);
		public static explicit operator GameIDType(GameID id) => id.ID;

		/// <summary>
		/// Obtains the game with this ID.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if there is no game with this ID.</exception>
		public static implicit operator Game(GameID id) => Get(id);

		/// <summary>
		/// Trys to fetch the game associated with this ID.
		/// </summary>
		/// <param name="output">The game with this ID or null when this returns false.</param>
		/// <returns>Returns true if a game with this ID was found and false otherwise.</returns>
		public bool TryGetGameObject([MaybeNullWhen(false)] out Game output) => TryGet(this,out output);

		public static bool operator ==(GameID a, GameID b) => a.ID == b.ID;
		public static bool operator !=(GameID a, GameID b) => a.ID != b.ID;

		public bool Equals(GameID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is GameID id && this == id;

		public static bool operator <(GameID a, GameID b) => a.ID < b.ID;
		public static bool operator <=(GameID a, GameID b) => a.ID <= b.ID;
		public static bool operator >(GameID a, GameID b) => a.ID > b.ID;
		public static bool operator >=(GameID a, GameID b) => a.ID >= b.ID;

		public int CompareTo(GameID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly GameIDType ID;

		/// <summary>
		/// The null ID never used by games.
		/// </summary>
		public static readonly GameID NULL = new GameID();

		/// <summary>
		/// The null ID value.
		/// </summary>
		private const GameIDType NullValue = 0;

		#region Static System
		/// <summary>
		/// Obtains a fresh ID that is not in use.
		/// </summary>
		/// <param name="obj">The game this ID will be assigned to.</param>
		/// <returns>Returns the new ID.</returns>
		public static GameID GetFreshID(Game obj)
		{
			GameID ret = new GameID(NextID);
			_lut[ret] = new WeakReference(obj,false);

			return ret;
		}

		/// <summary>
		/// Gets the game with the given ID.
		/// </summary>
		/// <param name="id">The ID of the game to obtain.</param>
		/// <returns>Returns the game with ID <paramref name="id"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="id"/> is not the ID of a game.</exception>
		public static Game Get(GameID id)
		{
			WeakReference ptr = _lut[id];
			
			if(ptr.Target is null)
			{
				ReleaseID(id.ID);
				throw new KeyNotFoundException();
			}
			
			return (Game)ptr.Target;
		}

		/// <summary>
		/// Attempts to get a game by its ID.
		/// </summary>
		/// <param name="id">The ID of the game to obtain.</param>
		/// <param name="ret">The discovered game or null when this returns false.</param>
		/// <returns>Returns true if there is a game with the given ID and false otherwise.</returns>
		public static bool TryGet(GameID id, [MaybeNullWhen(false)] out Game ret)
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

			ret = (Game)ptr.Target;
			return true;
		}

		/// <summary>
		/// Releases a game ID for future use.
		/// </summary>
		private static void ReleaseID(GameIDType id)
		{
			if(_lut.TryRemove(id,out _))
				_ndid.Push(id);
			
			return;
		}

		/// <summary>
		/// Obtains the next available ID for a game.
		/// This will recycle discarded game IDs.
		/// </summary>
		/// <remarks>The 0 ID is reserved for error results.</remarks>
		private static GameIDType NextID
		{
			get
			{
				if(_ndid.Count == 0)
					lock(_ndid_lock)
						return _nid++;

				if(_ndid.TryPop(out GameIDType ret))
					return ret;

				lock(_ndid_lock)
					return _nid++;
			}
		}

		private static ConcurrentStack<GameIDType> _ndid = new ConcurrentStack<GameIDType>();
		private static GameIDType _nid = 1;
		private static object _ndid_lock = new object();

		private static ConcurrentDictionary<GameID,WeakReference> _lut = new ConcurrentDictionary<GameID,WeakReference>();
		#endregion
	}

	/// <summary>
	/// Converts a GameID to/from a JSON format.
	/// </summary>
	file class JsonGameIDConverter : JsonBaseConverter<GameID>
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

		protected override GameID ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 1)
				throw new JsonException();

			return new GameID((GameIDType)properties["ID"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, GameID value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("ID",value.ID);
			return;
		}
	}
}