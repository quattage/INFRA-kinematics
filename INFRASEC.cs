
// SIGNATURE :)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Assets.quatworks.INFRASEC.Data;
using Assets.quatworks.INFRASEC.Data.Console;
using Assets.quatworks.INFRASEC.Extensions;
using Assets.quatworks.INFRASEC.Input;
using Assets.quatworks.INFRASEC.Kinematics.Core;
using Assets.quatworks.INFRASEC.Kinematics.Viewmodel;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Assets.quatworks.INFRASEC {

    [RegistrySubscriber]
    public class INFRA : MonoBehaviour {


        ////// global cvars ///////
        public IKwArg game_devmode;
        
        ////// physics ///////
        public IKwArg phys_skinwidth;
        public IKwArg phys_gravity;
        public IKwArg phys_tickrate;
        public IKwArg phys_timescale;
        public IKwArg phys_collsteps;
        public IKwArg phys_airfriction;
        public IKwArg phys_groundfriction;
        public IKwArg phys_frictionframes;

        ////// movement ///////
        public IKwArg move_maxslope;
        public IKwArg move_minceiling;
        public IKwArg move_wallrunangle;
        public IKwArg move_wallruntime;
        public IKwArg move_stepheight;
        public IKwArg move_stepdepth;
        public IKwArg move_speed;
        public IKwArg move_accel;
        public IKwArg move_airaccel;
        public IKwArg move_terminal;
        public IKwArg move_jumpstrength;

        public IKwArg opt_fov;

        [ConsoleVariableRegistry]
        public static void RegisterConsoleVariables(ConsoleVariableRegistrar convars) {
            convars
            .New("game_devmode")
                .SavesTo(INFRA.Game.Data.Client)
                .WithDescription("Controls the level of developer access to game systems.")
                .AsEnum()
                .WithValues("OFF", "DEBUG", "DEVELOP")
                .DefaultsTo(0)
                .Make()
            .New("phys_skinwidth")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("An epsilon value for controlling the virtualized width of physics colliders. Smaller values = More precise, Bigger values = more consistent")
                .AsFloat()
                .DefaultsTo(0.015f)
                .WithMinimum(0.001f)
                .WithMaximum(2f)
                .Make()
            .New("phys_gravity")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Universal gravity constant measured in meters per second squared.::Backed by UnityEngine.Physics.Gravity")
                .AsBackedFloat()
                .DefaultsTo(-9.81f)
                .WithSetter((float x) => Physics.gravity = new Vector3(0, x, 0))
                .WithGetter(() => Physics.gravity.y)
                .WithMinimum(-64)
                .WithMaximum(64)
                .Make()
            .New("phys_tickrate")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Tickrate of physics updates (in ticks per second)::Backed by UnityEngine.Time.fixedDeltaTime")
                .AsBackedInt()
                .DefaultsTo(60)
                .WithSetter((int x) => Time.fixedDeltaTime = (float)(1f / x))
                .WithGetter(() => (int)Mathf.Floor(1f / Time.fixedDeltaTime))
                .WithMinimum(1)
                .WithMaximum(128)
                .Make()
            .New("phys_timescale")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Simulation speed of game physics")
                .AsFloat()
                .DefaultsTo(1f)
                .WithMinimum(0.01f)
                .WithMaximum(128)
                .Make()
            .New("phys_collsteps")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("How many iterations of collision to perform on kinematic controllers")
                .AsInt()
                .DefaultsTo(3)
                .WithMinimum(2)
                .WithMaximum(16)
                .Make()
            .New("phys_airfriction")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("How much friction moving elements experience while in the air")
                .AsFloat()
                .DefaultsTo(0.008f)
                .WithMinimum(0f)
                .WithMaximum(16f)
                .Make()
            .New("phys_groundfriction")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("How much friction moving elements experience while touching the ground")
                .AsFloat()
                .DefaultsTo(0.22f)
                .WithMinimum(0f)
                .WithMaximum(16f)
                .Make()
            .New("phys_frictionframes")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("The number of frames after hitting the ground will friction begin to apply")
                .AsInt()
                .DefaultsTo(2)
                .WithMinimum(0)
                .WithMaximum(16)
                .Make()
            .New("move_maxslope")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Maximum slope angle in degrees that a player can climb without slipping")
                .AsInt()
                .DefaultsTo(65)
                .WithMinimum(20)
                .WithMaximum(89)
                .Make()
            .New("move_minceiling")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Maximum slope angle in degrees that a player can climb without slipping")
                .AsInt()
                .DefaultsTo(165)
                .WithMinimum(91)
                .WithMaximum(180)
                .Make()
            .New("move_wallrunangle")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Angle of incidence (relative to 90 degrees) for walls to be considered wallrunable")
                .AsInt()
                .DefaultsTo(30)
                .WithMinimum(1)
                .WithMaximum(45)
                .Make()
            .New("move_wallruntime")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Maximum time alotted to the player for wallruns")
                .AsInt()
                .DefaultsTo(3)
                .WithMinimum(1)
                .WithMaximum(64)
                .Make()
            .New("move_stepheight")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Maximum height (in meters) for a wall to be considered a climbable step")
                .AsFloat()
                .DefaultsTo(0.48f)
                .WithMinimum(0.01f)
                .WithMaximum(256f)
                .Make()
            .New("move_stepdepth")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Minimum depth (in meters) for a ground surface to be considered a steppable one")
                .AsFloat()
                .DefaultsTo(0.1f)
                .WithMinimum(0.01f)
                .WithMaximum(256f)
                .Make()
            .New("move_speed")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Base speed of player movement")
                .AsFloat()
                .DefaultsTo(6f)
                .WithMinimum(0.1f)
                .WithMaximum(64f)
                .Make()
            .New("move_accel")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("The strength of a player's movement authority")
                .AsFloat()
                .DefaultsTo(0.207f)
                .WithMinimum(0.01f)
                .WithMaximum(10f)
                .Make()
            .New("move_airaccel")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("The strength of a player's movement authority while in the air")
                .AsFloat()
                .DefaultsTo(0.10f)
                .WithMinimum(0.01f)
                .WithMaximum(10f)
                .Make()
            .New("move_terminal")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("Terminal velocity of movable bodies in meters per second")
                .AsFloat()
                .DefaultsTo(180f)
                .WithMinimum(0f)
                .WithMaximum(8192f)
                .Make()
            .New("move_jumpstrength")
                .SavesTo(INFRA.Game.Data.Server)
                .WithDescription("The power of the player's jump")
                .AsFloat()
                .DefaultsTo(12f)
                .WithMinimum(0)
                .WithMaximum(64f)
                .Make()


            // TODO MOVE THIS TO SETTINGS
            .New("opt_fov") 
                .SavesTo(INFRA.Game.Data.Client)
                .WithDescription("FOV of the first person camera")
                .AsFloat()
                .DefaultsTo(90f)
                .WithMinimum(55)
                .WithMaximum(110f)
                .Make();
        }

        // the CVar instances defined above need to be initialized
        internal void InitializeCVars() {
            game_devmode = _game.GetCVar("game_devmode");
            phys_skinwidth = _game.GetCVar("phys_skinwidth");
            phys_gravity = _game.GetCVar("phys_gravity");
            phys_tickrate = _game.GetCVar("phys_tickrate");
            phys_timescale = _game.GetCVar("phys_timescale");
            phys_collsteps = _game.GetCVar("phys_collsteps");
            phys_airfriction = _game.GetCVar("phys_airfriction");
            phys_groundfriction = _game.GetCVar("phys_groundfriction");
            phys_frictionframes = _game.GetCVar("phys_frictionframes");

            move_maxslope = _game.GetCVar("move_maxslope");
            move_minceiling = _game.GetCVar("move_minceiling");
            move_wallrunangle = _game.GetCVar("move_wallrunangle");
            move_wallruntime = _game.GetCVar("move_wallruntime");
            move_stepheight = _game.GetCVar("move_stepheight");
            move_stepdepth = _game.GetCVar("move_stepdepth");
            move_accel = _game.GetCVar("move_accel");
            move_speed = _game.GetCVar("move_speed");
            move_airaccel = _game.GetCVar("move_airaccel");
            move_terminal = _game.GetCVar("move_terminal");
            move_jumpstrength = _game.GetCVar("move_jumpstrength");


            opt_fov = _game.GetCVar("opt_fov");
        }
        //////////////////////////////////////////////////////




        private static string _pname = "PersistentContainer";

        private static INFRA _game;
        public static INFRA Game { get { 
            if(_game != null) return _game; 
            Debug.LogWarning("Game manager detected an instance invalidation, will be reacquired.");
            // InitializeGame();
            return _game;
        }}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeGame() {

            Profiler.BeginSample("INFRASEC Initialize");

            // gameobject instantiation
            GameObject parent = GameObject.FindGameObjectWithTag(_pname);
            if(parent == null) parent = new GameObject(_pname);
            parent.tag = _pname;
            _game = parent.GetComponent<INFRA>();
            if(_game == null) _game = parent.AddComponent<INFRA>();
            _game.Stage = GameLifecycle.LOADING;
            //

            // containers
            if(_game.Console == null) {
                GameObject consoleContainer = Resources.Load<GameObject>("DevConsole");
                GameObject instantiated = Instantiate(consoleContainer, Vector3.zero, Quaternion.identity);   
                _game.Console = instantiated.GetComponent<DevConsole>();
            }
            _game.Data ??= new DataContainer();
            _game.LoadDI();
            _game.Data.ReadFromDisk();
            _game.Input ??= new InputListener();
            //

            _game.InitializeCVars();
            _game.Input.InitializeCVars();

            // if(_game.game_devmode.GetInt() == 2)
                // SceneManager.LoadScene(2);
            // else SceneManager.LoadScene(1);

            _game.Stage = GameLifecycle.LOADED;
            Debug.Log("INFRASEC has loaded.");

            Profiler.EndSample();
        }



        /////// THE CLASS STARTS HERE //////

        public enum GameLifecycle { LOADING, LOADED, UNLOADING }
        public GameLifecycle Stage { get; private set; } = GameLifecycle.LOADING;
        public bool IsLoading { get { return Stage == GameLifecycle.LOADING; } }

        public DataContainer Data { get; private set; }
        public InputListener Input { get; private set; }
        public DevConsole Console { get; private set; }
        public PlayerViewable PlayerPuppet { get; set; } = null;

        public readonly Vector3 GravityNormal = new Vector3(0, 1, 0);
        public readonly Vector3 GravityPlane = new Vector3(1, 0, 1);

        public LayerMask MovableMask { get; private set; }
        public LayerMask LevelMask { get; private set; }

        public void Awake() {
            if(_game == null) {
                _game = this;
                DontDestroyOnLoad(this);
            } else if(_game != this) {
                Debug.LogWarning("GameManager instance cannot be re-instantiated.");
                Destroy(this);
            }
            MovableMask = LayerMask.GetMask("Movable");
            LevelMask = ~MovableMask;
        }

        public void Update() {
            _game.Input.Update();
        }

        public void FixedUpdate() {

        }

        public void Possess(PlayerViewable puppet) {
            if(puppet == null) return;
            if(puppet == PlayerPuppet) return;
            if(PlayerPuppet != null) PlayerPuppet.UnPossess();
            FreeCursor();
            PlayerPuppet = puppet;
            puppet.Possess();
            LockCursor();
            Debug.Log($"Possessed {puppet}");
        }

        public float GetDelta() {
            return Time.deltaTime * phys_timescale.GetFloat();
        }

        public float GetFixedDelta() {
            return Time.fixedDeltaTime * phys_timescale.GetFloat();
        }


        public Vector3 GetGravity() {
            return 0.05f * GetFixedDelta() * Physics.gravity; // ?? idk
        }

        public void FreeCursor() {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void FreeCursorConfined() {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void LockCursor() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // kindof sortof dependency injection to register console variables and console commands
        private void LoadDI() {

            Assembly target = Assembly.GetExecutingAssembly();
            IEnumerable containers = target.GetTypes()
                .Where(type => 
                    !type.IsInterface && 
                    (type.GetCustomAttribute<RegistrySubscriber>() != null)
                ).ToArray();

            CVarRegistrySummary cvarStack = new();
            ConsoleCommandRegistrar commandStack = new();

            foreach(Type t in containers) {
                IEnumerable<MethodInfo> methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(method => 
                        method.GetCustomAttribute<ConsoleVariableRegistry>() != null ||
                        method.GetCustomAttribute<ConsoleCommandRegistry>() != null ||
                        method.GetCustomAttribute<Startup>() != null);
                if(methods.Count() <= 0) { 
                    Debug.LogWarning($@"Class '{t.Name}' uses a RegistrySubscriber 
                        attribute but contains no valid registries.");
                    continue;
                }

                try {
                    foreach(MethodInfo method in methods) {
                        if(method.GetCustomAttribute<ConsoleVariableRegistry>() != null)
                            method.Invoke(null, new object[] { new ConsoleVariableRegistrar(cvarStack) });
                        else if(method.GetCustomAttribute<ConsoleCommandRegistry>() != null)
                            method.Invoke(null, new object[] { commandStack });
                        else if(method.GetCustomAttribute<Startup>() != null) {
                            Debug.Log($@"Loaded StartupSubscriber '{method.Name}' from class '{t.Name}'");
                            method.Invoke(null, new object[] {});
                        }
                    }
                } catch(Exception ex) {
                    Debug.LogError($@"Error encountered while loading RegistrySubscriber '{t.Name}' 
                        :  (( {ex} )) ---- Registration for this class has been skipped.");
                    continue;
                }
                cvarStack.PushRegistrations();
                commandStack.PushRegistrations();
            }
        }

        /// <summary>
        /// Gets the ConsoleVariable at the given ID.
        /// Returs null if no such ConsoleVariable exists
        /// at the given ID. Automatically casts for you, and
        /// logs errors if the ID isn't of the correct type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IKwArg GetCVar(string id) {
            SaveableData found = Data[id];
            if(found is IKwArg kwarg) return kwarg;
            if(found != null) {
                Debug.LogWarning($@"Error getting ConsoleVariable '{id}' - This ID is registered to a(n) {found.GetType().Name}, not a Console Veriable!");
                return new InvalidKeywordArgument(id);
            }
            Debug.LogWarning($@"Error getting Console Variable '{id}' - No reference to this ID could be found in the current Data Container!");
            return new InvalidKeywordArgument(id);
        }

        /// <summary>
        /// Instantiate anything by type - even MonoBehaviours and ScriptableObjects.
        /// <para/> if your type is a MonoBehaviour, this is super slow - but hey, it works
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object SmartInstantiate(Type type) {
            
            if(typeof(MonoBehaviour).IsAssignableFrom(type)) {
                GameObject temp = new GameObject();
                object instance = temp.AddComponent(type);
                Destroy(temp);
                return instance;
            }

            if(typeof(ScriptableObject).IsAssignableFrom(type)) {
                ScriptableObject instance = ScriptableObject.CreateInstance(type);
                return instance;
            }

            return CreateCtor(type)();
        }

        /// <summary>
        /// Instantiate anything by type - even MonoBehaviours and ScriptableObjects.
        /// <para/> if your type is a MonoBehaviour, this is super slow - but hey, it works
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object SmartInstantiate(Type type, params object[] args) {

            if(typeof(MonoBehaviour).IsAssignableFrom(type)) {
                Debug.LogError("Error using SmartInstantiate - MonoBehaviours cannot be instantaited with constructor arguments!");
                return null;
            }

            if(typeof(ScriptableObject).IsAssignableFrom(type)) {
                Debug.LogError("Error using SmartInstantiate - ScriptableObjects cannot be instantaited with constructor arguments!");
                return null;
            }

            Type[] argtypes = new Type[args.Length];
            for(int x = 0; x < args.Length; x++)
                argtypes[x] = args[x].GetType();

            return CreateCtor(type, argtypes)(args);
        }

        /// <summary>
        /// A faster alternative to Activator.CreateInstance();
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static InjectedObjectActivatorNoParams CreateCtor(Type type) {
            ConstructorInfo emptyCtor = type.GetConstructor(Type.EmptyTypes);
            DynamicMethod newCtor = new DynamicMethod("MakeInstance", type, Type.EmptyTypes);
            ILGenerator gen = newCtor.GetILGenerator();
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Newobj, emptyCtor);
            gen.Emit(OpCodes.Ret);
            return (InjectedObjectActivatorNoParams)newCtor.CreateDelegate(typeof(InjectedObjectActivatorNoParams));
        }

        internal delegate object InjectedObjectActivatorNoParams();


        /// <summary>
        /// A faster alternative to Activator.CreateInstance();
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static InjectedObjectActivator CreateCtor(Type type, params Type[] types) {

            ConstructorInfo ctor = type.GetConstructor(types);

            if(ctor == null) {
                throw new ArgumentException(@$"Error creating dynamic constructor for {type.Name} - 
                    No constructor matching the parameters {types.ContentsToString()} could be found!");
            }

            DynamicMethod newCtor = new DynamicMethod(
                "MakeInstance", 
                type, 
                new[] { typeof(object[]) },
                type.Module,
                true
            );

            ILGenerator gen = newCtor.GetILGenerator();
            for(int x = 0; x < types.Count(); x++) {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldc_I4, x);
                gen.Emit(OpCodes.Ldelem_Ref);
                Type pType = types[x];
                if(pType.IsValueType) {
                    gen.Emit(OpCodes.Unbox_Any, pType);
                    continue;
                }
                gen.Emit(OpCodes.Castclass, pType);
            }
            
            gen.Emit(OpCodes.Newobj, ctor);
            gen.Emit(OpCodes.Ret);

            return (InjectedObjectActivator)newCtor.CreateDelegate(typeof(InjectedObjectActivator));
        }

        internal delegate object InjectedObjectActivator(object[] ctorargs);

        public static string GetTimecode() {
            TimeSpan span = DateTime.Now - DateTime.Today;
            string formatted = span.ToString(@"hh\:mm\:ss");
            return "[" + formatted + "]";
        }
    }

    internal interface IRegistrySubscriber {
        abstract void OnLoad();
    }

    /// <summary>
    /// Add this attribute to a parent class to expose registry functionality.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RegistrySubscriber : System.Attribute {}

    /// <summary>
    /// Add this attribute to a static method that takes a ConsoleVariableRegistrarC as its only parameter.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class ConsoleVariableRegistry : System.Attribute {}

    /// <summary>
    /// Add this attribute to a static method that takes a ConsoleCommandRegistrar as its only parameter.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class ConsoleCommandRegistry : System.Attribute {}


    /// <summary>
    /// Add this attribute to a static method with no parameters to subscribe to
    /// INFRASEC's startup cycle, which will execute subscribed methods at the correct time.
    /// In this context, registries have not been hit yet, so they cannot be referenced.
    /// That means Console Variables, ConsoleCommands, and SaveableData are inaccessible at
    /// this stage.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class Startup : System.Attribute {}
}
