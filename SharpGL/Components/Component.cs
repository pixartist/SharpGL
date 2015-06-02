using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL.Components
{
    /// <summary>
    /// A Component can be attached to a GameObject and will perform a specific task on it. Only one component of each type can be attached to a GameObject
    /// </summary>
	public abstract class Component : DestructableObject
	{
        /// <summary>
        /// The GameObject this component is attached to
        /// </summary>
		public GameObject GameObject { get; private set; }
        /// <summary>
        /// The Transform of the GameObject this component is attached to
        /// </summary>
		public Transform Transform
		{
			get
			{
				if(GameObject != null)
					return GameObject.Transform;
				return null;
			}
		}
		protected Component()
		{
			
		}
		internal void Update(float dt)
		{
			OnUpdate(dt);
		}
		internal void Init(GameObject parent)
		{
			GameObject = parent;
			App = parent.App;
			OnInit();
		}
        /// <summary>
        /// Called when this component is initialized
        /// </summary>
		protected virtual void OnInit()
		{

		}
        /// <summary>
        /// Called each update loop
        /// </summary>
        /// <param name="dt">Delta time</param>
		protected virtual void OnUpdate(float dt)
		{

		}
        /// <summary>
        /// Tries to return the active camera of the current scene
        /// </summary>
        /// <param name="camera">Will contain the active camera or null if no active camera can be found</param>
        /// <returns>True if a camera could be found, false otherwise</returns>
		public bool TryGetActiveCamera(out Camera camera)
		{
			if (GameObject != null)
			{
				if (GameObject.App != null)
				{
					camera = GameObject.App.ActiveCamera;
					return camera != null;
				}
			}
			camera = null;
			return false;
		}
        /// <summary>
        /// Called before this Component is destroyed
        /// </summary>
		protected override void PreDestruction()
		{
			
		}
	}
}
