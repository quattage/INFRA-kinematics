
// SIGNATURE :)

using System;
using Assets.quatworks.INFRASEC.Extensions;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.Core {

    public interface Movable {
        
        /// <summary>
        /// Collide-And-Slide implementation based on:
        /// https://www.peroxide.dk/papers/collision/collision.pdf -- 
        /// There are 3 sepereate implementations of this algorithm which are intended for slightly different things: <para/>
        /// -GetTraceCollisionResult() Is intended for use with Movables that are translating along a horizontal plane.<para/>
        /// -GetGravityCollisionResult() Is intended for use with Movables that are moving along the Y axis as a result of gravity. <para/>
        /// -GetSlidingCollisionResult() Is intended for use with Movables that are actively translating downwards along a slope.<para/>
        /// Each of these implementations return the resulting Vector3 velocity corrected for the collisions that are 
        /// encountered during execution.
        /// Collision results must be applied as the very last operation in your motion stack, as making 
        /// any modifications to velocity after collisions are applied will most likely result in the introduction of 
        /// edge cases that enable the Mover to phase through objects.
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static Vector3 GetTraceCollisionResult(Movable mover, Vector3 workingDirection, Vector3 workingPosition) {

            Vector3 accumulatedVelocity = Vector3.zero;
            Vector3 firstNormal = Vector3.zero;
            // ContactPatch contact = mover.GetContacts();
            // bool isAcceleratingTowardsStep = false;

            for(int i = 0; i < INFRA.Game.phys_collsteps.GetInt(); i++) {

                // if the velocity has been reduced to zero as a result of
                // future iterations, no more collision checks need to be done
                if(Mathf.Approximately(workingDirection.magnitude, 0)) break;

                // project a collider cast away from this Mover in the current velocity direction
                float dist = workingDirection.magnitude + INFRA.Game.phys_skinwidth.GetFloat();
                Vector3 castDirection = workingDirection.normalized;
                if(!mover.GetCollider().CastFrom(
                    workingPosition, // small note: the cast's origin is from the working position, which is altered between iterations - this makes collision much more robust.
                    ref castDirection,
                    ref dist,
                    INFRA.Game.LevelMask,
                    out RaycastHit hit
                )) { return accumulatedVelocity + workingDirection; }

                // get the direction of collision response following the current velocity and colliding surface
                Vector3 velocityAlongSurface = castDirection * (hit.distance - INFRA.Game.phys_skinwidth.GetFloat());

                // if the resulting velocity is extremely small, just ignore it for this iteration
                if(velocityAlongSurface.magnitude <= INFRA.Game.phys_skinwidth.GetFloat()) 
                    velocityAlongSurface = Vector3.zero;

                Vector3 remainingVelocity = workingDirection - velocityAlongSurface;

                // second bounce evaluates crease cases where the collider is wedged
                if(i == 1) {
                    Vector3 crease = Vector3.Cross(firstNormal, hit.normal).normalized;
                    float dotCrease = Vector3.Dot(remainingVelocity, crease);
                    workingDirection = crease * dotCrease;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                    continue;
                }

                // first bounce calculates standard sliding collisions and stairs
                if(i == 0) {
                    firstNormal = hit.normal;

                    // if the surface can't be walked on and the controller is grounded, handle potential stair or steep wall edge cases
                    if(Vector3.Angle(Vector3.up, hit.normal) >= INFRA.Game.move_maxslope.GetFloat() && mover.IsTouchingBelow()) {
                        #region this shit still dont work

                        // Vector3 contactDirection = hit.point - workingPosition;
                        // contactDirection.y = 0;
                        // contactDirection = contactDirection.normalized;

                        // float stepDisplacement = hit.point.y - contact.GetLastGroundPoint().y;

                        // if(stepDisplacement <= INFRA.Game.move_stepheight.GetFloat()) {

                        //     float stepCastDistance = INFRA.Game.move_stepdepth.GetFloat() + (INFRA.Game.phys_skinwidth.GetFloat() * 2);

                        //     if(!mover.GetCollider().CastFrom(
                            
                        //         workingPosition + velocityAlongSurface + 
                        //             new Vector3(0,  mover.GetCollider().GetLength1() - stepDisplacement + INFRA.Game.phys_skinwidth.GetFloat(), 0),
                        //         ref contactDirection,
                        //         ref stepCastDistance,
                        //         INFRA.Game.LevelMask,
                        //         out RaycastHit stepCast
                        //     )) {isAcceleratingTowardsStep = true; }
                        //     else {
                        //         float stepResultAngle = Vector3.Angle(stepCast.normal, Vector3.up);
                        //         if((stepCast.distance - INFRA.Game.phys_skinwidth.GetFloat()) > INFRA.Game.move_stepdepth.GetFloat() || stepResultAngle <= INFRA.Game.move_maxslope.GetFloat())
                        //             isAcceleratingTowardsStep = true;

                        //         if(stepResultAngle <= INFRA.Game.move_maxslope.GetFloat())
                        //             mover.GetContacts().MarkGround(ref hit, stepResultAngle);
                        //     }

                        //     // snap the controller up the step
                        //     if(isAcceleratingTowardsStep) {
                        //         velocityAlongSurface.y += stepDisplacement + (mover.GetCollider().GetLength1() / 2f);
                        //         velocityAlongSurface += contactDirection * (INFRA.Game.phys_skinwidth.GetFloat() * 2);
                        //         velocityAlongSurface += GetGroundSnapResult(mover, workingPosition + remainingVelocity + velocityAlongSurface, 50);
                        //     }

                        //     // zero out the Y components of velocity since we already knoww the controller has been snapped to the floor
                        //     iteratedNormal.y = 0;
                        //     iteratedNormal = iteratedNormal.normalized;
                        //     remainingVelocity.y = 0;
                        // }
                        #endregion
                    }
                    remainingVelocity = Vector3.ProjectOnPlane(remainingVelocity, firstNormal);
                    workingDirection = remainingVelocity;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                }
            }

            return accumulatedVelocity;
        }

        /// <summary>
        /// Collide-And-Slide implementation based on:
        /// https://www.peroxide.dk/papers/collision/collision.pdf -- 
        /// There are 3 sepereate implementations of this algorithm which are intended for slightly different things: <para/>
        /// -GetTraceCollisionResult() Is intended for use with Movables that are translating along a horizontal plane.<para/>
        /// -GetGravityCollisionResult() Is intended for use with Movables that are moving along the Y axis as a result of gravity. <para/>
        /// -GetSlidingCollisionResult() Is intended for use with Movables that are actively translating downwards along a slope.<para/>
        /// Each of these implementations return the resulting Vector3 velocity corrected for the collisions that are 
        /// encountered during execution.
        /// Collision results must be applied as the very last operation in your motion stack, as making 
        /// any modifications to velocity after collisions are applied will most likely result in the introduction of 
        /// edge cases that enable the Mover to phase through objects.
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static Vector3 GetGravityCollisionResult(Movable mover, Vector3 workingDirection, Vector3 workingPosition) {

            Vector3 accumulatedVelocity = Vector3.zero;
            Vector3 iteratedNormal = Vector3.zero;

            for(int i = 0; i < INFRA.Game.phys_collsteps.GetInt(); i++) {

                if(Mathf.Approximately(workingDirection.magnitude, 0)) break;

                float dist = workingDirection.magnitude + INFRA.Game.phys_skinwidth.GetFloat();
                Vector3 castDirection = workingDirection.normalized;
                if(!mover.GetCollider().CastFrom(
                    workingPosition,
                    ref castDirection,
                    ref dist,
                    INFRA.Game.LevelMask,
                    out RaycastHit hit
                )) { return accumulatedVelocity + workingDirection; }

                // get the incline of the surface
                float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);

                // get the direction of collision response following the current velocity and colliding surface
                Vector3 velocityAlongSurface = castDirection * (hit.distance - INFRA.Game.phys_skinwidth.GetFloat());

                // if the resulting velocity is very small, just ignore it for this iteration
                if(velocityAlongSurface.magnitude <= INFRA.Game.phys_skinwidth.GetFloat()) 
                    velocityAlongSurface = Vector3.zero;

                // if the surface is nearly level, ignore all other calculations and return
                if(surfaceAngle <= INFRA.Game.move_maxslope.GetFloat()) {
                    accumulatedVelocity += velocityAlongSurface;
                    return accumulatedVelocity;
                }

                Vector3 remainingVelocity = workingDirection - velocityAlongSurface;

                // second bounce evaluates crease cases where the collider is wedged
                if(i == 1) {
                    Vector3 crease = Vector3.Cross(iteratedNormal, hit.normal).normalized;
                    float dotCrease = Vector3.Dot(remainingVelocity, crease);
                    workingDirection = crease * dotCrease;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                    continue;
                }

                // first bounce calculates barebones gravity collisions
                if(i == 0) {
                    iteratedNormal = hit.normal;
                    remainingVelocity = Vector3.ProjectOnPlane(remainingVelocity, iteratedNormal);
                    workingDirection = remainingVelocity;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                }
            }

            return accumulatedVelocity;
        }

        /// <summary>
        /// Collide-And-Slide implementation based on:
        /// https://www.peroxide.dk/papers/collision/collision.pdf -- 
        /// There are 3 sepereate implementations of this algorithm which are intended for slightly different things: <para/>
        /// -GetTraceCollisionResult() Is intended for use with Movables that are translating along a horizontal plane.<para/>
        /// -GetGravityCollisionResult() Is intended for use with Movables that are moving along the Y axis as a result of gravity. <para/>
        /// -GetSlidingCollisionResult() Is intended for use with Movables that are actively translating downwards along a slope.<para/>
        /// Each of these implementations return the resulting Vector3 velocity corrected for the collisions that are 
        /// encountered during execution.
        /// Collision results must be applied as the very last operation in your motion stack, as making 
        /// any modifications to velocity after collisions are applied will most likely result in the introduction of 
        /// edge cases that enable the Mover to phase through objects.
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static Vector3 GetSlidingCollisionResult(Movable mover, Vector3 workingDirection, Vector3 workingPosition) {

            Vector3 accumulatedVelocity = Vector3.zero;
            Vector3 iteratedNormal = new Vector3();

            for(int i = 0; i < INFRA.Game.phys_collsteps.GetInt(); i++) {

                if(Mathf.Approximately(workingDirection.magnitude, 0)) break;

                float dist = workingDirection.magnitude + INFRA.Game.phys_skinwidth.GetFloat();
                Vector3 castDirection = workingDirection.normalized;
                if(!mover.GetCollider().CastFrom(
                    workingPosition,
                    ref castDirection,
                    ref dist,
                    INFRA.Game.LevelMask,
                    out RaycastHit hit
                )) { return accumulatedVelocity + workingDirection; }

                // get the direction of collision response following the current velocity and colliding surface
                Vector3 velocityAlongSurface = castDirection * (hit.distance - INFRA.Game.phys_skinwidth.GetFloat());

                // if the resulting velocity is very small, just ignore it for this iteration
                if(velocityAlongSurface.magnitude <= INFRA.Game.phys_skinwidth.GetFloat()) 
                    velocityAlongSurface = Vector3.zero;

                Vector3 remainingVelocity = workingDirection - velocityAlongSurface;

                // second bounce evaluates crease cases where the collider is wedged
                if(i == 1) {
                    Vector3 crease = Vector3.Cross(iteratedNormal, hit.normal).normalized;
                    float dotCrease = Vector3.Dot(remainingVelocity, crease);
                    workingDirection = crease * dotCrease;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                    continue;
                }

                // first bounce calculates standard sliding collisions
                // in this case, slope steepness checks to eliminate sliding
                // are deliberately skipped.
                if(i == 0) {
                    iteratedNormal = hit.normal;
                    remainingVelocity = Vector3.ProjectOnPlane(remainingVelocity, iteratedNormal);
                    workingDirection = remainingVelocity;
                    accumulatedVelocity += velocityAlongSurface;
                    workingPosition += velocityAlongSurface;
                }
            }

            return accumulatedVelocity;
        }

        /// <summary>
        /// Projected velocity acceleration algorithm mirroring its implementation from the Source engine:
        /// https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/game/shared/gamemovement.cpp <para/>
        /// Returns a velocity vector representing the input velocity accelerated towards the wishdir at the given accel 
        /// and speed values. Speed is capped to cvar move_terminal internally.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static Vector3 GetSourcelikeAcceleration(ref Vector3 wishdir, Vector3 velocity, float wishspeed, float accel) {
            float projectedvel = Vector3.Dot(velocity, wishdir) / Time.fixedDeltaTime;
            float addedspeed = wishspeed - projectedvel;
            if(addedspeed <= 0) return velocity;
            float accelspeed = accel * Time.fixedDeltaTime * wishspeed;
            if(accelspeed > addedspeed) accelspeed = addedspeed;
            if(projectedvel > INFRA.Game.move_terminal.GetFloat())
                accelspeed = INFRA.Game.move_terminal.GetFloat() - projectedvel;
            return velocity + wishdir * accelspeed;
        }

        /// <summary>
        /// Projected velocity acceleration algorithm mirroring its implementation from the Source engine:
        /// https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/game/shared/gamemovement.cpp <para/>
        /// Returns a velocity vector representing the input velocity accelerated towards the wishdir at the given accel 
        /// and speed values. Speed is capped to cvar move_terminal internally.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static Vector3 GetSourcelikeAcceleration(Vector3 wishdir, Vector3 velocity, float wishspeed, float accel) {
            return GetSourcelikeAcceleration(ref wishdir, velocity, wishspeed, accel);
        }

        /// <summary>
        /// A standard implementation of the Surface Friction formula.
        /// Calls to this method automatically apply friction forces
        /// to the given Movable based on its surroundings.
        /// If the movable is not touching any known surface, 
        /// the Normal Force is assumed to be Vector3.Up.
        /// 
        /// Optionally, you may supply your own coefficient of friction
        /// to bypass the automatic surroundings check.
        /// 
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static void ApplyFriction(Movable mover) {
            Vector3 velo = mover.GetVelocity();
            ContactPatch contact = mover.GetContacts();
            Vector3 surfaceNormal = contact.GetGroundSurfaceNormal();
            Vector3 kF;
            if(mover.IsTouchingBelow()) 
                kF = -INFRA.Game.phys_groundfriction.GetFloat() * (velo - (Vector3.Dot(velo, surfaceNormal) * surfaceNormal));
            else kF = -INFRA.Game.phys_airfriction.GetFloat() * (velo - (Vector3.Dot(velo, surfaceNormal) * surfaceNormal));
            mover.SetVelocity(velo + kF);
        }

        /// <summary>
        /// A standard implementation of the Surface Friction formula.
        /// Calls to this method automatically apply friction forces
        /// to the given Movable based on its surroundings.
        /// If the movable is not touching any known surface, 
        /// the Normal Force is assumed to be Vector3.Up.
        /// 
        /// Optionally, you may supply your own coefficient of friction
        /// to bypass the automatic surroundings check.
        /// 
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="workingDirection"></param>
        /// <param name="workingPosition"></param>
        /// <returns></returns>
        public static void ApplyFriction(Movable mover, float coeff) {
            Vector3 velo = mover.GetVelocity();
            ContactPatch contact = mover.GetContacts();
            Vector3 surfaceNormal = contact.GetGroundSurfaceNormal();
            Vector3 kF = -coeff * (velo - (Vector3.Dot(velo, surfaceNormal) * surfaceNormal));
            mover.SetVelocity(velo + kF);
        }


        /// <summary>
        /// Returns TRUE if the mover is reasonably facing the given RaycastHit's normal vector.
        /// </summary>
        /// <param name="mover"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool IsFacing(Movable mover, RaycastHit? hit) {
            if(!hit.HasValue) return false;
            float diff = Mathf.Abs(mover.GetForward().DotXZ(hit.Value.normal));
            return diff < 0.8f;
        }


        /// <summary>
        /// Gets a vector representing the velocity required to snap the bottom of this 
        /// Mover to the nearest ground surface, if a ground surface exists close by.
        /// </summary>
        public static Vector3 GetGroundSnapResult(Movable mover, Vector3 position) {
            return GetGroundSnapResult(mover, position, INFRA.Game.phys_skinwidth.GetFloat() + INFRA.Game.move_stepheight.GetFloat());
        }

        /// <summary>
        /// Gets a vector representing the velocity required to snap the bottom of this 
        /// Mover to the nearest ground surface, if a ground surface exists close by.
        /// </summary>
        public static Vector3 GetGroundSnapResult(Movable mover, Vector3 position, float projectionDistance) {

            if(!mover.GetCollider().CastFrom(
                position, 
                Vector3.down, 
                ref projectionDistance, 
                INFRA.Game.LevelMask, 
                out RaycastHit hit
            )) { return Vector3.zero; }

            float difference = hit.distance - INFRA.Game.phys_skinwidth.GetFloat();
            float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);

            if(difference < INFRA.Game.move_stepheight.GetFloat() && surfaceAngle <= INFRA.Game.move_maxslope.GetFloat()) {
                mover.GetContacts().MarkGround(ref hit, surfaceAngle);
                return new Vector3(0, -difference, 0);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Collects contact information about ground collision 
        /// surfaces surrounding this Mover.
        /// </summary>
        public static bool InspectGroundSurface(Movable mover) {
            RaycastHit[] groundHits = mover.GetCollider().CastDownwards(INFRA.Game.LevelMask);
            for(int x = 0; x < groundHits.Length; x++) {
                RaycastHit thisHit = groundHits[x];
                if(Mathf.Approximately(thisHit.distance, 0)) continue;
                float surfaceAngle = Vector3.Angle(Vector3.up, thisHit.normal);
                mover.GetContacts().MarkGround(ref thisHit, surfaceAngle);
                if(surfaceAngle <= INFRA.Game.move_maxslope.GetFloat()) {
                    mover.SetIsGrounded(true);
                    return true;
                }
            }
            mover.SetIsGrounded(false);
            return false;
        }


        /// <summary>
        /// Collects contact information about the primary
        /// ceiling surface above this Mover.
        /// </summary>
        public static bool InspectCeilingSurface(Movable mover) {
            mover.GetContacts().ForgetGround();
            RaycastHit[] headBonks = mover.GetCollider().CastUpwards(INFRA.Game.LevelMask);
            if(headBonks.IsNullOrEmpty()) {
                mover.SetIsTouchingAbove(false);
                return false;
            }
            mover.SetIsTouchingAbove(true);
            float surfaceAngle = Vector3.Angle(Vector3.up, headBonks[0].normal);
            float motionAngle = Vector3.Angle(mover.GetVelocity().normalized, headBonks[0].normal);
            if(surfaceAngle >= INFRA.Game.move_minceiling.GetFloat() || motionAngle >= INFRA.Game.move_minceiling.GetFloat()) {
                mover.GetContacts().MarkCeiling(ref headBonks[0], surfaceAngle);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Collects contact information about wall surfaces
        /// to the left of this mover.
        /// </summary>
        public static bool InspectLeftWallSurface(Movable mover) {
            RaycastHit[] wallHits = mover.GetCollider().CastLeft(mover, INFRA.Game.LevelMask);
            for(int x = 0; x < wallHits.Length; x++) {
                RaycastHit thisHit = wallHits[x];
                if(Mathf.Approximately(thisHit.distance, 0)) continue;
                float surfaceAngle = Vector3.Angle(Vector3.up, thisHit.normal);
                mover.GetContacts().MarkLeftWall(ref thisHit, surfaceAngle);
            }
            return false;
        }

        /// <summary>
        /// Collects contact information about wall surfaces
        /// to the right of this mover.
        /// </summary>
        public static bool InspectRightWallSurface(Movable mover) {
            RaycastHit[] wallHits = mover.GetCollider().CastRight(mover, INFRA.Game.LevelMask);
            for(int x = 0; x < wallHits.Length; x++) {
                RaycastHit thisHit = wallHits[x];
                if(Mathf.Approximately(thisHit.distance, 0)) continue;
                float surfaceAngle = Vector3.Angle(Vector3.up, thisHit.normal);
                mover.GetContacts().MarkRightWall(ref thisHit, surfaceAngle);
            }
            return false;
        }

        /// <summary>
        /// Translation in all 3 axes. Useful for moving horizontally or flying.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void TranslateMove(ref Vector3 amount);

        /// <summary>
        /// Translation in all 3 axes. Useful for moving horizontally or flying.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void TranslateMove(Vector3 amount);

        /// <summary>
        /// A dedicated vertical axis of translation for jumping
        /// or flying upward. The amount passed should always be positive.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void TranslateUp(float amount);


        /// <summary>
        /// A dedicated vertical axis of translation for crouching,
        /// groundpounding, or flying downard. The amount passed 
        /// should always be negative.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void TranslateDown(float amount);

        /// <summary>
        /// Rotation of the mouse or gamepad joystick.
        /// Typically used to make a Movable look around.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void Rotate(ref Vector3 amount);

        /// <summary>
        /// Rotation of the mouse or gamepad joystick.
        /// Typically used to make a Movable look around.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void Rotate(ref Quaternion amount);

        /// <summary>
        /// An auxilury translation magnitude input. 
        /// Useful for sprinting.
        /// </summary>
        /// <param name="amount"></param>
        public abstract void Impulse(float amount);

        /// <summary>
        /// Initialize this Movable in a controlled
        /// context - Use this instead of Awake().
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Gets the currently active ControlMotor
        /// </summary>
        /// <returns></returns>
        public abstract MotionStack GetCurrentMotor();
        public abstract WrappedCollider GetCollider();

        /// <summary>
        /// Returns a ContactPatch describing
        /// the surrounding RaycastHit surface points
        /// that this Movable's collider is touching.
        /// </summary>
        /// <returns></returns>
        public abstract ContactPatch GetContacts();
        public abstract LayerMask GetMask();

        public abstract bool ToNextMotor();
        public abstract bool ToPreviousMotor();
        public abstract bool ToMotor(string id);

        public abstract Quaternion GetRootRotation();

        public abstract Vector3 GetEulers();

        /// <summary>
        /// Returns a normalized vector representing forward on the local Z.
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetForward();

        public abstract void SetVelocity(Vector3 velocity);
        public abstract void SetVelocity(float vX, float vY, float vZ);
        public abstract Vector3 GetVelocity();
        public abstract float GetSpeed();

        public abstract bool IsTouchingAbove();
        public abstract void SetIsTouchingAbove(bool value);
        public abstract bool IsTouchingBelow();
        public abstract void SetIsGrounded(bool value);


        /// <summary>
        /// Retrieves a Vector3 representing how much
        /// the rotation has changed since rotations
        /// were last applied to this Mover.
        /// (rotations are traditionally applied within the same game tick that they are received)
        /// <para/> Note: For some Movers, this
        /// method may not be relevent, depending on the
        /// implemenation requirements.
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetRotationDeltas();

        /// <summary>
        /// Sets the position of this Mover
        /// </summary>
        public abstract void SetPosition(Vector3 position);

        /// <summary>
        /// Gets the position of this Mover
        /// </summary>
        public abstract Vector3 GetPosition();

        /// <summary>
        /// Applies an overriding rotation regardless of Movable input.
        /// <para/> Differs from Rotate() in that this method SETS the rotation
        /// in the world-space, rather than modifying it.
        /// </summary>
        /// <param name="rotation"></param>
        public abstract void RotateToFace(Vector3 rotation, float delta);

        /// <summary>
        /// Applies an overriding rotation regardless of Movable input.
        /// <para/> Differs from Rotate() in that this method SETS the rotation
        /// in the world-space, rather than modifying it.
        /// </summary>
        /// <param name="rotation"></param>
        public abstract void RotateToFace(float rX, float rY, float rZ, float delta);

        /// <summary>
        /// Applies an overriding rotation regardless of Movable input.
        /// <para/> Differs from Rotate() in that this method SETS the rotation
        /// in the world-space, rather than modifying it.
        /// </summary>
        /// <param name="rotation"></param>
        public abstract void RotateToFace(Quaternion rotation, float delta);

        /// <summary>
        /// Applies an overriding rotation regardless of Movable input.
        /// <para/> Differs from Rotate() in that this method SETS the rotation
        /// in the world-space, rather than modifying it.
        /// </summary>
        /// <param name="rotation"></param>
        public abstract void RotateToFaceY(float rY, float delta);

        /// <summary>
        /// Immediately cancels all intertial forces and locks 
        /// this Mover in place
        /// </summary>
        public abstract void FreezeTranslation();

        /// <summary>
        /// Restores translation authority to this Mover
        ///  - The opposite of FreezeTranslation()
        /// </summary>
        public abstract void UnFreezeTranslation();

        /// <summary>
        /// Immediately cancels all rotational forces and locks 
        /// this Mover's rotation.
        /// </summary>
        public abstract void FreezeRotation();

        /// <summary>
        /// Restores rotation authority to this Mover
        ///  - The opposite of FreezeRotation()
        /// </summary>
        public abstract void UnFreezeRotation();
    }





    /// <summary>
    /// The basic instantiating composer for MotionStackss.
    /// </summary>
    public abstract class MovingElement : MonoBehaviour, Movable, IMotionStackCallbacks {

        [SerializeField]
        private MotionStack[] _movementSchema;
        protected MotionStack _current;
        private int _currentIndex;

        [SerializeField]
        public Rotatatron[] Children;
        private Quaternion _combinedRotation = Quaternion.identity;
        protected ContactPatch _contact = new EmptyContacter();

        [NonSerialized] public float RotDeltaX = 0;
        [NonSerialized] public float RotDeltaY = 0;
        [NonSerialized] public float RotDeltaZ = 0;

        private bool _canTranslate = true;
        private bool _isTouchingBelow;
        private bool _isTouchingAbove;

        public abstract void Initialize();
        protected abstract void OnUpdate();
        protected abstract void OnFixedUpdate();
        public abstract void SetVelocity(Vector3 velocity);
        public abstract void SetVelocity(float vX, float vY, float vZ);
        public abstract Vector3 GetVelocity();
        public abstract float GetSpeed();
        

        public abstract void SetPosition(Vector3 position);
        public abstract void SetPosition();
        public abstract Vector3 GetPosition();

        public LayerMask GetMask() {
            return INFRA.Game.MovableMask;
        }

        public abstract WrappedCollider GetCollider();

        public ContactPatch GetContacts() {
            return _contact;
        }

        private void Awake() {
            if(_movementSchema.IsNullOrEmpty())
                _movementSchema = MotionStack.Populate(this);
            _currentIndex = 0;
            _current = _movementSchema[_currentIndex];
            _current.OnActivate(this);
            Initialize();
            GetCollider().Initialize();
        }

        private void Update() {
            _current.OnUpdate(this);
            OnUpdate();
        }
        

        private void FixedUpdate() {
            _current.ExecuteMotionStack(this);
            OnFixedUpdate();
        }

        public virtual void OnDestroy() {
            for(int x = 0; x < _movementSchema.Length; x++) {
                if(_movementSchema[x] == null) continue;
                _movementSchema[x].OnDestroy();
                _movementSchema[x] = null;
            }
            _movementSchema = null;
            _current = null;
            _currentIndex = -1;
            Children = null;
            _contact = null;
            _combinedRotation = Quaternion.identity;
            _canTranslate = false;
        }

        public MotionStack GetCurrentMotor() {
            return _current;
        }

        public bool ToNextMotor() {
            if(_movementSchema.Length <= 1) return false;
            _currentIndex++;
            if(_currentIndex >= _movementSchema.Length) _currentIndex = 0;
            MotionStack oldMotor = _current;
            MotionStack newMotor = _movementSchema[_currentIndex];
            _current = newMotor;
            oldMotor.OnDeactivate(this, newMotor);
            newMotor.OnActivate(this);
            return true;
        }

        public bool ToPreviousMotor() {
            if(_movementSchema.Length <= 1) return false;
            _currentIndex--;
            if(_currentIndex <= 0) _currentIndex = _movementSchema.Length - 1;
            MotionStack oldMotor = _current;
            MotionStack newMotor = _movementSchema[_currentIndex];
            _current = newMotor;
            oldMotor.OnDeactivate(this, newMotor);
            newMotor.OnActivate(this);
            return true;
        }

        public bool ToMotor(string id) {
            if(_movementSchema.Length <= 1) return false;
            for(int x = 0; x < _movementSchema.Length; x++) {
                MotionStack newMotor = _movementSchema[x];
                if(!newMotor.GetID().Equals(id))
                    continue;

                MotionStack oldMotor = _current;
                if(oldMotor.Equals(newMotor))
                    return false;

                _currentIndex = x;
                _current = newMotor;
                oldMotor.OnDeactivate(this, newMotor);
                newMotor.OnActivate(this);
                return true;
            }
            Debug.LogWarning($"Attempted to swtich Mover '{gameObject.transform.name}' to MotionStacks '{id}' but a movement motor with this name doesn't exist!");
            return false;
        }

        public virtual void Rotate(ref Vector3 amount) {
            _current.OnRotate(ref amount, this);
        }

        public virtual void Rotate(ref Quaternion amount) {
            _current.OnRotate(ref amount, this);
        }

        public virtual void Rotate(Quaternion amount) {
            _current.OnRotate(ref amount, this);
        }

        public virtual void LerpYRotation(float yRot) {
            RotDeltaY += yRot * Time.deltaTime * 2.4f;
            PushRotations();
        }

        public virtual void PushRotations() {
            for(int x = 0; x < Children.Length; x++)
                Children[x].Rotate(RotDeltaX, RotDeltaY, RotDeltaZ);
            _combinedRotation *= Quaternion.Euler(RotDeltaX, RotDeltaY, RotDeltaZ); 
            RotDeltaX = 0;
            RotDeltaY = 0;
            RotDeltaZ = 0;
        }

        public void TranslateMove(ref Vector3 amount) {
            if(_canTranslate)
                _current.OnTranslate(ref amount, this);
        }

        public void TranslateMove(Vector3 amount) {
            if(_canTranslate)
                _current.OnTranslate(ref amount, this);
        }

        public void TranslateUp(float amount) {
            if(_canTranslate)
                _current.OnTranslateUp(amount, this);
        }

        public void TranslateDown(float amount) {
            if(_canTranslate)
                _current.OnTranslateDown(amount, this);
        }

        public void Impulse(float amount) {
            if(_canTranslate)
                _current.OnImpulse(amount, this);
        }

        public Quaternion GetRootRotation() {
            return Children[0].GetGlobalRotation();
        }

        public Vector3 GetForward() {
            return (GetRootRotation() * Vector3.forward).normalized;
        }
        
        public Vector3 GetEulers() {
            return _combinedRotation.eulerAngles;
        }

        public Vector3 GetRotationDeltas() {
            return new Vector3(RotDeltaX, RotDeltaY, RotDeltaZ);
        }

        public void RotateToFace(Vector3 rotation, float delta) {
            Vector3 current = GetEulers();
            RotDeltaX = Mathf.LerpAngle(RotDeltaX, current.x - rotation.x, delta);
            RotDeltaY = Mathf.LerpAngle(RotDeltaY, current.y - rotation.y, delta);
            RotDeltaZ = Mathf.LerpAngle(RotDeltaZ, current.z - rotation.z, delta);
            PushRotations();
        }

        public void RotateToFace(float rX, float rY, float rZ, float delta) {
            Vector3 current = GetEulers();
            RotDeltaX = Mathf.LerpAngle(RotDeltaX, current.x - rX, delta);
            RotDeltaY = Mathf.LerpAngle(RotDeltaY, current.y - rY, delta);
            RotDeltaZ = Mathf.LerpAngle(RotDeltaZ, current.z - rZ, delta);
            PushRotations();
        }

        public void RotateToFace(Quaternion rotation, float delta) {
            RotateToFace(rotation.eulerAngles, delta);
        }

        public void RotateToFaceY(float rY, float delta) {
            RotDeltaY = Mathf.LerpAngle(RotDeltaY, rY - GetRootRotation().eulerAngles.y, delta);
            PushRotations();
        }


        public void FreezeTranslation() {
            _canTranslate = false;
        }

        public void UnFreezeTranslation() {
            _canTranslate = true;
        }

        public void FreezeRotation() {
            for(int x = 0; x < Children.Length; x++)
                Children[x].Disable();
        }

        public void UnFreezeRotation() {
            for(int x = 0; x < Children.Length; x++)
                Children[x].Enable();
        }        

        public bool IsTouchingAbove() {
            return _isTouchingAbove;
        }

        public bool IsTouchingBelow() {
            return _isTouchingBelow;
        }

        public void SetIsTouchingAbove(bool value) {
            _isTouchingAbove = value;
        }

        public void SetIsGrounded(bool value) {
            _isTouchingBelow = value;
        }

        public virtual void OnJump(JumpContext ctx) {}
        public virtual  void WhileInAir() {}
        public virtual void OnLand() {}
        public virtual void OnWallrunEnter(WallRunContext ctx) {}
        public virtual void WhileWallrunPerformed(WallRunContext ctx) {}
        public virtual void OnWallrunExit() {}
        public virtual void OnCrouchEnter() {}
        public virtual void WhileCrouchPerformed() {}
        public virtual void OnCrouchExit() {}
        public virtual void OnSlideEnter() {}
        public virtual void WhileSlidePerformed() {}
        public virtual void OnSlideExit() {}
    }
}
