using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    [SerializeField]
    private float cameraRoatationLimit = 85f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //PlayerController.cs에서 _velocity 값을 가져온다
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    //PlayerController.cs에서 _rotation 값을 가져온다
    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    //PlayerController.cs에서 _cameraRotation 값을 가져온다
    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    //PlayerController.cs에서 _thrusterForce 값을 가져온다
    public void ApplyThruster(Vector3 _thrusterFoce)
    {
        thrusterForce = _thrusterFoce;
    }
    //고정된 프레임마다 호출 및 업데이트한다.
    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    //속도 변수에 기반한 이동
    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(
                rb.position +
                velocity *
                Time.fixedDeltaTime);  //객체의 위치 + 벡터값 * 프레임에 고정된 시간값.
        }

        if(thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    //회전 변수에 기반한 회전
    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if(cam != null)
        {
            //마우스 회전 계산
            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRoatationLimit, cameraRoatationLimit); //카메라 회전값을 y축이  뒤집히지 않게 clamp기능으로 잠근다.

            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
      

    }
}