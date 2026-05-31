using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ArqueroController : MonoBehaviour {

    public float GetFloatCollider;

    private bool Izquierda = false;
    private bool Derecha = false;
    private bool Arriba = false;
    private bool Medio = false;
    public Transform StartPosition;

    Animator anim;
    private AnimatorStateInfo currentBaseState;
    static int abajoDerechaState = Animator.StringToHash("Base Layer.AbajoDerecha");
    static int abajoIzquierdaState = Animator.StringToHash("Base Layer.AbajoIzquierda");
    static int ArribaDerechaState = Animator.StringToHash("Base Layer.ArribaDerecha");
    static int ArribaIzquierdaState = Animator.StringToHash("Base Layer.ArribaIzquierda");
    static int IdleState = Animator.StringToHash("Base Layer.Idle");

    CapsuleCollider col;

    public Text text;

    public Math jugador;
       
    public bool bot;

    public bool execute = false;

    // Use this for initialization
    void Start () {
        anim = transform.GetComponent<Animator>();
        col = transform.GetComponent<CapsuleCollider>();
        bot = false;
        jugador = GameObject.Find("Pelota").GetComponent<Math>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        var crono = Time.time;
        //text.text = System.Math.Ceiling(crono).ToString();
        
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);


        GetFloatCollider = anim.GetFloat("RotateCollider");

        if (currentBaseState.fullPathHash == abajoDerechaState || currentBaseState.fullPathHash == abajoIzquierdaState)
        {
            if (GetFloatCollider < 0)
            {
                col.direction = 0;
                col.center = new Vector3(0, 0.25f, 0);
            }
            else
            {
                col.direction = 1;
                col.center = new Vector3(0, 1f, 0);
            }
        }

        if (currentBaseState.fullPathHash == ArribaDerechaState || currentBaseState.fullPathHash == ArribaIzquierdaState)
        {
            if (GetFloatCollider < 0)
            {
                col.direction = 0;
                col.center = new Vector3(0, 1f, 0);

                if (Arriba && Input.GetKey(KeyCode.V))
                {
                    if (Derecha)
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.left + Vector3.up * 25f, ForceMode.Impulse);
                    }
                    else
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.right + Vector3.up * 25f, ForceMode.Impulse);
                    }
                }

            }
            else
            {
                col.direction = 1;
                col.center = new Vector3(0, 1f, 0);
            }
        }

        float h;
        float v;

        if (jugador.Shoot && bot && !execute)
        {
            h = Random.Range(-5, 5); // تغییر به مقادیر بزرگتر
            v = Random.Range(-5, 5); // تغییر به مقادیر بزرگتر

            anim.SetFloat("Direccion", h);

            if (h < 0)
            {
                Izquierda = true;
                Derecha = !Izquierda;
            }
            if (h > 0)
            {
                Derecha = true;
                Izquierda = !Derecha;
            }
            if (v > 0)
            {
                Arriba = true;
                Medio = false;
            }
            if (v < 0)
            {
                Medio = false;
                Arriba = false;
            }
            if (v == 0)
            {
                Medio = true;
            }

            if (Izquierda)
                anim.SetTrigger("Izquierda");
            if (Derecha)
                anim.SetTrigger("Derecha");

            if (!Arriba && !Medio)
            {
                anim.SetTrigger("Abajo");
            }
            else
            {
                anim.SetTrigger("Arriba");
            }

            if (!Arriba && !Medio)
            {
                if (Derecha)
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left * 500f, ForceMode.Force); // تغییر مقدار نیرو
                }
                else
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.right * 500f, ForceMode.Force); // تغییر مقدار نیرو
                }
            }
            else if (Arriba)
            {
                if (Derecha)
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left + Vector3.up * 250f, ForceMode.Force); // تغییر مقدار نیرو
                }
                else
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.right + Vector3.up * 250f, ForceMode.Force); // تغییر مقدار نیرو
                }
            }
            else
            {
                if (Derecha)
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left * 500f, ForceMode.Force); // تغییر مقدار نیرو
                }
                else
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(Vector3.right * 500f, ForceMode.Force); // تغییر مقدار نیرو
                }
            }

            execute = true;
        }
        else
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            anim.SetFloat("Direccion", h);


            if (h < 0)
            {
                Izquierda = true;
                Derecha = !Izquierda;
            }

            if (h > 0)
            {
                Derecha = true;
                Izquierda = !Derecha;

            }

            if (v > 0)
            {
                Arriba = true;
                Medio = false;
            }

            if (v < 0)
            {
                Medio = false;
                Arriba = false;
            }

            if (v == 0)
            {
                Medio = true;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {

                if (Izquierda)
                    anim.SetTrigger("Izquierda");
                if (Derecha)
                    anim.SetTrigger("Derecha");

                if (!Arriba && !Medio)
                {
                    anim.SetTrigger("Abajo");
                }
                else
                {
                    anim.SetTrigger("Arriba");
                }
            }

            if (Input.GetKey(KeyCode.V))
            {

                if (!Arriba && !Medio)
                {
                    if (Derecha)
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.left * 100f, ForceMode.Impulse);
                    }
                    else
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.right * 100f, ForceMode.Impulse);
                    }
                }
                else if (Arriba)
                {
                    //if (Derecha)
                    //{
                    //    gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.left+Vector3.up * 25f, ForceMode.Impulse);
                    //}
                    //else
                    //{
                    //    gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.right + Vector3.up * 25f, ForceMode.Impulse);
                    //}
                }
                else
                {
                    if (Derecha)
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.left * 100f, ForceMode.Impulse);
                    }
                    else
                    {
                        gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.right * 100f, ForceMode.Impulse);
                    }
                }


            }
        }
                












        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if(collision.gameObject.name == "Pelota")
        {
            //collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //collision.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //collision.gameObject.GetComponent<Rigidbody>().AddForce(0f, 0f,0f, ForceMode.Impulse);
            //collision.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            collision.gameObject.GetComponent<Rigidbody>().AddForce(-collision.gameObject.GetComponent<Rigidbody>().linearVelocity * collision.gameObject.GetComponent<Rigidbody>().mass, ForceMode.Impulse);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "Pelota")
        {
            //collision.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //collision.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //collision.gameObject.GetComponent<Rigidbody>().AddForce(0f, 0f, 0f, ForceMode.Impulse);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(-collision.gameObject.GetComponent<Rigidbody>().linearVelocity * collision.gameObject.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
        }
    }

    void Update()
    {

    }
}
