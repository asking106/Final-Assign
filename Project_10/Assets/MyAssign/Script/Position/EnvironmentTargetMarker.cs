using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentTargetMarker : MonoBehaviour
{
    
    public Transform target;           // 目标终点
    public RectTransform uiArrow;      // UI箭头图标
    public Camera mainCamera;
    public float screenEdgeBuffer = 30f;
    public TextMeshProUGUI distanceText;

    void Update()
    {
        mainCamera = Camera.main;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // 判断目标是否在屏幕前方
        bool isBehind = screenPos.z < 0;

        // 如果在摄像机后面，反转指示方向
        if (isBehind)
        {
            return;
        }

 

        // 设置 UI 位置
        uiArrow.position = screenPos;

        float distance = Vector3.Distance(mainCamera.transform.position, target.position);
       distanceText.text = $"{distance:F1}m";

    }
}
