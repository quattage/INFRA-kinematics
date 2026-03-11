using System.Collections;
using System.Collections.Generic;
using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;


namespace Assets.quatworks.INFRASEC.Kinematics.Core {

    public interface WrappedCollider {

        abstract void Initialize();

        abstract Vector3 GetCenter();

        /// <summary>
        /// A world-space vector at the center of this Collider's bottom
        /// </summary>
        /// <returns></returns>
        abstract Vector3 GetBottomSurface();

        /// <summary>
        /// CapsuleCollider: Bottom Center
        /// </summary>
        /// <returns></returns>
        abstract ref Vector3 GetPoint0();

        /// <summary>
        /// CapsuleCollider: Top center
        /// </summary>
        /// <returns></returns>
        abstract ref Vector3 GetPoint1();

        /// <summary>
        /// CapsuleCollider: UNUSED
        /// </summary>
        /// <returns></returns>
        abstract Vector3 GetPoint2();

        /// <summary>
        /// CapsuleCollider: Height
        /// </summary>
        /// <returns></returns>
        abstract ref float GetLength0();

        /// <summary>
        /// CapsuleCollider: Radius
        /// </summary>
        /// <returns></returns>
        abstract ref float GetLength1();

        /// <summary>
        /// Returns the raw collider object.
        /// </summary>
        /// <returns></returns>
        abstract Collider GetCollider();

