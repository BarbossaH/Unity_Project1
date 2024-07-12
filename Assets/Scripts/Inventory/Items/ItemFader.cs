
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void FadeSprite(bool isFading)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer component is missing.");
            return;
        }
        Color targetColor = new Color(1, 1, 1, isFading ? 0.7f : 1f);
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }

    private void OnDestroy()
    {
        // 取消所有与此对象关联的 tweens
        spriteRenderer.DOKill();
    }
}
