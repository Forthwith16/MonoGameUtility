namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// A dictionary of assets.
	/// </summary>
	public class AssetDictionary
	{
		/*/// <summary>
		/// Creates an empty asset dictionary.
		/// </summary>
		public AssetDictionary()
		{
			BackingDictionary = new Dictionary<AssetID,AssetBase>();
			ReleasedIDs = new Stack<AssetID>();

			Last = AssetID.NULL;
			return;
		}

		/// <summary>
		/// Adds an asset to this dictionary and assigns it an ID.
		/// </summary>
		/// <param name="asset">The asset to add. It is assigned an asset ID when added to this dictionary.</param>
		public void AddAsset(AssetBase asset)
		{
			//asset.LoadID = NextID;
			//BackingDictionary[asset.LoadID] = asset;

			return;
		}


		
		/// <summary>
		/// Trys to fetch the asset with ID <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The ID of the asset to get.</param>
		/// <param name="output">The asset with ID <paramref name="id"/> or null when this returns false.</param>
		/// <returns>Returns true if a asset with this ID was found and false otherwise.</returns>
		public bool TryGetAsset(AssetID id, [MaybeNullWhen(false)] out AssetBase output) => BackingDictionary.TryGetValue(id,out output);



		/// <summary>
		/// The backing data structure.
		/// </summary>
		protected Dictionary<AssetID,AssetBase> BackingDictionary
		{get;}

		/// <summary>
		/// The number of assets in this dictionary.
		/// </summary>
		public int Count => BackingDictionary.Count;

		/// <summary>
		/// The set of released asset IDs.
		/// </summary>
		protected Stack<AssetID> ReleasedIDs = new Stack<AssetID>();
		
		/// <summary>
		/// The last issued ID.
		/// </summary>
		protected AssetID Last
		{get; set;}

		/// <summary>
		/// The next ID available.
		/// </summary>
		protected AssetID NextID => ReleasedIDs.Count > 0 ? ReleasedIDs.Pop() : ++Last;*/
	}
}
