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
    public class GameManagerAr : Singleton<GameManagerAr>
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] float _ballDribbleForce = 5f;
        [SerializeField] ScoreSystemAr _ScoreSystem;
        [SerializeField] float _ballKickForce = 15;

        [SerializeField] Ball _ball;

        [SerializeField] Goal _goal;

        [SerializeField] GoalKeeper _goalKeeper;

        public
            RTLTextMeshPro _scoreText;

        bool _run = true;
        public int _score;
        Vector3 _ballInitPos;
        Quaternion _ballInitRot;

        protected Transform Cam; // A reference to the main camera in the scenes transform
        protected Vector3 CamForward; // The current forward direction of the camera

        public delegate void BallLaunch(float power, Vector3 target); //delegate to launch a ball

        public BallLaunch OnBallLaunch; //on ball launch
        [SerializeField] private GameObject Circle;
        [SerializeField] Vector2 _Poss;

        public override void Awake()
        {
            // register the game manager to some events
            _ball.OnBallLaunched += SoundManager.Instance.PlayBallKickedSound;
            _goalKeeper.OnPunchBall += SoundManager.Instance.PlayBallKickedSound;
            _goal.GoalTrigger.OnCollidedWithBall += SoundManager.Instance.PlayGoalScoredSound;
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
        }

        private void Instance_OnBallCollidedWithGoal()
        {
            // Delegate scoring to ScoreSystem (handles single or two-player modes)
            if (_ScoreSystem != null)
            {
                _ScoreSystem.AddScore();
            }
        }

        public void ShootAtNormalizedPosition(float normX, float normY)
        {
            Debug.Log($"[ShootAtNormalizedPosition] Input - normX: {normX}, normY: {normY}");

            if (_ScoreSystem != null)
            {
                if (_ScoreSystem.gameStarted)
                {
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
                    // تبدیل مقدار نرمال شده به مختصات صفحه نمایش
                    float screenX = normX * Screen.width;
                    float screenY = (1f - normY) * Screen.height; // معکوس کردن Y برای هماهنگی با سیستم مختصات یونیتی
                    
                    // استفاده از فاصله مناسب از دوربین (فاصله از دوربین)
                    float distanceFromCamera = 10f; // می‌توانید این مقدار را بر اساس نیاز تنظیم کنید
                    Vector3 screenPos = new Vector3(screenX, screenY, distanceFromCamera);
                 

                    // تبدیل به مختصات جهانی
                    Vector3 worldPoint = _mainCamera.ScreenToWorldPoint(screenPos);
  
                    GameObject Ins = Instantiate(Circle, Circle.transform.position, Circle.transform.rotation);
                    Ins.gameObject.transform.position = new Vector3(worldPoint.x,worldPoint.y,48.804f);
                    Destroy(Ins,2);
                    RaycastHit _hit;
                    Physics.Raycast(new Vector3(worldPoint.x, worldPoint.y, 48.767f), Vector3.forward, out _hit);


                    //  Destroy(Ins,2);
                    if (_hit.collider != null)
                    {
                      Debug.Log("_hit"+_hit.transform.gameObject.name);
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


        private void Update()
        {
            // if (_ScoreSystem != null)
            // {
            //   
            //     if (_ScoreSystem.gameOverPanel != null && _ScoreSystem.gameOverPanel.activeSelf) return;
            // }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ShootAtNormalizedPosition(_Poss.x,_Poss.y);
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
            yield return new WaitForSeconds(2f);
            
            // Only reset if we haven't already been reset by another process
            if (!_ball.gameObject.activeInHierarchy)
                yield break;
                
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
            // Only reset if the ball is active
            if (!_ball.gameObject.activeInHierarchy)
                return;

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