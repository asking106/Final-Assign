using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SafeInteractable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public string promptMessage; // ����ֶ���Ȼ��Ҫ�������ṩ��ʾ����
    public UnityEvent onInteract; // ����һ��Unity�¼���������Inspector�����ý����󴥷��ĺ���

    public void Interact()
    {
        onInteract.Invoke(); // �����¼�
    }
}
