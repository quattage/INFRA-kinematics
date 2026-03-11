
// SIGNATURE :)

using System;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.Viewmodel {

    public class Aimpuncher : MonoBehaviour {

        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _rootModifier;
        [SerializeField] private Animator _punchAnimator;
        private Vector3 _defaultRotation;

        private bool _shouldRoll;
        private Vector3 _targetRotation = Vector3.zero;
        [NonSerialized] public float RollPercent = 0;
        private float _lerpTime = 13;


        public void Awake() {
            _defaultRotation = _rootModifier.localEulerAngles;
        }

        public void Update() {
            if(!_shouldRoll) {
                _rootModifier.eulerAngles = _rootModifier.eulerAngles.LerpAngleTo(_defaultRotation, Time.deltaTime * _lerpTime);
                return;
            }
            _rootModifier.eulerAngles = _rootModifier.eulerAngles.LerpAngleTo(_defaultRotation.LerpAngleTo(_targetRotation, RollPercent), Time.deltaTime * _lerpTime);
        }

        public void InitiateRoll(Quaternion facing, bool isLeftWall) {
            _lerpTime = 13;
            _shouldRoll = true;
            _targetRotation = facing * new Vector3(0, 0, isLeftWall ? -10 : 10);
            RollPercent = 1;
        }

        public void UpdateRollDirection(Quaternion facing) {
            _lerpTime = 7;
            _targetRotation = facing * new Vector3(0, 0, -10);
        }

        public void ClearRoll() {
            _shouldRoll = false;
            RollPercent = 0;
        }

        public Aimpuncher PunchFOVTime(float fovPunchTime) {
            return this;
        }

        public Aimpuncher PunchFOV(float fov) {
            return this;
        }
    }
}