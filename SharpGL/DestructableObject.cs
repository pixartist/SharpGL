using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGL
{
	public abstract class DestructableObject
	{
		public bool Destroyed { get; private set; }
		public App App { get; protected set; }
		public void Destroy()
		{
			if (!Destroyed)
			{
				App.ScheduleDestruction(this);
			}
		}
		protected abstract void PreDestruction();
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
