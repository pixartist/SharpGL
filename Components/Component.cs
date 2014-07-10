using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL.Components
{
	public abstract class Component
	{
		public GameObject GameObject { get; private set; }
		public Transform Transform
		{
			get
			{
				if(GameObject != null)
					return GameObject.Transform;
				return null;
			}
		}
		public Component(GameObject parent)
		{
			GameObject = parent;
			Init();
		}
		public virtual void Destroy()
		{
			if (GameObject != null)
				GameObject.RemoveComponent(this);
		}
		internal virtual void Init()
		{ }
		internal virtual void Render(float time)
		{ }
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
	}
}
