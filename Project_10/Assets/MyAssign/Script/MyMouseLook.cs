using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyMouseLook : MonoBehaviour
{
    public float mouseSensitivity = 400f;
    private Transform playerbody;
    private float yRotation = 0f;
    

    private MyPlayerControls controls;
    private Vector2 lookInput;

    private CharacterController characterController;
    private PhotonView photonView;
    private float height = 1.8f;
    private float interpolationSpeed = 12f;
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
        float mouseX = (lookInput.x ) * mouseSensitivity * Time.deltaTime;
        float mouseY = (lookInput.y) * mouseSensitivity * Time.deltaTime;
        


        yRotation -= mouseY;
        yRotation += shakePitchOffset;
        shakePitchOffset = 0f;
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);
         

        transform.localRotation = Quaternion.Euler(yRotation  , 0f, 0f);
        playerbody.Rotate(Vector3.up * mouseX);
        float heightTarget= characterController.height * 0.9f;
        
       height= Mathf.Lerp(height,heightTarget,interpolationSpeed*Time.deltaTime);
        transform.localPosition = Vector3.up * height;
    }
    public void AddRecoil(float amount)
    {
        shakePitchOffset -= amount; // 模拟反冲往上偏移
    }
    //todo MOuse参数传回
}