        /// <summary>
        /// Calcuates the relevent world-space points to represent this WrappedCollider.
        /// Effectively "moves" the data in this object to where the collider actually is in the world.
        /// </summary>
        abstract void Recompute();

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() to 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool Cast(ref Vector3 direction, ref float castDistance, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() to 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool Cast(Vector3 direction, float castDistance, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() for up-to-date collider
        /// information. This method takes a center vector, which pretends that the collider
        /// is located there, instaed of where it's actually located.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool CastFrom(Vector3 center, ref Vector3 direction, ref float castDistnace, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() for up-to-date collider
        /// information. This method takes a center vector, which pretends that the collider
        /// is located there, instaed of where it's actually located.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool CastFrom(Vector3 center, Vector3 direction, ref float castDistnace, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() to 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool Cast(Vector3 direction, float castDistance, LayerMask mask, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider. Use UpdatePositions() to 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool Cast(ref Vector3 direction, ref float castDistance, LayerMask mask, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider.
        /// The cast will originate from the center provided, but the collider won't actually move, effectively
        /// pretending that the collider is at the center vector during the cast.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool CastFrom(Vector3 center, ref Vector3 direction, ref float castDistnace, LayerMask mask, out RaycastHit hit);

        /// <summary>
        /// Shape-cast for this collider.
        /// The cast will originate from the center provided, but the collider won't actually move, effectively
        /// pretending that the collider is at the center vector during the cast.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="direction"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        abstract bool CastFrom(Vector3 center, Vector3 direction, ref float castDistnace, LayerMask mask, out RaycastHit hit);

        /// <summary>
        /// Shape-Cast dedicated for the downwards direction. Casts occur along the Y axis, so simplified
        /// shapes can be used. For example, a Y-oriented capsule will cast a sphere here instead of a capsule, 
        /// since the end result is identical. Cast distance is fixed at skinwidth * 2. This method is designed 
        /// to be used for checking ground contact points in kinematic controllers, and returns a list of all 
        /// contacts rather than only the first.
        /// </summary>
        abstract RaycastHit[] CastDownwards();

        /// <summary>
        /// Shape-Cast dedicated for the downwards direction. Casts occur along the Y axis, so simplified
        /// shapes can be used. For example, a Y-oriented capsule will cast a sphere here instead of a capsule, 
        /// since the end result is identical. Cast distance is fixed at skinwidth * 2. This method is designed 
        /// to be used for checking ground contact points in kinematic controllers, and returns a list of all 
        /// contacts rather than only the first.
        /// </summary>
        abstract RaycastHit[] CastDownwards(LayerMask mask);

        /// <summary>
        /// Shape-Cast dedicated for the upwards direction. Casts occur along the Y axis, so simplified
        /// shapes can be used. For example, a Y-oriented capsule will cast a sphere here instead of a capsule, 
        /// since the end result is identical. Cast distance is fixed at skinwidth * 2. This method is designed 
        /// to be used for checking ceiling contact points in kinematic controllers, and returns a list of all 
        /// contacts rather than only the first.
        /// </summary>
        abstract RaycastHit[] CastUpwards();

        /// <summary>
        /// Shape-Cast dedicated for the upwards direction. Casts occur along the Y axis, so simplified
        /// shapes can be used. For example, a Y-oriented capsule will cast a sphere here instead of a capsule, 
        /// since the end result is identical. Cast distance is fixed at skinwidth * 2. This method is designed 
        /// to be used for checking ceiling contact points in kinematic controllers, and returns a list of all 
        /// contacts rather than only the first.
        /// </summary>
        abstract RaycastHit[] CastUpwards(LayerMask mask);

        /// <summary>
        /// Shape-Cast dedicated for dedecting walls to the local left of the provided mover.
        /// </summary>
        abstract RaycastHit[] CastLeft(Movable mover);

        /// <summary>
        /// Shape-Cast dedicated for dedecting walls to the local left of the provided mover.
        /// </summary>
        abstract RaycastHit[] CastLeft(Movable mover, LayerMask mask);

        /// <summary>
        /// Shape-Cast dedicated for dedecting walls to the local left of the provided mover.
        /// </summary>
        abstract RaycastHit[] CastRight(Movable mover);

        /// <summary>
        /// Shape-Cast dedicated for dedecting walls to the local left of the provided mover.
        /// </summary>
        abstract RaycastHit[] CastRight(Movable mover, LayerMask mask);


        /// <summary>
        /// Returns TRUE if this collider is within a very small threshold 
        /// distance to any other colliders.
        /// </summary>
        /// <returns></returns>
        abstract bool IsTouchingAnything();

        /// <summary>
        /// Returns TRUE if this collider is within a very small threshold 
        /// distance to any other colliders.
        /// </summary>
        /// <returns></returns>
        abstract bool IsTouchingAnything(LayerMask mask);

        /// <summary>
        /// Draws editor debug gizmos to visualize the current in-world location of this WrappedCapsule.
        /// The gizmo drawn may not line up with the collider's actual position. Calls to Recompute() will
        /// fix this.
        /// </summary>
        abstract void DrawGizmo();

        abstract bool ShrinkVertical(float percent, float speed);

        abstract bool ShrinkVertical(float percent, float speed, LayerMask mask);


        abstract float GetSizePercent();
    }

    [System.Serializable]
    public class WrappedCapsule : WrappedCollider {

        [SerializeField] private CapsuleCollider _collider;

        private Vector3 _bottom;  // point0
        private Vector3 _top;     // point1
        private float _height;    // length0
        private float _radius;    // length1

        private Vector3 _originalCenter;
        private float _originalHeight;
        private float _shrinkPercent = 1;

        public WrappedCapsule() {  }

        public void Initialize() {
            Recompute();
            _originalHeight = _height;
            _originalCenter = _collider.center;
        }

        public Vector3 GetCenter() {
            return _collider.transform.position + _collider.center;
        }

        public Vector3 GetBottomSurface() {
            Vector3 orient = new Vector3 {[_collider.direction] = 1};
            orient *= _radius;
            return _bottom - orient;
        }

        public Collider GetCollider() {
            return _collider;
        }

        public ref float GetLength0() {
            return ref _height;
        }

        public ref float GetLength1() {
            return ref _radius;
        }

        public ref Vector3 GetPoint0() {
            return ref _bottom;
        }

        public ref Vector3 GetPoint1() {
            return ref _top;
        }

        public Vector3 GetPoint2() {
            return Vector3.zero;
        }

        public void Recompute() {

            Vector3 center = _collider.transform.TransformPoint(_collider.center);
            Vector3 approxScale = _collider.transform.lossyScale.Absolute();
            Vector3 orient = Vector3.zero;

            switch (_collider.direction) {
                case 0:
                    _radius = Mathf.Max(approxScale.y, approxScale.z) * _collider.radius;
                    _height = approxScale.x * _collider.height;
                    orient = _collider.transform.TransformDirection(Vector3.right);
                    break;
                case 1: 
                    _radius = Mathf.Max(approxScale.x, approxScale.z) * _collider.radius;
                    _height = approxScale.y * _collider.height;
                    orient = _collider.transform.TransformDirection(Vector3.up);
                    break;
                case 2:
                    _radius = Mathf.Max(approxScale.x, approxScale.y) * _collider.radius;
                    _height = approxScale.z * _collider.height;
                    orient = _collider.transform.TransformDirection(Vector3.forward);
                    break;
            }

            _top = center + orient * (_height * 0.5f - _radius);
            _bottom = center - orient * (_height * 0.5f - _radius);
        }

        public bool Cast(Vector3 direction, float castDistance, out RaycastHit hit) {
            return Cast(ref direction, ref castDistance, ~0, out hit);
        }

        public bool Cast(ref Vector3 direction, ref float castDistance, out RaycastHit hit) {
            return Cast(ref direction, ref castDistance, ~0, out hit);
        }

        public bool Cast(Vector3 direction, float castDistance, LayerMask mask, out RaycastHit hit) {
            return Cast(ref direction, ref castDistance, mask, out hit);
        }

        public bool Cast(ref Vector3 direction, ref float castDistance, LayerMask mask, out RaycastHit hit) {
            return Physics.CapsuleCast(
                _bottom, 
                _top, 
                _radius, 
                direction, 
                out hit, 
                castDistance,
                mask
            );
        }

        public bool CastFrom(Vector3 center, ref Vector3 direction, ref float castDistance, out RaycastHit hit) {
            return CastFrom(center, ref direction, ref castDistance, ~0, out hit);
        }

        public bool CastFrom(Vector3 center, ref Vector3 direction, ref float castDistance, LayerMask mask, out RaycastHit hit) {

            center += _collider.center;
            Vector3 orient = Vector3.zero;

            switch (_collider.direction) {
                case 0:
                    orient = _collider.transform.TransformDirection(Vector3.right);
                    break;
                case 1: 
                    orient = _collider.transform.TransformDirection(Vector3.up);
                    break;
                case 2:
                    orient = _collider.transform.TransformDirection(Vector3.forward);
                    break;
            }

            return Physics.CapsuleCast(
                center + orient * (_height * 0.5f - _radius), 
                center - orient * (_height * 0.5f - _radius),
                _radius, 
                direction, 
                out hit, 
                castDistance,
                mask
            );
        }

        public bool CastFrom(Vector3 center, Vector3 direction, ref float castDistance, out RaycastHit hit) {
            return CastFrom(center, direction, ref castDistance, ~0, out hit);
        }

        public bool CastFrom(Vector3 center, Vector3 direction, ref float castDistance, LayerMask mask, out RaycastHit hit) {

            center += _collider.center;
            Vector3 orient = Vector3.zero;

            switch (_collider.direction) {
                case 0:
                    orient = _collider.transform.TransformDirection(Vector3.right);
                    break;
                case 1: 
                    orient = _collider.transform.TransformDirection(Vector3.up);
                    break;
                case 2:
                    orient = _collider.transform.TransformDirection(Vector3.forward);
                    break;
            }

            return Physics.CapsuleCast(
                center + orient * (_height * 0.5f - _radius), 
                center - orient * (_height * 0.5f - _radius),
                _radius, 
                direction, 
                out hit, 
                castDistance,
                mask
            );
        }

        public RaycastHit[] CastDownwards() {
            return CastDownwards(~0);
        }

        public RaycastHit[] CastDownwards(LayerMask mask) {
            return Physics.SphereCastAll(_bottom, _radius, Vector3.down, INFRA.Game.phys_skinwidth.GetFloat() * 2f, mask);
        }

        public RaycastHit[] CastUpwards() {
            return CastUpwards(~0);
        }

        public RaycastHit[] CastUpwards(LayerMask mask) {
            return Physics.SphereCastAll(_top, _radius, Vector3.up, INFRA.Game.phys_skinwidth.GetFloat() * 2f, mask);
        }

        public void DrawGizmo() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_top, _radius);
            Gizmos.DrawWireSphere(_bottom, _radius);
            Gizmos.DrawWireCube(_collider.transform.position + _collider.center, new Vector3(GetLength1(), _top.y - _bottom.y, GetLength1()));
        }

