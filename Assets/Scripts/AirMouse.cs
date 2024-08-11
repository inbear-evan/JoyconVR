using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND"; // 바닥 태그
    [SerializeField] LayerMask groundLayer;  // 바닥 레이어 마스크
    [SerializeField] LayerMask interactableLayer; // 특정 물체 레이어 마스크
    [SerializeField] float rotateSpeed = 5.0f;
    [SerializeField] float movementThreshold = 2.0f;
    [SerializeField] float holdTimeThreshold = 1.0f; // 누르고 있어야 하는 시간
    [SerializeField] float lineDistance = 10;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject groundCirclePrefab; // 원 이미지를 표시할 프리팹
    [SerializeField] float defaultLineWidth = 0.1f; // 기본 라인 두께
    [SerializeField] float interactLineWidth = 0.05f; // 상호작용 시 라인 두께

    private Vector3 lastMousePosition;
    private bool isMousePressed;
    private bool isCurveActive;
    private float mousePressTime;
    private Vector3 groundPoint;
    private bool validGroundPoint;
    private bool usingTag;
    private GameObject groundCircleInstance;
    Material groundCircleInstanceMaterial;
    RaycastHit hit;
    GameObject clickObject;

    void Start()
    {
        Cursor.visible = showMouse;
        lastMousePosition = Input.mousePosition;

        // LineRenderer 설정
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = defaultLineWidth;
            lineRenderer.endWidth = defaultLineWidth;
            ChangeLineColor(Color.white); // 기본 색상을 하얀색으로 설정
        }

        // Ground Circle 초기화
        if (groundCirclePrefab != null)
        {
            groundCircleInstance = Instantiate(groundCirclePrefab);
            groundCircleInstance.SetActive(false); // 초기에는 비활성화

            groundCircleInstanceMaterial = groundCircleInstance.GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        if (mouseDelta.magnitude > movementThreshold)
        {
            // 마우스 이동에 따라 회전 각도 계산
            float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
            float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;

            // 현재 로컬 회전 각도 가져오기
            Vector3 localEulerAngles = transform.localRotation.eulerAngles;

            // 로컬 X축 회전 제한 (-90도에서 90도 사이)
            float newLocalRotationX = localEulerAngles.x - rotateX;
            if (newLocalRotationX > 180) newLocalRotationX -= 360;
            newLocalRotationX = Mathf.Clamp(newLocalRotationX, -90, 90);

            // 로컬 Y축 회전 제한 (-90도에서 90도 사이)
            float newLocalRotationY = localEulerAngles.y + rotateY;
            if (newLocalRotationY > 180) newLocalRotationY -= 360;
            newLocalRotationY = Mathf.Clamp(newLocalRotationY, -90, 90);

            // 새로운 로컬 회전 각도로 설정
            transform.localRotation = Quaternion.Euler(newLocalRotationX, newLocalRotationY, 0);

            // 현재 마우스 위치를 마지막 위치로 업데이트
            lastMousePosition = currentMousePosition;
        }

        // 마우스 클릭 상태 업데이트
        if (Input.GetMouseButtonDown(0)) // 왼쪽 마우스 버튼 클릭
        {
            isMousePressed = true;
            //mousePressTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0)) // 왼쪽 마우스 버튼에서 손을 뗌
        {
            clickObject = hit.collider.gameObject;
            isMousePressed = false;
            if (isCurveActive && validGroundPoint && (((1 << clickObject.layer) & groundLayer) != 0))
            {
                TeleportToGroundPoint();
            }
            isCurveActive = false;
            HideGroundCircle();
           
        }

        // Raycast로 마우스 움직임에 따라 레이저 포인터 업데이트
        Ray ray = new Ray(transform.position, transform.forward);
      

        if (Physics.Raycast(ray, out hit, lineDistance, groundLayer | interactableLayer))
        {
            groundPoint = hit.point;
            validGroundPoint = true;

            //clickObject = hit.collider.gameObject;
            if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
            {
                //validGroundPoint = true;
                isCurveActive = true;
                ChangeLineColor(Color.red);
                ShowGroundCircle(groundPoint); // 텔레포트 기능이 활성화된 경우에만 원을 표시
                DrawBezierCurve(ray.origin, groundPoint); // 베지어 곡선 그리기
            }
            
            else if (((1 << hit.collider.gameObject.layer) & interactableLayer) != 0)
            {
                //Debug.Log(hit.collider.gameObject.name);
                isCurveActive = false;
                ChangeLineColor(Color.blue); // 특정 물체에 닿았을 때 파란색으로 변경
                SetLineWidth(interactLineWidth); // 특정 물체에 닿았을 때 라인을 얇게 변경
            }
            else
            {
                isCurveActive = false;
                ChangeLineColor(Color.white); // 그렇지 않으면 기본 색상으로 변경
                SetLineWidth(defaultLineWidth); // 기본 두께로 변경
            }

            if(!isCurveActive)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, hit.point); // 레이캐스트에 맞춘 지점으로 라인 그리기
                
            }
        }
        else
        {
            clickObject = null;
            validGroundPoint = false;
            HideGroundCircle(); // 유효하지 않은 경우 원을 숨김

            ChangeLineColor(Color.white); // 기본 색상으로 변경
            SetLineWidth(defaultLineWidth); // 기본 두께로 변경
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * lineDistance); // 기본 거리만큼 라인 그리기
        }
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
            Vector3 controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 2.0f; // Control point for the Bezier curve

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
        Vector3 p = uu * p0; // u^2 * P0
        p += 2 * u * t * p1; // 2 * u * t * P1
        p += tt * p2; // t^2 * P2
        return p;
    }

    void TeleportToGroundPoint()
    {
        // 텔레포트 기능 구현
        if (validGroundPoint)
        {
            transform.parent.parent.position = new Vector3(groundPoint.x, transform.parent.parent.position.y, groundPoint.z);
        }
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

    // 부딫힌 게임오브젝트 데이터
    public GameObject GetCollisionObject()
    {
        if (clickObject != null)
            return clickObject;
        else return null;
    }
}
