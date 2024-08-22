using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClick : MonoBehaviour
{
    public Camera mainCamera; // �� ī�޶�
    public Transform rayOriginTransform; // Ray�� �������� ������ Transform (Ư�� ������Ʈ)
    public LineRenderer lineRenderer; // LineRenderer�� ���� ����

    void Start()
    {
        // LineRenderer �ʱ� ����
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // LineRenderer�� �⺻ ����
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2; // �������� ���� �� ���� ��ġ�� ����
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // �⺻ ���̴�
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    void Update()
    {
        // ���콺 ��ġ�� �������� Ray�� ������ ����
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Ray�� �������� rayOriginTransform�� ��ġ�� ����
        Vector3 rayOrigin = rayOriginTransform.position;
        Vector3 rayDirection = (ray.GetPoint(100) - rayOrigin).normalized; // Ray�� ������ ���콺 ��ġ�� ���ϵ��� ����

        RaycastHit hit;

        // LineRenderer�� ������ ���� (Ray�� �߻� ��ġ)
        lineRenderer.SetPosition(0, rayOrigin);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit))
        {
            // Ray�� �浹�� �������� LineRenderer�� ������ ����
            lineRenderer.SetPosition(1, hit.point);

            // Ray�� �浹�� ������Ʈ�� UI ������Ʈ���� Ȯ��
            RectTransform rectTransform = hit.transform.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // �浹�� UI�� ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, hit.point);

                // ��ũ�� ��ǥ ���
                Debug.Log("Ray hit UI at screen point: " + screenPoint);
            }
            else
            {
                Debug.Log("Ray hit something that is not a UI element");
            }
        }
        else
        {
            // �浹���� �ʾҴٸ� Ray �������� ������ ��� �׸����� ����
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * 100f); // 100 ���� ���̷� ����
        }
    }
}
