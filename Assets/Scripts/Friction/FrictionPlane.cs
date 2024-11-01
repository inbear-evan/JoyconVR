using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FrictionPlane : MonoBehaviour
{

    [SerializeField] GameObject PlaneBox;
    [SerializeField] GameObject PlaneBox2;
    [SerializeField] TMP_Text angleTxt;

    [SerializeField] GameObject PlasticObj;
    [SerializeField] GameObject WoodObj;
    [SerializeField] GameObject AluObj;

    [SerializeField] GameObject PlasticObj2;
    [SerializeField] GameObject WoodObj2;
    [SerializeField] GameObject AluObj2;
    public GrapPosition grapPosition;
    public GrapPosition grapPosition2;

    public float planeAngle;
    // Start is called before the first frame update
    void Start()
    {
        PlaneBox.transform.rotation = Quaternion.Euler(-90, 90,0);
  
    }

    // Update is called once per frame
    void Update()
    {
        planeAngle = PlaneBox.transform.rotation.eulerAngles.x;

        // zAngle 값을 -180도에서 180도 범위로 변환
        if (planeAngle > 180)
        {
            planeAngle -= 360;
        }

        // 90도를 빼서 0부터 시작하게 만들고, 음수 값을 없애기 위한 보정
        float adjustedAngle = planeAngle - 90f;
        if (adjustedAngle < 0)
        {
            adjustedAngle += 180f; // 음수 값을 0 이상으로 보정
        }

        // 텍스트 업데이트
        angleTxt.text = adjustedAngle.ToString("F3") + " deg";

    }

    public void OnClickPlaneReset()
    {
        PlaneBox.transform.rotation = Quaternion.Euler(-90, 90, 0);

        PlasticObj.transform.position = new Vector3(-0.068f, 0.861f, 2.027f);
        PlasticObj.transform.rotation = Quaternion.identity;
        PlasticObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);

        WoodObj.transform.position = new Vector3(0.228f, 0.861f, 2.027f);
        WoodObj.transform.rotation = Quaternion.identity;
        WoodObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);

        AluObj.transform.position = new Vector3(0.555f, 0.861f, 2.027f);
        AluObj.transform.rotation = Quaternion.identity;
        AluObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);

        //PlaneBox.transform.rotation = Quaternion.Euler(-90, 90, 0);

        PlasticObj2.transform.position = new Vector3(-0.068f, 0.861f, 2.201f);
        PlasticObj2.transform.rotation = Quaternion.identity;
        PlasticObj2.transform.localScale = new Vector3(0.1f, 0.1f,0.02f);

        WoodObj2.transform.position = new Vector3(0.228f, 0.861f, 2.201f);
        WoodObj2.transform.rotation = Quaternion.identity;
        WoodObj2.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);

        AluObj2.transform.position = new Vector3(0.555f, 0.861f, 2.201f);
        AluObj2.transform.rotation = Quaternion.identity;
        AluObj2.transform.localScale = new Vector3(0.1f, 0.1f, 0.02f);

        grapPosition.Reset();
        grapPosition2.Reset();
    }
    
     
     
}
