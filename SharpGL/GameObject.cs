using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Components;
namespace SharpGL
{
	public class GameObject : DestructableObject
	{
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
			c.Init(this);
			return (T)c;
		}
		internal void RemoveComponent(Component c)
		{
			components.Remove(c.GetType());
		}
		internal void Update(float dt)
		{
			if(Math.Abs(Transform.Position.X) >= App.WorldSize.X || Math.Abs(Transform.Position.Y) >= App.WorldSize.Y || Math.Abs(Transform.Position.Z) >= App.WorldSize.Z)
			{
				Destroy();
			}
			foreach (var c in components.Values)
				c.Update(dt);
			foreach (var c in children)
				c.Update(dt);
		}
		protected override void PreDestruction()
		{
			if (Parent != null)
				Parent.children.Remove(this);
		}
		internal override void DestroyInternalRecursive()
		{
			
			var comps = components.Values.ToArray<Component>();
			foreach (var c in comps)
			{
				c.OnDestroy();
				c.DestroyInternalRecursive();
			}
			foreach (var c in children)
			{
				c.OnDestroy();
				c.DestroyInternalRecursive();
			}
			base.DestroyInternalRecursive();
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
