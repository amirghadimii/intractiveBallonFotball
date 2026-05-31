using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using detections;
using UnityEngine;

public class IntializeHuman : MonoBehaviour
{
    [SerializeField] private UDPReceive _udpReceive;
   
    public event Action<int, Vector2, Vector2> Placed;
    public event Action<int> Picked;


  
        void OnEnable()
    {
        DontDestroyOnLoad(this.gameObject);
        Picked += ClickEventPick;
        Placed += ClickEvent;
    }

    void OnDisable()
    {
        Picked -= ClickEventPick;
        Placed -= ClickEvent;
    }

    public void ClickEvent(int Id, Vector2 PosR, Vector2 PosL)
    {

    }




    public void ClickEventPick(int Id)
    {

    }

    // Update is called once per frame

}