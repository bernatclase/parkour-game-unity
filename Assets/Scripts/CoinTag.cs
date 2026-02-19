using UnityEngine;

/// <summary>
/// Script que identifica a las monedas y gestiona su recogida + explosión visual.
/// </summary>
public class CoinTag : MonoBehaviour
{
    private bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Buscar PlayerController en el propio GO o en padres
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();

        if (pc != null)
        {
            collected = true;
            CreateCoinExplosion();

            if (GameManager.Instance != null)
                GameManager.Instance.CollectCoin();

            Destroy(gameObject);
        }
    }

    void CreateCoinExplosion()
    {
        Vector3 pos     
          = transform.position;
        Color   coinColor = new Color(1f, 1f, 0f);

        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position   = pos;
            particle.transform.localScale = Vector3.one * 0.2f;

            Renderer r = particle.GetComponent<Renderer>();
            if (r != null)
            {
                Shader sh  = Shader.Find("Universal Render Pipeline/Lit");
                if (sh == null) sh = Shader.Find("Standard");
                Material mat = new Material(sh);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", coinColor);
                mat.color = coinColor;
                r.material = mat;
            }

            // Quitar colisión para no interferir
            Collider col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Física
            Rigidbody prb = particle.AddComponent<Rigidbody>();
            prb.useGravity = true;
            float angle = (360f / 8f) * i;
            Vector3 dir = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                1f,
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;
            prb.linearVelocity = dir * 5f;

            Destroy(particle, 0.5f);
        }
    }
}
