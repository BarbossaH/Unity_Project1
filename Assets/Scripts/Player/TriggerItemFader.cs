using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerFading(other, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TriggerFading(other, false);

    }

    private void TriggerFading(Collider2D other, bool isFading)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            foreach (var item in faders)
            {
                item.FadeSprite(isFading);
            }
        }
    }
}
