using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
    /// <summary>
    /// Base class for GameObjects and Components. Allows for managed destruction.
    /// </summary>
	public abstract class DestructableObject
	{
		public bool Destroyed { get; private set; }
		public App App { get; protected set; }
        /// <summary>
        /// Schedules the destruction of this object
        /// </summary>
		public void Destroy()
		{
			if (!Destroyed)
			{
				App.ScheduleDestruction(this);
			}
		}
        /// <summary>
        /// Called before destruction of this object.
        /// </summary>
		protected abstract void PreDestruction();
        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>
		public virtual void OnDestroy()
		{

		}
		
		internal void DestroyInternal()
		{
			if (!Destroyed)
			{
				PreDestruction();
				OnDestroy();
				DestroyInternalRecursive();
				Destroyed = true;
			}
		}
		internal virtual void DestroyInternalRecursive()
		{
			Destroyed = true;
		}
	}
}
