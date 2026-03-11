using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.Core {

    
    /// <summary>
    /// A MotionStack represents a single standalone kinematic
    /// or rigidbody-based movement implementation for anything - Vehicles,
    /// Players, etc. <para/>
    /// </summary>
    public abstract class MotionStack : MonoBehaviour {

        /// <summary>
        /// Called when the composing Mover is destroyed.
        /// </summary>
        public abstract void OnDestroy();
        

        /// <summary>
        /// A Unique ID for this MotionStack - Used for addressing
        /// this MotionStack by name.
        /// </summary>
        /// <returns></returns>
        internal abstract string GetID();

        /// <summary>
        /// Translation in all 3 axes. Useful for moving horizontally or flying.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnTranslate(ref Vector3 amount, MovingElement mover);

        /// <summary>
        /// A dedicated upward translation for jumping. The amount here
        /// will always be positive when passed.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnTranslateUp(float amount, MovingElement mover);


        /// <summary>
        /// A dedicated downward translation for crouching. The amount here
        /// will always be negative when passed.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnTranslateDown(float amount, MovingElement mover);

        /// <summary>
        /// Rotation of the mouse or gamepad joystick.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnRotate(ref Vector3 amount, MovingElement mover);

        /// <summary>
        /// Rotation of the mouse or gamepad joystick.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnRotate(ref Quaternion amount, MovingElement mover);

        /// <summary>
        /// An auxilury input intended to modify the magnitude of translation.
        /// Useful for sprinting or dodging
        /// </summary>
        /// <param name="amount"></param>
        public abstract void OnImpulse(float amount, MovingElement mover);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetDesiredMovement();

        public override bool Equals(object obj) {
            if(!(obj is MotionStack other)) return false;
            return GetID().Equals(other.GetID());
        }

        public override int GetHashCode() {
            return GetID().GetHashCode();
        }

        public override string ToString() {
            return $"MovementMotor '{GetID()}'";
        }

        /// <summary>
        /// Called by instantiating composer during Unity's Update().
        /// </summary>
        internal abstract void OnUpdate(MovingElement mover);

        /// <summary>
        /// Called by instantiating composer during Unity's FixedUpdate().
        /// </summary>
        internal abstract void ExecuteMotionStack(MovingElement mover);

        /// <summary>
        /// Called whenever implementing MovingELements switch to this MovementMotor
        /// <para/> Can be used as if it were a constructor, but this method can be called
        /// any number of times throughout this object's lifespan.
        /// </summary>
        internal abstract void OnActivate(MovingElement initializer);

        /// <summary>
        /// Called whenever implementing MovingELements switch from this
        /// MovementMotor to another. This call is made before the switch
        /// is complete. A subsequent call to OnActivate is made to the
        /// MovingMotor destination once a switch is made.
        /// </summary>
        internal abstract void OnDeactivate(MovingElement deinitializer, MotionStack subsequent);

        public static MotionStack[] Populate(MovingElement mover) {
            MotionStack[] motors = mover.GetComponents<MotionStack>();
            if(!motors.IsNullOrEmpty()) return motors;
            Debug.LogWarning($"MovingElement '{mover.name}' was found to have no MovementMotors - A static MovementMotor was added as a stand-in.");
            MotionStack staticMotor = mover.gameObject.AddComponent<NullMotionStack>();
            return new MotionStack[1] {
                staticMotor
            };
        }
    }


    /// <summary>
    /// A "null" motor with no implementation to avoid actual nulls
    /// </summary>
    public class NullMotionStack : MotionStack {
        internal override string GetID() { return "NULL"; }
        public override void OnImpulse(float amount, MovingElement mover) {}
        public override void OnRotate(ref Vector3 amount, MovingElement mover) {}
        public override void OnRotate(ref Quaternion amount, MovingElement mover) {}
        public override void OnTranslate(ref Vector3 amount, MovingElement mover) {}
        public override void OnTranslateUp(float amount, MovingElement mover) {}
        public override void OnTranslateDown(float amount, MovingElement mover) {}
        internal override void OnUpdate(MovingElement mover) {}
        internal override void ExecuteMotionStack(MovingElement mover) {}
        internal override void OnActivate(MovingElement initializer) {}
        internal override void OnDeactivate(MovingElement deinitializer, MotionStack subsequent) {}
        public override Vector3 GetDesiredMovement() { return Vector3.zero; }
        public override void OnDestroy() {}
    }

    /// <summary>
    /// A locked motor that only provides implementation for rotation movements.
    /// </summary>
    public class TurretMotionStack : MotionStack {
        internal override string GetID() { return "turret"; }
        public override void OnImpulse(float amount, MovingElement mover) {}
        public override void OnTranslate(ref Vector3 amount, MovingElement mover) {}
        public override void OnTranslateUp(float amount, MovingElement mover) {}
        public override void OnTranslateDown(float amount, MovingElement mover) {}
        // TODO IMPL
        public override void OnRotate(ref Vector3 amount, MovingElement mover) {}
        public override void OnRotate(ref Quaternion amount, MovingElement mover) {}
        internal override void OnUpdate(MovingElement mover) {}
        internal override void ExecuteMotionStack(MovingElement mover) {}
        internal override void OnActivate(MovingElement initializer) {}
        internal override void OnDeactivate(MovingElement deinitializer, MotionStack subsequent) {}
        public override Vector3 GetDesiredMovement() { return Vector3.zero; }
        public override void OnDestroy() {}
    }

    public interface IMotionStackCallbacks {
        public abstract void OnJump(JumpContext ctx);
        public abstract void WhileInAir();
        public abstract void OnLand();

        public abstract void OnWallrunEnter(WallRunContext ctx);
        public abstract void WhileWallrunPerformed(WallRunContext ctx);
        public abstract void OnWallrunExit();

        public abstract void OnCrouchEnter();
        public abstract void WhileCrouchPerformed();
        public abstract void OnCrouchExit();

        public abstract void OnSlideEnter();
        public abstract void WhileSlidePerformed();
        public abstract void OnSlideExit();
    }


    public class WallRunContext {
        public Quaternion? InitialRotation;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 LookProj;
        public float Alignment;
        public bool IsLeft;
        public float Time;
        public float EvalTime;

        public WallRunContext(ref Quaternion initRot, ref Vector3 point, ref Vector3 normal, ref Vector3 tangent, ref Vector3 lookProj, ref float alignment, bool isLeft, ref float time) {
            InitialRotation = initRot;
            Point = point;
            Normal = normal;
            Tangent = tangent;
            LookProj = lookProj;
            Alignment = alignment;
            IsLeft = isLeft;
            Time = time;
            EvalTime = 1 - time;
        }
    }

    public class JumpContext {
        public bool IsDouble;
        public JumpContext(bool isDouble) {
            IsDouble = isDouble;
        }
    }
}
