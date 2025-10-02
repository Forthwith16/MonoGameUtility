using GameEngine.Framework;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

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
			Storage = new AVLSet<GameObject>(Comparer<GameObject>.Create((a,b) => a.ID.CompareTo(b.ID)));

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
		/// Removes this set's components from <see cref="Game"/>.
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

		public bool Remove(T item) => SecretRemove(item);

		/// <summary>
		/// A private version of <see cref="Remove(T)"/> that does not care about type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="obj">The game object to remove.</param>
		/// <returns>Returns true if the remove was successful and false otherwise.</returns>
		private bool SecretRemove(GameObject obj)
		{
			if(InGame)
				Game.Components.Remove(obj);

			return Storage.Remove(obj);
		}

		/// <summary>
		/// Obtains an object by its ID.
		/// </summary>
		/// <param name="id">The ID of the object to fetch.</param>
		/// <param name="output">The output variable. This will be the game object with ID <paramref name="id"/> when this returns true and default(<typeparamref name="T"/>) otherwise.</param>
		/// <returns>Returns true if this contained the game object, which was assigned to <paramref name="output"/>, and false otherwise.</returns>
		/// <remarks>This method requires that we can search for <typeparamref name="T"/> types by their ID by creating a DummyGameObject with ID <paramref name="id"/>.</remarks>
		public bool TryGet(GameObjectID id, [MaybeNullWhen(false)] out T output)
		{
			if(Storage.Get(id,out GameObject? obj))
			{
				output = obj as T;
				return output is not null;
			}

			output = null;
			return false;
		}

		public bool Contains(T item) => Storage.Contains(item);

		public void Clear()
		{
			while(NotEmpty)
				SecretRemove(Storage.First());
			
			return;
		}

		public void CopyTo(T[] array, int offset)
		{
			if(offset < 0)
				throw new ArgumentOutOfRangeException();

			if(array.Length < offset + Count)
				throw new ArgumentException();

			int i = 0;

			foreach(T t in this)
				array[offset + i++] = t;

			return;
		}

		public IEnumerator<T> GetEnumerator() => Storage.Select(obj => (T)obj).GetEnumerator();
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
		protected AVLSet<GameObject> Storage
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
