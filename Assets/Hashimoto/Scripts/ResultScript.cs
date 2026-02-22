using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using unityroom.Api;

public class ResultScript : MonoBehaviour
{
    bool isOpenResult;
    RectTransform resultImage;
    public float minSwipeDistance = 80f;
    public TitleScript titleScript;

    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private bool isClosing;

    private enum SwipeDirection
    {
        None,
        Up
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resultImage = transform.Find("ResultBackGround").GetComponent<RectTransform>();
        resultImage.localScale = Vector3.zero;

        if (titleScript == null)
        {
            titleScript = FindObjectOfType<TitleScript>(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isResult && !isOpenResult)
        {
            resultImage.DOScale(Vector3.one, 0.5f);
            isOpenResult = true;
            resultImage.Find("ResultImage").Find("ResultScoreText").gameObject.GetComponent<Text>().text = GameManager.instance.score.ToString();
            isClosing = false;
            resultImage.Find("ResultScoreText").gameObject.GetComponent<Text>().text = GameManager.instance.score.ToString();
            UnityroomApiClient.Instance.SendScore(1, GameManager.instance.score, ScoreboardWriteMode.HighScoreDesc);
        }

        if (isOpenResult && !isClosing)
        {
            var swipe = GetSwipeDirection();
            if (swipe == SwipeDirection.Up)
            {
                ReturnToTitle();
            }
        }
    }

    public void ReturnToTitle()
    {
        if (isClosing)
        {
            return;
        }

        isClosing = true;
        resultImage.DOScale(Vector3.zero, 0.5f)
            .OnComplete(() =>
            {
                GameManager.instance.ResetGame();
                if (titleScript != null)
                {
                    titleScript.ShowTitle();
                }
            });
        isOpenResult = false;
        GameManager.instance.isResult = false;
        GameManager.instance.isStartGame = false;
    }

    private SwipeDirection GetSwipeDirection()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStart = touch.position;
                isSwipeTracking = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (!isSwipeTracking)
                {
                    return SwipeDirection.None;
                }

                isSwipeTracking = false;
                return EvaluateSwipe(touch.position);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            isSwipeTracking = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!isSwipeTracking)
            {
                return SwipeDirection.None;
            }

            isSwipeTracking = false;
            return EvaluateSwipe(Input.mousePosition);
        }

        return SwipeDirection.None;
    }

    private SwipeDirection EvaluateSwipe(Vector2 endPosition)
    {
        var delta = endPosition - swipeStart;
        if (Mathf.Abs(delta.y) < minSwipeDistance || Mathf.Abs(delta.y) < Mathf.Abs(delta.x))
        {
            return SwipeDirection.None;
        }

        return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.None;
    }
}
