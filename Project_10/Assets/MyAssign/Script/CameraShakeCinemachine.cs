using UnityEngine;
using System.Collections;

public class CameraShakeCinemachine : MonoBehaviour
{
    private MyMouseLook mouseLook;
    private Coroutine returnCoroutine;
    

    private void Start()
    {
       
        mouseLook = GetComponentInChildren<MyMouseLook>();  
    }

    public void CameraShake(float recoilAmount = 2f, float returnSpeed = 10f)
    {
        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        mouseLook.AddRecoil(recoilAmount);

        returnCoroutine = StartCoroutine(ReturnRecoil(recoilAmount, returnSpeed));
    }

    private IEnumerator ReturnRecoil(float amount, float speed)
    {
        yield return new WaitForSeconds(0.15f);
        float current = amount;
        while (current > 0f)
        {
            current -= speed * Time.deltaTime;
            mouseLook.AddRecoil(-speed * Time.deltaTime);  
            yield return null;
        }
    }
}
