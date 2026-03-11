
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics {

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicController : MovingElement {

        [SerializeField] protected Rigidbody _rigidbody;
        protected Vector3 _velocity = Vector3.zero;

        [SerializeField] protected WrappedCapsule _hitbox = new();
        protected float _speed;

        public override void SetPosition(Vector3 position) {
            _rigidbody.MovePosition(position);
        }

        public override void SetPosition() {
            _rigidbody.MovePosition(new Vector3(0, 0, 0));
        }

        public override Vector3 GetPosition() {
            return transform.position;
        }

        public override void Initialize() {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _hitbox.Recompute();
            _contact = new ContinuousContactPatch();
        }

        public override void SetVelocity(Vector3 velocity) {
            _velocity = velocity;
            _speed = _velocity.magnitude / INFRA.Game.GetFixedDelta();
            if(Mathf.Abs(_speed) < 0.001f) _speed = 0;
        }

        public override void SetVelocity(float vX, float vY, float vZ) {
            _velocity = new Vector3(vX, vY, vZ);
            _speed = _velocity.magnitude / INFRA.Game.GetFixedDelta();
            if(Mathf.Abs(_speed) < 0.001f) _speed = 0;
        }

        public override float GetSpeed() {
            return _speed;
        }

        public override Vector3 GetVelocity() {
            return _velocity;
        }

        protected override void OnFixedUpdate() {
            _rigidbody.MovePosition(transform.position + _velocity);
        }

        protected override void OnUpdate() {
            
        }

        public override WrappedCollider GetCollider() {
            return _hitbox;
        }

        public override string ToString() {
            return $"KinematicController '{_rigidbody.transform.name}'";
        }
    }
}