
// SIGNATURE :)

using System;
using Assets.quatworks.INFRASEC.Kinematics.Core;
using UnityEngine;

namespace Assets.quatworks.INFRASEC.Kinematics.MotionStacks {

    /// <summary>
    /// A MotionStack that implements source-like velocity, full collision detection,
    /// and gravity. This motion stack can also handle basic movement tasks such as
    /// jumping, crouching, and wallrunning.
    /// </summary>
    public class NimbleMotionStack : MotionStack {

        private Vector3 _wishdir = Vector3.zero;
        private int _groundframes = 0;

        private bool _isJumping = false;
        private bool _intendsToJump = false;
        private bool _usedSecondJump = false;
        private float _jumpCooldown = 0;
        
        private bool _isSliding = false;
        private bool _needsSpeedBoost = false;
        private float _slideTime = 0;

        private bool _isMovingBackward = false;
        private bool _isMovingForward = false;
        private bool _isTryingToSprint = false;
        private bool _isCrouching = false;
        private bool _intendsToCrouch = false;
        private bool _isAirstrafing = false;

        private bool _isWallrunning = false;
        private RaycastHit? _wall = null;
        private Vector3 _prevWallNormal = Vector3.zero;
        private float _wallRunCooldown = 1f;
        private float _wallRunTime = 0f;

        private float _posturePercent = 1;
        private float _sprintBonus = 1;

        [SerializeField] private Transform _head;
        [SerializeField] private float _crouchDisplacement;

        private Vector3 _originalHeadPos;
        private Vector3 _crouchHeadPos;
        private Vector3 _speed = Vector3.zero;

        private WallRunContext ctx;

        internal override string GetID() {
            return "nimble";
        }

        internal override void OnActivate(MovingElement initializer) {
            initializer.GetCollider().Recompute();
            _originalHeadPos = _head.transform.localPosition;
            _crouchHeadPos = _originalHeadPos + new Vector3(0, _crouchDisplacement, 0);
        }

        internal override void OnDeactivate(MovingElement deinitializer, MotionStack subsequent) {
            
        }

        internal override void OnUpdate(MovingElement mover) {
            _posturePercent = Mathf.Lerp(_posturePercent, mover.GetCollider().GetSizePercent(), Time.deltaTime * (1 / Time.fixedDeltaTime));
            _head.localPosition = Vector3.Lerp(_crouchHeadPos, _originalHeadPos, _posturePercent);
        }

