using GameEngine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events
{
	/// <summary>
	/// An event created when a GUICore changes its active component.
	/// </summary>
	public class FocusedComponentChangedEvent : EventArgs
	{
		/// <summary>
		/// Creates a new event.
		/// This event indicates that neither digital nor mouse movement resulted in the focused component changing.
		/// </summary>
		/// <param name="src">The source of the state change.</param>
		/// <param name="fc">The focused component.</param>
		/// <param name="lfc">The previous focused component.</param>
		public FocusedComponentChangedEvent(GUICore src, IGUI? fc, IGUI? lfc)
		{
			Source = src;
			
			FocusedComponent = fc;
			LastFocusedComponent = lfc;

			Mouse = false;
			Digital = false;
			Jump = true;

			return;
		}

		/// <summary>
		/// Creates a new event.
		/// This even indicates that a digital or mouse movement resulted in the focused component changing.
		/// </summary>
		/// <param name="src">The source of the state change.</param>
		/// <param name="mouse">If true, then the mouse caused the state change. Otherwise, a digital input cuased the state change.</param>
		/// <param name="fc">The focused component.</param>
		/// <param name="lfc">The previous focused component.</param>
		public FocusedComponentChangedEvent(GUICore src, IGUI? fc, IGUI? lfc, bool mouse)
		{
			Source = src;
			
			FocusedComponent = fc;
			LastFocusedComponent = lfc;

			Mouse = mouse;
			Digital = !mouse;
			Jump = false;

			return;
		}

		/// <summary>
		/// The source generating the event.
		/// </summary>
		public GUICore Source
		{get;}

		/// <summary>
		/// The component that is now focused.
		/// </summary>
		public IGUI? FocusedComponent
		{get;}

		/// <summary>
		/// The component that is was focused.
		/// </summary>
		public IGUI? LastFocusedComponent
		{get;}

		/// <summary>
		/// If true, then the mouse caused the focused state to change.
		/// </summary>
		public bool Mouse
		{get;}

		/// <summary>
		/// If true, then digital movement caused the focused state to change.
		/// </summary>
		public bool Digital
		{get;}

		/// <summary>
		/// If true, then a manual jump caused the focused state to change.
		/// </summary>
		public bool Jump
		{get;}
	}
}