        public bool ShrinkVertical(float percent, float speed) {
            return ShrinkVertical(percent, speed, ~0);
        }

        public bool ShrinkVertical(float percent, float speed, LayerMask mask) {

            float oldPercent = _shrinkPercent;
            _shrinkPercent = Mathf.Lerp(_shrinkPercent, percent, speed * Time.fixedDeltaTime);
            float newHeight = _originalHeight * _shrinkPercent;
            float heightDiff = _collider.height - newHeight;

            Vector3 orient = new Vector3 {[_collider.direction] = 1};
            Vector3 localTop = _originalCenter + (orient * ((_originalHeight * 0.5f) - _collider.radius));

            if(heightDiff < 0) {
                Collider[] headBonks = Physics.OverlapSphere(
                    _collider.transform.TransformPoint(localTop),
                    _radius - INFRA.Game.phys_skinwidth.GetFloat(),
                    mask
                );
                if(!headBonks.IsNullOrEmpty()) {

                    if(Mathf.Abs(_shrinkPercent - percent) < 0.01f) {
                        _shrinkPercent = oldPercent;
                        return false;
                    }

                    _shrinkPercent = oldPercent;
                    return false;
                }
            }

            _collider.height = newHeight;
            _collider.center = new Vector3(_collider.center.x, _collider.center.y - heightDiff / 2, _collider.center.z);

            if(Mathf.Abs(_shrinkPercent - percent) < 0.01f) {
                _shrinkPercent = percent;
                return true;
            }

            return false;
        }

