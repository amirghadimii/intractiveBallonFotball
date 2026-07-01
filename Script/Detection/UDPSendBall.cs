using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections;
using detections;
using System.Threading.Tasks;

namespace MyNamespace
{
    

public class UDPSendBall : MonoBehaviour
{
    private static UDPSendBall _instance;
    public static UDPSendBall Instance { get { return _instance; } }
    
    UdpClient client;
    public string serverIP = "127.0.0.1";  // لوکال هاست
    public int port = 26500;
    private bool isSending = true;
    private Task sendTask;
    private System.Threading.CancellationTokenSource cts;

    [Range(0f, 1f)]
    public float x = 0.5f;
    [Range(0f, 1f)]
    public float y = 0.5f;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        client = new UdpClient();
        cts = new System.Threading.CancellationTokenSource();
        StartSending();
    }

    private async void StartSending()
    {
        // Wait for 1 second before starting
        await Task.Delay(1000);
        
        while (isSending && !cts.IsCancellationRequested)
        {
            try
            {
                SendBallData();
                await Task.Delay(100, cts.Token); // 3 second delay
            }
            catch (System.OperationCanceledException)
            {
                // Task was cancelled, exit gracefully
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in send loop: {e.Message}");
                await Task.Delay(100); // Wait a bit before retrying
            }
        }
    }

    public void SendBallData()
    {
        if (!isSending) return;
        
        try
        {
            BallData data = new BallData
            {
                Ball = new Ball
                {
                    x = x,
                    y = y
                }
            };

            string json = JsonUtility.ToJson(data);
            byte[] sendBytes = Encoding.UTF8.GetBytes(json);
            client.Send(sendBytes, sendBytes.Length, serverIP, port);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending ball data: {e.Message}");
        }
    }

    void OnDisable()
    {
        // Don't stop sending when disabled
    }

    void OnDestroy()
    {
        StopSending();
        cts?.Cancel();
        client?.Close();
        client = null;
        _instance = null;
    }
    
    public void StopSending()
    {
        isSending = false;
        cts?.Cancel();
    }
}

[System.Serializable]
public class BallData
{
    public Ball Ball;
}}