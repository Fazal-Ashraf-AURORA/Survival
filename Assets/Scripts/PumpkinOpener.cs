using UnityEngine;

public class PortalOpener : MonoBehaviour {
    
    //Reference of the player
    public Transform Player;
    // Reference to the animator component of the portal
    public Animator portalAnimator;

    // Distance threshold for the player to trigger the portal opening
    public float activationDistance = 3f;

    // Flag to keep track of whether the portal is open
    public bool portalOpen = false;

    // Update is called once per frame
    void Update() {
        // Calculate the distance between the player and the portal
        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        // If the player is within the activation distance and the portal is not already open
        if (distanceToPlayer <= activationDistance && !portalOpen) {
            // Open the portal
            OpenPortal();
            
        }
        // If the player moves away from the portal and the portal is open
        else if (distanceToPlayer > activationDistance && portalOpen) {
            // Close the portal
            ClosePortal();
            
        }
    }

    // Method to open the portal
    void OpenPortal() {
        // Set the trigger parameter "Open" to true in the animator
        portalAnimator.SetBool("Open", true);
        // Set the portalOpen flag to true
        portalOpen = true;
    }

    // Method to close the portal
    void ClosePortal() {
        // Set the trigger parameter "Open" to false in the animator
        portalAnimator.SetBool("Open", false);
        // Set the portalOpen flag to false
        portalOpen = false;
    }
}
