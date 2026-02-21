using DG.Tweening;
using UnityEngine;

public class DOScaleUIScript : MonoBehaviour
{
    [SerializeField] float deltaScale;
    float scaleTime;
    Vector3 maxScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxScale = new Vector3(transform.localScale.x * deltaScale, transform.localScale.y * deltaScale, transform.localScale.z * deltaScale);
        scaleTime = Random.Range(0.8f, 2f);
        transform.DOScale(maxScale, scaleTime)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
