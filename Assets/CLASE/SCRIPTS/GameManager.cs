using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] dontDestroyOnLoadObjs;

    private void Awake()
    {
        foreach (GameObject go in dontDestroyOnLoadObjs)
        {
            if (go != null)
                DontDestroyOnLoad(go);
        }
    }
}