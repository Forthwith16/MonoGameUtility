
using GameEngine.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace GameEngine.DataStructures.Graphs
{
	/// <summary>
	/// Represents a graph backed by an adjacency list.
	/// </summary>
	/// <remarks>
	/// The graph, regardless of directedness, will store edges as outbound edges with their source vertex as well as inbound edges with their destination vertex.
	/// <para/>
	/// In general, this graph class is intended for low-intensity usage.
	/// When high performance, where every clock cycle matters, is required, implement a graph coupled to the application.
	/// </remarks>
	public class AdjacencyListGraph<V,E> : IGraph<V,E>
	{
		/// <summary>
		/// Creates a new, empty graph.
		/// </summary>
		/// <param name="dir">If true, this will be a directed graph. If false, this will be an undirected graph.</param>
		public AdjacencyListGraph(bool dir)
		{
			VertexSet = new HashSet<IVertex<V,E>>();
			Directed = dir;

			return;
		}

		public IVertex<V,E> AddVertex(V data)
		{
			AdjacencyListVertex<V,E> ret = new AdjacencyListVertex<V,E>(data);
			VertexSet.Add(ret);

			return ret;
		}

		public bool RemoveVertex(IVertex<V,E> v)
		{
			if(!ContainsVertex(v))
				throw new NoSuchVertexException();
			
			return VertexSet.Remove(v);
		}

		public bool TryRemoveVertex(IVertex<V,E> v) => VertexSet.Remove(v);

		public bool SetVertexData(IVertex<V,E> v, V data)
		{
			if(v is not AdjacencyListVertex<V,E> v2 || !ContainsVertex(v2))
				throw new NoSuchVertexException();

			v2.Data = data;
			return true;
		}

		public bool TrySetVertexData(IVertex<V,E> v, V data)
		{
			if(v is not AdjacencyListVertex<V,E> v2 || !ContainsVertex(v2))
				return false;

			v2.Data = data;
			return true;
		}

		public bool ContainsVertex(IVertex<V,E> v) => VertexSet.Contains(v);

		/// <summary>
		/// Adds an edge to this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to store in the edge.</param>
		/// <returns>Returns the (forward) edge <paramref name="src"/> -> <paramref name="dst"/> added and false otherwise.</returns>
		/// <exception cref="DuplicateEdgeException">Thrown if the edge <paramref name="src"/> -> <paramref name="dst"/> already exists.</exception>
		public IEdge<V,E> AddEdge(IVertex<V,E> src, IVertex<V,E> dst, E data)
		{
			if(!TryAddEdge(src,dst,data,out IEdge<V,E>? ret))
				throw new DuplicateEdgeException(); // This only fails when we attempt to add a duplicate, so this is fine

			return ret;
		}

		/// <summary>
		/// Adds an edge to this graph.
		/// </summary>
		/// <param name="src">The edge source vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="dst">The edge destination vertex. If this graph is undirected, the order of <paramref name="src"/> and <paramref name="dst"/> is unimportant unless the implementing class explicitly specifies otherwise.</param>
		/// <param name="data">The data to store in the edge.</param>
		/// <param name="e">Where (forward) added edge <paramref name="src"/> -> <paramref name="dst"/> is placed.</param>
		/// <returns>Returns true if the edge was added and false otherwise.</returns>
		public bool TryAddEdge(IVertex<V,E> src, IVertex<V,E> dst, E data, [MaybeNullWhen(false)] out IEdge<V,E> e)
		{
			if(!ContainsVertex(src) || !ContainsVertex(dst) || ContainsEdge(src,dst))
			{
				e = default;
				return false;
			}

			// We can only get here if these are the right type, so no worries
			AdjacencyListVertex<V,E> src_alv = (AdjacencyListVertex<V,E>)src;
			AdjacencyListVertex<V,E> dst_alv = (AdjacencyListVertex<V,E>)dst;

			AdjacencyListEdge<V,E> ret = new AdjacencyListEdge<V,E>(src_alv,dst_alv,data,Directed);
			e = ret; // This is the most sensible assignment of e

			src_alv.OutEdges.Add(dst_alv,ret);
			dst_alv.InEdges.Add(dst_alv,new AdjacencyListEdge<V,E>(dst_alv,src_alv,data,Directed));

			if(!Directed && src != dst)
			{
				src_alv.InEdges.Add(dst_alv,new AdjacencyListEdge<V,E>(dst_alv,src_alv,data,Directed));
				dst_alv.OutEdges.Add(dst_alv,new AdjacencyListEdge<V,E>(src_alv,dst_alv,data,Directed));
			}

			return true;
		}

		public bool RemoveEdge(IVertex<V,E> src, IVertex<V,E> dst)
		{
			if(!TryRemoveEdge(new DummyEdge<V,E>(src,dst)))
				throw new NoSuchEdgeException(); // In this class, we can only fail if we don't have the edge

			return true;
		}
		
		public bool RemoveEdge(IEdge<V,E> e)
		{
			if(!TryRemoveEdge(e))
				throw new NoSuchEdgeException(); // In this class, we can only fail if we don't have the edge

			return true;
		}
		
		public bool TryRemoveEdge(IVertex<V,E> src, IVertex<V,E> dst) => TryRemoveEdge(new DummyEdge<V,E>(src,dst));

		public bool TryRemoveEdge(IEdge<V,E> e)
		{
			if(!FetchOutboundEdge(e,out AdjacencyListEdge<V,E>? e2) || !FetchInboundEdge(e,out AdjacencyListEdge<V,E>? e3))
				return false;

			if(!Directed && e.Source != e.Destination)
			{
				DummyEdge<V,E> e_rev = new DummyEdge<V,E>(e.Source,e.Destination);

				if(!FetchOutboundEdge(e_rev,out AdjacencyListEdge<V,E>? e4) || !FetchInboundEdge(e_rev,out AdjacencyListEdge<V,E>? e5))
					return false;

				e4.ALVSource.OutEdges.Remove(e4.ALVDestination);
				e5.ALVDestination.InEdges.Remove(e5.ALVSource);
			}

			e2.ALVSource.OutEdges.Remove(e2.ALVDestination);
			e3.ALVDestination.InEdges.Remove(e3.ALVSource);

			return true;
		}

		public bool SetEdgeData(IVertex<V,E> src, IVertex<V,E> dst, E data)
		{
			if(!TrySetEdgeData(src,dst,data))
				throw new NoSuchEdgeException(); // In this class, we can only fail if we don't have the edge

			return true;
		}

		public bool SetEdgeData(IEdge<V,E> e, E data)
		{
			if(!TrySetEdgeData(e,data))
				throw new NoSuchEdgeException(); // In this class, we can only fail if we don't have the edge

			return true;
		}

		public bool TrySetEdgeData(IVertex<V,E> src, IVertex<V,E> dst, E data) => TrySetEdgeData(new DummyEdge<V,E>(src,dst),data);

		public bool TrySetEdgeData(IEdge<V,E> e, E data)
		{
			if(!FetchInboundEdge(e,out AdjacencyListEdge<V,E>? real_e_in) || !FetchOutboundEdge(e,out AdjacencyListEdge<V,E>? real_e_out))
				return false;

			// If we're not directed, we have to change four edges
			if(!Directed && e.Source != e.Destination)
			{
				IEdge<V,E> e_rev = new DummyEdge<V,E>(e.Destination,e.Source);

				if(!FetchInboundEdge(e_rev,out AdjacencyListEdge<V,E>? real_e_in_rev) || !FetchOutboundEdge(e_rev,out AdjacencyListEdge<V,E>? real_e_out_rev))
					return false;

				real_e_in_rev.Data = data;
				real_e_out_rev.Data = data;
			}

			real_e_in.Data = data;
			real_e_out.Data = data;

			return true;
		}

		#region Edge Fetching
		/// <summary>
		/// Fetches the inbound edge src -> dst.
		/// </summary>
		/// <param name="src">The source vertex.</param>
		/// <param name="dst">The destination vertex.</param>
		/// <param name="e">The place to store the found edge.</param>
		/// <returns>Returns true if the edge is found and false otherwise.</returns>
		protected bool FetchInboundEdge(IVertex<V,E> src, IVertex<V,E> dst, [MaybeNullWhen(false)] out AdjacencyListEdge<V,E> e)
		{
			if(ContainsVertex(src) && ContainsVertex(dst))
			{
				foreach(IEdge<V,E> e2 in dst.InboundEdges)
					if(e2.Source == src)
					{
						e = e2 as AdjacencyListEdge<V,E>;
						return e is not null;
					}
			}
			
			e = default;
			return false;
		}

		/// <summary>
		/// Fetches the inbound edge matching <paramref name="e"/>.
		/// </summary>
		/// <param name="e">The edge to look for.</param>
		/// <param name="e_out">The place to store the found edge.</param>
		/// <returns>Returns true if the edge is found and false otherwise.</returns>
		protected bool FetchInboundEdge(IEdge<V,E> e, [MaybeNullWhen(false)] out AdjacencyListEdge<V,E> e_out)
		{
			if(ContainsVertex(e.Source) && ContainsVertex(e.Destination))
			{
				foreach(IEdge<V,E> e2 in e.Destination.InboundEdges)
					if(e2.Source == e.Source)
					{
						e_out = e2 as AdjacencyListEdge<V,E>;
						return e_out is not null;
					}
			}
			
			e_out = default;
			return false;
		}

		/// <summary>
		/// Fetches the outbound edge src -> dst.
		/// </summary>
		/// <param name="src">The source vertex.</param>
		/// <param name="dst">The destination vertex.</param>
		/// <param name="e">The place to store the found edge.</param>
		/// <returns>Returns true if the edge is found and false otherwise.</returns>
		protected bool FetchOutboundEdge(IVertex<V,E> src, IVertex<V,E> dst, [MaybeNullWhen(false)] out AdjacencyListEdge<V,E> e)
		{
			if(ContainsVertex(src) && ContainsVertex(dst))
			{
				foreach(IEdge<V,E> e2 in src.OutboundEdges)
					if(e2.Destination == dst)
					{
						e = e2 as AdjacencyListEdge<V,E>;
						return e is not null;
					}
			}
			
			e = default;
			return false;
		}

		/// <summary>
		/// Fetches the outbound edge matching <paramref name="e"/>.
		/// </summary>
		/// <param name="e">The edge to look for.</param>
		/// <param name="e_out">The place to store the found edge.</param>
		/// <returns>Returns true if the edge is found and false otherwise.</returns>
		protected bool FetchOutboundEdge(IEdge<V,E> e, [MaybeNullWhen(false)] out AdjacencyListEdge<V,E> e_out)
		{
			if(ContainsVertex(e.Source) && ContainsVertex(e.Destination))
			{
				foreach(IEdge<V,E> e2 in e.Source.OutboundEdges)
					if(e2.Destination == e.Destination)
					{
						e_out = e2 as AdjacencyListEdge<V,E>;
						return e_out is not null;
					}
			}
			
			e_out = default;
			return false;
		}
		#endregion

		public bool ContainsEdge(IVertex<V,E> src, IVertex<V,E> dst) => ContainsEdge(new DummyEdge<V,E>(src,dst));
		
		public bool ContainsEdge(IEdge<V,E> e) => ContainsVertex(e.Source) && ContainsVertex(e.Destination) && e.Source.OutboundEdges.Contains(e); // Directedness doesn't change our ability to look up edges this way; double vertex check is a possible speedup depending on usage

		public void Clear()
		{
			VertexSet.Clear();
			return;
		}

		/// <summary>
		/// The concrete collection of vertices of this graph.
		/// </summary>
		protected HashSet<IVertex<V,E>> VertexSet
		{get;}

		public IEnumerable<IVertex<V,E>> Vertices => VertexSet;
		public int VertexCount => VertexSet.Count;

		public IEnumerable<IEdge<V,E>> Edges
		{
			get
			{
				IEnumerable<IEdge<V,E>> ret = Enumerable.Empty<IEdge<V,E>>();

				if(Directed)
					foreach(IVertex<V,E> v in VertexSet)
						ret = ret.Concat(v.OutboundEdges);
				else
				{
					HashSet<IVertex<V,E>> done = new HashSet<IVertex<V,E>>();

					foreach(IVertex<V,E> v in VertexSet)
					{
						ret = ret.Concat(v.OutboundEdges.Where(e => !done.Contains(e.Destination)));
						done.Add(v);
					}
				}

				return ret;
			}
		}

		public int EdgeCount
		{
			get
			{
				int ret = 0;

				foreach(IVertex<V,E> v in VertexSet)
					ret += v.OutDegree;

				return Directed ? ret : ret >> 1; // If this is an undirected graph, we have double counted
			}
		}

		public bool Directed
		{get;}
	}

	/// <summary>
	/// The vertex class for AdjacencyListGraph.
	/// </summary>
	public class AdjacencyListVertex<V,E> : IVertex<V,E>
	{
		/// <summary>
		/// Creates a new vertex.
		/// </summary>
		/// <param name="data">The data to store with the new vertex.</param>
		public AdjacencyListVertex(V data)
		{
			InEdges = new Dictionary<AdjacencyListVertex<V,E>,AdjacencyListEdge<V,E>>();
			OutEdges = new Dictionary<AdjacencyListVertex<V,E>,AdjacencyListEdge<V,E>>();

			Data = data;
			return;
		}

		/// <summary>
		/// The concrete list of inbound edges.
		/// </summary>
		protected internal Dictionary<AdjacencyListVertex<V,E>,AdjacencyListEdge<V,E>> InEdges
		{get;}

		public IEnumerable<IEdge<V,E>> InboundEdges => InEdges.Values;
		public IEnumerable<IVertex<V,E>> InboundNeighbors => InboundEdges.Select(e => e.Source);
		public int InDegree => InEdges.Count;

		/// <summary>
		/// The concrete list of outbound edges.
		/// </summary>
		protected internal Dictionary<AdjacencyListVertex<V,E>,AdjacencyListEdge<V,E>> OutEdges
		{get;}

		public IEnumerable<IEdge<V,E>> OutboundEdges => OutEdges.Values;
		public IEnumerable<IVertex<V,E>> OutboundNeighbors => OutboundEdges.Select(e => e.Destination);
		public int OutDegree => OutEdges.Count;

		public V Data
		{get; set;}
	}

	/// <summary>
	/// The edge class for AdjacencyListGraph.
	/// </summary>
	public class AdjacencyListEdge<V,E> : IEdge<V,E>
	{
		/// <summary>
		/// Creates a new edge.
		/// </summary>
		/// <param name="src">The source vertex.</param>
		/// <param name="dst">The destination vertex.</param>
		/// <param name="data">The data to store with the edge.</param>
		/// <param name="dir">True will make this a directed edge. False will make this an undirected edge.</param>
		public AdjacencyListEdge(AdjacencyListVertex<V,E> src, AdjacencyListVertex<V,E> dst, E data, bool dir)
		{
			ALVSource = src;
			ALVDestination = dst;
			Directed = dir;

			Data = data;
			return;
		}

		public static bool operator ==(AdjacencyListEdge<V,E> e1, IEdge<V,E> e2) => e1.Source == e2.Source && e1.Destination == e2.Destination;
		public static bool operator !=(AdjacencyListEdge<V,E> e1, IEdge<V,E> e2) => e1.Source != e2.Source || e1.Destination != e2.Destination;

		public override bool Equals(object? obj)
		{
			if(obj is not IEdge<V,E> e)
				return false;

			return this == e;
		}

		public override int GetHashCode() => HashCode.Combine(Source.GetHashCode(),Destination.GetHashCode());

		public IVertex<V,E> Source => ALVSource;

		public AdjacencyListVertex<V,E> ALVSource
		{get;}

		public IVertex<V,E> Destination => ALVDestination;

		public AdjacencyListVertex<V,E> ALVDestination
		{get;}

		public bool Directed
		{get; protected set;} // A protected set is provided in case a deriving class wants to make this mutable

		public E Data
		{get; set;}
	}
}
