using GameEngine.Framework;
using Microsoft.Xna.Framework;
using System.Collections;

namespace GameEngine.DataStructures.Collections
{
	/// <summary>
	/// A collection of game objects which are ordered in several different ways.
	/// </summary>
	/// <typeparam name="T">The type of game object to be stored.</typeparam>
	public class OrderedGameObjectCollection<T> : IGameComponent, IEnumerable<T> where T : IGameComponent
	{
		/// <summary>
		/// Creates a new, empty collection of game objects.
		/// </summary>
		public OrderedGameObjectCollection()
		{
			// Initialize the component system
			Components = new Dictionary<T,LastKnownValues>();
			ComponentsUpdateOrder = new SortedDictionary<int,HashSet<IUpdateable>>();
			ComponentsDrawOrder = new SortedDictionary<int,HashSet<IDrawable>>();
			DebugDrawOrder = new SortedDictionary<int,HashSet<IDebugDrawable>>();
			RenderDrawOrder = new SortedDictionary<int,HashSet<IRenderTargetDrawable>>();

			return;
		}

		/// <summary>
		/// Initializes all components.
		/// </summary>
		public void Initialize()
		{
			Initialized = true;

			foreach(T component in Components.Keys)
				component.Initialize();
			
			return;
		}

		/// <summary>
		/// This will call Update on every enabled component in the correct order.
		/// </summary>
		/// <param name="delta">The elapsed time since the last frame.</param>
		public void Update(GameTime delta)
		{
			foreach(HashSet<IUpdateable> us in ComponentsUpdateOrder.Values)
				foreach(IUpdateable component in us)
					if(component.Enabled)
						component.Update(delta);
			
			return;
		}
		
		/// <summary>
		/// This will call DrawRenderTarget on every visible component in the correct order.
		/// </summary>
		/// <param name="delta">The elapsed time since the last frame.</param>
		public void DrawRenderTarget(GameTime delta)
		{
			foreach(HashSet<IRenderTargetDrawable> us in RenderDrawOrder.Values)
				foreach(IRenderTargetDrawable component in us)
					if(component.Visible)
						component.DrawRenderTarget(delta);

			return;
		}

		/// <summary>
		/// This will call DrawDebugInfo on every visible component in the correct order.
		/// </summary>
		/// <param name="delta">The elapsed time since the last frame.</param>
		public void DrawDebugInfo(GameTime delta)
		{
			foreach(HashSet<IDebugDrawable> us in DebugDrawOrder.Values)
				foreach(IDebugDrawable component in us)
					if(component.Visible)
						component.DrawDebugInfo(delta);

			return;
		}

		/// <summary>
		/// This will call Draw on every visible component in the correct order.
		/// </summary>
		/// <param name="delta">The elapsed time since the last frame.</param>
		public void Draw(GameTime delta)
		{
			foreach(HashSet<IDrawable> us in ComponentsDrawOrder.Values)
				foreach(IDrawable component in us)
					if(component.Visible)
						component.Draw(delta);
			
			return;
		}

