
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class TranslateMoveAction : InputBehaviour<Vector2> {

        protected override string GetID() {
            return "Move";
        }

        public override void OnActionStarted(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void OnActionPerformed(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void WhileActionPerformed(Vector2 value, InputAction action) {
            Vector3 wishdir = new Vector3(value.x, 0, value.y);
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateMove(ref wishdir);
        }

        public override void OnActionEnded(Vector2 value, InputAction action, InputAction.CallbackContext ctx) {
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateMove(Vector3.zero);
        }

        public override bool ShouldBePaused() {
            return true;
        }
    }
}