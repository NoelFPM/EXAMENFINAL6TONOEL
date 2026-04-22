using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private float maxAngleY = 80f;
    [SerializeField] private float minAngleY = -80f;

    private SimpleKCC _kcc;

    [Networked] private float NetworkedXRotation { get; set; }

    public override void Spawned()
    {
        _kcc = GetComponentInParent<SimpleKCC>();

        if (!HasInputAuthority)
        {
            Camera cam = GetComponent<Camera>();
            AudioListener listener = GetComponent<AudioListener>();

            if (cam != null) cam.enabled = false;
            if (listener != null) listener.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_kcc == null) return;
        if (!GetInput(out InputInfo input)) return;

        float mouseX = input.lookDirection.x * mouseSensitivity * Runner.DeltaTime;
        float mouseY = input.lookDirection.y * mouseSensitivity * Runner.DeltaTime;

        NetworkedXRotation -= mouseY;
        NetworkedXRotation = Mathf.Clamp(NetworkedXRotation, minAngleY, maxAngleY);

        _kcc.AddLookRotation(0f, mouseX);
        transform.localRotation = Quaternion.Euler(NetworkedXRotation, 0f, 0f);
    }
}