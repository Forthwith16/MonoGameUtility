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
