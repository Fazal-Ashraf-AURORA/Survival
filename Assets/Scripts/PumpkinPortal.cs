using UnityEngine;
using UnityEngine.SceneManagement;

public class PumpkinPortal : MonoBehaviour
{
    public GameObject Player;
    public Animator playerAnimator;
    public SpriteRenderer playerRenderer;
    public GameObject portalEffect;
    // Name of the next level to load
    public string nextLevelName;
    // Delay time before loading the next level
    public float delay = 2f;

    // Called when an object enters the trigger zone
    private void OnTriggerEnter2D(Collider2D other) {
        // Check if the object that entered is the player
        if (other.CompareTag("Player")) {
            //disabling playerController and switching to idle state
            Player.GetComponent<PlayerController>().enabled = false;
            playerAnimator.Play("Idle");

            //enabling portal effect
            portalEffect.SetActive(true);
            portalEffect.GetComponent<Animator>().SetTrigger("Portal");
           

            // Invoke the LoadNextLevel method after the delay
            Invoke("LoadNextLevel", delay);
        }
    }

    // Method to load the next level
    private void LoadNextLevel() {
        // Load the next level
        SceneManager.LoadScene(nextLevelName);
    }

    public void FadePlayer() {
        playerRenderer.color = new Color(playerRenderer.color.r, playerRenderer.color.g, playerRenderer.color.b, 0f);
    }
}
