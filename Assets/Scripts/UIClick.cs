using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClick : MonoBehaviour
{
    public Camera mainCamera; // 주 카메라
    public Transform rayOriginTransform; // Ray의 시작점을 정의할 Transform (특정 오브젝트)
    public LineRenderer lineRenderer; // LineRenderer를 위한 변수

    void Start()
    {
        // LineRenderer 초기 설정
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // LineRenderer의 기본 설정
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2; // 시작점과 끝점 두 개의 위치를 가짐
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 기본 쉐이더
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    void Update()
    {
        // 마우스 위치를 기준으로 Ray의 방향을 설정
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Ray의 시작점을 rayOriginTransform의 위치로 설정
        Vector3 rayOrigin = rayOriginTransform.position;
        Vector3 rayDirection = (ray.GetPoint(100) - rayOrigin).normalized; // Ray의 방향을 마우스 위치를 향하도록 설정

        RaycastHit hit;

        // LineRenderer의 시작점 설정 (Ray의 발사 위치)
        lineRenderer.SetPosition(0, rayOrigin);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit))
        {
            // Ray가 충돌한 지점으로 LineRenderer의 끝점을 설정
            lineRenderer.SetPosition(1, hit.point);

            // Ray가 충돌한 오브젝트가 UI 오브젝트인지 확인
            RectTransform rectTransform = hit.transform.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 충돌한 UI의 월드 좌표를 스크린 좌표로 변환
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, hit.point);

                // 스크린 좌표 출력
                Debug.Log("Ray hit UI at screen point: " + screenPoint);
            }
            else
            {
                Debug.Log("Ray hit something that is not a UI element");
            }
        }
        else
        {
            // 충돌하지 않았다면 Ray 방향으로 라인을 계속 그리도록 설정
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * 100f); // 100 단위 길이로 설정
        }
    }
}
