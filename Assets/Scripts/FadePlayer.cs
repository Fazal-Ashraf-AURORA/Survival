using UnityEngine;

public class FadePlayer : MonoBehaviour
{
    public SpriteRenderer playerRenderer;

    public void FadeThePlayer() {
        playerRenderer.color = new Color(playerRenderer.color.r, playerRenderer.color.g, playerRenderer.color.b, 0f);
    }
}
