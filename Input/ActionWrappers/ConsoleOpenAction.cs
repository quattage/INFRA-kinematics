
// SIGNATURE :)

using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class ConsoleOpenAction : InputBehaviour<float> {

        protected override string GetID() {
            return "DevConsole";
        }

        public override void OnActionStarted(float value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void OnActionPerformed(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(value >= 0.5) INFRA.Game.Console.enabled = !INFRA.Game.Console.enabled;
        }

        public override void WhileActionPerformed(float value, InputAction action) {
            return;
        }

        public override void OnActionEnded(float value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override bool ShouldBePaused() {
            return false;
        }
    }
}