namespace GameEngine.Assets
{
	/// <summary>
	/// Marks something as an asset and defines its basic properties.
	/// </summary>
	/// <remarks>
	/// In-built MonoGame assets do not implement this interface.
	/// </remarks>
	public interface IAsset
	{
		/// <summary>
		/// The name of the asset.
		/// In general, this should be the absolute path (relative to the content root) of the asset including the file extension.
		/// <para/>
		/// Runtime created assets not loaded from a source file should assign the empty string to this.
		/// This is consistent with runtime created MonoGame assets.
		/// </summary>
		/// <remarks>
		/// If the implementing class also happens to have a Name property which leads to the asset in the content system, trust this over Name.
		/// This will have the file extension as well, which is more useful (if sometimes annoying to remove when needed).
		/// </remarks>
		public string AssetName
		{get;}
	}
}