		/// <summary>
		/// Adds a component to this.
		/// </summary>
		/// <param name="component">The component to add.</param>
		/// <returns>Returns true if <paramref name="component"/> was added successfull and false if it was not (such as when it is already present).</returns>
		/// <exception cref="InvalidOperationException">Thrown if the state of this becomes unstable as a result of a disasterous failed add.</exception>
		public bool AddComponent(T component)
		{
			// Add the component to the base set
			IUpdateable? u = component as IUpdateable;
			IDrawable? d = component as IDrawable;

			if(!Components.TryAdd(component,new LastKnownValues(u is null ? 0 : u.UpdateOrder,d is null ? 0 : d.DrawOrder)))
				return false;

			// Add the component to the update set
			if(u is not null)
			{
				HashSet<IUpdateable>? us;
			
				if(!ComponentsUpdateOrder.TryGetValue(u.UpdateOrder,out us) && !ComponentsUpdateOrder.TryAdd(u.UpdateOrder,us = new HashSet<IUpdateable>()))
					throw new InvalidOperationException();
			
				if(!us.Add(u))
					throw new InvalidOperationException();

				u.UpdateOrderChanged += RefreshUpdateOrder;
			}

			// Add the component to the draw set
			if(d is not null)
			{
				HashSet<IDrawable>? ds;

				if(!ComponentsDrawOrder.TryGetValue(d.DrawOrder,out ds) && !ComponentsDrawOrder.TryAdd(d.DrawOrder,ds = new HashSet<IDrawable>()))
					throw new InvalidOperationException();
			
				if(!ds.Add(d))
					throw new InvalidOperationException();

				d.DrawOrderChanged += RefreshDrawOrder;
			}

			// Add the component to the debug draw set
			if(component is IDebugDrawable debug)
			{
				HashSet<IDebugDrawable>? dds;

				if(!DebugDrawOrder.TryGetValue(debug.DrawDebugOrder,out dds) && !DebugDrawOrder.TryAdd(debug.DrawDebugOrder,dds = new HashSet<IDebugDrawable>()))
					throw new InvalidOperationException();
				
				if(!dds.Add(debug))
					throw new InvalidOperationException();

				debug.OnDrawDebugOrderChanged += RefreshDebugOrder;
			}

			// Add the component to the render target draw set
			if(component is IRenderTargetDrawable render)
			{
				HashSet<IRenderTargetDrawable>? rs;

				if(!RenderDrawOrder.TryGetValue(render.RenderTargetDrawOrder,out rs) && !RenderDrawOrder.TryAdd(render.RenderTargetDrawOrder,rs = new HashSet<IRenderTargetDrawable>()))
					throw new InvalidOperationException();
				
				if(!rs.Add(render))
					throw new InvalidOperationException();

				render.RenderTargetDrawOrderChanged += RefreshRenderOrder;
			}

			// If we are initialize, we need to initialize the component
			if(Initialized)
				component.Initialize();

			return true;
		}

		/// <summary>
		/// Removes a component from this.
		/// <para/>
		/// This does not Dispose any components removed, leaving that for the destructor to take care of later if they are lost.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if <paramref name="component"/> was removed and false otherwise.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the state of this becomes unstable as a result of a disasterous failed remove.</exception>
		public bool RemoveComponent(T component)
		{
			// If we fail to remove the initial component, then we're done
			if(!Components.Remove(component))
				return false;

			// Remove the update information
			if(component is IUpdateable u)
			{
				if(!ComponentsUpdateOrder.TryGetValue(u.UpdateOrder,out HashSet<IUpdateable>? us))
					throw new InvalidOperationException();

				if(!us.Remove(u))
					throw new InvalidOperationException();

				if(us.Count == 0 && !ComponentsUpdateOrder.Remove(u.UpdateOrder))
					throw new InvalidOperationException();

				u.UpdateOrderChanged -= RefreshUpdateOrder;
			}

			// Remove the draw information
			if(component is IDrawable d)
			{
				if(!ComponentsDrawOrder.TryGetValue(d.DrawOrder,out HashSet<IDrawable>? ds))
					throw new InvalidOperationException();

				if(!ds.Remove(d))
					throw new InvalidOperationException();

				if(ds.Count == 0 && !ComponentsDrawOrder.Remove(d.DrawOrder))
					throw new InvalidOperationException();

				d.DrawOrderChanged -= RefreshDrawOrder;
			}

			// Remove the debug information
			if(component is IDebugDrawable debug)
			{
				if(!DebugDrawOrder.TryGetValue(debug.DrawDebugOrder,out HashSet<IDebugDrawable>? dds))
					throw new InvalidOperationException();

				if(!dds.Remove(debug))
					throw new InvalidOperationException();

				if(dds.Count == 0 && !DebugDrawOrder.Remove(debug.DrawDebugOrder))
					throw new InvalidOperationException();

				debug.OnDrawDebugOrderChanged -= RefreshDebugOrder;
			}

			// Remove the render target information
			if(component is IRenderTargetDrawable render)
			{
				if(!RenderDrawOrder.TryGetValue(render.RenderTargetDrawOrder,out HashSet<IRenderTargetDrawable>? rs))
					throw new InvalidOperationException();

				if(!rs.Remove(render))
					throw new InvalidOperationException();

				if(rs.Count == 0 && !RenderDrawOrder.Remove(render.RenderTargetDrawOrder))
					throw new InvalidOperationException();

				render.RenderTargetDrawOrderChanged -= RefreshRenderOrder;
			}

			return true;
		}

