
// SIGNATURE :)

using System.Collections.Generic;
using System.Linq;
using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Assets.quatworks.INFRASEC.Data.Console {

    [RegistrySubscriber]
    public class DevConsole : MonoBehaviour {

        private IKwArg cv_console_scrollback;
        private IKwArg cv_console_logwhenclosed;

        private string version = "0.1a";
        // TODO instantiation management - client/server
        private bool headless = false;

        public string GetFormattedVersion() {
            string platform = headless ? "(headless)" : "(client)";
            return $"<br><b><color=#e1ffe9>INFRAConsole</color> <color=#6f9bff>v{version}</color></b> <color=#9aacbc>{platform}</color><br>";
        }

        [ConsoleVariableRegistry]
        public static void RegisterCVars(ConsoleVariableRegistrar cvars) {
            cvars
            .New("cv_console_scrollback")
                .SavesTo(INFRA.Game.Data.Client)
                .WithDescription("Dictates how many individual logging statements the console can keep track of and display.")
                .AsInt()
                .DefaultsTo(256)
                .WithMinimum(64)
                .WithMaximum(8192)
                .Make()
            .New("cv_console_logwhenclosed")
                .SavesTo(INFRA.Game.Data.Client)
                .WithDescription("If true, the console will continue to update its logs even while it is closed.")
                .AsBool()
                .DefaultsTo(true)
                .Make();
        }

        private void InitializeCvars() {
            cv_console_scrollback ??= INFRA.Game.GetCVar("cv_console_scrollback");
            cv_console_logwhenclosed ??= INFRA.Game.GetCVar("cv_console_logwhenclosed");
        }

        [SerializeField] private UIDocument _consoleUI;

        private TextField _consoleField;
        public TextField ConsoleField { get { 
            if(_consoleField == null && _consoleUI != null) {
                _consoleField = _consoleUI.rootVisualElement.Q<TextField>("ConsoleInput");
                _consoleField.RegisterValueChangedCallback(evt => { _cmdHistoryIndex = -1;} );
            }
            return _consoleField; 
        } }
        private ScrollView _textScroll;
        public ScrollView TextScroll { get {
            if(_textScroll == null && _consoleUI != null)
                _textScroll = _consoleUI.rootVisualElement.Q<ScrollView>("Entries");
            return _textScroll;
        }}

        private Button _closeButton;
        public Button CloseButton { get {
            if(_closeButton == null && _consoleUI != null) {
                _closeButton = _consoleUI.rootVisualElement.Q<Button>("CloseButton");
                _closeButton.pickingMode = PickingMode.Position;
            }
            return _closeButton;
        }}


        private CursorLockMode _prevCursorState;
        private bool _prevCursorVisibility;

        private List<string> _cmdHistory = new();
        private int _cmdHistoryIndex = -1;

        private bool _isLogSubscribed = false;

        public void Awake() {
            DontDestroyOnLoad(this.gameObject);
            _consoleUI.enabled = true;
            _consoleUI.rootVisualElement.style.display = DisplayStyle.None;
            enabled = false;

            if(cv_console_logwhenclosed == null || cv_console_logwhenclosed.GetBool())
            _isLogSubscribed = true;
            Application.logMessageReceived += ListenToLog;
        }


        public void Update() {
            if(INFRA.Game.Input.Cancel.isActive) {
                enabled = false;
                INFRA.Game.Input.Cancel.isActive = false;
            }

            if(Keyboard.current.anyKey.isPressed && ConsoleField.value.IsNullOrEmpty())
                ConsoleField.Focus();

            if(INFRA.Game.Input.Submit.isActive) {
                ConsoleField.Focus();
                INFRA.Game.Input.Submit.isActive = false;
                SubmitInput();
            }
        }


        public void OnEnable() {

            if(INFRA.Game.IsLoading) return;
            INFRA.Game.Input.PauseRelevent();
            InitializeCvars();
            
            _prevCursorState = UnityEngine.Cursor.lockState;
            _prevCursorVisibility = UnityEngine.Cursor.visible;

            INFRA.Game.FreeCursor();
            _consoleUI.enabled = true;
            _consoleUI.rootVisualElement.style.display = DisplayStyle.Flex;

            _consoleField = _consoleUI.rootVisualElement.Q<TextField>("ConsoleInput");
            _textScroll = _consoleUI.rootVisualElement.Q<ScrollView>("Entries");
            _closeButton = _consoleUI.rootVisualElement.Q<Button>("CloseButton");
            _closeButton.RegisterCallback<PointerDownEvent>(HandleCloseButton, TrickleDown.TrickleDown);

            // I actually cannot believe unity is this dumb it is baffling sometimes
            VisualElement button = _textScroll.Q<VisualElement>("unity-high-button");
            if(button != null) button.parent.Remove(button);
            button = _textScroll.Q<VisualElement>("unity-low-button");
            if(button != null)  button.parent.Remove(button);

            if(!_isLogSubscribed) {
                _isLogSubscribed = true;
                Application.logMessageReceived += ListenToLog;
            }

            ConsoleField.SetEnabled(true);
            ConsoleField.Focus();
            ScrollToBottom();

            // TODO layered UI handling
            
        }


        public void OnDisable() {
            if(INFRA.Game.IsLoading) return;
            InitializeCvars();

            UnityEngine.Cursor.lockState = _prevCursorState;
            UnityEngine.Cursor.visible = _prevCursorVisibility;
            
            _closeButton?.UnregisterCallback<PointerDownEvent>(HandleCloseButton, TrickleDown.TrickleDown);

            // cvar can be null here if unity applies changes while playmode is active
            if(cv_console_logwhenclosed == null) return;

            // ensure that the console gets unsubscribed when closed if applicable
            if(!cv_console_logwhenclosed.GetBool()) {
                if(_isLogSubscribed) {
                    _isLogSubscribed = false;
                    Application.logMessageReceived -= ListenToLog;
                }
                _consoleUI.enabled = false;
            } else
                _consoleUI.rootVisualElement.style.display = DisplayStyle.None;

            // TODO layered UI handling
            INFRA.Game.Input.ResumeRelevent();
        }


        public void OverrideCursorLock() {
            _prevCursorState = CursorLockMode.Locked;
            _prevCursorVisibility = false;
        }


        public void OnDestroy() {
            _cmdHistory.Clear();
            _cmdHistory = null;
            _consoleField = null;
            _textScroll = null;
            _closeButton = null;
        }


        // subscribed to Unity's Debug.Log
        private void ListenToLog(string logString, string stackTrace, LogType type) {

            if(!Application.isPlaying) return;
            if(TextScroll == null) return;

            Label entry;
            if(logString.StartsWith("$::")) {
                entry = new("<color=#797979><size=10> ▪ </size></color>" + logString[3..]) {
                    enableRichText = true
                };
            } else {
                entry = new("<color=#797979><size=10> ▪ " + INFRA.GetTimecode() + "</size></color> " + logString) {
                    enableRichText = true
                };
            }

            switch(type) {
                case LogType.Assert: 
                    return; // assertions don't need to be logged
                case LogType.Log:
                    entry.AddToClassList("console-message");
                    break;
                case LogType.Warning:
                    entry.AddToClassList("console-warning");
                    break;
                case LogType.Error:
                    entry.AddToClassList("console-error");
                    break;
                case LogType.Exception:
                    entry.AddToClassList("console-error");
                    break;
            }

            if(TextScroll.contentContainer.childCount % 2 == 0)
                entry.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.34f);

            TextScroll.Add(entry);

            if(cv_console_scrollback == null) {
                if(TextScroll.childCount > 1024)
                    TextScroll.contentContainer.RemoveAt(0);
            } else {
                if(TextScroll.childCount > cv_console_scrollback.GetInt())
                    TextScroll.contentContainer.RemoveAt(0);
            }
        }


        public void RestorePrevious() {

            if(!enabled) return;
            if(_cmdHistory.Count <= 0) return;
            _cmdHistoryIndex++;
            if(_cmdHistoryIndex > _cmdHistory.Count - 1) {
                _cmdHistoryIndex = _cmdHistory.Count;
                return;
            }

            int oldindex = _cmdHistoryIndex;
            string lookup = _cmdHistory[_cmdHistory.Count - 1 - _cmdHistoryIndex];
            if(lookup.IsNullOrEmpty()) return;

            // changing the value of the console field resets the history 
            // index to -1, hence why it is saved and re-set here.
            ConsoleField.value = lookup; 
            _cmdHistoryIndex = oldindex;
            ConsoleField.cursorIndex = ConsoleField.text.Length;
        }


        public void RestoreNext() {

            if(!enabled) return;
            if(_cmdHistory.Count <= 0) return;
            _cmdHistoryIndex--;
            if(_cmdHistoryIndex < 0) {
                _cmdHistoryIndex = _cmdHistory.Count;
                return;
            }

            int oldindex = _cmdHistoryIndex;
            string lookup = _cmdHistory[_cmdHistory.Count - 1 - _cmdHistoryIndex];
            if(lookup.IsNullOrEmpty()) return;

            // changing the value of the console field resets the history 
            // index to -1, hence why it is saved and re-set here.
            ConsoleField.value = lookup; 
            _cmdHistoryIndex = oldindex;
            ConsoleField.cursorIndex = ConsoleField.text.Length;
        }


        // subscribed to the close button
        private void HandleCloseButton(PointerDownEvent evt) {
            enabled = false;
        }


        private void SubmitInput() {
            string text = ConsoleField.text;
            if(text.IsNullOrEmpty()) return;
            ConsoleField.value = "";

            if(_cmdHistory.Count <= 0) {
                _cmdHistory.Add(text);
            } else if(!_cmdHistory.Last().Equals(text))
                _cmdHistory.Add(text);
            _cmdHistoryIndex = -1;

            if(_cmdHistory.Count > 32)
                _cmdHistory.RemoveAt(0);

            Label output = new("<color=#797979><size=10> ▪ " + INFRA.GetTimecode() + "</size></color> <color=#a4b6b0>⇒ " + text + "</color>");
            output.enableRichText = true;
            output.AddToClassList("console-message");

            if(TextScroll.contentContainer.childCount % 2 == 0)
                output.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.34f);

            TextScroll.contentContainer.Add(output);

            ExecuteCommand(text);
            ConsoleField.Focus();
            ScrollToBottom();
        }

        public void ScrollToBottom() {
            if(TextScroll.verticalScroller.highValue > TextScroll.verticalScroller.lowValue)
            TextScroll.schedule.Execute(() => {
                TextScroll.verticalScroller.value = TextScroll.verticalScroller.highValue;
            }).ExecuteLater(1);
        }


        public void ExecuteCommand(string input) {
            string[] split = input.Split(" ");
            ConsoleParseResult result = INFRA.Game.Data.ConsoleCommands.Execute(input, split);
            result.WriteToLog();
        }
    }
}