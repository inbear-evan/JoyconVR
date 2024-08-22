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

        // zAngle 값을 -180도에서 180도 범위로 변환
        if (planeAngle > 180)
        {
            planeAngle -= 360;
        }

        // 각도에 따라 텍스트 업데이트
        if (planeAngle >= -0.155f)
        {
            angleTxt.text = "0";
        }
        else
        {
            angleTxt.text = (-planeAngle).ToString("F2"); // 소수점 2자리까지 표시
        }

    }

    public void OnClickPlaneReset()
    {
        PlaneBox.transform.rotation = Quaternion.identity;

    }
}
