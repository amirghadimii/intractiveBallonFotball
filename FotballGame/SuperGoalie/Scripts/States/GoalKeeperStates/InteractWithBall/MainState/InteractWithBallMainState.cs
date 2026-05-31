using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.InteractWithBall.SubStates;
using RobustFSM.Base;

namespace Assets.SuperGoalie.Scripts.States.GoalKeeperStates.InteractWithBall.MainState
{
    public class InteractWithBallMainState : BHState
    {
        public override void AddStates()
        {
            // Remove CatchBall to ensure the keeper never enters a catch substate
            AddState<CheckIfBallIsCatchableOrPunchable>();
            AddState<ClearBall>();

            SetInitialState<CheckIfBallIsCatchableOrPunchable>();
        }
    }
}
