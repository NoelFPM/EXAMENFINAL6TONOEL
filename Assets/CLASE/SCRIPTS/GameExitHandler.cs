using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameExitHandler : NetworkBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisconnectAndExit();
        }
    }

    public void DisconnectAndExit()
    {
        if (Runner != null)
        {
            
            Runner.Shutdown();
           
            SceneManager.LoadScene("Menu");
        }
    }

    
    public override void Despawned(NetworkRunner runner, bool hasStats)
    {
       
        Debug.Log("El oponente se desconectó. Volviendo al menú...");
        SceneManager.LoadScene("Menu");
    }
}