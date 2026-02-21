using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SortZone : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var item = other.GetComponentInParent<SortableItem>();
        if (item == null)
        {
            return;
        }

        item.SetSortable(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var item = other.GetComponentInParent<SortableItem>();
        if (item == null)
        {
            return;
        }

        item.SetSortable(false);
    }
}
