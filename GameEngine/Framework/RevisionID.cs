using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

// This is the backing type for revision values
// Change this if you need more or fewer revision values
using RevisionType = uint;

namespace GameEngine.Framework
{
	/// <summary>
	/// A struct wrapper for revision IDs.
	/// <para/>
	/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
	/// Revision numbers thus always start from at least 2.
	/// </summary>
	[JsonConverter(typeof(JsonRevisionIDConverter))]
	public readonly struct RevisionID : IComparable<RevisionID>, IEquatable<RevisionID>
	{
		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		public RevisionID(RevisionType id)
		{
			ID = id;
			return;
		}

		public static implicit operator RevisionID(RevisionType id) => new RevisionID(id);
		public static explicit operator RevisionType(RevisionID id) => id.ID;

		public static RevisionID operator ++(RevisionID id) => new RevisionID(id.ID == RevisionType.MaxValue ? Initial.ID : id.ID + 1); // We allow ID looping for matrices, though hopefully it's never necessary

		public static bool operator ==(RevisionID a, RevisionID b) => a.ID == b.ID;
		public static bool operator !=(RevisionID a, RevisionID b) => a.ID != b.ID;

		public bool Equals(RevisionID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is RevisionID id && this == id;

		public int CompareTo(RevisionID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly RevisionType ID;

		/// <summary>
		/// The stale revision ID.
		/// </summary>
		public static readonly RevisionID Stale = new RevisionID(0);

		/// <summary>
		/// A no parent ID.
		/// </summary>
		public static readonly RevisionID NoParentInfo = new RevisionID(1);

		/// <summary>
		/// The initial revision ID.
		/// </summary>
		public static readonly RevisionID Initial = new RevisionID(2);
	}

	/// <summary>
	/// Converts a RevisionID to/from a JSON format.
	/// </summary>
	file class JsonRevisionIDConverter : JsonBaseConverter<RevisionID>
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

		protected override RevisionID ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 1)
				throw new JsonException();

			return new RevisionID((RevisionType)properties["ID"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, RevisionID value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("ID",value.ID);
			return;
		}
	}
}
