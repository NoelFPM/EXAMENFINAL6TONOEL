using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 3f;

    [Networked] private PlayerRef _shooter { get; set; }
    [Networked] private byte _damage { get; set; }
    private Rigidbody _rb;
    private float _timer;

    public void SetProjectileData(PlayerRef shooter, byte damage)
    {
        _shooter = shooter;
        _damage = damage;
    }

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
            _rb.linearVelocity = transform.forward * speed;

        _timer = lifeTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        _timer -= Runner.DeltaTime;
        if (_timer <= 0) TryDespawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
         
        if (!HasStateAuthority) return;

        Health health = collision.collider.GetComponentInParent<Health>();
        if (health != null)
        {
           
            health.RPC_TakeDamage(_damage, _shooter);
            TryDespawn();
        }
    }

    private void TryDespawn()
    {
        if (Object != null && Object.IsValid && HasStateAuthority)
            Runner.Despawn(Object);
    }
}