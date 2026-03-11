using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.Core {

    /// <summary>
    /// A ROTATATRON!!! is a transform that rotates as a child of a Movable.
    /// Rotatatrons are used for storing rotations such as the head of a character
    /// looking up/down.
    /// </summary>
    [System.Serializable]
    public class Rotatatron {

        [field:SerializeField] public Transform Transform { get; private set; }
        [SerializeField] private ClampedLockedRotation _rule;

        public Quaternion GetRotation() {
            return Transform.localRotation;
        }

        public Quaternion GetGlobalRotation() {
            return Transform.rotation;
        }

        public Vector3 GetEulerAngles() {
            return Transform.localEulerAngles;
        }

        public override string ToString() {
            return $"Rotatron '{Transform.name}' :: <br>" + _rule;
        }

        /// <summary>
        /// Modify this rotation by the given quaternion
        /// </summary>
        /// <param name="rDelta"></param>
        public void Rotate(ref Quaternion rDelta) {
            _rule.Rotate(Transform, ref rDelta);
        }

        /// <summary>
        /// Modify this rotation by the given deltas
        /// </summary>
        /// <param name="rDelta"></param>
        public void Rotate(float rDeltaX, float rDeltaY, float rDeltaZ) {
            _rule.Rotate(Transform, rDeltaX, rDeltaY, rDeltaZ);
        }

        /// <summary>
        /// Modify this rotation by the given euler angle
        /// </summary>
        /// <param name="rDelta"></param>
        public void Rotate(ref Vector3 rDelta) {
            _rule.Rotate(Transform, ref rDelta);
        }

        /// <summary>
        /// Set this rotation to the given quaternion
        /// </summary>
        /// <param name="rDelta"></param>
        public void SetRotation(ref Quaternion rotation) {
            _rule.SetRotation(Transform, ref rotation);
        }

         /// <summary>
        /// Set this rotation to the given quaternion
        /// </summary>
        /// <param name="rDelta"></param>
        public void SetGlobalRotation(ref Quaternion rotation) {
            _rule.SetGlobalRotation(Transform, ref rotation);
        }

        /// <summary>
        /// Set this rotation to the given euler angle
        /// </summary>
        /// <param name="rDelta"></param>
        public void SetRotation(ref Vector3 rotation) {
            _rule.SetRotation(Transform, ref rotation);
        }

        /// <summary>
        /// Unlocks all rotation axes. Does NOT alter the clamping policy
        /// of this ConstrainedRotation. Individual axis locks persist when a
        /// ConstrainedRotation is enabled.
        /// </summary>
        public void Enable() {
            _rule.Enable();
        }
        /// <summary>
        /// Locks all rotation axes, completely disallowing any rotation. Does NOT 
        /// alter the clamping policy of this ConstrainedRotation.
        /// </summary>
        public void Disable() {
            _rule.Disable();
        }
        /// <summary>
        /// Locks the X axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public void LockX() {
            _rule.LockX();
        }
        /// <summary>
        /// Locks the Y axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public void LockY() {
            _rule.LockY();
        }
        /// <summary>
        /// Locks the Z axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public void LockZ() {
            _rule.LockZ();
        }
        /// <summary>
        /// Frees rotation on the X axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public void FreeX() {
            _rule.FreeX();
        }
        /// <summary>
        /// Frees rotation on the Y axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public void FreeY() {
            _rule.FreeY();
        }
        /// <summary>
        /// Frees rotation on the Z axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public void FreeZ() {
            _rule.FreeZ();
        }
    }





    /// <summary>
    /// ConstrainedRotation objects allow for editor-configurable
    /// rules that transforms must follow when rotating as children
    /// of a MovingElement object.
    /// </summary>
    public interface ConstrainedRotation {

        /// <summary>
        /// Adds the supplied rotation to this ConstrainedRotation's current transform rotation.
        /// </summary>
        /// <returns></returns>
        public Quaternion Rotate(in Transform parent, ref Quaternion rDelta);

        /// <summary>
        /// Adds the supplied rotation to this ConstrainedRotation's current transform rotation.
        /// </summary>
        /// <returns></returns>
        public Vector3 Rotate(in Transform parent, ref Vector3 rDelta);

        /// <summary>
        /// Adds the supplied rotation to this ConstrainedRotation's current transform rotation.
        /// </summary>
        /// <returns></returns>
        public Vector3 Rotate(in Transform parent, float rDeltaX, float rDeltaY, float rDeltaZ);

        /// <summary>
        /// Sets this ConstrainedRotation's current transform rotation to the supplied absolute quaternion.
        /// </summary>
        /// <returns></returns>
        public void SetRotation(in Transform parent, ref Quaternion rotation);  

        /// <summary>
        /// Sets this ConstrainedRotation's current transform rotation to the supplied absolute euler angle.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public void SetRotation(in Transform parent, ref Vector3 rotation);

        /// <summary>
        /// Unlocks all rotation axes. Does NOT alter the clamping policy
        /// of this ConstrainedRotation. Individual axis locks persist when a
        /// ConstrainedRotation is enabled.
        /// </summary>
        public abstract void Enable();

        /// <summary>
        /// Locks all rotation axes, completely disallowing any rotation. Does NOT 
        /// alter the clamping policy of this ConstrainedRotation.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Locks the X axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public abstract void LockX();
        /// <summary>
        /// Locks the Y axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public abstract void LockY();
        /// <summary>
        /// Locks the Z axis. Alters the clamping policy of this ConstrainedRotation.
        /// </summary>
        public abstract void LockZ();
        /// <summary>
        /// Frees rotation on the X axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public abstract void FreeX();
        /// <summary>
        /// Frees rotation on the Y axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public abstract void FreeY();
        /// <summary>
        /// Frees rotation on the Z axis.
        /// <para/> Note: Individual axis locks will enable/disable
        /// singular axis rotation, but calls will NOT alter the clamping
        /// policy. 
        /// </summary>
        public abstract void FreeZ();
    }

    [System.Serializable]
    public class UnclampedLockedRotation : ConstrainedRotation {

        [SerializeField] private bool _canRotate = true;
        [SerializeField] private bool _doX = true;
        [SerializeField] private bool _doY = true;
        [SerializeField] private bool _doZ = true;

        public Quaternion Rotate(in Transform parent, ref Quaternion rDelta) {
            if(!_canRotate) return parent.rotation;
            if(!ContainsLocks()) {
                parent.rotation = rDelta;
                return parent.rotation;
            }

            Vector3 preRotation = parent.localEulerAngles;
            parent.rotation *= rDelta;

            Vector3 clamped = parent.localEulerAngles;
            if(!_doX) clamped.x = preRotation.x;
            if(!_doY) clamped.y = preRotation.y;
            if(!_doZ) clamped.z = preRotation.z;
            parent.localEulerAngles = clamped;

            return parent.rotation;
        }

        public Vector3 Rotate(in Transform parent, ref Vector3 rDelta) {
            if(!_canRotate) return parent.localEulerAngles;
            if(!_doX) rDelta.x = 0;
            if(!_doY) rDelta.y = 0;
            if(!_doZ) rDelta.z = 0;
            parent.rotation *= Quaternion.Euler(rDelta);
            return parent.localEulerAngles;
        }

        public Vector3 Rotate(in Transform parent, float rDeltaX, float rDeltaY, float rDeltaZ) {
            if(!_canRotate) return parent.localEulerAngles;
            if(!_doX) rDeltaX = 0;
            if(!_doY) rDeltaY = 0;
            if(!_doZ) rDeltaZ = 0;
            parent.rotation *= Quaternion.Euler(rDeltaX, rDeltaY, rDeltaZ);
            return parent.localEulerAngles;
        }

        public void SetRotation(in Transform parent, ref Quaternion rotation) {

            if(!_canRotate) _canRotate = true;
            if(!ContainsLocks()) {
                parent.rotation = rotation;
                return;
            }

            Vector3 preRotation = parent.localEulerAngles;
            parent.rotation *= rotation;

            Vector3 clamped = parent.localEulerAngles;
            if(!_doX) clamped.x = preRotation.x;
            if(!_doY) clamped.y = preRotation.y;
            if(!_doZ) clamped.z = preRotation.z;
            parent.localEulerAngles = clamped;
        }

        public void SetRotation(in Transform parent, ref Vector3 rotation) {
            if(!_canRotate) _canRotate = true;
            if(!_doX) rotation.x = 0;
            if(!_doY) rotation.y = 0;
            if(!_doZ) rotation.z = 0;
            parent.rotation = Quaternion.Euler(rotation);
        }

        private bool ContainsLocks() {
            if(!_doX) return true;
            if(!_doY) return true;
            if(!_doZ) return true;
            return false;
        }

        public void Enable() {
            _canRotate = true;
        }

        public void Disable() {
            _canRotate = false;
        }

        public void LockX() {
            _doX = false;
        }

        public void LockY() {
            _doY = false;
        }

        public void LockZ() {
            _doZ = false;
        }

        public void FreeX() {
            _doX = true;
        }

        public void FreeY() {
            _doY = true;
        }

        public void FreeZ() {
            _doZ = true;
        }
    }




    [System.Serializable]
    public class ClampedLockedRotation : ConstrainedRotation {

        [SerializeField] private bool _canRotate;
        [SerializeField] private AxisRule[] _axes = new AxisRule[3];

        public Quaternion Rotate(in Transform parent, ref Quaternion rDelta) {
            if(!_canRotate) return parent.rotation;

            // just in case this rotation doesn't lock, to avoid gimbal lock when converting to/from eulers
            if(!ContainsLocks()) {
                parent.rotation = rDelta;
                return rDelta;
            }

            parent.rotation *= rDelta;

            Vector3 clamped = parent.localEulerAngles;
            clamped.x = _axes[0].Clamp(clamped.x);
            clamped.y = _axes[1].Clamp(clamped.y);
            clamped.z = _axes[2].Clamp(clamped.z);
            parent.localEulerAngles = clamped;

            return parent.rotation;
        }

        public Vector3 Rotate(in Transform parent, ref Vector3 rDelta) {
            if(!_canRotate) return parent.localEulerAngles;
            rDelta.x = _axes[0].EvaluateDelta(parent.localEulerAngles.x, rDelta.x);
            rDelta.y = _axes[1].EvaluateDelta(parent.localEulerAngles.y, rDelta.y);
            rDelta.z = _axes[2].EvaluateDelta(parent.localEulerAngles.z, rDelta.z);
            parent.rotation *= Quaternion.Euler(rDelta);
            return parent.localEulerAngles;
        }

        public Vector3 Rotate(in Transform parent, float rDeltaX, float rDeltaY, float rDeltaZ) {
            if(!_canRotate) return parent.localEulerAngles;
            rDeltaX = _axes[0].EvaluateDelta(parent.localEulerAngles.x, rDeltaX);
            rDeltaY = _axes[1].EvaluateDelta(parent.localEulerAngles.y, rDeltaY);
            rDeltaZ = _axes[2].EvaluateDelta(parent.localEulerAngles.z, rDeltaZ);
            parent.rotation *= Quaternion.Euler(rDeltaX, rDeltaY, rDeltaZ);
            return parent.localEulerAngles;
        }

        public void SetRotation(in Transform parent, ref Quaternion rotation) {
            if(!_canRotate) _canRotate = true;
            if(!ContainsLocks()) {
                parent.rotation = rotation;
                return;
            }

            parent.rotation *= rotation;

            Vector3 clamped = parent.localEulerAngles;
            clamped.x = _axes[0].Clamp(clamped.x);
            clamped.y = _axes[1].Clamp(clamped.y);
            clamped.z = _axes[2].Clamp(clamped.z);
            parent.localEulerAngles = clamped;
        }

        public void SetGlobalRotation(in Transform parent, ref Quaternion rotation) {
            if(!_canRotate) _canRotate = true;
            if(!ContainsLocks()) {
                parent.rotation = rotation;
                return;
            }

            parent.rotation *= rotation;

            Vector3 clamped = parent.localEulerAngles;
            clamped.x = _axes[0].Clamp(clamped.x);
            clamped.y = _axes[1].Clamp(clamped.y);
            clamped.z = _axes[2].Clamp(clamped.z);
            parent.localEulerAngles = clamped;
        }

        public void SetRotation(in Transform parent, ref Vector3 rotation) {
            parent.localEulerAngles = rotation;
            Vector3 clamped = parent.localEulerAngles;
            clamped.x = _axes[0].Clamp(clamped.x);
            clamped.y = _axes[1].Clamp(clamped.y);
            clamped.z = _axes[2].Clamp(clamped.z);
            parent.localEulerAngles = clamped;
        }

        private bool ContainsLocks() {
            for(int x = 0; x < 3; x++) {
                if(!_axes[x]) return true;
            }
            return false;
        }

        public void Enable() {
            _canRotate = true;
        }

        public void Disable() {
            _canRotate = false;
        }

        public void LockX() {
            _axes[0].CanRotate = false;
        }

        public void LockY() {
            _axes[1].CanRotate = false;
        }

        public void LockZ() {
            _axes[2].CanRotate = false;
        }

        public void FreeX() {
            _axes[0].CanRotate = true;
        }

        public void FreeY() {
            _axes[1].CanRotate = true;
        }

        public void FreeZ() {
            _axes[2].CanRotate = true;
        }

        public override string ToString() {
            return $"X: {_axes[0]}<br>Y: {_axes[1]} <br>Z: {_axes[2]} <br>";
        }
    }

    /// <summary>
    /// A representation of a singular rotation axis's clamp policy.
    /// </summary>
    [System.Serializable]
    public class AxisRule {

        [SerializeField] internal bool CanRotate = true;
        [SerializeField] internal bool IsClamping = true;
        [SerializeField] internal float Min;
        [SerializeField] internal float Max;

        public float Clamp(float value) {
            if(!CanRotate) return 0;
            if(!IsClamping) return value;
            return Mathf.Clamp(value, Min, Max);
        }

        public float EvaluateDelta(float rotation, float delta) {
            if(!CanRotate) return 0;
            if(!IsClamping) return delta;
            float norm = NormalizeDegrees(rotation);
            if(norm + delta > Max) return Max - rotation;
            if(norm + delta < Min) return Min - rotation;
            return delta;
        }

        private float NormalizeDegrees(float input) {
            return ((input + 180f) % 360f) - 180f;
        }

        public static implicit operator bool(AxisRule rule) {
            return rule.CanRotate;
        }

        public override string ToString() {
            return $"[{Min} -> {Max}, Rotate? {CanRotate}, Clamping? {IsClamping}]";
        }
    }
}
