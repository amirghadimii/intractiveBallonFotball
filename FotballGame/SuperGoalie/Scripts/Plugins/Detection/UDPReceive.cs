using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

namespace detections
{
    public class UDPReceive : MonoBehaviour
    {
        Thread receiveThread;
        Thread StartPython;
        UdpClient client;
        public int port = 26500;
        public bool startRecieving = true;
        public bool printToConsole = false;
        public string data;
        public Vector2 RightPos;
        public Vector2 LeftPos;
        private float mass_ = 0.1f;
        public List<float> AllValue = new List<float>(4);
        public Process Cmd;
        public string[] DirectoryTarget;

        public string Targetpath = @"/home/user/UdpPython/PaintingDocker/Arsam_Painting_Detection3/Base";

        [SerializeField] private GameManager _GameHandler;

        // ✅ صف thread-safe برای انتقال داده‌ها از thread به main thread
        private static readonly ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();

        // اجرای پایتون
        public void EnableCode()
        {
            string file = @"/home/user/UdpPython/main.py";
            string strCmdText = "python3 " + file;
            Debug.Log(strCmdText);

            Cmd = Process.Start("bash", "-c \"" + strCmdText + "\"");
            Debug.Log("Run");
        }

        private void Awake()
        {
            if (_GameHandler == null)
            {
                _GameHandler = FindFirstObjectByType<GameManager>();
            }
        }

        public void Start()
        {
            StartPython = new Thread(EnableCode);
            receiveThread = new Thread(ReceiveData);
            StartPython.IsBackground = true;
            receiveThread.IsBackground = true;
            StartPython.Start();
            receiveThread.Start();
        }

        public void Update()
        {
            // ✅ داده‌ها را فقط در main thread پردازش می‌کنیم
            while (dataQueue.TryDequeue(out string json))
            {
                GetDataJson(json);
            }

            // برای تست دستی
            if (Input.GetKeyDown(KeyCode.A))
            {
                GetDataJson(data);
            }
        }

        // رشته‌های UDP را دریافت می‌کنیم
        private void ReceiveData()
        {
            client = new UdpClient(port);
            while (startRecieving)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                    byte[] dataByte = client.Receive(ref anyIP);
                    data = Encoding.UTF8.GetString(dataByte);
                    Debug.Log(data);

                    // ❌ دیگه اینجا نباید GetDataJson صدا بزنی
                    // ✅ فقط داده رو وارد صف کن
                    dataQueue.Enqueue(data);
                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }

        // داده‌ها را از JSON دریافت کرده و به انسان‌ها تبدیل می‌کنیم
        public void GetDataJson(string dataString)
        {
            if (string.IsNullOrEmpty(dataString)) return;

            Debug.Log("dataString: " + dataString);

            try
            {
                RootJson root = JsonUtility.FromJson<RootJson>(dataString);
                if (root != null && root.Ball != null)
                {
                    if (_GameHandler != null)
                    {
                        _GameHandler.LaunchFromNormalizedPosition(root.Ball.x, root.Ball.y);
                    }
                    else
                    {
                        Debug.LogWarning("UDPReceive: BallLauncher reference is missing; cannot launch ball.");
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid JSON format: " + dataString);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("JSON parse error: " + ex.Message);
            }
        }

        public void OnApplicationQuit()
        {
            try
            {
                Cmd?.Close();
                startRecieving = false;
                client?.Close();
                receiveThread?.Abort();
            }
            catch { }
        }
    }

    [System.Serializable]
    public class Ball
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class RootJson
    {
        public Ball Ball;
    }
}
