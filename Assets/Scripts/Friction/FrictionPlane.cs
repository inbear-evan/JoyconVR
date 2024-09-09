using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FrictionPlane : MonoBehaviour
{

    [SerializeField] GameObject PlaneBox;
    [SerializeField] TMP_Text angleTxt;

    [SerializeField] GameObject PlasticObj;
    [SerializeField] GameObject WoodObj;
    [SerializeField] GameObject AluObj;
    public GrapPosition grapPosition;

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

        // zAngle ���� -180������ 180�� ������ ��ȯ
        if (planeAngle > 180)
        {
            planeAngle -= 360;
        }

        // 90���� ���� 0���� �����ϰ� �����, ���� ���� ���ֱ� ���� ����
        float adjustedAngle = planeAngle - 90f;
        if (adjustedAngle < 0)
        {
            adjustedAngle += 180f; // ���� ���� 0 �̻����� ����
        }

        // �ؽ�Ʈ ������Ʈ
        angleTxt.text = adjustedAngle.ToString("F3") + " deg";

    }

    public void OnClickPlaneReset()
    {
        PlaneBox.transform.rotation = Quaternion.Euler(-90, 90, 0);

        PlasticObj.transform.position = new Vector3(0.611999989f, 0.888400018f, 1.69500005f);
        PlasticObj.transform.rotation = Quaternion.identity;
        PlasticObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);

        WoodObj.transform.position = new Vector3(0.614000022f, 0.871999979f, 1.875f);
        WoodObj.transform.rotation = Quaternion.identity;
        WoodObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);

        AluObj.transform.position = new Vector3(0.614000022f, 0.861000001f, 2.09699988f);
        AluObj.transform.rotation = Quaternion.identity;
        AluObj.transform.localScale = new Vector3(0.10864415f, 0.10864415f, 0.10864415f);
        grapPosition.Reset();
    }
    
     
     
}
