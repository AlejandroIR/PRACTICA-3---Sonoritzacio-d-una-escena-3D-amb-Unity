using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallScript : MonoBehaviour
{
    [Header("Effects")]
    public GameObject splashPrefab;
    public GameObject explosionPrefab;      // Explosión para impacto con fortaleza
    public GameObject impactParticlePrefab; // Partícula para otros impactos
    public AudioSource audioSource;
    public float damage = 10f;

    private bool hasSplashed = false;
    private bool hasExploded = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasSplashed)
            CheckWaterCollision();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        // Si golpea a una fortaleza
        if (collision.gameObject.CompareTag("Boat"))
        {
            hasExploded = true;

            // Crear efecto de explosión grande para fortaleza
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("Water"))
        {
            // Si golpea cualquier otra cosa que no sea agua ni fortaleza
            hasExploded = true;
            
            // Crear efecto de impacto más pequeño
            if (impactParticlePrefab != null)
            {
                Instantiate(impactParticlePrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    void CheckWaterCollision()
    {
        float waterHeight = 0;
        if (transform.position.y <= waterHeight)
        {
            hasSplashed = true;
            CreateSplashEffect();
        }
    }

    void CreateSplashEffect()
    {
        Instantiate(splashPrefab, transform.position, Quaternion.identity);
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.Play();
        Destroy(gameObject, 2f);
    }
}
