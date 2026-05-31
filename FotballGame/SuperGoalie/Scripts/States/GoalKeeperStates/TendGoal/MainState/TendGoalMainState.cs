using Assets.SuperGoalie.Scripts.Entities;
using Assets.SuperGoalie.Scripts.FSMs;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.Idle.MainState;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.IgnoreShot.MainState;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.InterceptShot.MainState;
using RobustFSM.Base;
using UnityEngine;

namespace Assets.SuperGoalie.Scripts.States.GoalKeeperStates.TendGoal.MainState
{
    public class TendGoalMainState : BState
    {
        float _timeSinceLastUpdate;
        Vector3 _steeringTarget;
        Vector3 _prevBallPosition;

        public override void Enter()
        {
            base.Enter();

            //set some data
            _prevBallPosition = 1000 * Vector3.one;
            _timeSinceLastUpdate = 0f;

            //set the rpg movement
            Owner.RPGMovement.SetSteeringOn();
            Owner.RPGMovement.Speed = Owner.TendGoalSpeed;

            //register to some events
            Owner.OnBallLaunched += Instance_OnBallLaunched;

            //set the animator
            Owner.Animator.SetTrigger("TendGoal");
        }

        public override void Execute()
        {
            base.Execute();

            //if the ball is not within threatening distance then idle
            if (!Owner.IsBallWithThreateningDistance())
                Machine.ChangeState<IdleMainState>();

            //get the entity positions
            Vector3 ballPosition = new Vector3(Owner.Ball.Position.x, 0f, Owner.Ball.Position.z);

            //set the look target
            Owner.RPGMovement.SetRotateFacePosition(ballPosition);

            // همیشه دروازه‌بان را در مرکز دروازه قرار بده
            Vector3 ballRelativePosToGoal = Vector3.zero;
            ballRelativePosToGoal.z = Owner.TendGoalDistance;
            ballRelativePosToGoal.x = 0f;  // همیشه وسط
            _steeringTarget = Owner.Goal.transform.TransformPoint(ballRelativePosToGoal);

            // حذف تایمر - دیگر نیازی نیست
           
            //set the ability to steer here - همیشه حرکت کن تا به مرکز برسد
            Owner.RPGMovement.Steer = Vector3.Distance(Owner.Position, _steeringTarget) >= 0.1f;
            Owner.RPGMovement.SetMoveTarget(_steeringTarget);

            //get my relative velocity
            Vector3 relativeVelocity = Owner.transform.InverseTransformDirection(Owner.RPGMovement.Velocity);
            float clampedForward = Mathf.Clamp(relativeVelocity.z, -1f, 0.5f);
            float clampedSide = Mathf.Clamp(relativeVelocity.x, -1f, 1f);

            //update the animator
            Owner.Animator.SetFloat("Forward", clampedForward, 0.1f, 0.1f);
            Owner.Animator.SetFloat("Turn", clampedSide, 0.1f, 0.1f);
        }


        public override void Exit()
        {
            base.Exit();

            //deregister to some events
            Owner.OnBallLaunched -= Instance_OnBallLaunched;

            //set the animator
            Owner.Animator.ResetTrigger("TendGoal");
        }

        private void Instance_OnBallLaunched(float flightTime, float velocity, Vector3 initial, Vector3 target)
        {
            //set some variables
            Owner.BallInitialPosition = initial;
            Owner.BallHitTarget = target;
            Owner.BallFlightTime = flightTime;
            Owner.BallVelocity = velocity;

            //change state
            if (Owner.IsShotOnTarget())
                Machine.ChangeState<InterceptShotMainState>();
            else
                Machine.ChangeState<IgnoreShotMainState>();
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
