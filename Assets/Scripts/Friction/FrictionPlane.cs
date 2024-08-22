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
    public float planeAngle;
    // Start is called before the first frame update
    void Start()
    {
        PlaneBox.transform.rotation = Quaternion.identity;
        
    }

    // Update is called once per frame
    void Update()
    {
        planeAngle = PlaneBox.transform.rotation.eulerAngles.z;

        // zAngle ���� -180������ 180�� ������ ��ȯ
        if (planeAngle > 180)
        {
            planeAngle -= 360;
        }

        // ������ ���� �ؽ�Ʈ ������Ʈ
        if (planeAngle >= -0.155f)
        {
            angleTxt.text = "0";
        }
        else
        {
            angleTxt.text = (-planeAngle).ToString("F2"); // �Ҽ��� 2�ڸ����� ǥ��
        }

    }

    public void OnClickPlaneReset()
    {
        PlaneBox.transform.rotation = Quaternion.identity;

    }
}
