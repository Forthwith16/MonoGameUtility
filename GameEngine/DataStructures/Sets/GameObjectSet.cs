using GameEngine.Framework;
using System.Collections;

namespace GameEngine.DataStructures.Sets
{
	/// <summary>
	/// A set of game objects that can be added or removed from a game together.
	/// </summary>
	/// <remarks>
	/// Note that while this is a set, it does not actually check for duplicates itself.
	/// To enable it to do so, back it with a set data structure which does check for duplicates.
	/// The intended purpose of this is to provide for convenient batch operations, not to error check.
	/// </remarks>
	public class GameObjectSet<T> : ICollection<T> where T : GameObject
	{
		/// <summary>
		/// Creates an empty game object set backed by a List.
		/// </summary>
		/// <param name="game">The game this set will add/remove objects from.</param>
		public GameObjectSet(RenderTargetFriendlyGame game)
		{
			Game = game;
			Storage = new AVLSet<T>(Comparer<T>.Create((a,b) => a.ID.CompareTo(b.ID)));

			InGame = false;
			return;
		}

		/// <summary>
		/// Adds all of this set's components to <see cref="Game"/>.
		/// </summary>
		public void AddAllToGame()
		{
			if(InGame)
				return;

			InGame = true;
			
			foreach(T t in this)
				try
				{Game.Components.Add(t);}
				catch(ArgumentException)
				{} // We don't actually care about failure on duplicate Add calls, since the Add technically succeeded

			return;
		}

		/// <summary>
		/// Remvoes this set's components from <see cref="Game"/>.
		/// </summary>
		public void RemoveAllFromGame()
		{
			if(!InGame)
				return;

			foreach(T t in this)
				Game.Components.Remove(t);

			InGame = false; // It's worse to Add before Remove, so we do this last and accept a possible thread unsafe double Remove
			return;
		}

		public void Add(T item)
		{
			if(InGame)
				Game.Components.Add(item);

			Storage.Add(item);
			return;
		}

		public bool Remove(T item)
		{
			if(InGame)
				Game.Components.Remove(item);

			return Storage.Remove(item);
		}

		public bool Contains(T item) => Storage.Contains(item);

		public void Clear()
		{
			while(NotEmpty)
				Remove(Storage.First());
			
			return;
		}

		public void CopyTo(T[] array, int offset) => Storage.CopyTo(array,offset);

		public IEnumerator<T> GetEnumerator() => Storage.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// The game this set adds/removes objects to/from.
		/// </summary>
		public RenderTargetFriendlyGame Game
		{get;}
		
		/// <summary>
		/// If true, then as far as this set knows, all of its components are in <see cref="Game"/>.
		/// If true, then as far as this set knows, all of its components are not in <see cref="Game"/>.
		/// </summary>
		public bool InGame
		{get; protected set;}

		/// <summary>
		/// The way we store objects.
		/// </summary>
		protected AVLSet<T> Storage
		{get;}

		/// <summary>
		/// Iff true, then this set is empty.
		/// </summary>
		public bool Empty => Count == 0;

		/// <summary>
		/// Iff true, then this set is not empty.
		/// </summary>
		public bool NotEmpty => Count > 0;

		public int Count => Storage.Count;
		public bool IsReadOnly => Storage.IsReadOnly;
	}
}
