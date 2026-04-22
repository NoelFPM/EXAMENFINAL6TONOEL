using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    public ShootMode shootMode;
    [SerializeField] protected Camera playerCam;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected LayerMask layers;
    [SerializeField] protected byte damage;
    [SerializeField] public float fireRate;
    [SerializeField] protected float range;
    [SerializeField] protected byte actualAmmo;
    [SerializeField] protected byte maxAmmoCapacity;
    [SerializeField] protected ushort ammoInStock;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected NetworkPrefabRef proyectil;

    public abstract void RigidBodyShoot();
    public abstract void RaycastShoot();

    public virtual void Reload()
    {
        if (ammoInStock <= 0)
            Debug.Log("No ammo in stock");
        return;
    }
}

public enum ShootMode
{
    Raycast, RigidBody
}