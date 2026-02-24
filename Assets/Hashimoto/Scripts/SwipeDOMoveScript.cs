using UnityEngine;
using DG.Tweening;

public class SwipeDOMoveScript : MonoBehaviour
{
    [SerializeField] float time;
    [SerializeField] Vector3 deltaMove;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DOMove(transform.position + deltaMove, time)
            .SetEase(Ease.InCubic)
            .SetLoops(-1,LoopType.Restart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
