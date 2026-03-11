
// SIGNATURE :)

using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input.ActionWrappers {

    public class JumpAction : InputBehaviour<float> {

        private bool _toggle;

        public void Consume() { _toggle = false; }
        
        protected override string GetID() {
            return "Jump";
        }

        // idk which one of these is invoked first so it's in both lol
        public override void OnActionStarted(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(_toggle) {
                if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateUp(1f);
            }
        }

        public override void OnActionPerformed(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(_toggle) {
                if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateUp(1f);
            }
        }

        public override void WhileActionPerformed(float value, InputAction action) {
            if(!_toggle) {
                if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateUp(0f);
            }
        }

        public override void OnActionEnded(float value, InputAction action, InputAction.CallbackContext ctx) {
            if(INFRA.Game.PlayerPuppet is Movable mb) mb.TranslateUp(0f);
            _toggle = true;
        }

        public override bool ShouldBePaused() {
            return true;
        }
    }
}