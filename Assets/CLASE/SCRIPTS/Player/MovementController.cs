using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

[RequireComponent(typeof(SimpleKCC))]
public class MovementController : NetworkBehaviour
{
    private SimpleKCC _kcc;
    [SerializeField] private Animator _animator;

    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 9f;

    [Networked] public Vector2 NetDir { get; set; }
    [Networked] public NetworkBool NetRunning { get; set; }

    public override void Spawned()
    {
        _kcc = GetComponent<SimpleKCC>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out InputInfo input))
        {
            float currentSpeed = input.isRunInputPressed ? runSpeed : walkSpeed;
            Vector3 moveDirection = transform.forward * input.playerPos.y + transform.right * input.playerPos.x;

            if (moveDirection.magnitude > 0.1f)
                _kcc.Move(moveDirection.normalized * currentSpeed);
            else
                _kcc.Move(Vector3.zero);

            if (HasStateAuthority)
            {
                NetDir = input.playerPos;
                NetRunning = input.isRunInputPressed;
            }
        }
    }

    public override void Render()
    {
        if (_animator == null) return;

        float multiplier = NetRunning ? 2f : 1f;

        _animator.SetFloat("WalkingX", NetDir.x * multiplier);
        _animator.SetFloat("WalkingZ", NetDir.y * multiplier);

        bool isMoving = NetDir.magnitude > 0.1f;
        _animator.SetBool("IsWalking", isMoving);
        _animator.SetBool("IsRunning", NetRunning && isMoving);
    }
}