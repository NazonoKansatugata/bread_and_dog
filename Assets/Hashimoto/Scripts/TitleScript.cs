using DG.Tweening;
using UnityEngine;

public class TitleScript : MonoBehaviour
{
    public Transform moveTarget;
    public float minSwipeDistance = 100f;
    public float moveDistance = 15f;
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.OutCubic;

    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private bool isStarted;

    private enum SwipeDirection
    {
        None,
        Up,
        Down
    }

    private void Awake()
    {
        if (moveTarget == null)
        {
            moveTarget = transform;
        }
    }

    private void Update()
    {
        if (isStarted)
        {
            return;
        }

        if (GetSwipeDirection() == SwipeDirection.Up)
        {
            BeginStartSequence();
        }
    }

    private void BeginStartSequence()
    {
        if (isStarted)
        {
            return;
        }

        isStarted = true;
        if (moveTarget != null)
        {
            moveTarget.DOMove(moveTarget.position + Vector3.up * moveDistance, moveDuration)
                .SetEase(moveEase)
                .OnComplete(GameStart);
        }
        else
        {
            GameStart();
        }
    }

    public void GameStart()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StartTimer();
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
        if (Mathf.Abs(delta.y) < minSwipeDistance || Mathf.Abs(delta.y) < Mathf.Abs(delta.x))
        {
            return SwipeDirection.None;
        }

        return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
    }
}
