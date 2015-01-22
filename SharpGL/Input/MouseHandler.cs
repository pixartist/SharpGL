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
	public class MouseHandler
	{
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
        public static bool MouseLocked { get; set; }
		private static Dictionary<MouseButton, bool> pressedButtons;
		private static Dictionary<MouseButton, Action> buttonActions;
		private static Vector2 mPos, mDelta;
        private static GameWindow _window;
		public static float X
		{
			get
			{
				return mPos.X;
			}
		}
		public static float Y
		{
			get
			{
				return mPos.Y;
			}
		}
		public delegate void MouseMoveHandler(Vector2 position, Vector2 delta);
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
		public static void RegisterButtonDown(MouseButton k, Action action)
		{
			pressedButtons.Add(k, false);
			buttonActions.Add(k, action);
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
