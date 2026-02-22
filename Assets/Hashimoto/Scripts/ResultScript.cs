using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using unityroom.Api;
using UnityEngine.SceneManagement;

public class ResultScript : MonoBehaviour
{
    bool isOpenResult;
    RectTransform resultImage;
    public float minSwipeDistance = 80f;
    public TitleScript titleScript;
    public GameController gameController;

    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private bool isClosing;

    private enum SwipeDirection
    {
        None,
        Up
    }

    void Start()
    {
        resultImage = transform.Find("ResultBackGround").GetComponent<RectTransform>();
        resultImage.anchoredPosition = new Vector2(0f, 960f);
        resultImage.gameObject.SetActive(false); // 初期は非表示
        
        isOpenResult = false;
        isClosing = false;

        if (titleScript == null)
        {
            titleScript = FindObjectOfType<TitleScript>(true);
        }
    }

    void Update()
    {
        if (GameManager.instance == null || resultImage == null) return;
        
        if (GameManager.instance.isResult && !isOpenResult)
        {
            if (gameController != null && gameController.bgmAudioSource != null)
            {
                gameController.bgmAudioSource.Stop();
            }

            isOpenResult = true;
            isClosing = false;
            
            // リザルト画像を表示
            resultImage.gameObject.SetActive(true);
            resultImage.Find("ResultImage").Find("ResultScoreText").gameObject.GetComponent<Text>().text = GameManager.instance.score.ToString();
            UnityroomApiClient.Instance.SendScore(1, GameManager.instance.score, ScoreboardWriteMode.HighScoreDesc);

            GameManager.instance.score = 0;
            GameManager.instance.timer = 0;
            GameManager.instance.combo = 0;

            resultImage.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
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
        
        // シーンを再読み込み（GameManagerは自動的にUI参照を更新）
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
