using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckCollision : MonoBehaviour {

    public Transform pelota;
    public Transform direccion;

    public float fuerza;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Pelota")
        {
            Debug.Log("Pego en la Red!!!");

            other.GetComponent<Rigidbody>().AddForce(-other.GetComponent<Rigidbody>().linearVelocity * other.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
            //other.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Update()
    {
        //Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
        //RaycastHit hitInfo = new RaycastHit();

        //Debug.DrawRay(transform.position, pelota.position - transform.position, Color.red);

        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    pelota.GetComponent<Rigidbody>().AddForce((transform.position - pelota.position) * fuerza);
        //}
    }


}
