using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Math : MonoBehaviour
{
    public Slider ejeX;
    public Slider ejeY;
    public float sensitivityY = 0.01f;
    public float sensitivityX = 0.04f;

    public Vector3 spawnPosition;
    public float Impulso;

    public bool Shoot;
    public bool Restart;

    public Transform ShootDirection;
    public Text text;

    void Start()
    {
        spawnPosition = transform.position;
        Shoot = false;
        Restart = false;
    }

    void Update()
    {
        // رسم خط برای نمایش مسیر شلیک
        Debug.DrawRay(transform.position, ShootDirection.position - transform.position, Color.black);

        // کنترل بازنشانی
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetShootState();
        }

        // بررسی لمس یا کلیک روی صفحه
        if (Input.GetMouseButtonDown(0)) // برای تاچ و کلیک
        {
            Vector3 touchPosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ShootDirection.position = hit.point;
                Debug.Log($"هدف جدید: {hit.point}");
                StartShoot();
            }
        }
    }

    private void StartShoot()
    {
        if (!Shoot)
        {
            text.text = "شلیک!";
            Shoot = true;
            StartCoroutine(WaitAndShoot(0.3f));
        }
    }

    private IEnumerator WaitAndShoot(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ShootDirection.position.Normalize();
        transform.GetComponent<Rigidbody>().AddForce((ShootDirection.position - transform.position) * Impulso, ForceMode.Impulse);
        Debug.Log("شلیک انجام شد!");
        Shoot = false;
    }

    private void ResetShootState()
    {
        transform.position = spawnPosition;
        transform.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        ejeX.value = 0;
        ejeY.value = 0;
        Shoot = false;
        text.text = "بازنشانی!";
    }
}
