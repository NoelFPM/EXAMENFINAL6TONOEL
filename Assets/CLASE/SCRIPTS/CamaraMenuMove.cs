using UnityEngine;

public class CameraMenuOrbit : MonoBehaviour
{
    [Header("orbita")]
    public Transform centroDelMapa;
    public float velocidadGiro = 5f;
    public float distancia = 8f;
    public float altura = 4f;

    private void Update()
    {
        if (centroDelMapa == null) return;

        float angulo = Time.time * velocidadGiro * 0.1f;

        Vector3 offset = new Vector3(
            Mathf.Cos(angulo) * distancia,
            altura,
            Mathf.Sin(angulo) * distancia
        );

        transform.position = centroDelMapa.position + offset;
        transform.LookAt(centroDelMapa.position);
    }
}