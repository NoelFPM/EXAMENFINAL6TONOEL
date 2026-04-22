using Fusion;
using UnityEngine;

public class Handgun : Weapon
{
    public override void Spawned()
    {
        if (HasInputAuthority)
            playerCam = GetComponentInParent<NetworkObject>().GetComponentInChildren<Camera>();
    }

    private Vector3 GetShootOrigin()
    {
        Transform camTransform = GetComponentInParent<NetworkObject>().GetComponentInChildren<Camera>().transform;
        return camTransform.position + camTransform.forward * 1.5f;
    }

    private Quaternion GetShootRotation()
    {
        return GetComponentInParent<NetworkObject>().GetComponentInChildren<Camera>().transform.rotation;
    }

    public override void RigidBodyShoot() => RPC_RigidBodyShoot(GetShootOrigin(), GetShootRotation());

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RigidBodyShoot(Vector3 pos, Quaternion rotation, RpcInfo info = default)
    {
      
        GetComponentInParent<SonidosPro>().RPC_DispararSonido();

        Runner.Spawn(proyectil, pos, rotation, Object.InputAuthority, (runner, obj) =>
        {
            obj.GetComponent<Projectile>().SetProjectileData(Object.InputAuthority, damage);
        });
    }

    public override void RaycastShoot() => RPC_RaycastShoot();

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RaycastShoot(RpcInfo info = default)
    {
        
        GetComponentInParent<SonidosPro>().RPC_DispararSonido();

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit, range, layers))
        {
            Health health = hit.collider.GetComponentInParent<Health>();
            if (health != null)
                health.RPC_TakeDamage(damage, Object.InputAuthority);
        }
    }

    public override void Reload()
    {
        base.Reload();
    }
}