using UnityEngine;

public class BGMManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private static BGMManager instance;
    void Awake()
    {
     
        if (instance == null)
        {
            instance = this;
           
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            
            Destroy(gameObject);
        }
    }
}
