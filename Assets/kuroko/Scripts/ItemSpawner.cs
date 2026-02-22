using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;
    public Transform[] slots;
    public int slotCount = 10;
    public Transform slotRoot;
    public Vector2 slotSpacing = new Vector2(0f, -1f);
    public float spawnInterval = 1f;
    public bool autoStart;

    private Coroutine spawnRoutine;
    private SortableItem[] slotItems;

    private void Awake()
    {
        InitializeSlots();
    }

    private void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            return;
        }

        InitializeSlots();
        FillEmptySlots();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0 || slots == null || slots.Length == 0)
        {
            return;
        }

        if (slotItems == null || slotItems.Length != slots.Length)
        {
            InitializeSlots();
        }

        for (var i = 0; i < slotItems.Length; i++)
        {
            slotItems[i].gameObject.GetComponent<Renderer>().sortingOrder = i;
            if (slotItems[i] != null)
            {
                continue;
            }

            SpawnIntoSlot(i);
            UpdateSortableFlags();
            return;
        }
    }

    public SortableItem GetBottomItem()
    {
        if (slotItems == null || slotItems.Length == 0)
        {
            return null;
        }

        return slotItems[slotItems.Length - 1];
    }

    public SortableItem RemoveBottomAndShift()
    {
        if (slotItems == null || slotItems.Length == 0)
        {
            return null;
        }

        var lastIndex = slotItems.Length - 1;
        var removed = slotItems[lastIndex];
        slotItems[lastIndex] = null;

        for (var i = lastIndex - 1; i >= 0; i--)
        {
            slotItems[i + 1] = slotItems[i];
            slotItems[i] = null;
        }

        ApplySlotTransforms();
        UpdateSortableFlags();
        FillEmptySlots();
        return removed;
    }

    private void InitializeSlots()
    {
        if (slots == null)
        {
            slots = new Transform[0];
        }

        if (slotCount < 1)
        {
            slotCount = 1;
        }

        if (slots.Length != slotCount)
        {
            var resized = new Transform[slotCount];
            for (var i = 0; i < resized.Length && i < slots.Length; i++)
            {
                resized[i] = slots[i];
            }

            slots = resized;
        }

        var root = slotRoot != null ? slotRoot : transform;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                continue;
            }

            var slot = new GameObject($"Slot_{i}").transform;
            slot.SetParent(root, false);
            slot.localPosition = new Vector3(slotSpacing.x * i, slotSpacing.y * i, 0f);
            slots[i] = slot;
        }

        slotItems = new SortableItem[slots.Length];
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            var existing = slots[i].GetComponentInChildren<SortableItem>();
            slotItems[i] = existing;
        }

        ApplySlotTransforms();
        UpdateSortableFlags();
    }

    private void SpawnIntoSlot(int slotIndex)
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0 || slots == null)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null)
        {
            return;
        }

        var prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        var instance = Instantiate(prefab, slots[slotIndex].position, slots[slotIndex].rotation);
        var item = instance.GetComponentInParent<SortableItem>();
        if (item == null)
        {
            Destroy(instance);
            return;
        }

        PlaceInSlot(slotIndex, item);
    }

    private void FillEmptySlots()
    {
        if (slotItems == null || slotItems.Length == 0)
        {
            return;
        }

        for (var i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == null)
            {
                SpawnIntoSlot(i);
            }
        }

        UpdateSortableFlags();
    }

    private void PlaceInSlot(int slotIndex, SortableItem item)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length || item == null)
        {
            return;
        }

        slotItems[slotIndex] = item;
        var target = slots[slotIndex];
        item.transform.SetParent(target, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    private void ApplySlotTransforms()
    {
        if (slots == null || slotItems == null)
        {
            return;
        }

        for (var i = 0; i < slotItems.Length; i++)
        {
            var item = slotItems[i];
            if (item == null || slots[i] == null)
            {
                continue;
            }

            item.transform.SetParent(slots[i], false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }
    }

    private void UpdateSortableFlags()
    {
        if (slotItems == null || slotItems.Length == 0)
        {
            return;
        }

        var lastIndex = slotItems.Length - 1;
        for (var i = 0; i < slotItems.Length; i++)
        {
            var item = slotItems[i];
            if (item == null)
            {
                continue;
            }

            var shouldBeSortable = i == lastIndex;
            if (item.isSortable != shouldBeSortable)
            {
                item.SetSortable(shouldBeSortable);
            }
        }
    }
}
