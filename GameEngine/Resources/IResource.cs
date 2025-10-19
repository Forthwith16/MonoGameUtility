using GameEngine.Assets;
using System.Diagnostics.CodeAnalysis;

namespace GameEngine.Resources
{
	/// <summary>
	/// Marks something as a resource and defines its basic properties.
	/// </summary>
	/// <remarks>
	/// In-built MonoGame resources do not implement this interface.
	/// </remarks>
	public interface IResource
	{
		/// <summary>
		/// Converts this resource into its asset form.
		/// </summary>
		/// <returns>
		/// Returns an asset representing this resource.
		/// The returned asset should have enough information to recreate this resource in its initial state.
		/// It is not, however, required to be able to recreate this object in its present state and probably should not.
		/// <para/>
		/// Alternatively, this may return null if the resource has no corresponding asset type.
		/// </returns>
		protected AssetBase? ToAsset();

		/// <summary>
		/// Converts this resource into its asset form.
		/// </summary>
		/// <typeparam name="AssetType">The target asset type.</typeparam>
		/// <param name="output">The asset created when this returns true or null when this returns false.</param>
		/// <returns>Returns true if this could be transformed into an asset of type <typeparamref name="AssetType"/> (or a derived type assignable to <typeparamref name="AssetType"/>). Returns false otherwise.</returns>
		public bool ToAsset<AssetType>([MaybeNullWhen(false)] out AssetType output) where AssetType : AssetBase
		{
			if(ToAsset() is AssetType ret)
			{
				output = ret;
				return true;
			}
			
			output = null;
			return false;
		}

		/// <summary>
		/// The name of the resource.
		/// In general, this should be the absolute path (relative to the content root) of the resource's asset source including the file extension.
		/// <para/>
		/// Runtime created resources not loaded from a source file should assign the empty string to this.
		/// This is consistent with runtime created MonoGame resource.
		/// </summary>
		/// <remarks>
		/// If the implementing class also happens to have a Name property which leads to the source asset in the content system, trust this over Name.
		/// This will have the file extension as well, which is more useful (if sometimes annoying to remove when needed).
		/// </remarks>
		public string ResourceName
		{get;}
	}
}
