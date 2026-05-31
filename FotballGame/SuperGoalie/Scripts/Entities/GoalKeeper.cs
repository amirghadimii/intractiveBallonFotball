using Assets.SimpleSteering.Scripts.Movement;
using Assets.SuperGoalie.Scripts.FSMs;
using System;
using UnityEngine;

namespace Assets.SuperGoalie.Scripts.Entities
{
    [RequireComponent(typeof(GoalKeeperFSM))]
    [RequireComponent(typeof(RPGMovement))]
    public class GoalKeeper : MonoBehaviour
    {
        /// <summary>
        /// A reference to the dive speed of this instance
        /// </summary>
        [SerializeField]
		float _diveSpeed = 7f;

        /// <summary>
        /// A refernce to the goal keeping of this instance
        /// </summary>
        [SerializeField]
		float _goalKeeping = 0.95f;

        /// <summary>
        /// A reference to the height of this instance
        /// </summary>
        float _height = 1.9f;

        /// <summary>
        /// A refernce to the jump distance of this instance
        /// </summary>
        [SerializeField]
		float _jumpDistance = 2.5f;

        /// <summary>
        /// A reference to the jump height of this instance
        /// </summary>
        [SerializeField]
		float _jumpHeight = 1.3f;

        /// <summary>
        /// A refernce to the goal keeping of this instance
        /// </summary>
        [SerializeField]
		float _reach = 1.5f;

        [Header("Keeper Settings")]
        [Tooltip("When enabled, the keeper will have enhanced diving and saving abilities")]
        [SerializeField]
        bool _isStrongKeeper = false;

        [Header("Strong Keeper Colliders")]
        [Tooltip("Colliders that are enabled only when the keeper is in strong mode")]
        [SerializeField]
        SphereCollider _strongKeeperCollider1;
        
        [SerializeField]
        SphereCollider _strongKeeperCollider2;

        /// <summary>
        /// Gets/Sets the reach distance of the goalkeeper
        /// </summary>
        public float Reach
        {
            get { return _reach; }
            set { _reach = value; }
        }

        /// <summary>
        /// Gets/Sets whether this is a strong keeper with enhanced abilities
        /// </summary>
        public bool IsStrongKeeper 
        { 
            get { return _isStrongKeeper; } 
            set { _isStrongKeeper = value; } 
        }

        /// <summary>
        ///  reference to the tend goal distance of this instance
        /// </summary>
        [SerializeField]
		float _tendGoalDistance = 5f;

        /// <summary>
        ///  reference to the tend goal speed of this instance
        /// </summary>
        [SerializeField]
		float _tendGoalSpeed = 5f;

        /// <summary>
        /// A reference to this instance's animator
        /// </summary>
        [SerializeField]
        Animator _animator;

        /// <summary>
        /// A reference to the ball instance
        /// </summary>
        [SerializeField]
        Ball _ball;

        /// <summary>
        /// A reference to the goal instance
        /// </summary>
        [SerializeField]
        Goal _goal;

        /// <summary>
        /// A reference to the model root
        /// </summary>
        [SerializeField]
        Transform _modelRoot;

        public Action OnHasNoBall;

        public Action OnHasBall;

        public Action OnPunchBall;

        public delegate void BallLaunched(float flightPower, float velocity, Vector3 initial, Vector3 target);
        public BallLaunched OnBallLaunched;

		[SerializeField]
		bool _autoReturnHome = true;

		[SerializeField]
		float _homeSnapDistance = 0.1f;

		Vector3 _homePosition;
		Quaternion _homeRotation;

		bool _hasBall;
		public bool HasBall
		{
			get { return _hasBall; }
			set
			{
				if (_hasBall == value)
					return;
				_hasBall = value;
				if (_hasBall)
				{
					Action temp = OnHasBall;
					if (temp != null)
						temp.Invoke();
				}
				else
				{
					Action temp = OnHasNoBall;
					if (temp != null)
						temp.Invoke();
					if (_autoReturnHome)
						TriggerReturnHome();
				}
			}
		}

        public float BallFlightTime { get; set; }

        public Vector3 BallHitTarget { get; set; }

        public Vector3 BallInitialPosition { get; internal set; }

        public GoalKeeperFSM FSM { get; set; }

        public RPGMovement RPGMovement { get; set; }

		private void Awake()
        {
            FSM = GetComponent<GoalKeeperFSM>();
            RPGMovement = GetComponent<RPGMovement>();
        }

		private void Update()
        {
            // Toggle strong/weak mode with 'J' key
            if (Input.GetKeyDown(KeyCode.J))
            {
                _isStrongKeeper = !_isStrongKeeper;
                
                // Toggle colliders based on the new state
                if (_strongKeeperCollider1 != null)
                    _strongKeeperCollider1.enabled = _isStrongKeeper;
                    
                if (_strongKeeperCollider2 != null)
                    _strongKeeperCollider2.enabled = _isStrongKeeper;
                    
                Debug.Log($"Goalkeeper is now {(_isStrongKeeper ? "STRONG" : "NORMAL")} mode");
            }
        }

