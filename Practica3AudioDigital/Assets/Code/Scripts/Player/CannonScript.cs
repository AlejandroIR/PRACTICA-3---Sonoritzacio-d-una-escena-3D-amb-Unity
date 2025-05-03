using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonScript : MonoBehaviour
{
    public Transform cannonMuzzle;
    public GameObject cannonballPrefab;
    public GameObject smokeParticlesPrefab; // Prefab de partículas de humo
    public float fireVelocity = 10f;

    //public LineRenderer trajectoryLine; // Asigna un LineRenderer desde el Inspector
    public int trajectoryPoints = 50;   // Número de puntos para la trayectoria
    public float timeStep = 0.1f;       // Intervalo de tiempo entre puntos
    public AudioSource audioSource;

    public Light cannonLight; // Light component on the cannon muzzle
    public float lightIntensity = 20f;
    public float lightFadeDuration = 0.5f;

    public bool canFire = true; // Flag to prevent multiple firings
    public float fireCooldown = 2f; // Cooldown between firing

    private bool playerInTrigger = false; // Track if player is in trigger zone
    public float fireInterval = 1f; // Interval between shots when player is in zone
    private Coroutine firingCoroutine; // Reference to the firing coroutine

    // Start is called before the first frame update
    void Start()
    {

        // Fire(fireVelocity, Vector3.zero); // Call Fire to ensure the cannon is ready to fire

        // Ensure the light starts with intensity 0
        if (cannonLight != null)
        {
            cannonLight.intensity = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Empty for now, logic moved to trigger events
    }

    // This method will be called when another collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            // Start interval firing coroutine
            if (firingCoroutine == null)
            {
                firingCoroutine = StartCoroutine(IntervalFiring());
            }
        }
    }

    // Called when something exits the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            // Stop firing when player leaves
            if (firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
                firingCoroutine = null;
            }
        }
    }

    // Coroutine for interval firing
    private IEnumerator IntervalFiring()
    {
        while (playerInTrigger)
        {
            if (canFire)
            {
                Fire(fireVelocity, Vector3.zero);
                StartCoroutine(FireCooldown());
            }

            // Wait for the specified interval before firing again
            yield return new WaitForSeconds(fireInterval);
        }
    }

    // Make this method public so it can be called from FireTriggerScript
    public IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }

    public void Aim()
    {
        // Implement aiming logic here
        // For example, rotate the cannon towards the target direction
    }

    public void Fire(float fireVelocity, Vector3 boatVelocity)
    {
        if (cannonMuzzle != null && cannonballPrefab != null)
        {
            Debug.Log("disparando");
            //Debug.Log($"Creating cannonball at {cannonMuzzle.position}");
            GameObject cannonball = Instantiate(cannonballPrefab, cannonMuzzle.position, cannonMuzzle.rotation);
            // Debug.Log(trajectoryLine);
            // trajectoryLine.enabled = false;
            audioSource.Play();


            if (cannonball != null)
            {
                //Debug.Log($"Cannonball created: {cannonball.name}");
                Rigidbody rb = cannonball.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 initialVelocity = cannonMuzzle.forward * fireVelocity;
                    //Debug.Log($"Initial velocity: {initialVelocity}");

                    Vector3 totalVelocity = initialVelocity + boatVelocity;
                    //Debug.Log($"Total velocity after applying boat velocity: {totalVelocity}");

                    //Debug.Log($"Setting velocity to cannonball from {gameObject.name}");
                    rb.velocity = totalVelocity;
                    //Debug.Log($"Cannon fired from {gameObject.name} at {Time.time} with initial velocity {initialVelocity} and boat velocity {boatVelocity}");
                }
                else
                {
                    Debug.LogError("Rigidbody not found on cannonball");
                }
            }
            else
            {
                Debug.LogError("Failed to create cannonball");
            }

            // Instanciar las partículas de humo en el muzzle del cañón
            if (smokeParticlesPrefab != null)
            {
                GameObject smokeParticles = Instantiate(smokeParticlesPrefab, cannonMuzzle.position, cannonMuzzle.rotation, cannonMuzzle);
                Destroy(smokeParticles, 1f); // Destruir las partículas después de 1 segundo
            }
            else
            {
                Debug.LogError("Smoke particles prefab is null");
            }

            // Increase light intensity
            if (cannonLight != null)
            {
                StartCoroutine(FadeLight());
            }
        }
        else
        {
            Debug.LogError("Cannon muzzle or cannonball prefab is null");
        }
        
    }
    public (List<Vector3> points, Vector3 impactPoint) CalculateTrajectory(float fireVelocity, Vector3 boatVelocity)
    {

        Vector3 impactPoint = Vector3.zero;
        List<Vector3> points = new List<Vector3>();

        Vector3 initialVelocity = cannonMuzzle.forward * fireVelocity + boatVelocity;
        Vector3 currentPosition = cannonMuzzle.position;

        // Iterar para calcular cada punto de la trayectoria
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;

            // Fórmulas de movimiento para proyectil considerando gravedad en el eje Y
            Vector3 displacement = new Vector3(
                initialVelocity.x * time,
                initialVelocity.y * time - 0.5f * Mathf.Abs(Physics.gravity.y) * time * time,
                initialVelocity.z * time
            );

            Vector3 point = currentPosition + displacement;

            // Detener el cálculo si el punto ha alcanzado el suelo
            if (point.y <= 0)
            {
                point.y = 0;
                impactPoint = point;
                points.Add(point);
                break;
            }

            points.Add(point);
        }

        return (points, impactPoint);
    }

    private IEnumerator FadeLight()
    {
        if (cannonLight != null)
        {
            // Increase intensity to the specified value
            cannonLight.intensity = lightIntensity;

            // Wait for a short duration
            yield return new WaitForSeconds(lightFadeDuration);

            // Gradually decrease intensity back to 0
            float elapsedTime = 0f;
            while (elapsedTime < lightFadeDuration)
            {
                cannonLight.intensity = Mathf.Lerp(lightIntensity, 0f, elapsedTime / lightFadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the light is completely off
            cannonLight.intensity = 0f;
        }
    }

    //public void CreateImpactCircle(Vector3 impactPoint)
    //{
    //    GameObject impactCircle = new GameObject("ImpactCircle");
    //    SpriteRenderer renderer = impactCircle.AddComponent<SpriteRenderer>();
    //    renderer.sprite = Resources.Load<Sprite>("ImpactCircleSprite"); // Asegúrate de tener un sprite llamado "ImpactCircleSprite" en tus Resources

    //    // Ajusta el tamaño y posición del círculo
    //    impactCircle.transform.position = impactPoint;
    //    impactCircle.transform.localScale = new Vector3(1, 1, 1); // Cambia el tamaño según lo necesites

    //    // Destruye el círculo después de un tiempo para que no permanezca en escena
    //    Destroy(impactCircle, 2.0f); // El círculo desaparecerá después de 2 segundos
    //}



}