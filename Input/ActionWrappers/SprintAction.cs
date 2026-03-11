
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class SprintAction : InputBehaviour<float> {
        
        protected override string GetID() {
            return "Sprint";
        }

        public override void OnActionStarted(float value, InputAction action, InputAction.CallbackContext ctx) {
            
        }

        public override void OnActionPerformed(float value, InputAction action, InputAction.CallbackContext ctx) {
            return;
        }

        public override void WhileActionPerformed(float value, InputAction action) {
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.Impulse(1);
        }

        public override void OnActionEnded(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.Impulse(0);
        }

        public override bool ShouldBePaused() {
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.Impulse(0);
            return true;
        }
    }
}