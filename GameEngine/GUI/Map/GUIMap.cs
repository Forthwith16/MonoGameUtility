using GameEngine.Utility.ExtensionMethods.EnumExtensions;

namespace GameEngine.GUI.Map
{
	/// <summary>
	/// Allows for navigation around a GUI system via digital inputs.
	/// </summary>
	public class GUIMap
	{
		/// <summary>
		/// Creates an empty GUI map.
		/// </summary>
		public GUIMap()
		{
			Vertices = new Dictionary<IGUI,GUIVertex>();
			return;
		}

		/// <summary>
		/// Gets the next GUI component to make active after an input in the direction <paramref name="dir"/> from the active component in Owner.
		/// </summary>
		/// <param name="Cursor">The position to move away from.</param>
		/// <param name="dir">The direction to move away from the active component in Owner.</param>
		/// <returns>Returns the new component to make the active component.</returns>
		public IGUI? GetNext(IGUI? Cursor, GUIMapDirection dir)
		{
			switch(dir)
			{
			case GUIMapDirection.UP:
				if(Cursor is null || !Vertices.TryGetValue(Cursor,out GUIVertex? vu))
					return UnknownCursorUp;

				return vu.Up;
			case GUIMapDirection.DOWN:
				if(Cursor is null || !Vertices.TryGetValue(Cursor,out GUIVertex? vd))
					return UnknownCursorDown;

				return vd.Down;
			case GUIMapDirection.LEFT:
				if(Cursor is null || !Vertices.TryGetValue(Cursor,out GUIVertex? vl))
					return UnknownCursorLeft;

				return vl.Left;
			case GUIMapDirection.RIGHT:
				if(Cursor is null || !Vertices.TryGetValue(Cursor,out GUIVertex? vr))
					return UnknownCursorRight;

				return vr.Right;
			}

			return null;
		}

		/// <summary>
		/// Adds a new vertex to this GUIMap.
		/// </summary>
		/// <param name="component">The GUI component the vertex will represent. This value must be distinct from all other vertices.</param>
		/// <returns>Returns true if the vertex could be created and false otherwise.</returns>
		public bool AddVertex(IGUI component)
		{
			if(Vertices.ContainsKey(component))
				return false;

			Vertices.Add(component,new GUIVertex(component,this));
			return true;
		}

		/// <summary>
		/// Removes a vertex from this GUIMap.
		/// </summary>
		/// <param name="component">The GUI component the vertex represents. All references to this vertex will be removed.</param>
		/// <returns>Returns true if the vertex exists and was removed. Returns false otherwise.</returns>
		public bool RemoveVertex(IGUI component)
		{
			if(!Vertices.ContainsKey(component))
				return false;

			Vertices.Remove(component);

			foreach(GUIVertex v in Vertices.Values)
				v.Expunge(component);

			if(UnknownCursorUp == component)
				UnknownCursorUp = null;

			if(UnknownCursorDown == component)
				UnknownCursorDown = null;

			if(UnknownCursorLeft == component)
				UnknownCursorLeft = null;

			if(UnknownCursorRight == component)
				UnknownCursorRight = null;

			return true;
		}

		/// <summary>
		/// Sets the null/unknown destination for <paramref name="dir"/> to <paramref name="component"/>.
		/// </summary>
		/// <param name="dir">The direction of movement.</param>
		/// <param name="component">The component to move to. If null, it will move to the null/unknown state. If this is not null and not a vertex in this map, it will create one for it.</param>
		public void SetNullTarget(GUIMapDirection dir, IGUI? component)
		{
			if(component is not null && !Vertices.ContainsKey(component))
				AddVertex(component);

			switch(dir)
			{
			case GUIMapDirection.UP:
				UnknownCursorUp = component;
				break;
			case GUIMapDirection.DOWN:
				UnknownCursorDown = component;
				break;
			case GUIMapDirection.LEFT:
				UnknownCursorLeft = component;
				break;
			case GUIMapDirection.RIGHT:
				UnknownCursorRight = component;
				break;
			}

			return;
		}