        public float GetSizePercent() {
            return _shrinkPercent;
        }

        public RaycastHit[] CastLeft(Movable mover) {
            return CastLeft(mover, ~0);
        }

        public RaycastHit[] CastLeft(Movable mover, LayerMask mask) {
            Vector3 dir = mover.GetRootRotation() * Vector3.left;
            return Physics.SphereCastAll(Vector3.Lerp(_top, _bottom, 0.5f), _radius - INFRA.Game.phys_skinwidth.GetFloat(), dir, INFRA.Game.phys_skinwidth.GetFloat() * 4f, mask);
        }

        public RaycastHit[] CastRight(Movable mover) {
            return CastRight(mover, ~0);
        }

        public RaycastHit[] CastRight(Movable mover, LayerMask mask) {
            Vector3 dir = mover.GetRootRotation() * Vector3.right;
            return Physics.SphereCastAll(Vector3.Lerp(_top, _bottom, 0.5f), _radius - INFRA.Game.phys_skinwidth.GetFloat(), dir, INFRA.Game.phys_skinwidth.GetFloat() * 4f, mask);
        }


        public bool IsTouchingAnything() {
            return IsTouchingAnything(~0);
        }

        public bool IsTouchingAnything(LayerMask mask) {
            return !Physics.OverlapSphere(Vector3.Lerp(_top, _bottom, 0.5f), _radius + INFRA.Game.phys_skinwidth.GetFloat() * 2, mask).IsNullOrEmpty();
        }
    }




    /// <summary>
    /// A ContactPatch is a collection of RaycastHits describing the conditions
    /// immediately surrounding a MovingElement's collider.
    /// </summary>
    public abstract class ContactPatch : IEnumerable<RaycastHit> {

