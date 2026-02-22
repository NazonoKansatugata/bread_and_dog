using DG.Tweening;
using UnityEngine;

public enum SortableType
{
    Corgi,
    Bread,
    Mixed
}

public class SortableItem : MonoBehaviour
{
    [Header("Item")]
    public SortableType itemType = SortableType.Corgi;

    [Header("State")]
    public bool isSortable;
    public bool isSorted;

    private float freshnessElapsed;

    public float FreshnessElapsed => freshnessElapsed;

    private void Update()
    {
        if (isSortable && !isSorted)
        {
            freshnessElapsed += Time.deltaTime;
        }
    }

    public void SetSortable(bool value)
    {
        isSortable = value;
        if (value)
        {
            freshnessElapsed = 0f;
        }
    }

    public void MarkSorted()
    {
        isSorted = true;
        isSortable = false;
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }

    public void PlaySortAndDespawn(Vector2 direction, float distance, float duration)
    {
        if (distance <= 0f || duration <= 0f)
        {
            Despawn();
            return;
        }

        var dir = direction.normalized;
        var offset = new Vector3(dir.x, dir.y, 0f) * distance;
        transform.SetParent(null, true);
        transform.DOMove(transform.position + offset, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(Despawn);
    }

    public void PlayWrongAndReturn(Vector2 direction, float distance, float duration, System.Action onComplete = null)
    {
        if (distance <= 0f || duration <= 0f)
        {
            onComplete?.Invoke();
            return;
        }

        var startPos = transform.position;
        var dir = direction.normalized;
        var offset = new Vector3(dir.x, dir.y, 0f) * distance;
        var wrongPos = startPos + offset;

        transform.SetParent(null, true);
        transform.DOMove(wrongPos, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMove(startPos, duration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
            });
    }
}
