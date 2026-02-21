using UnityEngine;
using DG.Tweening;

public class ResultScript : MonoBehaviour
{
    bool isOpenResult;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isResult && !isOpenResult)
        {
            transform.DOScale(Vector3.one, 0.5f);
            isOpenResult = true;
        }
    }

    public void CloseReslt()
    {
        transform.DOScale(Vector3.zero, 0.5f);
        isOpenResult = false;
        GameManager.instance.isResult = false;
        GameManager.instance.isStartGame = false;
        GameManager.instance.ResetGame();
    }
}
