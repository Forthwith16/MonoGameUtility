using GameEngine.GUI;

namespace GameEngine.Events
{
	/// <summary>
	/// An event created when a GUICore changes its active component.
	/// </summary>
	public class ActiveComponentChangedEvent : EventArgs
	{
		/// <summary>
		/// Creates a new event.
		/// This event indicates that neither digital nor mouse movement resulted in the active component changing.
		/// </summary>
		/// <param name="src">The source of the state change.</param>
		/// <param name="ac">The active component.</param>
		/// <param name="lac">The previous active component.</param>
		public ActiveComponentChangedEvent(GUICore src, IGUI? ac, IGUI? lac)
		{
			Source = src;
			
			ActiveComponent = ac;
			LastActiveComponent = lac;

			Mouse = false;
			Digital = false;
			Jump = true;

			return;
		}

		/// <summary>
		/// Creates a new event.
		/// This even indicates that a digital or mouse movement resulted in the active component changing.
		/// </summary>
		/// <param name="src">The source of the state change.</param>
		/// <param name="mouse">If true, then the mouse caused the state change. Otherwise, a digital input cuased the state change.</param>
		/// <param name="ac">The active component.</param>
		/// <param name="lac">The previous active component.</param>
		public ActiveComponentChangedEvent(GUICore src, IGUI? ac, IGUI? lac, bool mouse)
		{
			Source = src;
			
			ActiveComponent = ac;
			LastActiveComponent = lac;

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
		/// The component that is now active.
		/// </summary>
		public IGUI? ActiveComponent
		{get;}

		/// <summary>
		/// The component that was active.
		/// </summary>
		public IGUI? LastActiveComponent
		{get;}

		/// <summary>
		/// If true, then the mouse caused the active state to change.
		/// </summary>
		public bool Mouse
		{get;}

		/// <summary>
		/// If true, then digital movement caused the active state to change.
		/// </summary>
		public bool Digital
		{get;}

		/// <summary>
		/// If true, then a manual jump caused the active state to change.
		/// </summary>
		public bool Jump
		{get;}
	}
}
