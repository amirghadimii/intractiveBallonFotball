using Assets.SuperGoalie.Scripts.Entities;
using Assets.SuperGoalie.Scripts.States.GoalKeeperStates.Idle.MainState;
using Patterns.Singleton;
using System;
using System.Collections;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Components;
namespace Assets.SuperGoalie.Scripts.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] float _ballDribbleForce = 5f;
        [SerializeField] ScoreSystem _ScoreSystem;
        [SerializeField] float _ballKickForce = 15;

        [SerializeField] Ball _ball;

        [SerializeField] Goal _goal;

        [SerializeField] GoalKeeper _goalKeeper;

        public
            RTLTextMeshPro _scoreText;
        [SerializeField] private Camera _mainCamera;

        bool _run = true;
        public int _score;
        Vector3 _ballInitPos;
        Quaternion _ballInitRot;
        [SerializeField] private GameObject Circle;
        [SerializeField] Vector2 _Poss;
        
        [Header("Ball Materials")]
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material whiteMaterial;
        
        protected Transform Cam; // A reference to the main camera in the scenes transform
        protected Vector3 CamForward; // The current forward direction of the camera

        public delegate void BallLaunch(float power, Vector3 target); //delegate to launch a ball

        public BallLaunch OnBallLaunch; //on ball launch

        [Header("World Remap Bounds (Menu)")] [SerializeField]
        float worldMinX = -10f;

        [SerializeField] float worldMaxX = 10f;
        [SerializeField] float worldMinY = -5f;
        [SerializeField] float worldMaxY = 5f;
        public override void Awake()
        {
            // register the game manager to some events
            _ball.OnBallLaunched += SoundManager.Instance.PlayBallKickedSound;
            _goalKeeper.OnPunchBall += SoundManager.Instance.PlayBallKickedSound;
            _goalKeeper.OnPunchBall += RaiseBallIfTooLow;
            _goalKeeper.OnPunchBall += OnKeeperNoBall; // ensure physics enabled before punch
            _goal.GoalTrigger.OnCollidedWithBall += SoundManager.Instance.PlayGoalScoredSound;

            // keeper possession events
            _goalKeeper.OnHasBall += OnKeeperHasBall;
            _goalKeeper.OnHasNoBall += OnKeeperNoBall;
            //register entities to entitiy delegates
            _ball.OnBallLaunched += _goalKeeper.Instance_OnBallLaunched;

            //register entities to entitiy delegates
            _goal.GoalTrigger.OnCollidedWithBall += Instance_OnBallCollidedWithGoal;

            //register entities to local delegates
            OnBallLaunch += _ball.Instance_OnBallLaunch;

            //cache the initial data
            _ballInitPos = _ball.Position;
            _ballInitRot = _ball.Rotation;

            // get the transform of the main camera
            if (Camera.main != null)
                Cam = Camera.main.transform;
            else
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            
            // Validate material assignments
            if (redMaterial == null || blueMaterial == null || whiteMaterial == null)
            {
                Debug.LogError("[GameManager] One or more ball materials are NOT assigned in the inspector (red/blue/white).");
            }

            // Set initial ball material to white
            if (_ball != null && _ball.ballMeshRenderer != null)
            {
                Debug.Log("[GameManager] Awake -> SetBallMaterial(-1) initial white");
                SetBallMaterial(-1); // -1 for white/neutral
            }
            else
            {
                Debug.LogError("[GameManager] Ball or ballMeshRenderer is null in Awake.");
            }
        }
        
        /// <summary>
        /// Changes the ball's material based on the current player's turn
        /// </summary>
        /// <param name="playerTurn">0 for red, 1 for blue, -1 for white/neutral</param>
        public void SetBallMaterial(int playerTurn)
        {
            if (_ball == null || _ball.ballMeshRenderer == null)
            {
                Debug.LogError("[GameManager] Ball or Ball MeshRenderer is not assigned!");
                return;
            }

            Material target = null;
            switch (playerTurn)
            {
                case 0: // Red player's turn
                    target = redMaterial;
                    break;
                case 1: // Blue player's turn
                    target = blueMaterial;
                    break;
                default: // White/neutral
                    target = whiteMaterial;
                    break;
            }

            if (target == null)
            {
                Debug.LogError($"[GameManager] Target material for playerTurn {playerTurn} is NULL. Assign materials in inspector.");
                return;
            }

            _ball.ballMeshRenderer.material = target;
            Debug.Log($"[GameManager] SetBallMaterial(playerTurn={playerTurn}) -> applied '{target.name}' to '{_ball.ballMeshRenderer.gameObject.name}'");
        }

        private void Instance_OnBallCollidedWithGoal()
        {
            // Delegate scoring to ScoreSystem (handles single or two-player modes)
            if (_ScoreSystem != null)
            {
                _ScoreSystem.AddScore();
            }
        }

        // Keeper just caught the ball: freeze physics to hold cleanly
        private void OnKeeperHasBall()
        {
            if (_ball == null || _ball.Rigidbody == null || _ball.SphereCollider == null) return;
            var rb = _ball.Rigidbody;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            try { _ball.Rigidbody.linearVelocity = Vector3.zero; } catch { }
            rb.isKinematic = true;
            _ball.SphereCollider.isTrigger = true;
        }

        // Keeper releases/loses the ball: enable physics back
        private void OnKeeperNoBall()
        {
            if (_ball == null || _ball.Rigidbody == null || _ball.SphereCollider == null) return;
            var rb = _ball.Rigidbody;
            rb.isKinematic = false;
            _ball.SphereCollider.isTrigger = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Prevent tunneling by ensuring ball is a bit above the ground right before punch
        private void RaiseBallIfTooLow()
        {
            if (_ball == null || _ball.Rigidbody == null)
                return;

            var rb = _ball.Rigidbody;
            var pos = rb.position;
            // Adjust this threshold to your ground level and ball radius
            const float minY = 0.15f;
            if (pos.y < minY)
            {
                pos.y = minY;
                rb.position = pos;
            }

            // Ensure robust collision settings at punch moment
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.isKinematic = false;
        }

  public void ShootAtNormalizedPosition(float normX, float normY)
        {
            Debug.Log($"[ShootAtNormalizedPosition] Input - normX: {normX}, normY: {normY}");


            if (_ScoreSystem != null)
            {
                if (_ScoreSystem.gameStarted)
                {
                    if (!_run  )
                        return;
                    Debug.Log("rrrrrr55555555");
                    // تبدیل مقدار نرمال شده به مختصات صفحه نمایش
                    float screenX = normX * Screen.width;
                    float screenY = normY * Screen.height;
                    Vector3 Pso = new Vector3(screenX, screenY,0);

                    Camera cameraToUse = _mainCamera != null ? _mainCamera : Camera.main;
                    if (cameraToUse == null)
                    {
                        Debug.LogError("No camera assigned and Camera.main is null!");
                        return;
                    }
                    Debug.Log($"[ShootAtNormalizedPosition] Input - normX: {normX}, normY: {normY}");

                    Ray ray = cameraToUse.ScreenPointToRay(Pso);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        _run = false;
                        Vector3 target = hit.point;

                        BallLaunch tempBallLaunch = OnBallLaunch;
                        tempBallLaunch?.Invoke(_ballKickForce, target);

                        StartCoroutine(Reset());
                    }
                }
                else
                {
                    Debug.Log("rrrrrr");
                 // تبدیل مقدار نرمال شده به مختصات صفحه نمایش
                    float screenX = normX * 1024;
                    float screenY = normY * 768;
                    Debug.Log("screenY" + screenY);

                    Vector3 screenPos = new Vector3(screenX, screenY, 47.904f);


// محاسبه عمق Z فعلی (اگر ثابت می‌خواهی همان 48.804f را نگه دار)
                    Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(screenPos);
                    float zDepth = worldPoint.z;

// ریمپ مستقیمِ ورودی‌های نرمال به محدوده فضای جهان (بهترین راه)
           
                    float remapY = Remap(worldPoint.y, 6.07f, -8, worldMinY, worldMaxY);
                    float remapX = Remap(worldPoint.x,   -10f,10f , worldMinX, worldMaxX);
                    Vector3 remappedWorldPoint = new Vector3(remapX, remapY, worldPoint.z);

                    GameObject Ins = Instantiate(Circle, Circle.transform.position, Circle.transform.rotation);
                    Ins.gameObject.transform.position =
                        new Vector3(remappedWorldPoint.x, remappedWorldPoint.y, 47.904f);
                    Destroy(Ins, 2);

// اگر همچنان Raycast را از نقطه‌ی جهان می‌خواهی:
                    RaycastHit _hit;
                    Physics.Raycast(new Vector3(remappedWorldPoint.x, remappedWorldPoint.y, 47.9f), Vector3.forward,
                        out _hit);


                    //  Destroy(Ins,2);
                    if (_hit.collider != null)
                    {
                        Debug.Log("_hit" + _hit.transform.gameObject.name);
                        if (_hit.transform.gameObject.tag == "SelectPlayer")
                        {
                            Debug.Log("2");

                            _ScoreSystem.StartMode(false);
                        }
                        else if (_hit.transform.gameObject.tag == "SelectPlayer2")
                        {
                            _ScoreSystem.StartMode(true);
                        }
                        else if (_hit.transform.gameObject.tag == "Retry")
                        {
                            _ScoreSystem.ReturnToMenu();
                        }
                    }
                }
            }
        }

        private static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // بدون clamp/lerp
            float fromRange = fromMax - fromMin;
            float toRange = toMax - toMin;

            if (Mathf.Approximately(fromRange, 0f))
                return toMin; // یا می‌تونی return toMax; یا خود value

            float t = (value - fromMin) / fromRange;
            return toMin + t * toRange;
        }
        private void Update()
        {
            if (_ScoreSystem != null)
            {
                if (!_ScoreSystem.gameStarted) return;
                
            }

            if (!_run  )
                return;
              if (Input.GetKeyDown(KeyCode.K))
            {
                ShootAtNormalizedPosition(_Poss.x, _Poss.y);
            }

            #region TriggerShooting

            //get the mouse
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Input.mousePosition" + Input.mousePosition.x + "Input.mousePosition" +
                          Input.mousePosition.y + "Input.mousePositionzz" + Input.mousePosition.z);

                //create a ray from mouse clicked position
                Vector2 normalizedMousePos = new Vector2(
                    Input.mousePosition.x / Screen.width,
                    Input.mousePosition.y / Screen.height
                );
                Debug.Log("normalizedMousePosXX" + normalizedMousePos.x + "normalizedMousePosYY" +
                          normalizedMousePos.y);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //run a raycast into the scene
                if (Physics.Raycast(ray, out hit))
                {
                    //I can no longer run
                    _run = false;

                    //get the target
                    Vector3 target = hit.point;

                    //launch the ball
                    BallLaunch tempBallLaunch = OnBallLaunch;
                    if (tempBallLaunch != null)
                        tempBallLaunch.Invoke(_ballKickForce, target);

                    //start the reset coroutine
                    StartCoroutine(Reset());
                }
            }
            // else
            // {
            //     //capture input
            //     float horizontalRot = Input.GetAxisRaw("Horizontal");
            //     float verticalRot = Input.GetAxisRaw("Vertical");
            //     verticalRot = 0f;
            //
            //     //calculate the direction to rotate to
            //     Vector3 input = new Vector3(horizontalRot, 0f, verticalRot);
            //
            //     //process if any key down
            //     if (Input.anyKeyDown)
            //     {
            //         Vector3 Movement = new Vector3();
            //
            //         // calculate move direction to pass to character
            //         if (Cam != null)
            //         {
            //             // calculate camera relative direction to move:
            //             CamForward = Vector3.Scale(Cam.forward, new Vector3(1, 0, 1)).normalized;
            //             Movement = input.z * CamForward + input.x * Cam.right;
            //         }
            //         else
            //         {
            //             // we use world-relative directions in the case of no main camera
            //             Movement = input.z * Vector3.forward + input.x * Vector3.right;
            //         }
            //
            //         //kick the ball
            //         Movement.y = 0.03f;
            //         _ball.Rigidbody.velocity = Movement * _ballDribbleForce;
            //     }
            // }

            #endregion
        }

        
        private IEnumerator Reset()
        {
            yield return new WaitForSeconds(3f);
            ResetBallAndKeeper();
            
            yield return new WaitForSeconds(1f);
            _run = true;
            _ball.gameObject.SetActive(true);
            _goal.GoalTrigger.gameObject.SetActive(true);
            if (_ScoreSystem != null)
            {
                _ScoreSystem.OnShotResolved();
            }
        }
        
        public void ResetGame()
        {
            // Reset ball and keeper immediately
            ResetBallAndKeeper();
            
            // Reset game state
            _run = true;
            _ball.gameObject.SetActive(true);
            _goal.GoalTrigger.gameObject.SetActive(true);
        }
        
        private void ResetBallAndKeeper()
        {
            // Reset ball
            _ball.gameObject.SetActive(false);
            _ball.Stop();
            _ball.Position = _ballInitPos;
            _ball.Rotation = _ballInitRot;
            
            // Reset keeper
            _goalKeeper.FSM.ChangeState<IdleMainState>();
        }
    }
}