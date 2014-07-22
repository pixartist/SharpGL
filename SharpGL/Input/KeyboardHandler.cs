using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
namespace SharpGL.Input
{
    public static class KeyboardHandler
    {
        private static Dictionary<Key, bool> pressedKeys;
        private static Dictionary<Key, Action> keyActions;
        internal static void Init(GameWindow window)
        {
            window.KeyDown += window_KeyDown;
            window.KeyUp += window_KeyUp;
            pressedKeys = new Dictionary<Key, bool>();
            keyActions = new Dictionary<Key, Action>();
        }
        public static void RegisterKeyDown(Key k, Action action)
        {
            pressedKeys.Add(k, false);
            keyActions.Add(k, action);
        }
        private static void window_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (pressedKeys.ContainsKey(e.Key))
            {
                pressedKeys[e.Key] = false;
            }
        }

        private static void window_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(pressedKeys.ContainsKey(e.Key))
            {
                pressedKeys[e.Key] = true;
            }
        }
        internal static void Update()
        {
            foreach(Key k in keyActions.Keys)
            {
                if (pressedKeys[k])
                    keyActions[k]();
            }
        }
    }
}
