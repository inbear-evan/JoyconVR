using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND";
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grabbaleLayer;
    [SerializeField] float rotateSpeed = 5.0f;
    [SerializeField] float movementThreshold = 0.01f; // Viewport 기준 민감도
    [SerializeField] float holdTimeThreshold = 1.0f;
    [SerializeField] float lineDistance = 10;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject groundCirclePrefab;
    [SerializeField] float defaultLineWidth = 0.1f;
    [SerializeField] float interactLineWidth = 0.05f;

    private Vector3 lastMouseViewportPosition;
    private bool isMousePressed;
    private bool isCurveActive;
    private Vector3 groundPoint;
    private bool validGroundPoint;
    private GameObject groundCircleInstance;
    private Material groundCircleInstanceMaterial;
    private RaycastHit hit;
    private GameObject clickObject;
    private GameObject selectedObject;

    private bool isGrabbing = false;
    private Vector3 grabOffset;
    private float grabDistance = 3.0f;
    private Vector3 initialRayOrigin;
    private Vector3 initialRayDirection;

    void Start()
    {
        Cursor.visible = showMouse;
        lastMouseViewportPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition); // Viewport 좌표로 초기화

        // LineRenderer 설정
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = defaultLineWidth;
            lineRenderer.endWidth = defaultLineWidth;
            ChangeLineColor(Color.white);
        }

        // Ground Circle 초기화
        if (groundCirclePrefab != null)
        {
            groundCircleInstance = Instantiate(groundCirclePrefab);
            groundCircleInstance.SetActive(false);
            groundCircleInstanceMaterial = groundCircleInstance.GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        Vector3 currentMouseViewportPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition); // 현재 마우스 위치를 Viewport 좌표로 변환
        Vector3 mouseViewportDelta = currentMouseViewportPosition - lastMouseViewportPosition;

        // Viewport 기반 회전 로직
        if (mouseViewportDelta.magnitude > movementThreshold)
        {
            float rotateX = mouseViewportDelta.y * rotateSpeed;
            float rotateY = mouseViewportDelta.x * rotateSpeed;

            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            float newLocalRotationX = localEulerAngles.x - rotateX;
            if (newLocalRotationX > 180) newLocalRotationX -= 360;
            newLocalRotationX = Mathf.Clamp(newLocalRotationX, -90, 90);

            float newLocalRotationY = localEulerAngles.y + rotateY;
            if (newLocalRotationY > 180) newLocalRotationY -= 360;
            newLocalRotationY = Mathf.Clamp(newLocalRotationY, -90, 90);

            transform.localRotation = Quaternion.Euler(newLocalRotationX, newLocalRotationY, 0);
            lastMouseViewportPosition = currentMouseViewportPosition; // 마지막 위치 업데이트
        }

        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
            print("Mouse Down Event Triggered");
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isGrabbing)
            {
                ReleaseObject();
                print("ReleaseObject Called");
            }

            selectedObject = null;
            isMousePressed = false;
        }

        // Transform 기준으로 Ray 생성
        Ray ray = new Ray(transform.position, transform.forward);
        Vector3 rayEndPoint = ray.origin + ray.direction * lineDistance;

        if (!isGrabbing)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, lineDistance, groundLayer | interactableLayer | grabbaleLayer))
            {
                groundPoint = hitInfo.point;
                validGroundPoint = true;
                clickObject = hitInfo.collider.gameObject;

                if (((1 << hitInfo.collider.gameObject.layer) & groundLayer) != 0)
                {
                    isCurveActive = true;
                    ChangeLineColor(Color.red);
                    ShowGroundCircle(groundPoint);
                    DrawBezierCurve(ray.origin, groundPoint);
                }
                else if (((1 << hitInfo.collider.gameObject.layer) & interactableLayer | grabbaleLayer) != 0)
                {
                    ChangeLineColor(Color.blue);
                    SetLineWidth(interactLineWidth);
                    lineRenderer.SetPosition(1, hitInfo.point);
                    selectedObject = hitInfo.collider.gameObject;
                }
                else
                {
                    isCurveActive = false;
                    ChangeLineColor(Color.white);
                    SetLineWidth(defaultLineWidth);
                }

                if (!isCurveActive)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, ray.origin);
                    lineRenderer.SetPosition(1, hitInfo.point);
                }

                // Grab 처리
                if (((1 << hitInfo.collider.gameObject.layer) & grabbaleLayer) != 0 && isMousePressed && !isGrabbing)
                {
                    selectedObject = hitInfo.collider.gameObject;
                    if (selectedObject != null)
                    {
                        isGrabbing = true;
                        selectedObject.GetComponent<Rigidbody>().isKinematic = true;
                        grabDistance = Vector3.Distance(transform.position, hitInfo.point);
                        grabOffset = selectedObject.transform.position - ray.GetPoint(grabDistance);
                        initialRayOrigin = ray.origin;
                        initialRayDirection = ray.direction;
                    }
                }
            }
            else
            {
                clickObject = null;
                validGroundPoint = false;
                HideGroundCircle();
                ChangeLineColor(Color.white);
                SetLineWidth(defaultLineWidth);
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, rayEndPoint);
            }
        }
        else if (isGrabbing && selectedObject != null)
        {
            Vector3 targetPosition = initialRayOrigin + initialRayDirection * grabDistance + grabOffset;
            selectedObject.transform.position = targetPosition;

            lineRenderer.SetPosition(0, initialRayOrigin);
            lineRenderer.SetPosition(1, selectedObject.transform.position);

            // 그랩 상태에서도 회전 가능하도록
            if (isMousePressed && ((1 << selectedObject.layer) & interactableLayer) != 0)
            {
                float rotationSpeed = 100.0f;
                float mouseYRotation = Input.GetAxis("Mouse Y");
                float rotationAmount = -mouseYRotation * rotationSpeed * Time.deltaTime;
                selectedObject.transform.Rotate(Vector3.right, rotationAmount, Space.Self);
            }
        }
    }

    public GameObject GetCollisionObject()
    {
        return clickObject;
    }

    void ReleaseObject()
    {
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Rigidbody>().isKinematic = false;
            selectedObject = null;
        }
        isGrabbing = false;
    }

    void ChangeLineColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.material.color = color;
        }

        if (groundCircleInstanceMaterial != null)
        {
            groundCircleInstanceMaterial.color = color;
        }
    }

    void SetLineWidth(float width)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }

    void DrawBezierCurve(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer != null)
        {
            Vector3 controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 2.0f;
            int segmentCount = 20;
            lineRenderer.positionCount = segmentCount + 1;
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 bezierPoint = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
                lineRenderer.SetPosition(i, bezierPoint);
            }
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    void ShowGroundCircle(Vector3 position)
    {
        if (groundCircleInstance != null)
        {
            groundCircleInstance.transform.position = position;
            groundCircleInstance.SetActive(true);
        }
    }

    void HideGroundCircle()
    {
        if (groundCircleInstance != null)
        {
            groundCircleInstance.SetActive(false);
        }
    }
}