        internal override void ExecuteMotionStack(MovingElement mover) {
            
            float currentSize = mover.GetCollider().GetSizePercent();
            bool isShrunk = Mathf.Abs(currentSize - 1) > 0.001f;
            bool isExpanded = currentSize > 0.5f;

            HandleCrouching(mover, ref isShrunk, ref isExpanded);
            ContactPatch contact = mover.GetContacts();
            mover.GetCollider().Recompute();
            contact.Clear();
            Movable.InspectGroundSurface(mover);
            Movable.InspectCeilingSurface(mover);
            Movable.InspectRightWallSurface(mover);
            Movable.InspectLeftWallSurface(mover);
            HandleWallrunInitiation(contact, mover, ref isExpanded);
            HandleFriction(mover);

            if(_jumpCooldown > 0) _jumpCooldown -= Time.fixedDeltaTime * 6f;
            else _jumpCooldown = 0;

            if(_isWallrunning) {
                // early return for cancelling wallruns when the player crouches or leaves the wall
                if(contact.IsTouchingGround() || _intendsToCrouch || !isExpanded || !_wall.HasValue || _wallRunTime > INFRA.Game.move_wallruntime.GetFloat()) {
                    contact.ForgetGround();
                    contact.ForgetLeftWall();
                    contact.ForgetRightWall();
                    mover.SetIsGrounded(false);
                    _isSliding = false;
                    _needsSpeedBoost = false;
                    _wall = null;
                    _isWallrunning = false;
                    _wallRunTime = 0f;
                    ApplyVelocity(ref _speed, mover);
                    mover.OnWallrunExit();
                    return;
                }

                // Once the player has contacted a wall, they're stuck to it. This cast collects nearby walls
                // continuously as long as the wallrun timer hasn't run out.
                WrappedCollider coll = mover.GetCollider();
                RaycastHit[] wallHits = Physics.SphereCastAll(coll.GetCenter(), coll.GetLength1() - INFRA.Game.phys_skinwidth.GetFloat() * 2, -_prevWallNormal, 2, INFRA.Game.LevelMask);
                if(wallHits.Length <= 0) {   // <- early return if SphereCastAll produces no wall collisions
                    contact.ForgetGround();
                    contact.ForgetLeftWall();
                    contact.ForgetRightWall();
                    mover.SetIsGrounded(false);
                    _isSliding = false;
                    _needsSpeedBoost = false;
                    _wall = null;
                    _isWallrunning = false;
                    _wallRunTime = 0f;
                    ApplyVelocity(ref _speed, mover);
                    mover.OnWallrunExit();
                    return;
                }

                // Set up values for this iteration and acquire an average normal vector from 
                // any number of contact hit points.
                // This should only ever iterate 3 or so times in worst case scenarios, it just
                // smooths out the transition between walls made of multiple colliders.
                _wallRunCooldown = 1f;
                _isJumping = false;
                _isSliding = false;
                _usedSecondJump = false;
                _wallRunTime += Time.fixedDeltaTime;
                Vector3 avgNormal = Vector3.zero;
                Vector3 wallPoint = Vector3.zero;
                float totalWeight = 0;
                float distance = float.MaxValue;
                for(int x = 0; x < wallHits.Length; x++) {
                    RaycastHit wallHit = wallHits[x];
                    float weight = 1f / (wallHit.distance + 0.01f);
                    totalWeight += weight;
                    avgNormal += wallHit.normal * weight;
                    if(wallHit.distance < distance) {
                        distance = wallHit.distance;
                        if(wallHit.collider is MeshCollider mc) {
                            if(mc.convex) {
                                wallPoint = Physics.ClosestPoint(mover.GetPosition(), wallHit.collider, wallHit.collider.transform.position, wallHit.collider.transform.rotation);
                                continue;
                            }
                            if(!Physics.Raycast(mover.GetPosition(), -wallHit.normal, out RaycastHit tryHit)) 
                                continue;
                            wallPoint = tryHit.point;
                            continue;
                        }
                        wallPoint = Physics.ClosestPoint(mover.GetPosition(), wallHit.collider, wallHit.collider.transform.position, wallHit.collider.transform.rotation);
                    }
                }
                avgNormal /= totalWeight; avgNormal.Normalize();
                /////////////////////////////////////////////////////////////

                float incline = Vector3.Angle(avgNormal, Vector3.up);
                float grade = Vector3.Angle(avgNormal, _prevWallNormal);

                if(Mathf.Abs(grade) > 25f) {
                    contact.ForgetGround();
                    contact.ForgetLeftWall();
                    contact.ForgetRightWall();
                    mover.SetIsGrounded(false);
                    _isSliding = false;
                    _needsSpeedBoost = false;
                    _wall = null;
                    _isWallrunning = false;
                    _wallRunTime = 0f;
                    ApplyVelocity(ref _speed, mover);
                    mover.OnWallrunExit();
                    return;
                }

                Vector3 tangent = Vector3.Cross(avgNormal, Vector3.up).normalized;
                Quaternion initial = mover.GetRootRotation();
                Vector3 localF = initial * Vector3.forward; 
                Vector3 handedTangent = localF.Project(avgNormal);
                float aimAlignment = handedTangent.DotXZ(tangent);
                float lookAlignment = localF.DotXZ(avgNormal);

                if(_wallRunTime < 2f * Time.fixedDeltaTime) {
                    float opN = avgNormal.DotXZ(mover.GetVelocity().normalized);
                    if(!_usedSecondJump && Mathf.Abs(opN) < 0.001f) {
                        _wallRunTime = 0f;
                        _wall = null;
                        _isWallrunning = false;
                        ApplyVelocity(ref _speed, mover);
                        return;
                    }
                    ctx = new(ref initial, ref wallPoint, ref avgNormal, ref tangent, ref handedTangent, ref lookAlignment, contact.IsTouchingLeftWall(), ref _wallRunTime);
                    mover.OnWallrunEnter(ctx);  
                    _speed -= avgNormal * Time.fixedDeltaTime * 40f;
                } else {
                    ctx.Normal = avgNormal;
                    ctx.Point = wallPoint;
                    ctx.Tangent = tangent;
                    ctx.LookProj = handedTangent;
                    ctx.Time = _wallRunTime;
                    ctx.Alignment = lookAlignment;
                }

                if(incline > 90 + INFRA.Game.move_wallrunangle.GetFloat() || incline < 90 - INFRA.Game.move_wallrunangle.GetFloat()) {
                    _wallRunTime = 0f;
                    _wall = null;
                    _isWallrunning = false;
                    ApplyVelocity(ref _speed, mover);
                    mover.OnWallrunExit();
                    return;
                }

                _speed = Movable.GetSourcelikeAcceleration(_wishdir, mover.GetVelocity().Project(avgNormal), INFRA.Game.move_speed.GetFloat() * 2.2f, INFRA.Game.move_accel.GetFloat() * 0.7f);
                _speed = _speed.ApproachYZero(Time.fixedDeltaTime * ctx.EvalTime * 8f).Project(avgNormal);
                float moveMagnitude = _speed.XZMagnitude();
                if(moveMagnitude < 0.09) _wallRunTime += Time.fixedDeltaTime * 2f;
                Vector3 currentGravity = Vector3.Lerp(INFRA.Game.GetGravity(), Vector3.zero, ctx.EvalTime);

                if(_intendsToJump) {
                    INFRA.Game.Input.Jump.Consume();
                    if(!_isJumping && _jumpCooldown <= 0) {
                        _isJumping = true;
                        _isSliding = false;
                        _needsSpeedBoost = false;
                        _speed.y = INFRA.Game.move_jumpstrength.GetFloat() * Time.fixedDeltaTime * 0.81f;
                        _speed += avgNormal * Time.fixedDeltaTime * 0.2f;
                        contact.ForgetGround();
                        contact.ForgetLeftWall();
                        contact.ForgetRightWall();
                        mover.SetIsGrounded(false);
                        _wall = null;
                        _isWallrunning = false;
                        _jumpCooldown = 1;
                        mover.OnJump(new JumpContext(false));
                        ApplyVelocity(ref _speed, mover);
                        mover.OnWallrunExit();
                        return;
                    }
                }

                // refund wallrun timer as the player runs across a concave corner 
                if(_wallRunTime > 2f * Time.fixedDeltaTime) {
                    float gradeAngle = Vector3.SignedAngle(_prevWallNormal, avgNormal, Vector3.up);
                    if(aimAlignment > 0 && gradeAngle > 8) {
                        _wallRunTime -= 5f * Time.fixedDeltaTime;
                        if(_wallRunTime < 2.1f * Time.fixedDeltaTime) 
                            _wallRunTime = 2.1f * Time.fixedDeltaTime;
                    } else if(aimAlignment < 0 && gradeAngle < -8) {
                        _wallRunTime -= 5f * Time.fixedDeltaTime;
                        if(_wallRunTime < 2.1f * Time.fixedDeltaTime) 
                            _wallRunTime = 2.1f * Time.fixedDeltaTime;
                    }
                }

                _prevWallNormal = avgNormal;
                Vector3 wallVelo = Movable.GetTraceCollisionResult(mover, _speed, transform.position);
                wallVelo += Movable.GetGravityCollisionResult(mover, currentGravity, transform.position + wallVelo);
                mover.SetVelocity(wallVelo);
                mover.WhileWallrunPerformed(ctx);
                return;

            } else if(mover.IsTouchingBelow()) {
                _wallRunTime = 0f;
                _wallRunCooldown = 0f;
                _isJumping = false;

                if(_groundframes < 1025) _groundframes++;
                if(_isMovingForward && _isTryingToSprint && isExpanded) {
                    _sprintBonus = 2;
                } else if(_sprintBonus > 1) {
                    _sprintBonus -= Time.fixedDeltaTime * 5f;
                } else _sprintBonus = 1;

                if(mover.GetSpeed() > 7 && isShrunk && _intendsToCrouch) {
                    _isSliding = true;
                    _needsSpeedBoost = true;
                }

                if(_isSliding) {
                    if(_slideTime < Time.fixedDeltaTime) mover.OnSlideEnter();
                    if(_slideTime < 1) _slideTime += Time.fixedDeltaTime;
                    else _slideTime = 1;
                    _speed = Movable.GetSourcelikeAcceleration(mover.GetVelocity(), mover.GetVelocity(), INFRA.Game.move_speed.GetFloat() * Mathf.Pow(_posturePercent, 1.5f) * _sprintBonus, INFRA.Game.move_accel.GetFloat());
                    if(_needsSpeedBoost) {
                        if(mover.GetSpeed() < 15f)
                            _speed += _wishdir * Time.fixedDeltaTime * 4.8f;
                        _needsSpeedBoost = false;
                    }
                    mover.WhileSlidePerformed();
                    if(mover.GetSpeed() < 7) {
                        mover.OnSlideExit();
                        _isSliding = false;
                    }
                } else {
                    if(_slideTime > 0) {
                        _slideTime = 0;
                        mover.OnSlideExit();
                    }
                    _speed = Movable.GetSourcelikeAcceleration(ref _wishdir, mover.GetVelocity(), INFRA.Game.move_speed.GetFloat() * Mathf.Pow(_posturePercent, 1.5f) * _sprintBonus, INFRA.Game.move_accel.GetFloat());
                }

                if(contact.IsTouchingGround()) _speed.Project(contact.GetGround().Value.normal);
                if(mover.IsTouchingAbove()) {  }

                if(_intendsToJump) {
                    INFRA.Game.Input.Jump.Consume();
                    if(!_isJumping && _jumpCooldown <= 0) {
                        _isJumping = true;
                        _isSliding = false;
                        _needsSpeedBoost = false;
                        _speed.y = INFRA.Game.move_jumpstrength.GetFloat() * Time.fixedDeltaTime * 0.81f;
                        contact.ForgetGround();
                        mover.SetIsGrounded(false);
                        _jumpCooldown = 1;
                        mover.OnJump(new JumpContext(false));
                    }
                }
                _usedSecondJump = false;
            } else {
                _isSliding = false;
                _needsSpeedBoost = false;
                _wallRunTime = 0f;

                if(_wallRunCooldown > 0) {
                    _wallRunCooldown -= Time.fixedDeltaTime;
                } else _wallRunCooldown = 0f;

                float accel = _isAirstrafing ? INFRA.Game.move_airaccel.GetFloat() : INFRA.Game.move_airaccel.GetFloat() * 0.3f;
                _speed = Movable.GetSourcelikeAcceleration(ref _wishdir, mover.GetVelocity(), INFRA.Game.move_speed.GetFloat(), accel);
                _groundframes = 0;

                if(!_usedSecondJump && _intendsToJump && _jumpCooldown <= 0) {
                    _isJumping = true;
                    _isSliding = false;
                    _needsSpeedBoost = false;
                    _usedSecondJump = true;
                    _speed.y = INFRA.Game.move_jumpstrength.GetFloat() * Time.fixedDeltaTime * 0.7f;
                    contact.ForgetGround();
                    mover.SetIsGrounded(false);
                    _jumpCooldown = 1;
                    mover.OnJump(new JumpContext(true));
                }
            }

            ApplyVelocity(ref _speed, mover);
        }


