using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace GameEngine.Assets.Serialization
{
	/// <summary>
	/// Provides static information about an asset.
	/// In particular, this tells us how to load an asset from disc into memory.
	/// <para/>
	/// This attribute will be inherited, but only one is allowed to exist on a class at a time.
	/// When this is applied to a child class whose parent already has this attribute, it replaces the parent's version in the class hierarchy from that child class on.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = true)]
	public class AssetLoader : Attribute
	{
		/// <summary>
		/// This provides an asset type with information about how to load it from disc into memory.
		/// Be sure to read the parameter specifications carefully to ensure this attribute provides the correct information.
		/// </summary>
		/// <param name="loading_type">The is the type to reflect into in order to find the static method specified by <paramref name="loading_method_name"/>.</param>
		/// <param name="loading_method_name">
		/// This is the name of a static method in a type specified by <paramref name="loading_type"/>.
		/// This method must have a signature of <c>A loading_method_name(string)</c>.
		/// A may technically be any type, but its use-case is to assign its return value to variables that derive from AssetBase in <see cref="LoadAsset{A}(string, out A)"/>.
		/// In particular, the return type should usually be the type this attribute is applied to (which should inherit from AssetBase).
		/// <para/>
		/// If <paramref name="loading_type"/> and this parameter together do not specify a valid method, then this attribute will silently fail whenever a call to <see cref="LoadAsset{A}(string, out A)"/> is made.
		/// </param>
		public AssetLoader(Type loading_type, string loading_method_name)
		{
			LoadingType = loading_type;
			LoadingMethod = LoadingType.GetMethod(loading_method_name,BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,[typeof(string)]);
			
			return;
		}

		/// <summary>
		/// Loads an asset of type <typeparamref name="A"/> from <paramref name="path"/> on disc into memory.
		/// Note that the type of asset actually returned may be a derived type of <typeparamref name="A"/> depending on the loading method and the contents of the asset file on disc.
		/// </summary>
		/// <typeparam name="A">The asset type to load. This is a minimum requirement. The actual return type may be more derived.</typeparam>
		/// <param name="path">The path to a file on disc describing the asset. This path may be absolute or relative to the working directory.</param>
		/// <param name="output">The asset loaded from disc when this returns true or null when this returns false.</param>
		/// <returns>Returns true if the asset could be loaded from disc and assigned to <paramref name="output"/>. Returns false otherwise for any reason.</returns>
		public bool LoadAsset<A>(string path, [MaybeNullWhen(false)] out A output) where A : AssetBase
		{
			// If we failed to get a loading method, do nothing
			if(LoadingMethod is null)
			{
				output = null;
				return false;
			}

			// If we can assign a loaded asset to a variable of type A, then we may proceed
			// It is better to check this ahead of time so we don't waste time of the load, since that takes a lot longer than some mild reflection
			if(LoadingMethod.ReturnType.IsAssignableTo(typeof(A)))
			{
				try
				{output = LoadingMethod.Invoke(null,[path]) as A;}
				catch
				{output = null;}

				return output is not null;
			}
			else
			{
				// We have one backup trick: implicit conversions
				// The way we check for this without using under the hood tricks is kinda jank, but whatever
				try
				{Expression.Convert(Expression.Parameter(LoadingMethod.ReturnType),typeof(A));}
				catch // If we caught an error, that means there is no conversion path from the return type to A
				{
					output = null;
					return false;
				}

				// We cleared the implicit conversion check, so proceed as planned
				try
				{output = LoadingMethod.Invoke(null,[path]) as A;}
				catch
				{output = null;}

				return output is not null;
			}

			// We couldn't assign to type A, so we failed
			output = null;
			return false;
		}

		/// <summary>
		/// This is the type that will perform the loading action.
		/// </summary>
		public Type LoadingType
		{get;}

		/// <summary>
		/// This is the (static) method that loads the asset from disc into memory.
		/// Its signature must be <c>A LoadingMethod(string)</c>, where A is any type assignable to AssetBase.
		/// This will be null if the method specified did not exist or had the wrong signiture.
		/// </summary>
		protected MethodInfo? LoadingMethod
		{get;}
	}
}
