using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Components;
namespace SharpGL
{
    /// <summary>
    /// A GameObject is the base class for any actor which can be positioned in the game world. Every GameObject has a Transform component which holds the transformation data.
    /// It also holds a list of children game objects as well as a collection of components
    /// </summary>
	public class GameObject : DestructableObject
	{
        /// <summary>
        /// The Transform component of the GameObject
        /// </summary>
		public Transform Transform
		{
			get
			{
				return Component<Transform>();
			}
		}
        /// <summary>
        /// The name of the GameObject
        /// </summary>
		public string Name { get; set; }

        /// <summary>
        /// The parent GameObject of this GameObject. Null for a root object
        /// </summary>
		public GameObject Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != null)
                    parent.children.Remove(this);
                if (value != null)
                    value.children.Add(this);
                parent = value;
            }
        }
        private GameObject parent;
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
        /// <summary>
        /// Adds a component to this GameObject. A GameObject can only hold one component of each type
        /// </summary>
        /// <typeparam name="T">The type of the component, must inherit from Component</typeparam>
        /// <returns>the component object</returns>
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
        /// <summary>
        /// Called before an object is destroyed
        /// </summary>
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
        /// <summary>
        /// Tries to return a Component of type T
        /// </summary>
        /// <typeparam name="T">Type of the component</typeparam>
        /// <returns>the component of the provided type or null if component does not exist</returns>
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
