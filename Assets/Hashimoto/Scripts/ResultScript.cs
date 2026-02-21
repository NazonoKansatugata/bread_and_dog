using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ResultScript : MonoBehaviour
{
    bool isOpenResult;
    Transform resultImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultImage = transform.Find("ResultBackGround");
        resultImage.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isResult && !isOpenResult)
        {
            resultImage.DOScale(Vector3.one, 0.5f);
            isOpenResult = true;
            resultImage.Find("ResultScoreText").gameObject.GetComponent<Text>().text = GameManager.instance.score.ToString();
        }
    }

    public void CloseResult()
    {
        resultImage.DOScale(Vector3.zero, 0.5f)
            .OnComplete(()=> GameManager.instance.ResetGame());
        isOpenResult = false;
        GameManager.instance.isResult = false;
        GameManager.instance.isStartGame = false;
    }
}
