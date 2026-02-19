using UnityEngine;

/// <summary>
/// Generador de escudos para plataformas.
/// Instancia un escudo al azar en la plataforma.
/// </summary>
public class ShieldSpawner : MonoBehaviour
{
    [Header("Escudo")]
    public GameObject shieldPrefab;
    
    [Header("Probabilidad")]
    public float spawnChance = 0.3f; // 30% de chance de tener un escudo
    
    [Header("Posición")]
    public Vector3 spawnOffset = Vector3.up * 1f;

    private bool hasSpawnedShield = false;

    void Start()
    {
        // Decidir si esta plataforma tendrá un escudo
        if (Random.value < spawnChance)
        {
            // Si no hay prefab, crear uno automáticamente
            if (shieldPrefab == null)
            {
                CreateShieldAutomatically();
            }
            else
            {
                SpawnShield();
            }
        }
    }

    private void SpawnShield()
    {
        if (hasSpawnedShield || shieldPrefab == null)
            return;

        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject shield = Instantiate(shieldPrefab, spawnPos, Quaternion.identity, transform);
        hasSpawnedShield = true;
        Debug.Log($"Escudo spawneado en plataforma {gameObject.name}");
    }

    private void CreateShieldAutomatically()
    {
        if (hasSpawnedShield)
            return;

        // Crear un GameObject para el escudo
        GameObject shieldObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shieldObj.name = "Shield";
        shieldObj.transform.parent = transform;
        shieldObj.transform.position = transform.position + spawnOffset;
        
        // Escalar y colorear
        shieldObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Cambiar color a dorado/amarillo
        Renderer renderer = shieldObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material shieldMat = new Material(Shader.Find("Standard"));
            shieldMat.color = new Color(1f, 0.84f, 0f, 1f); // Dorado
            renderer.material = shieldMat;
        }
        
        // Remover el collider del cubo primitivo
        Collider primitiveColl = shieldObj.GetComponent<Collider>();
        if (primitiveColl != null)
            DestroyImmediate(primitiveColl);
        
        // Añadir el script Shield
        Shield shieldScript = shieldObj.AddComponent<Shield>();
        
        hasSpawnedShield = true;
        Debug.Log($"Escudo automático creado en plataforma {gameObject.name}");
    }

    /// <summary>
    /// Método para forzar la aparición de un escudo (si no hay ya uno).
    /// </summary>
    public void ForceSpawnShield()
    {
        if (!hasSpawnedShield)
        {
            SpawnShield();
        }
    }
}
