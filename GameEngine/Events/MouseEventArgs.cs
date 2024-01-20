using GameEngine.Input.Bindings.MouseBindings;
using Microsoft.Xna.Framework;

namespace GameEngine.Events
{
	/// <summary>
	/// Event arguments for a mouse event.
	/// </summary>
	public abstract class MouseEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes the base mouse event data.
		/// This version is a result of a manual initiation of a mouse event rather than from user input.
		/// </summary>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseEventArgs(bool left, int x, int y, int dx, int dy)
		{
			IsMouse = false;
			IsDigitalMouse = false;
			IsManualEvent = true;

			LeftDown = left;

			X = x;
			Y = y;

			DeltaX = dx;
			DeltaY = dy;
			
			return;
		}

		/// <summary>
		/// Initializes the base mouse event data.
		/// </summary>
		/// <param name="digital">If true, this is an emulated digital mouse event. Otherwise, this is a true mouse event.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseEventArgs(bool digital, bool left, int x, int y, int dx, int dy)
		{
			IsMouse = !digital;
			IsDigitalMouse = digital;
			IsManualEvent = false;

			LeftDown = left;

			X = x;
			Y = y;

			DeltaX = dx;
			DeltaY = dy;
			
			return;
		}

		/// <summary>
		/// If true, then this is a true mouse event.
		/// </summary>
		public bool IsMouse
		{get;}

		/// <summary>
		/// If true, then this is a simulated digital mouse event.
		/// </summary>
		public bool IsDigitalMouse
		{get;}

		/// <summary>
		/// If true, then this event is manually created rather than generated through user input.
		/// </summary>
		public bool IsManualEvent
		{get;}

		/// <summary>
		/// The mouse X position.
		/// </summary>
		public int X
		{get;}

		/// <summary>
		/// The mouse y position.
		/// </summary>
		public int Y
		{get;}

		/// <summary>
		/// The mouse position.
		/// </summary>
		public Point Position
		{get => new Point(X,Y);}

		/// <summary>
		/// The mouse X delta.
		/// <para/>
		/// If this is a digital mouse emulation, then this value is 1 if right is pressed, -1 if left is pressed, and 0 if both or neither is pressed.
		/// </summary>
		public int DeltaX
		{get;}

		/// <summary>
		/// The mouse y delta.
		/// <para/>
		/// If this is a digital mouse emulation, then this value is 1 if down is pressed, -1 if up is pressed, and 0 if both or neither is pressed.
		/// </summary>
		public int DeltaY
		{get;}

		/// <summary>
		/// If true, then the mouse moved.
		/// If false, then the mouse did not move.
		/// </summary>
		public bool Moved
		{get => DeltaX != 0 || DeltaY != 0;}

		/// <summary>
		/// If true, then the left mouse button is down.
		/// If false, then the left mouse button is up.
		/// <para/>
		/// If this is a digital mouse emulation, then this value corresponds to if an emulated click is currently occuring.
		/// </summary>
		public bool LeftDown
		{get;}

		/// <summary>
		/// If true, then the left mouse button is up.
		/// If false, then the left mouse button is down.
		/// <para/>
		/// If this is a digital mouse emulation, then this value corresponds to if an emulated click is currently not occuring.
		/// </summary>
		public bool LeftUp
		{get => !LeftDown;}
	}

	/// <summary>
	/// A mouse clicked event (a true mouse click that has been pressed and then released).
	/// </summary>
	public class MouseClickedEventArgs : MouseEventArgs
	{
		/// <summary>
		/// Initializes mouse clicked data.
		/// </summary>
		/// <param name="b">The button clicked.</param>
		/// <param name="digital">If true, this is an emulated digital mouse event. Otherwise, this is a true mouse event.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseClickedEventArgs(MouseButton b, bool digital, bool left, int x, int y, int dx, int dy) : base(digital,left,x,y,dx,dy)
		{
			Button = b;
			return;
		}

		/// <summary>
		/// The button clicked.
		/// </summary>
		public MouseButton Button
		{get;}
	}

	/// <summary>
	/// A mouse click event.
	/// </summary>
	public class MouseClickEventArgs : MouseEventArgs
	{
		/// <summary>
		/// Initializes mouse click data.
		/// </summary>
		/// <param name="b">The button pressed or released.</param>
		/// <param name="pressed">If true, then <paramref name="b"/> was pressed. If false, then <paramref name="b"/> was released.</param>
		/// <param name="digital">If true, this is an emulated digital mouse event. Otherwise, this is a true mouse event.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseClickEventArgs(MouseButton b, bool pressed, bool digital, bool left, int x, int y, int dx, int dy) : base(digital,left,x,y,dx,dy)
		{
			Button = b;
			Click = pressed;

			return;
		}

		/// <summary>
		/// The button pressed or released.
		/// </summary>
		public MouseButton Button
		{get;}

		/// <summary>
		/// If true, then Button was pressed.
		/// If false, then Button was released.
		/// </summary>
		public bool Click
		{get;}

		/// <summary>
		/// If true, then Button was released.
		/// If false, then Button was pressed.
		/// </summary>
		public bool Release
		{get => !Click;}
	}

	/// <summary>
	/// A mouse hover event.
	/// </summary>
	public class MouseHoverEventArgs : MouseEventArgs
	{
		/// <summary>
		/// Initializes mouse hover data.
		/// This version is the result of a manual jump between components rather than user input.
		/// </summary>
		/// <param name="hover">If true, then the mouse hovered. If false, then the mouse exited.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseHoverEventArgs(bool hover, bool left, int x, int y, int dx, int dy) : base(left,x,y,dx,dy)
		{
			Hover = hover;
			return;
		}

		/// <summary>
		/// Initializes mouse hover data.
		/// </summary>
		/// <param name="hover">If true, then the mouse hovered. If false, then the mouse exited.</param>
		/// <param name="digital">If true, this is an emulated digital mouse event. Otherwise, this is a true mouse event.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseHoverEventArgs(bool hover, bool digital, bool left, int x, int y, int dx, int dy) : base(digital,left,x,y,dx,dy)
		{
			Hover = hover;
			return;
		}

		/// <summary>
		/// If true, then the mouse hovered.
		/// If false, then the mouse exited.
		/// </summary>
		public bool Hover
		{get;}

		/// <summary>
		/// If true, then the mouse exited.
		/// If false, then the mouse hovered.
		/// </summary>
		public bool Exit
		{get;}
	}

	/// <summary>
	/// A mouse move event.
	/// </summary>
	public class MouseMoveEventArgs : MouseEventArgs
	{
		/// <summary>
		/// Initializes mouse move data.
		/// </summary>
		/// <param name="digital">If true, this is an emulated digital mouse event. Otherwise, this is a true mouse event.</param>
		/// <param name="left">If true, then the left mouse button (real or emulated) is down. If false, it is up.</param>
		/// <param name="x">This is the mouse x position. An emulated digital event should have this be the x position of the GUI component sending the event.</param>
		/// <param name="y">This is the mouse y position. An emulated digital event should have this be the y position of the GUI component sending the event.</param>
		/// <param name="dx">The mouse x delta. An emulated digital event should make this value [right button down] - [left button down].</param>
		/// <param name="dy">The mouse y delta. An emulated digital event should make this value [down button down] - [up button down].</param>
		public MouseMoveEventArgs(bool digital, bool left, int x, int y, int dx, int dy) : base(digital,left,x,y,dx,dy)
		{return;}
	}
}