		/// <summary>
		/// Refreshes the udpate order of a component.
		/// </summary>
		/// <param name="sender">The sending object.</param>
		/// <param name="e">An unreliable variable of effectively no meaning.</param>
		/// <exception cref="InvalidOperationException">Thrown if this enters an unstable state as a result of this refresh.</exception>
		private void RefreshUpdateOrder(object? sender, EventArgs e)
		{
			// We need to calculate the new and old orders first
			if(sender is not IUpdateable d || sender is not T component)
				return;

			int new_order = d.UpdateOrder;
			
			LastKnownValues v = Components[component];
			int old_order = v.UpdateOrder;

			// This should never be the case, but just in case
			if(new_order == old_order)
				return;

			// Perform the removeal logic
			if(!ComponentsUpdateOrder.TryGetValue(old_order,out HashSet<IUpdateable>? ds))
				return;

			if(!ds.Remove(d))
				throw new InvalidOperationException();

			if(ds.Count == 0 && !ComponentsUpdateOrder.Remove(old_order))
				throw new InvalidOperationException();

			// Now add the item back in
			if(!ComponentsUpdateOrder.TryGetValue(new_order,out ds) && !ComponentsUpdateOrder.TryAdd(new_order,ds = new HashSet<IUpdateable>()))
				throw new InvalidOperationException();
			
			if(!ds.Add(d))
				throw new InvalidOperationException();

			Components[component] = new LastKnownValues(new_order,v.DrawOrder);
			return;
		}

		/// <summary>
		/// Refreshes the draw order of a component.
		/// </summary>
		/// <param name="sender">The sending object.</param>
		/// <param name="e">An unreliable variable of effectively no meaning.</param>
		/// <exception cref="InvalidOperationException">Thrown if this enters an unstable state as a result of this refresh.</exception>
		private void RefreshDrawOrder(object? sender, EventArgs e)
		{
			// We need to calculate the new and old orders first
			if(sender is not IDrawable d || sender is not T component)
				return;
			
			int new_order = d.DrawOrder;

			LastKnownValues v = Components[component];
			int old_order = v.DrawOrder;

			// This should never be the case, but just in case
			if(new_order == old_order)
				return;

			// Perform the removeal logic
			if(!ComponentsDrawOrder.TryGetValue(old_order,out HashSet<IDrawable>? ds))
				return;

			if(!ds.Remove(d))
				throw new InvalidOperationException();

			if(ds.Count == 0 && !ComponentsDrawOrder.Remove(old_order))
				throw new InvalidOperationException();

			// Now add the item back in
			if(!ComponentsDrawOrder.TryGetValue(new_order,out ds) && !ComponentsDrawOrder.TryAdd(new_order,ds = new HashSet<IDrawable>()))
				throw new InvalidOperationException();
			
			if(!ds.Add(d))
				throw new InvalidOperationException();

			Components[component] = new LastKnownValues(v.UpdateOrder,new_order);
			return;
		}

		/// <summary>
		/// Refreshes the debug draw order of a component.
		/// </summary>
		/// <param name="sender">The component to refresh.</param>
		/// <param name="new_order">The new debug draw order of the component.</param>
		/// <param name="old_order">The old debug draw order of the component.</param>
		/// <exception cref="InvalidOperationException">Thrown if this enters an unstable state as a result of this refresh.</exception>
		private void RefreshDebugOrder(IDebugDrawable sender, int new_order, int old_order)
		{
			// This should never be the case, but just in case
			if(new_order == old_order)
				return;

			// Perform the removal logic
			if(!DebugDrawOrder.TryGetValue(old_order,out HashSet<IDebugDrawable>? ds))
				return;

			if(!ds.Remove(sender))
				throw new InvalidOperationException();

			if(ds.Count == 0 && !DebugDrawOrder.Remove(old_order))
				throw new InvalidOperationException();

			// Now add the item back in
			if(!DebugDrawOrder.TryGetValue(sender.DrawDebugOrder,out ds) && !DebugDrawOrder.TryAdd(sender.DrawDebugOrder,ds = new HashSet<IDebugDrawable>()))
				throw new InvalidOperationException();
			
			if(!ds.Add(sender))
				throw new InvalidOperationException();

			return;
		}