        public IEnumerator<RaycastHit> GetEnumerator() { return Points.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return Points.GetEnumerator(); }

        public abstract List<RaycastHit> Points { get; }
        public abstract void Clear();

        public abstract void MarkLeftWall(ref RaycastHit hit, float contactAngle);
        public abstract void MarkRightWall(ref RaycastHit hit, float contactAngle);
        public abstract void MarkGround(ref RaycastHit hit, float contactAngle);
        public abstract void MarkCeiling(ref RaycastHit hit, float contactAngle);

        public abstract Vector3 GetLastGroundPoint();
        public abstract RaycastHit? GetGround();
        public abstract float GetGroundAngle();
        public abstract void ForgetGround();
        public abstract float GetExperiencedFriction();
        public abstract Vector3 GetGroundSurfaceNormal();

        public abstract RaycastHit? GetCeiling();
        public abstract void ForgetCeiling();
        public abstract float GetCeilingAngle();

        public abstract bool IsTouchingGround();
        public abstract bool IsGroundedFirmly();
        public abstract bool IsSliding();
        
        public abstract bool IsTouchingCeiling();
        public abstract bool IsTouchingWall();

        public abstract RaycastHit? GetLeftWall();
        public abstract bool IsTouchingLeftWall();
        public abstract void ForgetLeftWall();
        public abstract float GetLeftWallAngle();

        public abstract RaycastHit? GetRightWall();
        public abstract bool IsTouchingRightWall();
        public abstract void ForgetRightWall();
        public abstract float GetRightWallAngle();

        public abstract void DrawGizmos();
    }


    /// <summary>
    /// An EmptyContactor is a stand-in for an implementing contact patch
    /// MovingElements that use empty contactors are basically just saying
    /// "I do not need collisions" so they always return false or null
    /// when querying contact information.
    /// </summary>
    public class EmptyContacter : ContactPatch {
        
        public override List<RaycastHit> Points { get => null; }

        public override void Clear() { return; }

        public override void MarkLeftWall(ref RaycastHit hit, float contactAngle) { return; }
        public override void MarkRightWall(ref RaycastHit hit, float contactAngle) { return; }
        public override void MarkGround(ref RaycastHit hit, float contactAngle) { return; }
        public override void MarkCeiling(ref RaycastHit hit, float contactAngle) { return; }

        public override RaycastHit? GetGround() { return null; }
        public override Vector3 GetLastGroundPoint() { return Vector3.zero; }
        public override float GetGroundAngle() { return 0; } 
        public override void ForgetGround() { return; }
        public override float GetExperiencedFriction() { return 0f; }
        public override Vector3 GetGroundSurfaceNormal() { return Vector3.zero; }

        public override RaycastHit? GetCeiling() { return null; }
        public override float GetCeilingAngle() { return 0; } 
        public override void ForgetCeiling() { return; }

        public override bool IsTouchingGround() { return false; }
        public override bool IsSliding() { return false; }
        public override bool IsGroundedFirmly() { return false; }

        public override bool IsTouchingCeiling() { return false; }
        public override bool IsTouchingWall() { return false; }

        public override RaycastHit? GetLeftWall() { return null; }
        public override bool IsTouchingLeftWall() { return false; }
        public override void ForgetLeftWall() { return; }
        public override float GetLeftWallAngle() { return 0; }
        public override RaycastHit? GetRightWall() { return null; }
        public override bool IsTouchingRightWall() { return false; }
        public override void ForgetRightWall() { return; }
        public override float GetRightWallAngle() { return 0; }

        public override void DrawGizmos() { return; }
    }

    public class ContinuousContactPatch : ContactPatch {

        private RaycastHit? _ground = null;
        private float _groundAngle = 0f;
        private Vector3 _lastGroundPoint = Vector3.zero;

        private RaycastHit? _ceiling = null;
        private float _ceilingAngle = 0f;

