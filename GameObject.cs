using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Components;
namespace SharpGL
{
	public class GameObject
	{
		public App App { get; private set; }
		public Transform Transform
		{
			get
			{
				return Component<Transform>();
			}
		}
		public string Name { get; set; }
		public GameObject Parent { get; private set; }
		private List<GameObject> children;
		private Dictionary<Type, Component> components;
		internal GameObject(string name, App app)
		{
			App = app;
			Name = name;
			children = new List<GameObject>();
			components = new Dictionary<Type, Component>();
			AddComponent<Transform>();
		}
		internal void Render(float dTime)
		{
			foreach (var c in components.Values)
				c.Render(dTime);
		}
		public GameObject AddChild(GameObject child)
		{
			children.Add(child);
			child.Parent = this;
			return child;
		}
		public T AddComponent<T>() where T : Component
		{

			Type t = typeof(T);
			if (components.ContainsKey(t))
				throw (new InvalidOperationException("A component of this type already exists."));
			Component c = (Component)Activator.CreateInstance(t);
			components.Add(t, c);
			c.GameObject = this;
			c.Init();
			return (T)c;
		}
		internal void RemoveComponent(Component c)
		{
			components.Remove(c.GetType());
		}
		public void Destroy()
		{
			Parent.children.Remove(this);
			var comps = components.Values.ToArray<Component>();
			foreach(var c in comps)
			{
				c.Destroy();
			}
		}
		public T Component<T>() where T : Component
		{
			Component tmp;
			if(components.TryGetValue(typeof(T), out tmp))
			{
				return (T)tmp;
			}
			return null;
		}
	}
}
