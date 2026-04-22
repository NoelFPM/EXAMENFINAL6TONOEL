using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaveToMenu : MonoBehaviour
{
    private NetworkRunner _runner;
    private bool leaving = false;

    private void Start()
    {
        _runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveGame();
        }
    }

    public void LeaveGame()
    {
        if (leaving) return;
        leaving = true;

        Debug.Log("Saliendo al menú...");

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_runner != null)
        {
            _runner.Shutdown();
            Destroy(_runner.gameObject);
        }

        SceneManager.LoadScene(0);
    }
}