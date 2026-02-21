using UnityEngine;
using DG.Tweening;

public class ResultScript : MonoBehaviour
{
    bool isOpenResult;
    Transform resultImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultImage = transform.Find("ResultImage");
        resultImage.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isResult && !isOpenResult)
        {
            resultImage.DOScale(Vector3.one, 0.5f);
            isOpenResult = true;
        }
    }

    public void CloseReslt()
    {
        resultImage.DOScale(Vector3.zero, 0.5f)
            .OnComplete(()=> GameManager.instance.ResetGame());
        isOpenResult = false;
        GameManager.instance.isResult = false;
        GameManager.instance.isStartGame = false;
    }
}
