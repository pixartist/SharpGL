using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
namespace SharpGL.Input
{
	public class MouseHandler
	{
		private static Dictionary<MouseButton, bool> pressedButtons;
		private static Dictionary<MouseButton, Action> buttonActions;
		private static Vector2 mPos, mDelta;
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
			window.Mouse.ButtonDown += Mouse_ButtonDown;
			window.Mouse.ButtonUp += Mouse_ButtonUp;
			window.Mouse.Move += Mouse_Move;
			pressedButtons = new Dictionary<MouseButton, bool>();
			buttonActions = new Dictionary<MouseButton, Action>();
			
		}

		static void Mouse_Move(object sender, MouseMoveEventArgs e)
		{
			mDelta.X = mPos.X - e.X;
			mDelta.Y = mPos.Y - e.Y;
			mPos.X = e.X;
			mPos.Y = e.Y;

			if (OnMouseMove != null)
				OnMouseMove(mPos, mDelta);
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
			foreach (MouseButton k in buttonActions.Keys)
			{
				if (pressedButtons[k])
					buttonActions[k]();
			}
		}
	}
}
