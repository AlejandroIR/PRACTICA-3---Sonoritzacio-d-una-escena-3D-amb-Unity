using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTriggerScript : MonoBehaviour
{
    public CannonScript[] cannons; // Assign the cannons that should fire when triggered
    public float sequentialFireInterval = 1f; // 1 second interval between each cannon firing
    private bool isSequenceFiring = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isSequenceFiring)
        {
            // Start the sequential firing coroutine
            StartCoroutine(FireCannonsSequentially());
        }
    }

    private IEnumerator FireCannonsSequentially()
    {
        isSequenceFiring = true;

        // Fire each cannon with a delay between them
        for (int i = 0; i < cannons.Length; i++)
        {
            if (cannons[i] != null && cannons[i].canFire)
            {
                // Fire the current cannon
                cannons[i].Fire(cannons[i].fireVelocity, Vector3.zero);
                cannons[i].StartCoroutine(cannons[i].FireCooldown());
                
                // Wait for the specified interval before firing the next cannon
                yield return new WaitForSeconds(sequentialFireInterval);
            }
        }

        isSequenceFiring = false;
    }

    // Optional: Add this method to handle when player exits the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // You can decide if you want to stop the sequence when player exits
            // or let it complete once started
        }
    }
}
