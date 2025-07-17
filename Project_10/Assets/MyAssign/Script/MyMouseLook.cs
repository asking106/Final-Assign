using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyMouseLook : MonoBehaviour
{
    public float mouseSensitivity = 400f;
    private Transform playerbody;
    
    

    private MyPlayerControls controls;
    private Vector2 lookInput;
    private float mousex;
    private float mousey;

    private CharacterController characterController;
    private PhotonView photonView;
  
    private float shakePitchOffset = 0f; // 上下震动偏移

    private void Awake()
    {

        
        controls = new MyPlayerControls();
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
         
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerbody = transform.GetComponentInParent<CharacterController>().transform;
        characterController = GetComponentInParent<CharacterController>();
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        
        if (photonView != null)
        {
            if(!photonView.IsMine)
            {
                return;
                 
            }
        }
        if(playerbody.GetComponent<Myplayer>().isDead)
        {
            return;
        }
        // 摄像机绕角色旋转（鼠标X控制水平角度）
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        // 摄像机上下旋转（鼠标Y控制垂直角度）
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // 更新角度
        mousex += mouseX;
        mousey -= mouseY;
        mousey = Mathf.Clamp(mousey, -60f, 60f);

        // 应用旋转
        transform.rotation = Quaternion.Euler(mousey, mousex, 0f);

        //  float heightTarget= characterController.height * 0.9f;

        // height= Mathf.Lerp(height,heightTarget,interpolationSpeed*Time.deltaTime);
        // transform.localPosition = Vector3.up * height;
    }
    public void AddRecoil(float amount)
    {
        shakePitchOffset -= amount; // 模拟反冲往上偏移
    }
    //todo MOuse参数传回
}