		/// <summary>
		/// Refreshes the render target draw order of a component.
		/// </summary>
		/// <param name="sender">The component to refresh.</param>
		/// <param name="e">The event arguments.</param>
		/// <exception cref="InvalidOperationException">Thrown if this enters an unstable state as a result of this refresh.</exception>
		private void RefreshRenderOrder(object? sender, RenderTargetDrawOrderEventArgs e)
		{
			// This should never be the case, but just in case
			if(e.NewOrder == e.OldOrder)
				return;

			// Perform the removeal logic
			if(!RenderDrawOrder.TryGetValue(e.OldOrder,out HashSet<IRenderTargetDrawable>? ds))
				return;

			if(!ds.Remove(e.Sender))
				throw new InvalidOperationException();

			if(ds.Count == 0 && !RenderDrawOrder.Remove(e.OldOrder))
				throw new InvalidOperationException();
			
			// Now add the item back in
			if(!RenderDrawOrder.TryGetValue(e.NewOrder,out ds) && !RenderDrawOrder.TryAdd(e.NewOrder,ds = new HashSet<IRenderTargetDrawable>()))
				throw new InvalidOperationException();
			
			if(!ds.Add(e.Sender))
				throw new InvalidOperationException();

			return;
		}

		/// <summary>
		/// Determines if this contains <paramref name="component"/>.
		/// </summary>
		/// <param name="component">The component to check for.</param>
		/// <returns>Returns true if this contains <paramref name="component"/> and false otherwise.</returns>
		public bool ContainsComponent(T component) => Components.ContainsKey(component);

		/// <summary>
		/// Determines if this game object contains a component of type <typeparamref name="E"/>.
		/// </summary>
		/// <typeparam name="E">The type to check for.</typeparam>
		/// <returns>Returns true if this contains a componet of type <typeparamref name="E"/> and false otherwise.</returns>
		/// <remarks>This is a linear time operation.</remarks>
		public bool ContainsComponent<E>() where E : IGameComponent
		{
			foreach(T component in Components.Keys)
				if(component is E)
					return true;

			return false;
		}

		/// <summary>
		/// Obtains the first component of this which is of <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="E">The type to look for.</typeparam>
		/// <returns>Returns the first component of type <typeparamref name="T"/> found or default(E) if no such component exists.</returns>
		/// <remarks>This is a linear time operation.</remarks>
		public E? GetComponent<E>() where E : IGameComponent
		{
			foreach(T component in Components.Keys)
				if(component is E e)
					return e;
			
			return default(E);
		}

		/// <summary>
		/// Obtains every component of this which is of <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="E">The type to look for.</typeparam>
		/// <returns>Returns an enumerable list of components of type <typeparamref name="T"/>. The list will be empty if this has no components of type <typeparamref name="T"/>.</returns>
		/// <remarks>This is a linear time operation.</remarks>
		public IList<E> GetComponents<E>() where E : IGameComponent
		{
			List<E> ret = new List<E>();

			foreach(T component in Components.Keys)
				if(component is E e)
					ret.Add(e);
			
			return ret;
		}

		/// <summary>
		/// Removes all components from this.
		/// <para/>
		/// This does not Dispose any components removed, leaving that for the destructor to take care of later if they are lost.
		/// </summary>
		public void ClearComponents()
		{
			Components.Clear();

			foreach(HashSet<IUpdateable> us in ComponentsUpdateOrder.Values)
				foreach(IUpdateable component in us)
					component.UpdateOrderChanged -= RefreshUpdateOrder;

			foreach(HashSet<IDrawable> us in ComponentsDrawOrder.Values)
				foreach(IDrawable component in us)
					component.DrawOrderChanged -= RefreshDrawOrder;

			foreach(HashSet<IDebugDrawable> us in DebugDrawOrder.Values)
				foreach(IDebugDrawable component in us)
					component.OnDrawDebugOrderChanged -= RefreshDebugOrder;

			foreach(HashSet<IRenderTargetDrawable> us in RenderDrawOrder.Values)
				foreach(IRenderTargetDrawable component in us)
					component.RenderTargetDrawOrderChanged -= RefreshRenderOrder;
			
			ComponentsUpdateOrder.Clear();
			ComponentsDrawOrder.Clear();
			DebugDrawOrder.Clear();
			RenderDrawOrder.Clear();

			return;
		}

		public IEnumerator<T> GetEnumerator() => Components.Keys.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// If true, then this has been initialized.
		/// If false, it has not yet been.
		/// </summary>
		public bool Initialized
		{get; private set;}

		/// <summary>
		/// The components of this game object.
		/// </summary>
		private Dictionary<T,LastKnownValues> Components
		{get;}

