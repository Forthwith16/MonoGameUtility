using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for AssetID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converter's read type below as well
using AssetIDType = uint;

namespace GameEngine.Assets
{
	/// <summary>
	/// A struct wrapper for asset IDs.
	/// </summary>
	[JsonConverter(typeof(JsonAssetIDConverter))]
	public readonly struct AssetID : IComparable<AssetID>, IEquatable<AssetID>, IDisposable
	{
		/// <summary>
		/// Constructs the null ID.
		/// </summary>
		public AssetID()
		{
			ID = NullValue;
			return;
		}

		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		/// <remarks>
		///	The intended way to obtain new IDs is to call <see cref="GetFreshID(AssetBase)"/>.
		///	This is used for creating dummy IDs to search for things or store information.
		/// </remarks>
		public AssetID(AssetIDType id)
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

		public static implicit operator AssetID(AssetIDType id) => new AssetID(id);
		public static explicit operator AssetIDType(AssetID id) => id.ID;

		/// <summary>
		/// Obtains the asset with this ID.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown if there is no asset with this ID.</exception>
		public static implicit operator AssetBase(AssetID id) => Get(id);

		/// <summary>
		/// Trys to fetch the asset associated with this ID.
		/// </summary>
		/// <param name="output">The asset with this ID or null when this returns false.</param>
		/// <returns>Returns true if an asset with this ID was found and false otherwise.</returns>
		public bool TryGetAsset([MaybeNullWhen(false)] out AssetBase output) => TryGet(this,out output);

		public static bool operator ==(AssetID a, AssetID b) => a.ID == b.ID;
		public static bool operator !=(AssetID a, AssetID b) => a.ID != b.ID;

		public bool Equals(AssetID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetID id && this == id;

		public static bool operator <(AssetID a, AssetID b) => a.ID < b.ID;
		public static bool operator <=(AssetID a, AssetID b) => a.ID <= b.ID;
		public static bool operator >(AssetID a, AssetID b) => a.ID > b.ID;
		public static bool operator >=(AssetID a, AssetID b) => a.ID >= b.ID;

		public int CompareTo(AssetID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly AssetIDType ID;

		/// <summary>
		/// The null ID never used by assets.
		/// </summary>
		public static readonly AssetID NULL = new AssetID();

		/// <summary>
		/// The null ID value.
		/// </summary>
		private const AssetIDType NullValue = 0;

		#region Static System
		/// <summary>
		/// Obtains a fresh ID that is not in use.
		/// </summary>
		/// <param name="obj">The asset this ID will be assigned to.</param>
		/// <returns>Returns the new ID.</returns>
		public static AssetID GetFreshID(AssetBase obj)
		{
			AssetID ret = new AssetID(NextID);
			_lut[ret] = new WeakReference(obj,false);

			return ret;
		}

		/// <summary>
		/// Gets the asset with the given ID.
		/// </summary>
		/// <param name="id">The ID of the asset to obtain.</param>
		/// <returns>Returns the asset with ID <paramref name="id"/>.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="id"/> is not the ID of an asset.</exception>
		public static AssetBase Get(AssetID id)
		{
			WeakReference ptr = _lut[id];
			
			if(ptr.Target is null)
			{
				ReleaseID(id.ID);
				throw new KeyNotFoundException();
			}
			
			return (AssetBase)ptr.Target;
		}

		/// <summary>
		/// Attempts to get an asset by its ID.
		/// </summary>
		/// <param name="id">The ID of the asset to obtain.</param>
		/// <param name="ret">The discovered asset or null when this returns false.</param>
		/// <returns>Returns true if there is a asset with the given ID and false otherwise.</returns>
		public static bool TryGet(AssetID id, [MaybeNullWhen(false)] out AssetBase ret)
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

			ret = (AssetBase)ptr.Target;
			return true;
		}

		/// <summary>
		/// Releases an asset ID for future use.
		/// </summary>
		private static void ReleaseID(AssetIDType id)
		{
			if(_lut.TryRemove(id,out _))
				_ndid.Push(id);
			
			return;
		}

		/// <summary>
		/// Obtains the next available ID for an asset.
		/// This will recycle discarded asset IDs.
		/// </summary>
		/// <remarks>The 0 ID is reserved for error results.</remarks>
		private static AssetIDType NextID
		{
			get
			{
				if(_ndid.Count == 0)
					lock(_ndid_lock)
						return _nid++;

				if(_ndid.TryPop(out AssetIDType ret))
					return ret;

				lock(_ndid_lock)
					return _nid++;
			}
		}

		private static ConcurrentStack<AssetIDType> _ndid = new ConcurrentStack<AssetIDType>();
		private static AssetIDType _nid = 1;
		private static object _ndid_lock = new object();

		private static ConcurrentDictionary<AssetID,WeakReference> _lut = new ConcurrentDictionary<AssetID,WeakReference>();
		#endregion
	}

	/// <summary>
	/// Converts an AssetID to/from a JSON format.
	/// </summary>
	file class JsonAssetIDConverter : JsonBaseConverter<AssetID>
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

		protected override AssetID ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 1)
				throw new JsonException();

			return new AssetID((AssetIDType)properties["ID"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, AssetID value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("ID",value.ID);
			return;
		}
	}
}