        private RaycastHit? _leftWall = null;
        private float _leftWallAngle = 0f;

        private RaycastHit? _rightWall = null;
        private float _rightWallAngle = 0f;

        public override List<RaycastHit> Points { get => MakePointsList(); }

        private List<RaycastHit> MakePointsList() { 
            List<RaycastHit> output = new();
            if(_ground.HasValue) output.Add(_ground.Value);
            if(_ceiling.HasValue) output.Add(_ceiling.Value);
            if(_leftWall.HasValue) output.Add(_leftWall.Value);
            if(_rightWall.HasValue) output.Add(_rightWall.Value);
            return output;
        }

        public override void Clear() { 
            ForgetGround();
            ForgetCeiling(); 
            ForgetLeftWall();
            ForgetRightWall();
        }

        public override void MarkLeftWall(ref RaycastHit hit, float contactAngle) { 
            _leftWall = hit;
            _leftWallAngle = contactAngle;
        }

        public override void MarkRightWall(ref RaycastHit hit, float contactAngle) { 
            _rightWall = hit;
            _rightWallAngle = contactAngle;
        }

        public override void MarkGround(ref RaycastHit hit, float contactAngle) { 
            _ground = hit;
            _groundAngle = contactAngle;
            _lastGroundPoint = hit.point;
        }

        public override void MarkCeiling(ref RaycastHit hit, float contactAngle) { 
            _ceiling = hit;
            _ceilingAngle = contactAngle;
        }

        public override float GetExperiencedFriction() {
            if(IsTouchingGround()) return INFRA.Game.phys_groundfriction.GetFloat();
            return INFRA.Game.phys_airfriction.GetFloat();
        }

        public override Vector3 GetGroundSurfaceNormal() {        
            if(IsTouchingGround()) return _ground.Value.normal;
            else return INFRA.Game.GravityNormal;
        }

        public override RaycastHit? GetGround() { return _ground; }
        public override Vector3 GetLastGroundPoint() { return _lastGroundPoint; }
        public override void ForgetGround() { _ground = null; _groundAngle = 0f; }
        public override float GetGroundAngle() { return _groundAngle; } 

        public override RaycastHit? GetCeiling() { return _ceiling; }
        public override void ForgetCeiling() { _ceiling = null; _ceilingAngle = 0f; }
        public override float GetCeilingAngle() { return _ceilingAngle; } 

        public override bool IsTouchingGround() { return _ground != null; }
        public override bool IsSliding() { return _groundAngle > INFRA.Game.move_maxslope.GetFloat(); }
        public override bool IsGroundedFirmly() { return false; }

        public override bool IsTouchingCeiling() { return _ceiling != null; }
        public override bool IsTouchingWall() { return IsTouchingLeftWall() || IsTouchingRightWall(); }

        public override RaycastHit? GetLeftWall() { return _leftWall; }
        public override bool IsTouchingLeftWall() { return _leftWall.HasValue; }
        public override void ForgetLeftWall() { _leftWall = null; _leftWallAngle = 0;}
        public override float GetLeftWallAngle() { return _leftWallAngle; }

        public override RaycastHit? GetRightWall() { return _rightWall; }
        public override bool IsTouchingRightWall() { return _rightWall.HasValue; }
        public override void ForgetRightWall() { _rightWall = null; _leftWallAngle = 0;}
        public override float GetRightWallAngle() { return _rightWallAngle; }

        public override void DrawGizmos() {
            if(_ground.HasValue) Draw(_ground.Value, Color.red, Color.white);
            if(_ceiling.HasValue) Draw(_ceiling.Value, Color.blue, Color.white);
            foreach(RaycastHit hit in Points) Draw(hit, Color.magenta, Color.yellow);
        }

        private void Draw(RaycastHit hit, Color circle, Color line) {
            Gizmos.color = circle;
            Gizmos.DrawWireSphere(hit.point, 0.25f);
            Gizmos.color = line;
            Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
        }
    }
}
