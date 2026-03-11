
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class RotateLookAction : InputBehaviour<Vector2> {

        public RotateLookAction() : base() {}

        public override void OnActionStarted(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void OnActionPerformed(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void WhileActionPerformed(Vector2 value, InputAction action) {
            Vector3 converted = ToVisuomotorVector(ref value);
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.Rotate(ref converted);
        }

        public override void OnActionEnded(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        protected override string GetID() {
            return "Look";
        }

        public static Vector3 ToVisuomotorVector(ref Vector2 delta) {
            return new Vector3(-delta.y, delta.x, 0) * 0.03f * INFRA.Game.Input.set_looksens.GetFloat();
        }

        public override bool ShouldBePaused() {
            return true;
        }
    }
}