        private void HandleCrouching(MovingElement mover, ref bool isShrunk, ref bool isExpanded) {
            if(_intendsToCrouch) {
                if(isExpanded) {
                    if(!isShrunk) mover.OnCrouchEnter();
                    if(mover.GetCollider().ShrinkVertical(0.50f, _isSliding ? 20f : 11f, INFRA.Game.LevelMask))
                        _isCrouching = true;
                }
            } else if(mover.GetCollider().ShrinkVertical(1f, 6f, INFRA.Game.LevelMask)) {
                _isCrouching = false;
                _isSliding = false;
            } else {
                if(!isExpanded) mover.OnCrouchExit();
                _isSliding = false;
            }
        }


        private void HandleFriction(MovingElement mover) {

            if(_isWallrunning) {
                Movable.ApplyFriction(mover, 0.1f);
                return;
            }

            if(!mover.IsTouchingBelow()) {
                Movable.ApplyFriction(mover);
                return;
            }

            if(_isSliding) {
                if(mover.GetVelocity().y > 0.05f) {
                    Movable.ApplyFriction(mover, _slideTime);
                    return;
                } 
                
                if(mover.GetVelocity().y < 0f) {
                    _slideTime = 0.4f;
                    Movable.ApplyFriction(mover, _slideTime * 0.09f);
                    return;
                }

                Movable.ApplyFriction(mover, _slideTime * 0.1f);
                return;
            }

            Movable.ApplyFriction(mover);
        }

