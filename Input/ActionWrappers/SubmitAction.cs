
// SIGNATURE :)

using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class SubmitAction : InputBehaviour<float> {

        public bool isActive = false;

        protected override string GetID() {
            return "Submit";
        }

        public override void OnActionStarted(float value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void OnActionPerformed(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(value >= 0.5) isActive = true;
        }

        public override void WhileActionPerformed(float value, InputAction action) {
            return;
        }

        public override void OnActionEnded(float value, InputAction action, InputAction.CallbackContext ctx) {
            isActive = false;
        }

        public override bool ShouldBePaused() {
            return false;
        }
    }
}