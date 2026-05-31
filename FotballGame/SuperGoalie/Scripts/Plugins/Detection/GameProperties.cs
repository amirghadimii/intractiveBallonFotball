
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class properties 
{
    [SerializeField] public int GameCount;
    [SerializeField] public int Xmin;
    [SerializeField] public int Xmax;
    [SerializeField] public int Ymin;
    [SerializeField] public int Ymax;
}
[CreateAssetMenu]
public class GameProperties : ScriptableObject
{
    public List<properties> _properties;
        
}