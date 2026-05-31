using Assets.SuperGoalie.Scripts.Entities;
using Assets.SuperGoalie.Scripts.FSMs;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.Idle.MainState;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.InterceptShot.MainState;
using RobustFSM.Base;
using System;
using UnityEngine;

namespace Assets.SuperGoalie.Scripts.States.GoalKeeperStates.Dive.MainState
{
    public class PunchBallMainState : BState
    {
        bool _ballTrapable;
        float _height;
        float _time;
        float _turn;
        float _weightMultiplier;
        Vector3 _leftHandTargetPosition;
        Vector3 _rightHandTargetPosition;

        public override void Enter()
        {
            base.Enter();

            _time = 0f;

            //get some important data
            _ballTrapable = Machine.GetState<InterceptShotMainState>().BallTrapable;
            _leftHandTargetPosition = Machine.GetState<InterceptShotMainState>().LeftHandTargetPosition;
            _rightHandTargetPosition = Machine.GetState<InterceptShotMainState>().RightHandTargetPosition;
            _turn = Machine.GetState<InterceptShotMainState>().Turn;

            //if the ball is trappable then catch it
            if (_ballTrapable)
            {
                // choose the catching hand based on turn or proximity
                Transform leftHand = Owner.Animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform rightHand = Owner.Animator.GetBoneTransform(HumanBodyBones.RightHand);

                Transform targetHand = rightHand;
                if (_turn == -1f) targetHand = leftHand;
                else if (_turn == 1f) targetHand = rightHand;
                else
                {
                    float distL = Vector3.Distance(leftHand.position, Owner.Ball.Position);
                    float distR = Vector3.Distance(rightHand.position, Owner.Ball.Position);
                    targetHand = distL <= distR ? leftHand : rightHand;
                }

                // stop and attach the ball to the hand
                Owner.Ball.Stop();
                Owner.Ball.SphereCollider.enabled = false;
                Owner.Ball.transform.SetParent(targetHand);
                Owner.Ball.transform.localPosition = Vector3.zero;
                Owner.Ball.transform.localRotation = Quaternion.identity;

                // mark possession and exit to idle
                Owner.HasBall = true;
            }

            //set the animator to exit the dive state
            Owner.Animator.SetTrigger("Exit");

            //raise the punch ball event
            Action temp = Owner.OnPunchBall;
            if (temp != null)
                temp.Invoke();
        }

        public override void Execute()
        {
            base.Execute();

            // if we have the ball, go to idle immediately
            if (Owner.HasBall)
                Machine.ChangeState<IdleMainState>();
            else
            {
                //go to idle state the moment the player gets into idle state
                if (Owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    Machine.ChangeState<IdleMainState>();
            }
        }

        public override void OnAnimatorIK(int layerIndex)
        {
            base.OnAnimatorIK(layerIndex);

            //declare the weights
            float leftHandWeight = 0f;
            float rightHandWeight = 0f;
            float lookAtWeight = 0f;

            //set the time
            if(_time < 1f)
                _time += 10 * Time.deltaTime;

            //set the weight multiplier
            _weightMultiplier = Mathf.Lerp(1f, 0f, _time);

            //choose which hands to effect
            if (_turn == 0f)
            {
                //set the weights
                leftHandWeight = _weightMultiplier;
                rightHandWeight = _weightMultiplier;
                lookAtWeight = _weightMultiplier;

                //set the animations weights
                Owner.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                Owner.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);

                //set the animations positions
                Owner.Animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandTargetPosition);
                Owner.Animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTargetPosition);
            }
            else if (_turn == -1)
            {
                //set the weights
                leftHandWeight = _weightMultiplier;
                lookAtWeight = _weightMultiplier;

                //set the animations weights
                Owner.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);

                //set the animations positions
                Owner.Animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandTargetPosition);

            }
            else if (_turn == 1)
            {
                //set the weights
                rightHandWeight = _weightMultiplier;
                lookAtWeight = _weightMultiplier;

                //set the animations weights
                Owner.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);

                //set the animations positions
                Owner.Animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTargetPosition);
            }

            //set the look target
            Owner.Animator.SetLookAtWeight(lookAtWeight);
            Owner.Animator.SetLookAtPosition(Owner.Ball.Position);
        }

        public override void OnAnimatorMove()
        {
            base.OnAnimatorMove();

            //manipulate the player height
            Owner.ModelRoot.transform.localPosition = Vector3.zero;
        }

        GoalKeeper Owner
        {
            get
            {
                return ((GoalKeeperFSM)SuperMachine).Owner;
            }
        }
    }
}
