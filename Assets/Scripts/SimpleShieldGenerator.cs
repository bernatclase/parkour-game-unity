using UnityEngine;

/// <summary>
/// Generador automático de escudos para plataformas sin necesidad de prefabs.
/// Simplemente añade este script a una plataforma.
/// </summary>
public class SimpleShieldGenerator : MonoBehaviour
{
    [Header("Configuración")]
    [Range(0f, 1f)]
    public float spawnChance = 0.3f;
    
    [Header("Apariencia")]
    public Vector3 shieldScale = new Vector3(0.4f, 0.4f, 0.4f);
    public Vector3 spawnHeight = Vector3.up * 1f;
    public Color shieldColor = new Color(1f, 0.84f, 0f, 1f); // Dorado

    private bool hasSpawned = false;

    void OnEnable()
    {
        // Generar escudo al activar
        if (!hasSpawned && Random.value < spawnChance)
        {
            GenerateShield();
        }
    }

    void GenerateShield()
    {
        if (hasSpawned)
            return;

        // Crear objeto visual del escudo
        GameObject shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldVisual.name = "Shield_Visual";
        shieldVisual.transform.parent = transform;
        shieldVisual.transform.localPosition = spawnHeight;
        shieldVisual.transform.localScale = shieldScale;
        
        // Colorear el escudo
        Renderer renderer = shieldVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material shieldMat = new Material(Shader.Find("Standard"));
            shieldMat.color = shieldColor;
            renderer.material = shieldMat;
        }
        
        // Remover el collider del primitivo
        Collider primitiveColl = shieldVisual.GetComponent<Collider>();
        if (primitiveColl != null)
            DestroyImmediate(primitiveColl);
        
        // Crear objeto para el trigger
        GameObject triggerObj = new GameObject("Shield_Trigger");
        triggerObj.transform.parent = shieldVisual.transform;
        triggerObj.transform.localPosition = Vector3.zero;
        
        SphereCollider trigger = triggerObj.AddComponent<SphereCollider>();
        trigger.radius = 0.6f;
        trigger.isTrigger = true;
        
        // Añadir el script Shield
        Shield shield = shieldVisual.AddComponent<Shield>();
        triggerObj.GetComponent<SphereCollider>().enabled = true;
        
        hasSpawned = true;
        Debug.Log($"Escudo generado automáticamente en {gameObject.name}");
    }

    public void RegenerateShield()
    {
        hasSpawned = false;
        GenerateShield();
    }
}
