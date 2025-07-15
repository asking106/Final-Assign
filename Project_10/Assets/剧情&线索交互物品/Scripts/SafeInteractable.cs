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

    //public string promptMessage; // 这个字段依然需要，用来提供提示文字
    public UnityEvent onInteract; // 定义一个Unity事件，方便在Inspector中设置交互后触发的函数

    public void Interact()
    {
        onInteract.Invoke(); // 触发事件
    }
}
