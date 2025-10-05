using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for AssetID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converter's read type below as well
using AssetIDType = uint;

namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// A struct wrapper for asset IDs.
	/// </summary>
	[JsonConverter(typeof(JsonAssetIDConverter))]
	public readonly struct AssetID : IComparable<AssetID>, IEquatable<AssetID>
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
		///	The intended way to obtain new IDs is to call <see cref="GetFreshID(IAsset)"/>.
		///	This is used for creating dummy IDs to search for things or store information.
		/// </remarks>
		public AssetID(AssetIDType id)
		{
			ID = id;
			return;
		}

		public static implicit operator AssetID(AssetIDType id) => new AssetID(id);
		public static explicit operator AssetIDType(AssetID id) => id.ID;

		public static bool operator ==(AssetID a, AssetID b) => a.ID == b.ID;
		public static bool operator !=(AssetID a, AssetID b) => a.ID != b.ID;

		public bool Equals(AssetID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetID id && this == id;

		public static bool operator <(AssetID a, AssetID b) => a.ID < b.ID;
		public static bool operator <=(AssetID a, AssetID b) => a.ID <= b.ID;
		public static bool operator >(AssetID a, AssetID b) => a.ID > b.ID;
		public static bool operator >=(AssetID a, AssetID b) => a.ID >= b.ID;

		public static AssetID operator ++(AssetID id) => NextID(id);

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

		/// <summary>
		/// Obtains the next ID after <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The ID to increment.</param>
		/// <returns>The successor function on asset IDs returns the next ID after the given ID.</returns>
		public static AssetID NextID(AssetID id) => new AssetID(id.ID + 1);
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