		private void Start()
		{
			_homePosition = transform.position;
			_homeRotation = transform.rotation;
			
			// تنظیم مقادیر برای گرفتن توپ‌های دورتر
			_reach = 1.8f;          // برد دست بیشتر
			_jumpDistance = 3.2f;   // فاصله پرش بیشتر
			_jumpHeight = 1.5f;     // ارتفاع پرش بیشتر
			_diveSpeed = 15f;
			_goalKeeping = 0.95f;
			_tendGoalDistance = 1.3f;
			_tendGoalSpeed = 3f;
			
			// تنظیم مقادیر RPGMovement
			if (RPGMovement != null)
			{
				RPGMovement.Speed = 3;
				RPGMovement.Acceleration = 7f;
				RPGMovement.RotationSpeed = 7f;
				RPGMovement.Agility = 1.5f;
			}
            
            // Initialize colliders based on initial state
            if (_strongKeeperCollider1 != null)
                _strongKeeperCollider1.enabled = _isStrongKeeper;
                
            if (_strongKeeperCollider2 != null)
                _strongKeeperCollider2.enabled = _isStrongKeeper;
		}

        public bool IsBallWithChasingDistance()
        {
            return DistanceOfBallToGoal() <= 30f;  // افزایش از 20 به 30
        }

        public bool IsBallWithThreateningDistance()
        {
            return DistanceOfBallToGoal() <= 60;  // افزایش از 45 به 60
        }

        public bool IsShotOnTarget()
        {
            return _goal.IsPositionWithinGoalMouthFrustrum(BallHitTarget);
        }

        public float DistanceOfBallToGoal()
        {
            return Vector3.Distance(_ball.transform.position, _goal.transform.position);
        }

        public void Instance_OnBallLaunched(float flightTime, float velocity, Vector3 initial, Vector3 target)
        {
            BallLaunched temp = OnBallLaunched;
            if (temp != null)
                temp.Invoke(flightTime, velocity, initial, target);
        }

        public Vector3 Position
        {
            get
            {
                return transform.position;
            }

            set
            {
                transform.position = value;
            }
        }

        public float GoalKeeping
        {
            get
            {
                return _goalKeeping;
            }

            set
            {
                _goalKeeping = value;
            }
        }

        public float JumpDistance
        {
            get
            {
                return _jumpDistance;
            }

            set
            {
                _jumpDistance = value;
            }
        }

        /// <summary>
        /// Gets the jump reach of the goalkeeper
        /// </summary>
        public float JumpReach
        { 
            get
            {
                return _reach;
            }
        }

        public float TendGoalDistance
        {
            get
            {
                return _tendGoalDistance;
            }

            set
            {
                _tendGoalDistance = value;
            }
        }

        public float TendGoalSpeed
        {
            get
            {
                return _tendGoalSpeed;
            }

            set
            {
                _tendGoalSpeed = value;
            }
        }

        public Animator Animator
        {
            get
            {
                return _animator;
            }

            set
            {
                _animator = value;
            }
        }

        public Ball Ball
        {
            get
            {
                return _ball;
            }

            set
            {
                _ball = value;
            }
        }

        public Goal Goal
        {
            get
            {
                return _goal;
            }

            set
            {
                _goal = value;
            }
        }

        public float Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        public float JumpHeight
        {
            get
            {
                return _jumpHeight;
            }

            set
            {
                _jumpHeight = value;
            }
        }

        public Transform ModelRoot
        {
            get
            {
                return _modelRoot;
            }

            set
            {
                _modelRoot = value;
            }
        }

        public float BallVelocity { get; internal set; }

		Coroutine _returnHomeRoutine;

		public void TriggerReturnHome()
		{
			if (_returnHomeRoutine != null)
				StopCoroutine(_returnHomeRoutine);
			_returnHomeRoutine = StartCoroutine(ReturnHomeRoutine());
		}

		public void ReturnHomeImmediate()
		{
			if (_returnHomeRoutine != null)
			{
				StopCoroutine(_returnHomeRoutine);
				_returnHomeRoutine = null;
			}
			RPGMovement.Reset();
			transform.position = _homePosition;
			transform.rotation = _homeRotation;
			RPGMovement.SnapLocalXToZero();
		}

		System.Collections.IEnumerator ReturnHomeRoutine()
		{
			RPGMovement.SetSteeringOn();
			RPGMovement.SetTrackingOn();
			while ((transform.position - _homePosition).sqrMagnitude > _homeSnapDistance * _homeSnapDistance)
			{
				RPGMovement.SetMoveTarget(_homePosition);
				RPGMovement.SetRotateFacePosition(_homePosition);
				yield return null;
			}
			RPGMovement.Reset();
			transform.position = _homePosition;
			transform.rotation = _homeRotation;
			RPGMovement.SnapLocalXToZero();
			_returnHomeRoutine = null;
		}
    }
}
