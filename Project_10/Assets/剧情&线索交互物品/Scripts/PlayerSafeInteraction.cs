using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSafeInteraction : MonoBehaviour
{
    private SafeInteractable currentInteractable; // ��ǰ�ɽ����Ķ���

    void Update()
    {
        // ʹ���µ�����ϵͳ (���ʹ�þɵģ������� Input.GetKeyDown(KeyCode.O))
        if (Input.GetKeyDown(KeyCode.O) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ����ҽ��봥������Χ
        if (other.CompareTag("Interactable")) // ȷ���ɽ��������� "Interactable" ��ǩ
        {
            currentInteractable = other.GetComponent<SafeInteractable>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ������뿪��������Χ
        if (other.CompareTag("Interactable") && other.GetComponent<SafeInteractable>() == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}
