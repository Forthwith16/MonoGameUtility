using GameEngine.Exceptions;
using GameEngine.Utility.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Graphs
{
	/// <summary>
	/// Represents a graph with vertices and edges connecting vertices.
	/// </summary>
	/// <typeparam name="V">The data type stored in vertices.</typeparam>
	/// <typeparam name="E">The data type stored in edges.</typeparam>
	[JsonConverter(typeof(JsonIGraphConverter))]
	public interface IGraph<V,E>
	{
		/// <summary>
		/// Adds a vertex to this graph.
		/// </summary>
		/// <param name="data">The data to store in the vertex.</param>
		/// <returns>Returns the added vertex.</returns>
		public IVertex<V,E> AddVertex(V data);

		/// <summary>
		/// Removes a vertex from this graph.
		/// </summary>
		/// <param name="v">The vertex to remove.</param>
		/// <returns>Returns true if <paramref name="v"/> was removed and false otherwise.</returns>
		/// <exception cref="NoSuchVertexException">Thrown if <paramref name="v"/> does not exist in this graph.</exception>
		public bool RemoveVertex(IVertex<V,E> v);

		/// <summary>
		/// Removes a vertex from this graph.
		/// </summary>
		/// <param name="v">The vertex to remove.</param>
		/// <returns>Returns true if <paramref name="v"/> was removed and false otherwise.</returns>
		public bool TryRemoveVertex(IVertex<V,E> v);

		/// <summary>
		/// Sets the vertex data of vertex <paramref name="v"/> to <paramref name="data"/>.
		/// </summary>
		/// <param name="v">The vertex whose data should be set.</param>
		/// <param name="data">The data to assign to <paramref name="v"/>.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		/// <exception cref="NoSuchVertexException">Thrown if <paramref name="v"/> does not exist in this graph.</exception>
		/// <exception cref="NotImplementedException">Thrown if this graph does not support data alteration.</exception>
		public bool SetVertexData(IVertex<V,E> v, V data);

		/// <summary>
		/// Sets the vertex data of vertex <paramref name="v"/> to <paramref name="data"/>.
		/// </summary>
		/// <param name="v">The vertex whose data should be set.</param>
		/// <param name="data">The data to assign to <paramref name="v"/>.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		public bool TrySetVertexData(IVertex<V,E> v, V data);

		/// <summary>
		/// Determines if this graph contains <paramref name="v"/>.
		/// </summary>
		/// <param name="v">The vertex to check for.</param>
		/// <returns>Returns true if <paramref name="v"/> is present in this graph and false otherwise.</returns>
		public bool ContainsVertex(IVertex<V,E> v);

		/// <summary>
		/// Adds an edge to this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to store in the edge.</param>
		/// <returns>Returns the added edge.</returns>
		/// <exception cref="DuplicateEdgeException">Thrown if the edge <paramref name="src"/> -> <paramref name="dst"/> already exists.</exception>
		public IEdge<V,E> AddEdge(IVertex<V,E> src, IVertex<V,E> dst, E data);

		/// <summary>
		/// Adds an edge to this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to store in the edge.</param>
		/// <param name="e">The added edge.</param>
		/// <returns>Returns true if the edge was added and false otherwise.</returns>
		public bool TryAddEdge(IVertex<V,E> src, IVertex<V,E> dst, E data, [MaybeNullWhen(false)] out IEdge<V,E> e);

		/// <summary>
		/// Removes an edge from this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge was removed and false otherwise.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist in this graph.</exception>
		public bool RemoveEdge(IVertex<V,E> src, IVertex<V,E> dst);
		
		/// <summary>
		/// Removes an edge from this graph.
		/// </summary>
		/// <param name="e">The edge to remove. If this graph is undirected, the direction of representation of <paramref name="e"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge was removed and false otherwise.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist in this graph.</exception>
		public bool RemoveEdge(IEdge<V,E> e);
		
		/// <summary>
		/// Removes an edge from this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge was removed and false otherwise.</returns>
		public bool TryRemoveEdge(IVertex<V,E> src, IVertex<V,E> dst);

		/// <summary>
		/// Removes an edge from this graph.
		/// </summary>
		/// <param name="e">The edge to remove. If this graph is undirected, the direction of representation of <paramref name="e"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge was removed and false otherwise.</returns>
		public bool TryRemoveEdge(IEdge<V,E> e);

		/// <summary>
		/// Obtains the edge <paramref name="src"/> -> <paramref name="dst"/>.
		/// </summary>
		/// <param name="src">The source vertex.</param>
		/// <param name="dst">The destination vertex.</param>
		/// <returns>Returns the edge.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist.</exception>
		public IEdge<V,E> GetEdge(IVertex<V,E> src, IVertex<V,E> dst);

		/// <summary>
		/// Obtains the edge <paramref name="src"/> -> <paramref name="dst"/>.
		/// </summary>
		/// <param name="src">The edge to fetch. This should be a dummy edge, otherwise this is a no op.</param>
		/// <returns>Returns the edge.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist.</exception>
		public IEdge<V,E> GetEdge(IEdge<V,E> e);

		/// <summary>
		/// Obtains the edge <paramref name="src"/> -> <paramref name="dst"/>.
		/// </summary>
		/// <param name="src">The source vertex.</param>
		/// <param name="dst">The destination vertex.</param>
		/// <param name="result">The obtained edge.</param>
		/// <returns>Returns the edge.</returns>
		public bool TryGetEdge(IVertex<V,E> src, IVertex<V,E> dst, [MaybeNullWhen(false)] out IEdge<V,E> result);

		/// <summary>
		/// Obtains the edge <paramref name="src"/> -> <paramref name="dst"/>.
		/// </summary>
		/// <param name="src">The edge to fetch. This should be a dummy edge, otherwise this is a no op.</param>
		/// <param name="result">The obtained edge.</param>
		/// <returns>Returns the edge.</returns>
		public bool TryGetEdge(IEdge<V,E> e, [MaybeNullWhen(false)] out IEdge<V,E> result);

		/// <summary>
		/// Sets the edge data of an edge to <paramref name="data"/>.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to assign to the edge.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist in this graph.</exception>
		/// <exception cref="NotImplementedException">Thrown if this graph does not support data alteration.</exception>
		public bool SetEdgeData(IVertex<V,E> src, IVertex<V,E> dst, E data);

		/// <summary>
		/// Sets the edge data of an edge to <paramref name="data"/>.
		/// </summary>
		/// <param name="e">The edge to remove. If this graph is undirected, the direction of representation of <paramref name="e"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to assign to the edge.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		/// <exception cref="NoSuchEdgeException">Thrown if the edge does not exist in this graph.</exception>
		/// <exception cref="NotImplementedException">Thrown if this graph does not support data alteration.</exception>
		public bool SetEdgeData(IEdge<V,E> e, E data);

		/// <summary>
		/// Sets the edge data of an edge to <paramref name="data"/>.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to assign to the edge.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		public bool TrySetEdgeData(IVertex<V,E> src, IVertex<V,E> dst, E data);

		/// <summary>
		/// Sets the edge data of an edge to <paramref name="data"/>.
		/// </summary>
		/// <param name="e">The edge to remove. If this graph is undirected, the direction of representation of <paramref name="e"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to assign to the edge.</param>
		/// <returns>Returns true if the data was set and false otherwise.</returns>
		public bool TrySetEdgeData(IEdge<V,E> e, E data);

		/// <summary>
		/// Determines if this graph contains an edge.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge is present in this graph and false otherwise.</returns>
		public bool ContainsEdge(IVertex<V,E> src, IVertex<V,E> dst);
		
		/// <summary>
		/// Determines if this graph contains an edge.
		/// </summary>
		/// <param name="e">The edge to remove. If this graph is undirected, the direction of representation of <paramref name="e"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <returns>Returns true if the edge is present in this graph and false otherwise.</returns>
		public bool ContainsEdge(IEdge<V,E> e);

		/// <summary>
		/// Clears this graph.
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown if this graph does not support removal.</exception>
		public void Clear();

		/// <summary>
		/// The vertices of this graph.
		/// </summary>
		public IEnumerable<IVertex<V,E>> Vertices
		{get;}
		
		/// <summary>
		/// The number of vertices in this graph.
		/// </summary>
		public int VertexCount
		{get;}

		/// <summary>
		/// The edges of this graph.
		/// <para/>
		/// If this is an undirected graph, each edge is listed only once.
		/// In this case, each edge's direction of represention in undefined unless explicitly specified by an implementing class.
		/// </summary>
		public IEnumerable<IEdge<V,E>> Edges
		{get;}

		/// <summary>
		/// The number of edges in this graph.
		/// </summary>
		public int EdgeCount
		{get;}

		/// <summary>
		/// If true, then this graph is directed.
		/// Otherwise, it is undirected.
		/// </summary>
		public bool Directed
		{get;}

		/// <summary>
		/// If true, then this graph is undirected.
		/// Otherwise, it is directed.
		/// </summary>
		public bool Undirected => !Directed;
	}

	/// <summary>
	/// A basic vertex definition.
	/// </summary>
	/// <typeparam name="V">The data type stored in this vertex.</typeparam>
	/// <typeparam name="E">The data type stored in edges connected to this vertex.</typeparam>
	public interface IVertex<V,E>
	{
		/// <summary>
		/// The inbound edges to this vertex.
		/// If this is an undirected graph, then this will contain the same set of edges as OutboundEdges.
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown if this vertex belongs to a directed graph and following edges in reverse is not permitted.</exception>
		/// <remarks>If the graph structure is altered, all priorly generated versions of this have undefined behavior.</remarks>
		public IEnumerable<IEdge<V,E>> InboundEdges
		{get;}

		/// <summary>
		/// The neighbors of this vertex from inbound edges.
		/// If this is an undirected graph, then this will contain the same set of vertices as OutboundNeighbors.
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown if this vertex belongs to a directed graph and following edges in reverse is not permitted.</exception>
		/// <remarks>If the graph structure is altered, all priorly generated versions of this have undefined behavior.</remarks>
		public IEnumerable<IVertex<V,E>> InboundNeighbors
		{get;}

		/// <summary>
		/// The in degree of this vertex.
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown if this vertex belongs to a directed graph and following edges in reverse is not permitted.</exception>
		public int InDegree
		{get;}
		
		/// <summary>
		/// The outbound edges from this vertex.
		/// If this is an undirected graph, then this will contain the same set of edges as InboundEdges.
		/// </summary>
		/// <remarks>If the graph structure is altered, all priorly generated versions of this have undefined behavior.</remarks>
		public IEnumerable<IEdge<V,E>> OutboundEdges
		{get;}

		/// <summary>
		/// The neighbors of this vertex following outbound edges.
		/// If this is an undirected graph, then this will contain the same set of vertices as InboundNeighbors.
		/// </summary>
		/// <remarks>If the graph structure is altered, all priorly generated versions of this have undefined behavior.</remarks>
		public IEnumerable<IVertex<V,E>> OutboundNeighbors
		{get;}

		/// <summary>
		/// The out degree of this vertex.
		/// </summary>
		public int OutDegree
		{get;}

		/// <summary>
		/// This vertex's data.
		/// </summary>
		public V Data
		{get;}
	}

	/// <summary>
	/// A basic edge definition representing the edge Source -> Destination.
	/// If this is an undirected edge, then it also represents the edge Destination -> Source.
	/// </summary>
	/// <typeparam name="V">The data type stored in vertex endpoints of this edge.</typeparam>
	/// <typeparam name="E">The data type stored in this edge.</typeparam>
	public interface IEdge<V,E>
	{
		/// <summary>
		/// The vertex this edge leaves.
		/// </summary>
		public IVertex<V,E> Source
		{get;}

		/// <summary>
		/// The vertex this edge enters.
		/// </summary>
		public IVertex<V,E> Destination
		{get;}

		/// <summary>
		/// If true, then this edge is directed.
		/// Otherwise, it is undirected.
		/// </summary>
		public bool Directed
		{get;}

		/// <summary>
		/// If true, then this edge is undirected.
		/// Otherwise, it is directed.
		/// </summary>
		public bool Undirected => !Directed;

		/// <summary>
		/// This edge's data.
		/// </summary>
		public E Data
		{get;}
	}

	/// <summary>
	/// A dummy edge class useful for searching for edges by providing an edge containing only the source and destinations.
	/// It does not require data or directedness and will error if trying to access either property.
	/// </summary>
	/// <typeparam name="V">The data type stored in vertex endpoints of this edge.</typeparam>
	/// <typeparam name="E">The data type stored in this edge.</typeparam>
	public sealed class DummyEdge<V,E> : IEdge<V,E>
	{
		/// <summary>
		/// Creates a dummy edge Source -> Destination.
		/// </summary>
		/// <param name="source">The source vertex.</param>
		/// <param name="destination">The destination vertex.</param>
		public DummyEdge(IVertex<V,E> source, IVertex<V,E> destination)
		{
			Source = source;
			Destination = destination;

			return;
		}

		public static bool operator ==(DummyEdge<V,E> e1, IEdge<V,E> e2) => e1.Source == e2.Source && e1.Destination == e2.Destination;
		public static bool operator !=(DummyEdge<V,E> e1, IEdge<V,E> e2) => e1.Source != e2.Source || e1.Destination != e2.Destination;

		public override bool Equals(object? obj)
		{
			if(obj is not IEdge<V,E> e)
				return false;

			return this == e;
		}

		public override int GetHashCode() => HashCode.Combine(Source.GetHashCode(),Destination.GetHashCode());

		public IVertex<V,E> Source
		{get;}

		public IVertex<V,E> Destination
		{get;}

		public bool Directed => throw new NotImplementedException();
		public E Data => throw new NotImplementedException();
	}

	/// <summary>
	/// Creates JSON converters for graphs.
	/// </summary>
	file class JsonIGraphConverter : JsonBaseConverterFactory
	{
		public JsonIGraphConverter() : base((t,ops) => [],typeof(IGraph<,>),typeof(IGC<,>))
		{return;}

		private class IGC<V,E> : JsonBaseTypeConverter<IGraph<V,E>>
		{}
	}
}
