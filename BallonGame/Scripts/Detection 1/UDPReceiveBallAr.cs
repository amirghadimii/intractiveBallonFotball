using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets.SuperGoalie.Scripts.Managers;
using detections;
using Debug = UnityEngine.Debug;

namespace detections
{
    public class UDPReceiveBallAr : MonoBehaviour
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
        [SerializeField] private bool HaveBall=false;
        [SerializeField] private RootJson root;
        public string
            Targetpath = @"/home/user/UdpPython/PaintingDocker/Arsam_Painting_Detection3/Base"; // مسیر جدید لینوکس

        [SerializeField] private GameManagerAr _GameManagerAr;

        // این متد اسکریپت پایتون رو اجرا می‌کنه
        public void EnableCode()
        {
            // مسیر اسکریپت پایتون رو در لینوکس تنظیم می‌کنیم
            string file = @"/home/user/UdpPython/main.py"; // مسیر جدید لینوکس
            string strCmdText = "python3 " + file; // استفاده از python3 در لینوکس
            Debug.Log(strCmdText);

            // اجرا کردن اسکریپت پایتون
            Cmd = Process.Start("bash",
                "-c \"" + strCmdText + "\""); // از bash برای اجرای اسکریپت پایتون استفاده می‌کنیم
            Debug.Log("Run");
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
            if (Input.GetKeyDown(KeyCode.A))
            {
                GetDataJson(data);
            }

            if (HaveBall)
            {
                _GameManagerAr.ShootAtNormalizedPosition(root.Ball.x, root.Ball.y);
                HaveBall = false;
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
                    GetDataJson(data);
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
//            Debug.Log("dataString" + dataString.ToString());

             root = JsonUtility.FromJson<RootJson>(dataString);
          //  Debug.Log("root" + root.ToString());
          HaveBall = true;
        
        }

        // وقتی که برنامه بسته می‌شود، اسکریپت را ببندیم
        public void OnApplicationQuit()
        {
            Cmd.Close();
        }
    }

}