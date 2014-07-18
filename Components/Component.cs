using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL.Components
{
	public abstract class Component
	{
		public GameObject GameObject { get; internal set; }
		public Transform Transform
		{
			get
			{
				if(GameObject != null)
					return GameObject.Transform;
				return null;
			}
		}
		public App App
		{
			get
			{
				if (GameObject != null)
					return GameObject.App;
				return null;
			}
		}
		protected Component()
		{

		}
		public virtual void Destroy()
		{
			if (GameObject != null)
				GameObject.RemoveComponent(this);
		}
		internal virtual void Init()
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
