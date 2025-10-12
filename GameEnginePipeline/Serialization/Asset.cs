using GameEnginePipeline.Assets;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GameEnginePipeline.Serialization
{
	/// <summary>
	/// The asset attribute tag.
	/// This denote that a class is an asset version of a more proper object for a game.
	/// <para/>
	/// This attribute takes a single Type parameter (call this type <c>T</c>) specifying what the proper object version of the asset class is.
	/// It will use type <c>T</c> to construct assets from proper objects.
	/// <para/>
	/// This attribute can only be applied to assets.
	/// It will silently enforce that the class it's attached to (call this type <c>A</c>) derives from AssetBase.
	/// It will also silently enforce that <c>A</c> has a constructor available requiring only a single parameter of type <c>T</c> at runtime.
	/// This will allow for serialization from type <c>T</c> to type <c>A</c> without knowledge of either type.
	/// If this constructor is not present, this will fail to generate an entry in this class for such construction.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class,AllowMultiple = true,Inherited = false)]
	public class Asset : Attribute
	{
		/// <summary>
		/// Constructs the asset double dictionary.
		/// </summary>
		static Asset()
		{
			AssetInfo = new Dictionary<Type,Dictionary<Type,ConstructorInfo>>();
			return;
		}

		/// <summary>
		/// Creates an asset from a raw game type.
		/// </summary>
		/// <typeparam name="A">The asset type to produce.</typeparam>
		/// <param name="raw">The raw game object to turn into an asset.</param>
		/// <returns>Returns the asset created or null if no asset could be produced from <paramref name="raw"/>.</returns>
		public static A? CreateAsset<A>(object raw) where A : AssetBase
		{
			Type a = typeof(A);

			// The first thing to do is check if type A is a type we know about
			if(!AssetInfo.TryGetValue(a,out Dictionary<Type,ConstructorInfo>? raw_info))
				if(!LoadType(a,out raw_info))
					return null;

			// At this point, we know at least something about A, but it remains to show if we know something useful about A
			if(!raw_info.TryGetValue(raw.GetType(),out ConstructorInfo? a_info))
				return null; // We know nothing about turning raw into an A asset

			try
			{return (A)a_info.Invoke([raw]);}
			catch
			{return null;}
		}

		/// <summary>
		/// Determines if a concrete object of type <paramref name="concrete_type"/> can be converted into an asset of type <paramref name="asset_type"/>.
		/// </summary>
		/// <param name="concrete_type">The concrete type to turn into an asset type.</param>
		/// <param name="asset_type">The asset type to create from <paramref name="concrete_type"/>.</param>
		/// <returns>Returns true if the conversion is possible and false otherwise.</returns>
		public static bool CanCreateAsset(Type concrete_type, Type asset_type)
		{
			// Get or load and get the information for asset_type
			if(!AssetInfo.TryGetValue(asset_type,out Dictionary<Type,ConstructorInfo>? raw_info))
				if(!LoadType(asset_type,out raw_info))
					return false;

			return raw_info.ContainsKey(concrete_type);
		}

		/// <summary>
		/// Loads asset info for type <paramref name="asset_type"/>.
		/// </summary>
		/// <param name="asset_type">The asset type to load info for.</param>
		/// <param name="raw_info">The asset information loaded when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="asset_type"/> was a valid asset type and something was loaded for it and false otherwise.</returns>
		protected static bool LoadType(Type asset_type, [MaybeNullWhen(false)] out Dictionary<Type,ConstructorInfo> raw_info)
		{
			// We've never heard of asset_type, so let's learn about it
			raw_info = new Dictionary<Type,ConstructorInfo>();

			// asset_type may have multiple Asset attributes, and we need to document each one
			foreach(Asset asset in asset_type.GetCustomAttributes<Asset>())
			{
				// We accept asset_type as an asset if A has a constructor that takes just the concrete type
				ConstructorInfo? c_info = asset_type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,[asset.AssetConcreteType]);

				if(c_info is not null)
					raw_info[asset.AssetConcreteType] = c_info;
			}

			// If we learned nothing, we're done
			if(raw_info.Count == 0)
			{
				raw_info = null;
				return false;
			}

			// Otherwise, we learned SOMETHING and should remember it for later
			AssetInfo[asset_type] = raw_info;

			return true;
		}

		/// <summary>
		/// The information required to build assets from concrete objects.
		/// The first index is the asset type.
		/// The second index is the concrete type.
		/// The value, then, is the constructor that makes an asset from the raw type.
		/// </summary>
		private static Dictionary<Type,Dictionary<Type,ConstructorInfo>> AssetInfo;

		#region Nonstatic Definition
		/// <summary>
		/// Denote a type as an asset which can be used to construct a concrete game type.
		/// </summary>
		/// <param name="concrete_type">The concrete game type this asset can be turned into.</param>
		public Asset(Type concrete_type)
		{
			AssetConcreteType = concrete_type;
			return;
		}

		/// <summary>
		/// This is the concrete type that this asset can become.
		/// </summary>
		public Type AssetConcreteType
		{get;}
		#endregion
	}
}
