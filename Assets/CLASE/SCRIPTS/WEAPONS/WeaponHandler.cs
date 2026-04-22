using Fusion;
using System;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon actualWeapon;

    private Action _shoot;
    private float _shootCooldown;

    public override void Spawned()
    {
        switch (actualWeapon.shootMode)
        {
            case ShootMode.Raycast:
                _shoot = actualWeapon.RaycastShoot;
                break;
            case ShootMode.RigidBody:
                _shoot = actualWeapon.RigidBodyShoot;
                break;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (!GetInput(out InputInfo input)) return;

        _shootCooldown -= Runner.DeltaTime;

        if (input.isShooting && _shootCooldown <= 0)
        {
            _shoot?.Invoke();
            _shootCooldown = 1f / actualWeapon.fireRate;
        }

        if (input.isReloading) actualWeapon.Reload();
    }
}