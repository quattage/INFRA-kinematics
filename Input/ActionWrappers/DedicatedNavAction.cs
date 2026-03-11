
// SIGNATURE :)

using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class DedicatedNavAction : InputBehaviour<float> {
        public bool isActive;

        public override void OnActionStarted(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(value >= 0.5) INFRA.Game.Console.RestorePrevious();
            else INFRA.Game.Console.RestoreNext();
        }

        public override void OnActionPerformed(float value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void WhileActionPerformed(float value, InputAction action) {
            return;
        }

        public override void OnActionEnded(float value, InputAction action, InputAction.CallbackContext ctx) {
            isActive = false;
        }

        protected override string GetID() {
            return "DedicatedNavigate";
        }

        public override bool ShouldBePaused() {
            return false;
        }
    }
}