namespace GameEngine.Assets
{
	/// <summary>
	/// The base class for assets.
	/// </summary>
	public abstract class Asset
	{
		/// <summary>
		/// The base constructor for assets.
		/// </summary>
		/// <param name="name">The name to give the asset.</param>
		protected Asset(string name)
		{
			Name = name;
			return;
		}

		/// <summary>
		/// The name of the asset.
		/// </summary>
		public string Name
		{get;}
	}
}
