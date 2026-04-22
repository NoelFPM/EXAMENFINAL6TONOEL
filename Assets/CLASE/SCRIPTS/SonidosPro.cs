using Fusion;
using UnityEngine;

public class SonidosPro : NetworkBehaviour
{
    public AudioSource fuente;
    public AudioClip clipDisparo;
    public AudioClip clipSpawn;

    public override void Spawned()
    {
       
        ReproducirSpawn();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DispararSonido()
    {
        if (clipDisparo != null) fuente.PlayOneShot(clipDisparo);
    }

  
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SpawnSonido()
    {
        ReproducirSpawn();
    }

    private void ReproducirSpawn()
    {
        if (clipSpawn != null) fuente.PlayOneShot(clipSpawn);
    }
}