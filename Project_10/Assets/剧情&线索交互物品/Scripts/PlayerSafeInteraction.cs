using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSafeInteraction : MonoBehaviour
{
    private PhotonView photonView;

    private SafeInteractable currentInteractable; // 当前可交互的对象

    private void Awake()
    {
        // 3. 获取附加在同一个GameObject上的PhotonView组件
        photonView = GetComponent<PhotonView>();
    }
    
    void Update()
    {
        if (!photonView.IsMine)
        {
            // 如果不是我的，就直接退出Update，不执行任何输入检测和交互逻辑
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.O) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        if (other.CompareTag("Interactable")) // 确保可交互对象有 "Interactable" 标签
        {
            currentInteractable = other.GetComponent<SafeInteractable>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        if (other.CompareTag("Interactable") && other.GetComponent<SafeInteractable>() == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}