		/// <summary>
		/// Sets the directed edge out of <paramref name="from"/> in the direction of <paramref name="dir"/> to vertex <paramref name="to"/>.
		/// </summary>
		/// <param name="dir">The direction of the edge.</param>
		/// <param name="from">The vertex to leave. If this is null, it will set the null/unknown movement via SetNullTarget. If this is not null and not a vertex in this map, it will create one for it.</param>
		/// <param name="to">The vertex to go to. If this value is null, it will go to the null/unknown state. If this is not null and not a vertex in this map, it will create one for it.</param>
		/// <param name="symmetric">If true, then the symmetric edge will be added. If false, then no further action is taken.</param>
		public void SetEdge(GUIMapDirection dir, IGUI? from, IGUI? to, bool symmetric = false)
		{
			if(from is null)
			{
				SetNullTarget(dir,to);
				return;
			}

			if(!Vertices.ContainsKey(from))
				AddVertex(from);

			if(to is not null && !Vertices.ContainsKey(to))
				AddVertex(to);

			switch(dir)
			{
			case GUIMapDirection.UP:
				Vertices[from].Up = to;
				break;
			case GUIMapDirection.DOWN:
				Vertices[from].Down = to;
				break;
			case GUIMapDirection.LEFT:
				Vertices[from].Left = to;
				break;
			case GUIMapDirection.RIGHT:
				Vertices[from].Right = to;
				break;
			}

			// If we asked for the symmetric case, do it, and don't do another symmetric case after
			if(symmetric)
				SetEdge(dir.Reflect(),to,from,false);

			return;
		}

		/// <summary>
		/// Removes an edge from this GUIMap by setting its destination to null.
		/// </summary>
		/// <param name="dir">The direction of the edge.</param>
		/// <param name="v">The source vertex. If this is null, it will void the null/unknown movement via SetNullTarget. If this vertex does not exist, nothing will happen.</param>
		public void RemoveEdge(GUIMapDirection dir, IGUI? v)
		{
			if(v is null)
				SetNullTarget(dir,null);
			else
				SetEdge(dir,v,null);

			return;
		}

		/// <summary>
		/// Clears this map of all data.
		/// </summary>
		public void Clear()
		{
			Vertices.Clear();
			UnknownCursorUp = UnknownCursorDown = UnknownCursorLeft = UnknownCursorRight = null;

			return;
		}

		/// <summary>
		/// Removes every edge from this map.
		/// </summary>
		public void ClearEdges()
		{
			foreach(GUIVertex v in Vertices.Values)
				v.ClearEdges();

			UnknownCursorUp = UnknownCursorDown = UnknownCursorLeft = UnknownCursorRight = null;
			return;
		}

		/// <summary>
		/// The map information.
		/// </summary>
		protected Dictionary<IGUI,GUIVertex> Vertices
		{get; init;}

		/// <summary>
		/// When we don't know where our cursor should be, this is where we move to when we press up.
		/// </summary>
		protected IGUI? UnknownCursorUp
		{get; set;}

		/// <summary>
		/// When we don't know where our cursor should be, this is where we move to when we press down.
		/// </summary>
		protected IGUI? UnknownCursorDown
		{get; set;}

		/// <summary>
		/// When we don't know where our cursor should be, this is where we move to when we press left.
		/// </summary>
		protected IGUI? UnknownCursorLeft
		{get; set;}

		/// <summary>
		/// When we don't know where our cursor should be, this is where we move to when we press right.
		/// </summary>
		protected IGUI? UnknownCursorRight
		{get; set;}
	}

	/// <summary>
	/// Directions of motion in a GUIMap.
	/// </summary>
	public enum GUIMapDirection
	{
		UP,
		DOWN,
		LEFT,
		RIGHT,
		NONE
	}
}
