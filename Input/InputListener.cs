
// SIGNATURE :)

using System.Collections.Generic;
using Assets.quatworks.INFRASEC.Data;
using Assets.quatworks.INFRASEC.Data.Console;
using Assets.quatworks.INFRASEC.Extensions;
using Assets.quatworks.INFRASEC.Input.ActionWrappers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.quatworks.INFRASEC.Input {

    [RegistrySubscriber]
    public class InputListener {

        public IKwArg set_looksens;

        [ConsoleCommandRegistry]
        public static void RegisterCommands(ConsoleCommandRegistrar commands) {
            commands
            .New(typeof(CMD_TestInput), "input query")
                .WithDescription("Polls the input system for the given input. Input name is case-sensitive.")
                .Accepts("name")
                    .WithDescription("The name of the input to access")
                    .AsString()
                    .DefaultsTo("")
                    .Make()
                .Build();
        }

        [ConsoleVariableRegistry]
        public static void RegisterConsoleVariables(ConsoleVariableRegistrar convars) {
            convars
            .New("set_looksens")
                .SavesTo(INFRA.Game.Data.Client)
                .WithDescription("Sensitivity for looking around")
                .AsFloat()
                .DefaultsTo(2.425f)
                .WithMinimum(0.001f)
                .WithMaximum(16f)
                .Make();
        }

        public void InitializeCVars() {
            set_looksens = INFRA.Game.GetCVar("set_looksens");
        }

        internal class CMD_TestInput : SimpleSyntaxCommand {
            public CMD_TestInput(string cid, string desc, IKwArg[] possibleArgs) : base(cid, desc, possibleArgs) {}

            public override ConsoleParseResult RunCommand(IKwArg[] args) {
                string id = args[0].GetString();
                InputAction action = InputSystem.actions.FindAction(id);
                if(action != null) return ConsoleParseResult.Pass($"Action '{action.name}' exists within '{action.actionMap.name}'");
                return ConsoleParseResult.Fail($"Action '{id}' coudln't be found in any action maps.");
            }
        }

        private readonly List<InputWrapper> _actions;

        /// <summary>
        /// Movement inputs (WASD)
        /// </summary>
        /// <value></value>
        public TranslateMoveAction TranslateMove { get => _translatemove; }
        private readonly TranslateMoveAction _translatemove = new();

        /// <summary>
        /// Jump (Space)
        /// </summary>
        public JumpAction Jump { get => _jump; }
        private JumpAction _jump = new();

        /// <summary>
        /// Jump (Space)
        /// </summary>
        public CrouchAction Crouch { get => _crouch; }
        private CrouchAction _crouch = new();

        /// <summary>
        /// Sprint (shift)
        /// </summary>
        /// <value></value>
        public SprintAction Sprint { get => _sprint; }
        private SprintAction _sprint = new();

        /// <summary>
        /// Rotation inputs (Mouse X & Y)
        /// </summary>
        /// <value></value>
        public RotateLookAction Rotate { get => _rotate; }
        private readonly RotateLookAction _rotate = new();

        /// <summary>
        /// Mouse1 (Left Click)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> Fire { get => _fire; }
        private InputBehaviour<bool> _fire;

        /// <summary>
        /// Mouse2 (Right Click)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> FireAlt { get => _firealt; }
        private InputBehaviour<bool> _firealt;

        /// <summary>
        /// Mouse3 (Middle Click)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> FireAux { get => _fireaux; }
        private InputBehaviour<bool> _fireaux;

        /// <summary>
        /// Auxilury Interact (E)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> Interact { get => _interact; }
        private InputBehaviour<bool> _interact;

        /// <summary>
        /// Positive (Scroll Up)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> Next { get => _next; }
        private InputBehaviour<bool> _next;

        /// <summary>
        /// Negative (Scroll Down)
        /// </summary>
        /// <value></value>
        public InputBehaviour<bool> Previous { get => _previous; }
        private InputBehaviour<bool> _previous;

        /// <summary>
        /// F1 to toggle developer features
        /// </summary>
        /// <value></value>
        public ConsoleOpenAction ConsoleOpen { get => _consoleopen; }
        private ConsoleOpenAction _consoleopen = new();

        /// <summary>
        /// Up/Down arrows for accessing console history
        /// </summary>
        /// <value></value>
        public DedicatedNavAction VerticalNav { get => _verticalnav; }
        private DedicatedNavAction _verticalnav = new();

        /// <summary>
        /// Escape to back out of stuff
        /// </summary>
        /// <value></value>
        public CancelAction Cancel { get => _cancel; }
        private CancelAction _cancel = new();

        /// <summary>
        /// Enter to confirm stuff
        /// </summary>
        /// <value></value>
        public SubmitAction Submit { get => _submit; }
        private SubmitAction _submit = new();

        public InputListener() {

            // a list is used here instead of an array (for now) to accomodate the possibility
            // of runtime keybind registration should my sorry ass ever get to doing that
            _actions = new List<InputWrapper>() {
                _translatemove,
                _jump,
                _crouch,
                _sprint,
                _rotate,
                _fire,
                _firealt,
                _fireaux,
                _interact,
                _next,
                _previous,
                _consoleopen,
                _verticalnav,
                _cancel,
                _submit
            };
            // TODO ^^ automatic initialization ^^
        }

        public void Update() {
            if(INFRA.Game.IsLoading) return;
            InputSystem.Update();
            for(int x = 0; x < _actions.Count; x++) {
                InputWrapper thisAction = _actions[x];
                if(thisAction == null) continue;
                if(!thisAction.Wrapped.enabled) continue;
                if(thisAction.Wrapped.inProgress) {
                    thisAction.WhileActionPerformed(thisAction.Wrapped);
                }
            }
        }

        /// <summary>
        /// Pauses any InputAction marked as pausable.
        /// </summary>
        public void PauseRelevent() {
            for(int x = 0; x < _actions.Count; x++) {
                InputWrapper thisAction = _actions[x];
                if(thisAction == null) continue;
                thisAction.Pause();
            }
        }

        /// <summary>
        /// Resumes all InputActions
        /// </summary>
        public void ResumeRelevent() {
            for(int x = 0; x < _actions.Count; x++) {
                InputWrapper thisAction = _actions[x];
                if(thisAction == null) continue;
                thisAction.Resume();
            }
        }
    }

    public abstract class InputWrapper {

        protected readonly string _id;
        public readonly InputAction Wrapped;

        public InputWrapper() {
            _id = GetID();

            if(_id.IsNullOrEmpty()) {
                Debug.LogError($@"Error registering InputBehaviour '{_id}' - This is invalid! (probably null or empty string)");
                return;
            }
            Wrapped = InputSystem.actions.FindAction(_id);
            if(Wrapped == null) {
                Debug.LogError($@"Error registering InputBehaviour '{_id}' - This action doesn't exist in the ActionMap!");
                return;
            }
            Debug.Log($@"Subscribed input behaviour '{GetType().Name}' to InputAction '{GetID()}'");
        }

        public void Pause() {
            if(!ShouldBePaused()) return;
            Wrapped.Disable();
        }

        public void Resume() {
            Wrapped.Enable();
        }

        /// <summary>
        /// Whether or not this input shouldn't receive continuous updates
        /// while in-game menus are blocking the player's view.
        /// </summary>
        /// <returns></returns>
        public abstract bool ShouldBePaused();

        /// <summary>
        /// An Input wrapper's ID cooresponds to its name in Unity's 
        /// InputSystem component.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetID();

        /// <summary>
        /// Called continuously while the action is being performed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        public abstract void WhileActionPerformed(InputAction action);
    }


    public abstract class InputBehaviour<T> : InputWrapper where T: struct {

        public InputBehaviour() {
            Wrapped.started += HandleAction;
            Wrapped.performed += HandleAction;
            Wrapped.canceled += HandleAction;
        }

        internal virtual void HandleAction(InputAction.CallbackContext ctx) {
            switch(ctx.phase) {
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    OnActionWaiting(ctx.action.ReadValue<T>(), ctx.action, ctx);
                    break;
                case InputActionPhase.Started:
                    OnActionStarted(ctx.action.ReadValue<T>(), ctx.action, ctx);
                    break;
                case InputActionPhase.Performed:
                    OnActionPerformed(ctx.action.ReadValue<T>(), ctx.action, ctx);
                    break;
                case InputActionPhase.Canceled:
                    OnActionEnded(ctx.action.ReadValue<T>(), ctx.action, ctx);
                    break;
            }
        }

        public override string ToString() {
            return $@"[{GetType().Name} -> '{GetID()}']";
        }

        /// <summary>
        /// Called once when this action first starts.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="action"></param>
        /// <param name="ctx"></param>
        public abstract void OnActionStarted(T value, InputAction action, InputAction.CallbackContext ctx);

        /// <summary>
        /// Called once when this action is first being performed.
        /// <para/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        /// <param name="ctx"></param>
        public abstract void OnActionPerformed(T value, InputAction action, InputAction.CallbackContext ctx);

        public override void WhileActionPerformed(InputAction action) {
            WhileActionPerformed(action.ReadValue<T>(), action);
        }

        /// <summary>
        /// Called continuously while the action is being performed.
        /// Provides access to the raw value and the InputAction itself.
        /// No callbacks exist in this context, since this method can be
        /// invoked at any time while a button is held.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        public abstract void WhileActionPerformed(T value, InputAction action);

        /// <summary>
        /// Called once whenever this action's input is no longer supplied.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="action"></param>
        /// <param name="ctx"></param>
        public abstract void OnActionEnded(T value, InputAction action, InputAction.CallbackContext ctx);

        /// <summary>
        /// Called once when this action returns to idle. This can be used
        /// sort of like a constructor, if you want to pull and store info 
        /// provided by the action or callback.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        /// <param name="ctx"></param>
        public virtual void OnActionWaiting(T value, InputAction action, InputAction.CallbackContext ctx) {}
    }
}