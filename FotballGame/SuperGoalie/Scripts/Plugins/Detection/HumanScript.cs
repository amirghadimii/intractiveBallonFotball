using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public GameObject LegR;
    [SerializeField] public GameObject LegL;
    [SerializeField] private int Count;
    [SerializeField] private IntializeHuman _intializeHuman;
    public void OnEnable()
    {
        _intializeHuman = GameObject.FindWithTag("IntializeHuman").GetComponent<IntializeHuman>();
        LegR.transform.position = new Vector3(0, 0, 0);
        LegL.transform.position = new Vector3(0, 0, 0);
    }

    public IEnumerator GestureToLegsOffset()
    {
        while (true)
        {
            float Dis = Vector3.Distance(LegR.transform.localPosition, LegL.transform.localPosition);
            if (Dis>2)
            {
                Count++;
                if (Count>6)
                {
                  
                }

            }
            
            yield return new WaitForSeconds(0.5f);

        }
    }

}