		/// <summary>
		/// A convenient way to obtain the update order of the components.
		/// </summary>
		private SortedDictionary<int,HashSet<IUpdateable>> ComponentsUpdateOrder
		{get;}

		/// <summary>
		/// Enumerates the components in update order.
		/// </summary>
		public IEnumerable<IUpdateable> UpdateOrder => new DoubleHashEnumerable<IUpdateable>(ComponentsUpdateOrder);

		/// <summary>
		/// A convenient way to obtain the draw order of the components.
		/// </summary>
		private SortedDictionary<int,HashSet<IDrawable>> ComponentsDrawOrder
		{get;}

		/// <summary>
		/// Enumerates the components in draw order.
		/// </summary>
		public IEnumerable<IDrawable> DrawOrder => new DoubleHashEnumerable<IDrawable>(ComponentsDrawOrder);

		/// <summary>
		/// A convenient way to obtain the draw order of the components which draw debug info.
		/// </summary>
		private SortedDictionary<int,HashSet<IDebugDrawable>> DebugDrawOrder
		{get;}

		/// <summary>
		/// Enumerates the components in debug draw order.
		/// </summary>
		public IEnumerable<IDebugDrawable> DebugOrder => new DoubleHashEnumerable<IDebugDrawable>(DebugDrawOrder);

		/// <summary>
		/// A convenient way to obtain the draw order of the components which use render targets.
		/// </summary>
		private SortedDictionary<int,HashSet<IRenderTargetDrawable>> RenderDrawOrder
		{get;}

		/// <summary>
		/// Enumerates the components in render draw order.
		/// </summary>
		public IEnumerable<IRenderTargetDrawable> RenderOrder => new DoubleHashEnumerable<IRenderTargetDrawable>(RenderDrawOrder);

		/// <summary>
		/// The number of components belonging to this circuit.
		/// </summary>
		public int ComponentCount => Components.Count;

		/// <summary>
		/// A data wrapper for dumb values we can't get any other way.
		/// </summary>
		private struct LastKnownValues
		{
			public LastKnownValues(int _uo, int _do)
			{
				UpdateOrder = _uo;
				DrawOrder = _do;

				return;
			}

			public int UpdateOrder
			{get;}

			public int DrawOrder
			{get;}
		}

		/// <summary>
		/// A wrapper class to make an awkward enumerable.
		/// </summary>
		/// <typeparam name="E">The end type we are trying to enumerate.</typeparam>
		private class DoubleHashEnumerable<E> : IEnumerable<E>
		{
			public DoubleHashEnumerable(SortedDictionary<int,HashSet<E>> set)
			{
				enumerate_me = set;
				return;
			}

			public IEnumerator<E> GetEnumerator() => new DoubleHashEnumerator<E>(enumerate_me);
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			private SortedDictionary<int,HashSet<E>> enumerate_me;
		}

		/// <summary>
		/// Iterates over a double hash set.
		/// </summary>
		/// <typeparam name="E">The type being enumerated.</typeparam>
		private class DoubleHashEnumerator<E> : IEnumerator<E>
		{
			/// <summary>
			/// Creates a new enumerator.
			/// </summary>
			/// <param name="hash">The double hash set to enumerate.</param>
			public DoubleHashEnumerator(SortedDictionary<int,HashSet<E>> hash)
			{
				start = hash;
				top = null;
				inline = null;

				finished = false;
				return;
			}

			public bool MoveNext()
			{
				if(finished)
					return false;

				top ??= start.GetEnumerator();

				if(inline is not null && !inline.MoveNext())
					inline = null;

				if(inline is null)
				{
					if(!top.MoveNext())
					{
						top = null;
						finished = true;

						return false;
					}

					// inline must have at least one value to exist, so we don't need to check that
					inline = top.Current.Value.GetEnumerator();
					inline.MoveNext();
				}

				return true;
			}

			public void Reset()
			{
				top = null;
				inline = null;

				finished = false;
				return;
			}

			public void Dispose()
			{
				if(top is not null)
					top.Dispose();

				if(inline is not null)
					inline.Dispose();

				return;
			}

			public E Current => inline is null ? throw new InvalidOperationException() : inline.Current;
			object? IEnumerator.Current => Current;
			
			private SortedDictionary<int,HashSet<E>> start;
			private IEnumerator<KeyValuePair<int,HashSet<E>>>? top;
			private IEnumerator<E>? inline;
			private bool finished;
		}
	}
}
