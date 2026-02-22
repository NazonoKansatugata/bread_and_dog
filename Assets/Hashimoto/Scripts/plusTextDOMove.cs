using UnityEngine;
using DG.Tweening;

public class plusTextDOMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("DOMovePlusText", 1);
        
    }

    private void DOMovePlusText()
    {
        transform.DOMoveY(5, 0.5f)
            .OnComplete(()=> Destroy(gameObject));
    }
}
