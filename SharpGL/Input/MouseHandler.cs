using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using System.Windows.Forms;
using System.Drawing;
namespace SharpGL.Input
{
    /// <summary>
    /// Handles mouse movement as well as mouse button events.
    /// </summary>
	public class MouseHandler
	{
        /// <summary>
        /// Shows or hides the mouse cursor
        /// </summary>
        public static bool CursorVisible
        { 
            get
            {
                return _window.CursorVisible;
            }
            set
            {
                _window.CursorVisible = value;
            }
        }
        /// <summary>
        /// If true, the mouse will be locked to the center of the screen.
        /// </summary>
        public static bool MouseLocked { get; set; }
		private static Dictionary<MouseButton, bool> pressedButtons;
		private static Dictionary<MouseButton, Action> buttonActions;
		private static Vector2 mPos, mDelta;
        private static GameWindow _window;
        /// <summary>
        /// X-Position of the mouse cursor
        /// </summary>
		public static float X
		{
			get
			{
				return mPos.X;
			}
		}
        /// <summary>
        /// Y-Position of the mouse cursor
        /// </summary>
		public static float Y
		{
			get
			{
				return mPos.Y;
			}
		}
		public delegate void MouseMoveHandler(Vector2 position, Vector2 delta);
        /// <summary>
        /// Called when the mouse cursor is moved.
        /// </summary>
		public static event MouseMoveHandler OnMouseMove;
		internal static void Init(GameWindow window)
		{
            _window = window;
			window.Mouse.ButtonDown += Mouse_ButtonDown;
			window.Mouse.ButtonUp += Mouse_ButtonUp;
           
			pressedButtons = new Dictionary<MouseButton, bool>();
			buttonActions = new Dictionary<MouseButton, Action>();
			
		}

		static void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
		{
			if(pressedButtons.ContainsKey(e.Button))
            {
                pressedButtons[e.Button] = false;
            }
		}

		static void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (pressedButtons.ContainsKey(e.Button))
            {
                pressedButtons[e.Button] = true;
            }
		}
        /// <summary>
        /// Registers an action to be called when the specified mouse button is pressed
        /// </summary>
        /// <param name="b">Mouse button</param>
        /// <param name="action">Event to be called</param>
		public static void RegisterButtonDown(MouseButton b, Action action)
		{
			pressedButtons.Add(b, false);
			buttonActions.Add(b, action);
		}
		internal static void Update()
		{
            mDelta.X = mPos.X - Cursor.Position.X;
            mDelta.Y = mPos.Y - Cursor.Position.Y;
            if (mDelta.LengthSquared > 0)
            {
                if (MouseLocked)
                    Cursor.Position = new Point(_window.X + _window.Width / 2, _window.Y + _window.Height / 2);
                mPos.X = Cursor.Position.X;
                mPos.Y = Cursor.Position.Y;
                if (OnMouseMove != null)
                    OnMouseMove(mPos, mDelta);
            }
			foreach (MouseButton k in buttonActions.Keys)
			{
				if (pressedButtons[k])
					buttonActions[k]();
			}
		}
	}
}
