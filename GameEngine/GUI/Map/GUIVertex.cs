namespace GameEngine.GUI.Map
{
	/// <summary>
	/// Encapulates the digital exits out of a GUI component in a GUICore.
	/// </summary>
	public class GUIVertex
	{
		/// <summary>
		/// Creates a GUI vertex for <paramref name="me"/>.
		/// </summary>
		/// <param name="me">The GUI component this GUI vertex is for.</param>
		/// <param name="owner">The GUIMap this vertex will belong to.</param>
		public GUIVertex(IGUI me, GUIMap owner)
		{
			Me = me;
			Owner = owner;

			return;
		}

		/// <summary>
		/// Removes all references to <paramref name="v"/> from this vertex.
		/// </summary>
		/// <param name="v">The reference to extirpate. If this is Me, nothing will happen.</param>
		public void Expunge(IGUI v)
		{
			if(v == Me)
				return;

			if(Up == v)
				Up = null;

			if(Down == v)
				Down = null;

			if(Left == v)
				Left = null;

			if(Right == v)
				Right = null;

			return;
		}

		/// <summary>
		/// Clears this vertex of all connections.
		/// </summary>
		public void ClearEdges()
		{
			Left = Right = Up = Down = null;
			return;
		}

		/// <summary>
		/// The GUI component this vertex is for.
		/// </summary>
		public IGUI Me
		{get; init;}

		/// <summary>
		/// The owner of this vertex.
		/// </summary>
		protected GUIMap Owner
		{get; set;}

		/// <summary>
		/// A digital up takes us from Me to this GUI component.
		/// <para/>
		/// If this is null, a digital up takes us nowhere.
		/// </summary>
		public IGUI? Up
		{get; set;}

		/// <summary>
		/// A digital down takes us from Me to this GUI component.
		/// <para/>
		/// If this is null, a digital down takes us nowhere.
		/// </summary>
		public IGUI? Down
		{get; set;}

		/// <summary>
		/// A digital left takes us from Me to this GUI component.
		/// <para/>
		/// If this is null, a digital left takes us nowhere.
		/// </summary>
		public IGUI? Left
		{get; set;}

		/// <summary>
		/// A digital right takes us from Me to this GUI component.
		/// <para/>
		/// If this is null, a digital right takes us nowhere.
		/// </summary>
		public IGUI? Right
		{get; set;}
	}
}
