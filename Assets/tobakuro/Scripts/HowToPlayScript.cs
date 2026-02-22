using DG.Tweening;
using UnityEngine;

public class HowToPlayScript : MonoBehaviour
{
    [Header("Swipe Settings")]
    public float minSwipeDistance = 100f;
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.OutCubic;

    [Header("HowToPlay Panel")]
    public RectTransform howToPlayPanel;

    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private bool isShowing;
    private Vector2 panelInitialAnchoredPosition;

    private enum SwipeDirection
    {
        None,
        Left,
        Right
    }

    private void Awake()
    {
        if (howToPlayPanel != null)
        {
            panelInitialAnchoredPosition = howToPlayPanel.anchoredPosition;
            howToPlayPanel.anchoredPosition = panelInitialAnchoredPosition + new Vector2(1080f, 0f);
            howToPlayPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isShowing)
        {
            return;
        }

        if (GetSwipeDirection() == SwipeDirection.Left)
        {
            ShowHowToPlay();
        }
    }

    private void ShowHowToPlay()
    {
        if (isShowing)
        {
            return;
        }

        isShowing = true;

        if (howToPlayPanel != null)
        {
            howToPlayPanel.gameObject.SetActive(true);
            howToPlayPanel.anchoredPosition = panelInitialAnchoredPosition + new Vector2(1080f, 0f);
            howToPlayPanel.DOAnchorPos(panelInitialAnchoredPosition, moveDuration)
                .SetEase(moveEase);
        }
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
        if (Mathf.Abs(delta.x) < minSwipeDistance || Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
        {
            return SwipeDirection.None;
        }

        return delta.x < 0f ? SwipeDirection.Left : SwipeDirection.Right;
    }
}
