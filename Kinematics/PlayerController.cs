
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using Assets.quatworks.INFRASEC.Kinematics.Viewmodel;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics {

    public class PlayerController : KinematicController, PlayerViewable {
        
        [SerializeField] private Aimpuncher _aimpuncher;
        [SerializeField] private Transform _viewmodelTransform; 
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private Camera _viewmodelCamera;

        [SerializeField] private AnimationCurve _wallrunRollCurve;
        private bool _isPossessed = false;
        private WallRunContext _ctx;

        public override void Initialize() {
            base.Initialize();
            if(!_isPossessed) INFRA.Game.Possess(this);
        }

        protected override void OnUpdate() {
            CorrectForWallrun();
            if(UnityEngine.Input.GetKey(KeyCode.F3)) {
                RotateToFaceY(90, Time.deltaTime);
            }
        }

        private void CorrectForWallrun() {
            if(_ctx == null) return;
            if(_ctx.Alignment < 0) {
                Quaternion target = Quaternion.LookRotation(_ctx.LookProj, Vector3.up);
                Quaternion delta = Quaternion.Inverse(GetRootRotation()) * target;
                float yaw = Mathf.DeltaAngle(0, delta.eulerAngles.y);
                LerpYRotation(yaw);
            }
        }

        public override void Rotate(ref Vector3 amount) {
            _current.OnRotate(ref amount, this);
        }

        public Camera GetPrimaryCamera() {
            return _viewmodelCamera;
        }

        public Transform GetViewmodelTransform() {
            return _viewmodelTransform;
        }

        public bool IsAvailable() {
            return !_isPossessed;
        }

        public void Possess() {
            _isPossessed = true;
            _viewmodelCamera.enabled = true;
            Initialize();
        }

        public void UnPossess() {
            _isPossessed = false;
            _viewmodelCamera.enabled = false;
        }

        public bool IsPossessed() {
            return _isPossessed;
        }

        public override void OnDestroy() {
            UnPossess();
            if(INFRA.Game != null) {
                if(this.Equals(INFRA.Game.PlayerPuppet)) 
                    INFRA.Game.PlayerPuppet = null;
            }
            base.OnDestroy();
        }

        public override void OnWallrunEnter(WallRunContext ctx) {
            // _aimpuncher.PunchUp();
            _ctx = ctx;
            _aimpuncher.InitiateRoll(Quaternion.LookRotation(_ctx.Tangent, Vector3.up), ctx.IsLeft);
            ctx.EvalTime = _wallrunRollCurve.Evaluate(ctx.Time / INFRA.Game.move_wallruntime.GetFloat());
            _aimpuncher.RollPercent = ctx.EvalTime;
        }

        public override void WhileWallrunPerformed(WallRunContext ctx) {
            _ctx = ctx;
            _aimpuncher.UpdateRollDirection(Quaternion.LookRotation(_ctx.Tangent, Vector3.up));
            ctx.EvalTime = _wallrunRollCurve.Evaluate(ctx.Time / INFRA.Game.move_wallruntime.GetFloat());
            _aimpuncher.RollPercent = ctx.EvalTime;
        }

        public override void OnWallrunExit() {
            _ctx = null;
            _aimpuncher.ClearRoll();
        }

        public override void OnSlideEnter() {
            _aimpuncher.PunchFOVTime(0.1f).PunchFOV(INFRA.Game.opt_fov.GetFloat() + 15);
        }
        public override void WhileSlidePerformed() {}
        public override void OnSlideExit() {
            _aimpuncher.PunchFOVTime(0.1f).PunchFOV(INFRA.Game.opt_fov.GetFloat());
        }

        public override void OnJump(JumpContext ctx) {
            if(ctx.IsDouble) {
                // _aimpuncher.PunchDownHard();
                return;
            }

            // _aimpuncher.PunchDownSoft();
        }

        private void OnDrawGizmosSelected() {
            if(_viewmodelCamera != null) {
                Gizmos.color = Color.white;
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(_viewmodelCamera.transform.position, _viewmodelCamera.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.4f, 0.4f, 0.7f));
                Gizmos.DrawFrustum(Vector3.zero, _viewmodelCamera.fieldOfView, 0.5f, 0.1f, _viewmodelCamera.aspect);
                Gizmos.matrix = temp;
            }
            if(GetCollider() != null) {
                if(!Application.isPlaying)
                    GetCollider().Recompute();
                GetCollider().DrawGizmo();
            }
            Gizmos.color = Color.grey;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetContacts().GetLastGroundPoint(), 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(GetCollider().GetBottomSurface(), 0.2f);
            Gizmos.color = new Color(1, 0.24f, 0.42f);
            Gizmos.DrawRay(GetPosition(), GetVelocity() / Time.deltaTime * 0.1f);
            if(_current != null) {
                Gizmos.color = new Color(0.24f, 0.83f, 1);
                Gizmos.DrawRay(GetPosition(), _current.GetDesiredMovement() / Time.deltaTime * 0.005f);
            }
        }
    }
}