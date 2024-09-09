using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND"; // 바닥 태그
    [SerializeField] LayerMask groundLayer;  // 바닥 레이어 마스크
    [SerializeField] LayerMask interactableLayer; // 특정 물체 레이어 마스크
    [SerializeField] LayerMask grabbaleLayer; // 특정 물체 레이어 마스크
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
    private bool isInteracting;
    private GameObject selectedObject;
    private Quaternion initialRotation;


    private bool isGrabbing = false; // 그랩 상태 여부
    public float grabDistance = 3.0f;  // 잡고 있는 물체와의 거리
    private Vector3 grabOffset;
    private Vector3 grabPoint; // Ray 끝 지점
    public GrapPosition grapPosition;
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
        if (selectedObject != null)
        {
            initialRotation = selectedObject.transform.localRotation;
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
            print("Mouse Down Event Triggered");
        }

        if (Input.GetMouseButtonUp(0)) // 왼쪽 마우스 버튼에서 손을 뗌
        {
      
            if (isGrabbing)
            {
                if (GameObject.Find("GrabPosition") != null)
                {
                    grapPosition.DisableUp(selectedObject);
                }
                ReleaseObject();
                print("ReleaseObject Called");
            }

            // Reset selectedObject
            clickObject = hit.collider != null ? hit.collider.gameObject : null;
            if (clickObject != null)
            {
                print("Hit Object: " + clickObject.name);
            }
            else
            {
                print("No object hit.");
            }

            selectedObject = null;
            isMousePressed = false;

            if (isCurveActive && validGroundPoint && (((1 << clickObject.layer) & groundLayer) != 0))
            {
                TeleportToGroundPoint();
            }

            if (clickObject != null && clickObject.layer == LayerMask.NameToLayer("INTERACTUI"))
            {
                Button button = clickObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    Debug.Log("Button clicked: " + hit.transform.name);
                }
            }

            isCurveActive = false;
            HideGroundCircle();
        }

        // 마우스 포인터의 스크린 좌표로부터 Ray를 발사
        Ray rayScreen = Camera.main.ScreenPointToRay(currentMousePosition);

        // Ray의 끝 지점을 계산
        Vector3 rayEndPoint = rayScreen.origin + rayScreen.direction * 5000;
        if (isGrabbing) { 
            if (Physics.Raycast(rayScreen, out RaycastHit hitInfo, 5000, grabbaleLayer))
            {
                rayEndPoint = hitInfo.point;

            }
        }   
        else
        {
            if (Physics.Raycast(rayScreen, out RaycastHit hitInfo, 5000))
            {
                rayEndPoint = hitInfo.point;

            }
        }
            // Raycast로 마우스 움직임에 따라 레이저 포인터 업데이트
            Ray ray = new Ray(transform.position, rayEndPoint - transform.position);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, lineDistance, groundLayer | interactableLayer | grabbaleLayer))
        {
            groundPoint = hit.point;
            validGroundPoint = true;

            if (!isGrabbing)
            {
                //clickObject = hit.collider.gameObject;
                if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
                {
                    //validGroundPoint = true;
                    isCurveActive = true;
                    ChangeLineColor(Color.red);
                    ShowGroundCircle(groundPoint); // 텔레포트 기능이 활성화된 경우에만 원을 표시
                    DrawBezierCurve(ray.origin, groundPoint); // 베지어 곡선 그리기
                }

                else if (((1 << hit.collider.gameObject.layer) & interactableLayer | grabbaleLayer) != 0)
                {

                    //Debug.Log(hit.collider.gameObject.name);
                    ChangeLineColor(Color.blue); // 특정 물체에 닿았을 때 파란색으로 변경
                    SetLineWidth(interactLineWidth); // 특정 물체에 닿았을 때 라인을 얇게 변경

                    Vector3 hitPoint = hit.point; // 충돌 지점의 위치를 얻음
                    lineRenderer.SetPosition(1, hitPoint); // 라인렌더러의 끝 위치를 충돌 지점으로 설정

                    selectedObject = hit.collider.gameObject;
                    //Debug.Log(selectedObject.name);

                }
                else
                {
                    isCurveActive = false;
                    ChangeLineColor(Color.white); // 그렇지 않으면 기본 색상으로 변경
                    SetLineWidth(defaultLineWidth); // 기본 두께로 변경
                }
            }
            if(!isCurveActive)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, hit.point); // 레이캐스트에 맞춘 지점으로 라인 그리기
            }

            // Frictions
            if (isMousePressed && selectedObject != null && ((1 << selectedObject.layer) & interactableLayer) != 0 &&
    selectedObject.layer != LayerMask.GetMask("INTERACTUI"))
            {
                float rotationSpeed = 100.0f; // 회전 속도 조절을 위한 계수
                float mouseY = Input.GetAxis("Mouse Y"); // 마우스 Y축 이동 값
                float rotationAmount = 0; // 회전할 각도 계산

                // 현재 로테이션을 초기 로테이션 기준으로 변환 (상대적인 각도)
                Quaternion currentRotation = selectedObject.transform.localRotation;
                Quaternion relativeRotation = Quaternion.Inverse(initialRotation) * currentRotation;

                // Z축 회전 각도를 가져옴
                float zAngle = relativeRotation.eulerAngles.x;

                // zAngle 값을 -180도에서 180도 범위로 변환
                if (zAngle > 180)
                {
                    zAngle -= 360;
                }

                // 회전하려는 값이 지정된 범위를 넘어가지 않도록 제한
                float targetAngle = zAngle - mouseY * rotationSpeed * Time.deltaTime;
                targetAngle = Mathf.Clamp(targetAngle, -25, 0);

                // 제한된 회전 각도에서 현재 각도를 빼서 실제 회전할 양을 계산
                rotationAmount = targetAngle - zAngle;

                // 실제로 Z축 기준으로 회전 적용
                selectedObject.transform.Rotate(-Vector3.right, rotationAmount);

                // 회전이 완료된 후에도 계속 초기 로테이션을 기준으로 유지
                //initialRotation = selectedObject.transform.localRotation;
            }

            //Grab
            groundPoint = hit.point;
            validGroundPoint = true;
            //Grab - 클릭한 오브젝트가 Grabbable 레이어에 속하는 경우
            if (((1 << hit.collider.gameObject.layer) & grabbaleLayer) != 0)
            {
                // 그랩 가능한 오브젝트를 클릭할 경우
                if (isMousePressed && !isGrabbing)
                {
                    selectedObject = hit.collider.gameObject;
                    if (selectedObject != null)
                    {
                        isGrabbing = true;
                        selectedObject.GetComponent<Rigidbody>().isKinematic = true; // 물리효과 제거

                        // 처음 잡았을 때의 Ray 시작점과 물체 사이의 거리 계산
                        grabDistance = Vector3.Distance(Camera.main.transform.position, hit.point);

                        // 물체와 Ray 간의 상대적인 위치 (오프셋) 계산
                        grabOffset = selectedObject.transform.position - hit.point;
                    }
                }

                // 그랩 상태인 경우, 처음 잡은 거리와 상대적 위치를 유지하면서 움직임
                if (isGrabbing && selectedObject != null)
                {
                    Vector3 targetPosition = ray.GetPoint(grabDistance) + grabOffset;
                    selectedObject.transform.position = ray.origin + ray.direction * grabDistance; 
                }
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

    public void RecenterView()
    {
        Api.Recenter();
    }
    void ReleaseObject()
    {
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Rigidbody>().isKinematic = false; // 물리효과 다시 적용
            selectedObject = null;
            
        }
        isGrabbing = false;
    }

}
