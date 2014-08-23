using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
namespace SharpGL.Components
{
	public class PlayerControllerGhost : Component
	{
		public float Drag { get; set; }
		public Vector3 Velocity { get; set; }
		public float Acceleration { get; set; }
		public bool LerpTranslation { get; set; }
		protected override void OnInit()
		{
			Drag = 0.05f;
			Acceleration = 0.2f;
			LerpTranslation = true;
		}
		public void MoveForward()
		{
			Translate(Transform.LocalForward);
		}
		public void MoveRight()
		{
			Translate(Transform.LocalRight);
		}
		public void MoveUp()
		{
			Translate(Transform.LocalUp);
		}
		public void MoveBack()
		{
			Translate(-Transform.LocalForward);
		}
		public void MoveLeft()
		{
			Translate(-Transform.LocalRight);
		}
		public void MoveDown()
		{
			Translate(-Transform.LocalUp);
		}
		protected override void OnUpdate(float dt)
		{
			if(LerpTranslation)
			{
				Transform.Position += Velocity * dt;
				Velocity *= 1 - Drag;
			}
		}
		public void Translate(Vector3 direction)
		{
			direction.Normalize();
			if (LerpTranslation)
			{
				TranslateTargetPosition(direction * Acceleration);
			}
			else
			{
				Transform.Translate(direction * Acceleration);
			}
		}
		private void TranslateTargetPosition(Vector3 amount)
		{
			Velocity += amount;
		}
	}
}
