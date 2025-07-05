using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool hasLoggedIn = false;
   

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持切换场景时不被销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

}
