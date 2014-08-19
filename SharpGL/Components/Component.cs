using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL.Components
{
	public abstract class Component : DestructableObject
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
		protected Component()
		{
			
		}
		internal void Update()
		{
			OnUpdate();
		}
		internal void Init(GameObject parent)
		{
			GameObject = parent;
			App = parent.App;
			OnInit();
		}
		protected virtual void OnInit()
		{

		}
		protected virtual void OnUpdate()
		{

		}
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
		protected override void PreDestruction()
		{
			
		}
	}
}
