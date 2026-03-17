using UnityEngine;

public class RespawnOnTouch : MonoBehaviour
{
    // Optional: Set a specific respawn point
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched has a Player tag
        if (other.CompareTag("Player"))
        {
            RespawnPlayer(other.gameObject);
        }
    }

    // Alternative: Use OnCollisionEnter for physical collisions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RespawnPlayer(collision.gameObject);
        }
    }

    private void RespawnPlayer(GameObject player)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        // Disable controller before moving (prevents physics issues)
        if (controller != null)
            controller.enabled = false;

        // Move player to respawn point or original position
        if (respawnPoint != null)
            player.transform.position = respawnPoint.position;
        else
            player.transform.position = Vector3.zero; // Default spawn

        // Re-enable controller
        if (controller != null)
            controller.enabled = true;

        Debug.Log("Player respawned!");
    }
}