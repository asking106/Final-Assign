using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSafeInteraction : MonoBehaviour
{
    private SafeInteractable currentInteractable; // 当前可交互的对象

    void Update()
    {
        // 使用新的输入系统 (如果使用旧的，可以用 Input.GetKeyDown(KeyCode.O))
        if (Input.GetKeyDown(KeyCode.O) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 当玩家进入触发器范围
        if (other.CompareTag("Interactable")) // 确保可交互对象有 "Interactable" 标签
        {
            currentInteractable = other.GetComponent<SafeInteractable>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 当玩家离开触发器范围
        if (other.CompareTag("Interactable") && other.GetComponent<SafeInteractable>() == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}