        private void HandleWallrunInitiation(ContactPatch contact, Movable mover, ref bool isExpanded) {
            if(_isWallrunning || mover.IsTouchingBelow() || !isExpanded || mover.GetSpeed() <= 1f )
                return;

            if(contact.IsTouchingLeftWall() && Movable.IsFacing(mover, contact.GetLeftWall())) {
                if(_wallRunCooldown <= 0f ) {
                    _prevWallNormal = contact.GetLeftWall().Value.normal;
                    _isWallrunning = true;
                    _isSliding = false;
                    _needsSpeedBoost = false;
                    _wallRunCooldown = 0f;
                    _wall = contact.GetLeftWall().Value;
                } else {
                    float diff = Vector3.Dot(_prevWallNormal, contact.GetLeftWall().Value.normal);
                    if(diff < 0.5f) {
                        _prevWallNormal = contact.GetLeftWall().Value.normal;
                        _isWallrunning = true;
                        _isSliding = false;
                        _needsSpeedBoost = false;
                        _wallRunCooldown = 0f;
                        _wall = contact.GetLeftWall().Value;
                    }
                }
                return;
            }

            if(contact.IsTouchingRightWall() && Movable.IsFacing(mover, contact.GetRightWall())) {
                if(_wallRunCooldown <= 0f ) {
                    _prevWallNormal = contact.GetRightWall().Value.normal;
                    _isWallrunning = true;
                    _wallRunCooldown = 0f;
                    _wall = contact.GetRightWall().Value;
                } else {
                    float diff = Vector3.Dot(_prevWallNormal, contact.GetRightWall().Value.normal);
                    if(diff < 0.5f) {
                        _prevWallNormal = contact.GetRightWall().Value.normal;
                        _isWallrunning = true;
                        _wallRunCooldown = 0f;
                        _wall = contact.GetRightWall().Value;
                    }
                }
            }
        }



