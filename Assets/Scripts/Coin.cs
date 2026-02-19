using UnityEngine;

/// <summary>
/// Moneda coleccionable. Rota sobre sí misma y tiene un trigger.
/// Asignar tag "Coin" al GameObject.
/// </summary>
public class Coin : MonoBehaviour
{
    public float rotateSpeed = 180f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public float spinTilt = 25f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rotación múltiple (para que gire en diferentes ejes)
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, rotateSpeed * 0.3f * Time.deltaTime, Space.Self);

        // Movimiento arriba-abajo
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Tilt dinámico según rotación
        float tilt = Mathf.Sin(Time.time * bobSpeed * 0.5f) * spinTilt;
        transform.rotation *= Quaternion.Euler(0, 0, tilt * Time.deltaTime);
    }
}
