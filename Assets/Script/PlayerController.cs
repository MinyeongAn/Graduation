using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))] //ConfigurableJoint 참조
[RequireComponent(typeof(PlayerMotor))] //PlayerMotor 참조

public class PlayerController : MonoBehaviour
{

    [SerializeField] // ==public 과 같은 역활을 하지만 private 를 인스펙터에 보여줄수 있어 보안에 좋다
    private float speed = 10f; //캐릭터 동장 속도

    [SerializeField]
    private float lookSensitivity = 3f; //마우스 민감도 변수값

    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmout = 1f;

    public float GetThrusterFuelAmout()
    {
        return thrusterFuelAmout;
    }

    [SerializeField]
    private LayerMask environmentMask;

    [Header("점프 세팅")]
    [SerializeField]
    private JointProjectionMode jointMode = JointProjectionMode.PositionAndRotation;
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;
    [SerializeField]
    private float jointDamper = 0f;

    //Chching  구성요소
    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
    }


    void Update()
    {
        if(PauseMenu.IsOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;

            motor.Move(Vector3.zero);
            motor.Rotate(Vector3.zero);
            motor.RotateCamera(0f);

            return;
        }

        if(Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        //점프 타겟 포지션
        //지표면 말고 다른 오브젝트도 타겟팅하게 한다.
        RaycastHit _jumphit;
        if(Physics.Raycast(transform.position,Vector3.down, out _jumphit, 100f,environmentMask))
        {
            joint.targetPosition = new Vector3(0f, _jumphit.point.y , 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        //이동, 속도, 방향 계산
        float _xMove = Input.GetAxis("Horizontal"); //가로 입력 값
        float _zMove = Input.GetAxis("Vertical"); //세로 입력 값

        Vector3 _MoveHorizontal = transform.right * _xMove; //(1,0,0) x값 이동
        Vector3 _MoveVertical = transform.forward * _zMove; //(0,0,1) z값 이동

        //두개의 이동 벡터를 병합한다음 평면 벡터에 속도를 곱해 이동값을 정한다.
        Vector3 _velocity = (_MoveHorizontal + _MoveVertical) * speed;

        //애니메이션 적용
        animator.SetFloat("ForwardVelocity", _zMove);

        //이동 적용
        motor.Move(_velocity);

        //카메라 이동을 위한 계산
        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

        //회전 적용
        motor.Rotate(_rotation);

        //카메라 이동을 위한 계산
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensitivity;

        //회전 적용
        motor.RotateCamera(_cameraRotationX);


        //계산
        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmout > 0f)
        {
            thrusterFuelAmout -= thrusterFuelBurnSpeed * Time.deltaTime;

            if(thrusterFuelAmout >= 0.01f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }

            
        }else
        {
            thrusterFuelAmout += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSettings(jointSpring);
        }

        thrusterFuelAmout = Mathf.Clamp(thrusterFuelAmout, 0f, 1f);

        //점프 힘 적용
        motor.ApplyThruster(_thrusterForce);

        //카메라 민감도 
        if (Input.GetKeyDown("["))
        {
            if(lookSensitivity < 2)
            {
                return;
            }
            lookSensitivity -= 1;
            Debug.Log("현재 민감도는 : " + lookSensitivity);
        }
        if (Input.GetKeyDown("]"))
        {
            if(lookSensitivity > 19)
            {
                return;
            }
            lookSensitivity += 1;
            Debug.Log("현재 민감도는 : " + lookSensitivity);
        }

    }

    private void SetJointSettings(float _jointSpring)
    {
         joint.projectionMode = jointMode;

        joint.yDrive = new JointDrive
        {
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce,
            positionDamper = jointDamper
        };
    }

}