using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using BulletSharp;
using SharpGL.Components;
namespace SharpGL.Components.BulletPhysics
{
	public class PlayerControllerFPS : Component
	{
        public KinematicCharacterController Controller { get; private set; }
        public PairCachingGhostObject CharacterGhost { get; private set; }
        public float EyeHeight { get; set; }
        
		protected override void OnInit()
		{
            
            
			base.OnInit();
            EyeHeight = 1.65f;
            var shape = new CapsuleShape(0.6f, 1.8f);
            CharacterGhost = new PairCachingGhostObject();
            CharacterGhost.WorldTransform = Transform.GetMatrixInverse();
            CharacterGhost.CollisionShape = shape;
            CharacterGhost.CollisionFlags = CollisionFlags.CharacterObject;
            CharacterGhost.UserObject = GameObject;
            Controller = new KinematicCharacterController(CharacterGhost, shape, 0.1f);
            SetMaxJumpHeight(1.0f);
            SetJumpSpeed(15.0f);
            Controller.SetFallSpeed(850.0f);
            App.PhysicsWorld.AddCollisionObject(CharacterGhost, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
            App.PhysicsWorld.AddAction(Controller);
            App.PhysicsWorld.Broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new GhostPairCallback());
		}
        public void SetMaxJumpHeight(float jumpHeight)
        {
            Controller.SetMaxJumpHeight(jumpHeight);
        }
        public void SetJumpSpeed(float jumpSpeed)
        {
            Controller.SetJumpSpeed(jumpSpeed);
        }
		public void MoveForward(float strength = 1.0f)
		{
			Translate(Transform.LocalForward, strength);
		}
		public void MoveRight(float strength = 1.0f)
		{
			Translate(Transform.LocalRight, strength);
		}
		public void Jump()
		{
            
            if(Controller.OnGround)
                Controller.Jump();
			//Translate(Transform.LocalUp, strength);
		}
		public void MoveBack(float strength = 1.0f)
		{
			Translate(Transform.LocalForward, -strength);
		}
		public void MoveLeft(float strength = 1.0f)
		{
			Translate(Transform.LocalRight, -strength);
		}
        public void Translate(Vector3 direction, float strength)
        {
            Translate(direction * strength);
        }
        public void Translate(Vector3 translation)
        {
            CharacterGhost.WorldTransform *= Matrix4.CreateTranslation(translation * 0.1f);
        }
		protected override void OnUpdate(float dt)
		{
            Transform.Position = Vector3.Lerp(Transform.Position, CharacterGhost.WorldTransform.ExtractTranslation() + new Vector3(0f, EyeHeight -0.9f, 0f), 0.1f);
		}
	}
}