        private void ApplyVelocity(ref Vector3 velo, MovingElement mover) {
            velo = Movable.GetTraceCollisionResult(mover, velo, transform.position);
            if(_isSliding && mover.IsTouchingBelow())
                velo += Movable.GetSlidingCollisionResult(mover, INFRA.Game.GetGravity() * 3, transform.position + velo);
            else {
                if(!mover.IsTouchingBelow())
                    velo += Movable.GetGravityCollisionResult(mover, INFRA.Game.GetGravity(), transform.position + velo);
            }
            
            if(!_intendsToJump && mover.IsTouchingBelow()) velo += Movable.GetGroundSnapResult(mover, mover.GetPosition());
            mover.SetVelocity(velo);
        }

        public override void OnTranslate(ref Vector3 amount, MovingElement mover) {
            _isMovingForward = mover.GetSpeed() > 0 && amount.z > 0;
            _isMovingBackward = mover.GetSpeed() > 0 && amount.z < 0;
            if(!mover.IsTouchingBelow() && !Mathf.Approximately(amount.x, 0) && Mathf.Approximately(amount.z, 0)) {
                _isAirstrafing = true;
                // additional friction stuff? idk
            } else _isAirstrafing = false;

            _wishdir = mover.GetRootRotation() * amount;

            if(_isSliding && !_isAirstrafing && _intendsToCrouch) {
                _wishdir *= 0.04f;
                if(amount.z > 0) _wishdir.z = 0;
            }
        }

        public override void OnTranslateUp(float amount, MovingElement mover) {
            _intendsToJump = amount > 0;
        }

        public override void OnTranslateDown(float amount, MovingElement mover) {
            _intendsToCrouch = amount < 0;
        }

        public override void OnRotate(ref Vector3 amount, MovingElement mover) {
            mover.RotDeltaX = amount.x;
            mover.RotDeltaY = amount.y;
            mover.RotDeltaZ = amount.z;
            mover.PushRotations();
        }

        public override void OnRotate(ref Quaternion amount, MovingElement mover) {
            Vector3 converted = amount.eulerAngles;
            OnRotate(ref converted, mover);
        }

        public override void OnImpulse(float amount, MovingElement mover) {
            if(amount > 0) _isTryingToSprint = true;
            else _isTryingToSprint = false;
        }

        public override Vector3 GetDesiredMovement() {
            return _wishdir;
        }

        public override void OnDestroy() {
            _wall = null;
            _head = null;
        }
    }
}