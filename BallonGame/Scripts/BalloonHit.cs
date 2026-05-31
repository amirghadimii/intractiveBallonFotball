using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonHit : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject Circle;
    [SerializeField] private  StartGameBalloon startGameBalloon ;
    void OnEnable()
    {
        
    }
    public void ShootAtNormalizedPosition(float normX, float normY)
    {
     

        // تبدیل مقدار نرمال شده به مختصات صفحه نمایش
        float screenX = normX * Screen.width;
        float screenY = normY * Screen.height;
        Vector3 Pso = new Vector3(screenX, screenY, 50);

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Pso);
        Debug.Log($"normX: {worldPoint.x}, normY: {worldPoint.y}");
        GameObject Ins = Instantiate(Circle, Circle.transform.position,Circle.transform.rotation);
        Ins.gameObject.transform.position = worldPoint;
        Destroy(Ins,2);
        RaycastHit2D _hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (_hit.collider!=null)
        {
            Balloon balloon = _hit.collider.GetComponent<Balloon>();
            if (balloon != null)
            {
                Debug.Log("Balloon Found!");
                // هر کاری می‌خوای با balloon انجام بده
                balloon.BalloonHit();
        
                // مثلا اگه همچین تابعی داشته باشه
            }
            else
            {

                    SelectPlayer _SelectPlayer = _hit.collider.GetComponent<SelectPlayer>();

                    if (_SelectPlayer!=null)
                    {
                        _SelectPlayer.SelectPlayerButton();
                        startGameBalloon.OnStartGameBalloonHit();
                    }
                
            }
        }
        else
        {
            Debug.Log("Nothing hit.");
        }
    }


}
