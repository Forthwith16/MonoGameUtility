using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for InternalAssetID values
// Change this if you need more or fewer ID values
// Be sure to change the JSON converter's read type below as well
using InternalAssetIDType = uint;

namespace GameEnginePipeline.Serialization
{
	/// <summary>
	/// A struct wrapper for (internal use only) asset IDs.
	/// </summary>
	[JsonConverter(typeof(JsonInternalAssetIDConverter))]
	public readonly struct InternalAssetID : IComparable<InternalAssetID>, IEquatable<InternalAssetID>
	{
		/// <summary>
		/// Constructs the null ID.
		/// </summary>
		public InternalAssetID()
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
		public InternalAssetID(InternalAssetIDType id)
		{
			ID = id;
			return;
		}

		public static implicit operator InternalAssetID(InternalAssetIDType id) => new InternalAssetID(id);
		public static explicit operator InternalAssetIDType(InternalAssetID id) => id.ID;

		public static bool operator ==(InternalAssetID a, InternalAssetID b) => a.ID == b.ID;
		public static bool operator !=(InternalAssetID a, InternalAssetID b) => a.ID != b.ID;

		public bool Equals(InternalAssetID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is InternalAssetID id && this == id;

		public static bool operator <(InternalAssetID a, InternalAssetID b) => a.ID < b.ID;
		public static bool operator <=(InternalAssetID a, InternalAssetID b) => a.ID <= b.ID;
		public static bool operator >(InternalAssetID a, InternalAssetID b) => a.ID > b.ID;
		public static bool operator >=(InternalAssetID a, InternalAssetID b) => a.ID >= b.ID;

		public static InternalAssetID operator ++(InternalAssetID id) => NextID(id);

		public int CompareTo(InternalAssetID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly InternalAssetIDType ID;

		/// <summary>
		/// The null ID never used by assets.
		/// </summary>
		public static readonly InternalAssetID NULL = new InternalAssetID();

		/// <summary>
		/// The null ID value.
		/// </summary>
		private const InternalAssetIDType NullValue = 0;

		/// <summary>
		/// Obtains the next ID after <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The ID to increment.</param>
		/// <returns>The successor function on asset IDs returns the next ID after the given ID.</returns>
		public static InternalAssetID NextID(InternalAssetID id) => new InternalAssetID(id.ID + 1);
	}

	/// <summary>
	/// Converts an InternalAssetID to/from a JSON format.
	/// </summary>
	file class JsonInternalAssetIDConverter : JsonBaseConverter<InternalAssetID>
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

		protected override InternalAssetID ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 1)
				throw new JsonException();

			return new InternalAssetID((InternalAssetIDType)properties["ID"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, InternalAssetID value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("ID",value.ID);
			return;
		}
	}
}
