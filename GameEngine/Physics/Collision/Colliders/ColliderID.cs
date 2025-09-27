using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for ColliderID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converters's read type below as well
using ColliderIDType = uint;

namespace GameEngine.Physics.Collision.Colliders
{
	/// <summary>
	/// A struct wrapper for collider IDs.
	/// </summary>
	/// <typeparam name="T">The actual collider type.</typeparam>
	[JsonConverter(typeof(JsonColliderIDConverter))]
	public readonly struct ColliderID<T> : IComparable<ColliderID<T>>, IEquatable<ColliderID<T>>, IDisposable where T : ICollider<T>
	{
		/// <summary>
		/// Constructs the null ID.
		/// </summary>
		public ColliderID()
		{
			ID = NullValue;
			return;
		}

		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		/// <remarks>
		///	The intended way to obtain new IDs it so call <see cref="GetFreshID(T)"/>.
		///	This is used for creating dummy IDs to search for things or store information.
		/// </remarks>
		public ColliderID(ColliderIDType id)
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

		public static implicit operator ColliderID<T>(ColliderIDType id) => new ColliderID<T>(id);
		public static explicit operator ColliderIDType(ColliderID<T> id) => id.ID;

		/// <summary>
		/// Obtains the collider with this ID.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if there is no collider with this ID.</exception>
		public static implicit operator T(ColliderID<T> id) => Get(id);

		/// <summary>
		/// Trys to fetch the collider associated with this ID.
		/// </summary>
		/// <param name="output">The collider with this ID or null when this returns false.</param>
		/// <returns>Returns true if a collider with this ID was found and false otherwise.</returns>
		public bool TryGetCollider([MaybeNullWhen(false)] out T output) => TryGet(this,out output);

		public static bool operator ==(ColliderID<T> a, ColliderID<T> b) => a.ID == b.ID;
		public static bool operator !=(ColliderID<T> a, ColliderID<T> b) => a.ID != b.ID;

		public bool Equals(ColliderID<T> other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is ColliderID<T> id && this == id;

		public static bool operator <(ColliderID<T> a, ColliderID<T> b) => a.ID < b.ID;
		public static bool operator <=(ColliderID<T> a, ColliderID<T> b) => a.ID <= b.ID;
		public static bool operator >(ColliderID<T> a, ColliderID<T> b) => a.ID > b.ID;
		public static bool operator >=(ColliderID<T> a, ColliderID<T> b) => a.ID >= b.ID;

		public int CompareTo(ColliderID<T> other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly ColliderIDType ID;

		/// <summary>
		/// The null ID never used by colliders.
		/// </summary>
		public static readonly ColliderID<T> NULL = new ColliderID<T>();

		/// <summary>
		/// The null ID value.
		/// </summary>
		private const ColliderIDType NullValue = 0;

		#region Static System
		/// <summary>
		/// Obtains a fresh ID that is not in use.
		/// </summary>
		/// <param name="obj">The collider this ID will be assigned to.</param>
		/// <returns>Returns the new ID.</returns>
		public static ColliderID<T> GetFreshID(T obj)
		{
			ColliderID<T> ret = new ColliderID<T>(NextID);
			_lut[ret] = new WeakReference(obj,false);

			return ret;
		}

		/// <summary>
		/// Gets the collider with the given ID.
		/// </summary>
		/// <param name="id">The ID of the collider to obtain.</param>
		/// <returns>Returns the collider with ID <paramref name="id"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="id"/> is not the ID of a collider.</exception>
		public static T Get(ColliderID<T> id)
		{
			WeakReference ptr = _lut[id];
			
			if(ptr.Target is null)
			{
				ReleaseID(id.ID);
				throw new KeyNotFoundException();
			}
			
			return (T)ptr.Target;
		}

		/// <summary>
		/// Attempts to get a collider by its ID.
		/// </summary>
		/// <param name="id">The ID of the collider to obtain.</param>
		/// <param name="ret">The discovered collider or default(T) when this returns false.</param>
		/// <returns>Returns true if there is a collider with the given ID and false otherwise.</returns>
		public static bool TryGet(ColliderID<T> id, [MaybeNullWhen(false)] out T ret)
		{
			if(!_lut.TryGetValue(id,out WeakReference? ptr))
			{
				ret = default(T);
				return false;
			}

			if(ptr.Target is null)
			{
				ReleaseID(id.ID);

				ret = default(T);
				return false;
			}

			ret = (T)ptr.Target;
			return true;
		}

		/// <summary>
		/// Releases a collider ID for future use.
		/// </summary>
		private static void ReleaseID(ColliderIDType id)
		{
			if(_lut.TryRemove(id,out _))
				_ndid.Push(id);

			return;
		}

		/// <summary>
		/// Obtains the next available ID for a collider.
		/// This will recycle discarded collider IDs.
		/// </summary>
		/// <remarks>The 0 ID is reserved for error results.</remarks>
		private static ColliderIDType NextID
		{
			get
			{
				if(_ndid.Count == 0)
					lock(_ndid_lock)
						return _nid++;

				if(_ndid.TryPop(out ColliderIDType ret))
					return ret;

				lock(_ndid_lock)
					return _nid++;
			}
		}

		private static ConcurrentStack<ColliderIDType> _ndid = new ConcurrentStack<ColliderIDType>();
		private static ColliderIDType _nid = 1;
		private static object _ndid_lock = new object();

		private static ConcurrentDictionary<ColliderID<T>,WeakReference> _lut = new ConcurrentDictionary<ColliderID<T>,WeakReference>();
		#endregion
	}

	/// <summary>
	/// Converts a ColliderID to/from a JSON format.
	/// </summary>
	file class JsonColliderIDConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonColliderIDConverter() : base((t,ops) => [],typeof(ColliderID<>),typeof(JGOIDC<>))
		{return;}

		/// <summary>
		/// Converts a ColliderID<T> to/from a JSON format.
		/// </summary>
		/// <typeparam name="T">The actual collider type.</typeparam>
		private class JGOIDC<T> : JsonBaseConverter<ColliderID<T>> where T : ICollider<T>
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

			protected override ColliderID<T> ConstructT(Dictionary<string,object?> properties)
			{
				if(properties.Count != 1)
					throw new JsonException();

				return new ColliderID<T>((ColliderIDType)properties["ID"]!);
			}

			protected override void WriteProperties(Utf8JsonWriter writer, ColliderID<T> value, JsonSerializerOptions ops)
			{
				writer.WriteNumber("ID",value.ID);
				return;
			}
		}
	}
}
