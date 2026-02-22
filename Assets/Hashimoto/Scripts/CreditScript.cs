using DG.Tweening;
using UnityEngine;

public class CreditScript : MonoBehaviour
{
    public Transform moveTarget;
    public float minSwipeDistance = 100f;
    public float moveDistance = 15f;
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.OutCubic;

    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private bool isStarted;
    private Vector3 initialPosition;
    private bool isHidden;

    private enum SwipeDirection
    {
        None,
        Right,
        Left
    }

    int TitlePosition;

    private void Awake()
    {
        TitlePosition = 1;
        if (moveTarget == null)
        {
            moveTarget = transform;
        }

        initialPosition = moveTarget.position;
    }

    private void Update()
    {
        if (isStarted)
            return;

        SwipeDirection dir = GetSwipeDirection();   // ← 1回だけ呼ぶ

        if (dir == SwipeDirection.Right)
        {
            if (TitlePosition > 0)   // 左スワイプで減らす
            {
                TitlePosition--;
                BeginStartSequence();
            }
        }
        else if (dir == SwipeDirection.Left)
        {
            if (TitlePosition < 3)   // 右スワイプで増やす
            {
                TitlePosition++;
                BeginStartSequence();
            }
        }

        Debug.Log("TitlePosition: " + TitlePosition);
    }

    private void BeginStartSequence()
    {
        if (isStarted)
        {
            return;
        }

        if (moveTarget != null)
        {
            if(TitlePosition == 0)
            {
                moveTarget.DOMove(initialPosition + Vector3.right * moveDistance, moveDuration)
                .SetEase(moveEase);
            }
            if (TitlePosition == 1)
            {
                moveTarget.DOMove(initialPosition, moveDuration)
                .SetEase(moveEase);
            }
            if (TitlePosition == 2)
            {
                moveTarget.DOMove(initialPosition + Vector3.left * moveDistance, moveDuration)
                .SetEase(moveEase);
            }
            if (TitlePosition == 3)
            {
                moveTarget.DOMove(initialPosition + Vector3.left * moveDistance * 2, moveDuration)
                .SetEase(moveEase);
            }
        }
        else
        {
            //OnTitleMoved();
        }
    }

    //private void OnTitleMoved()
    //{
    //    GameStart();
    //    HideTitle();
    //}

    //public void GameStart()
    //{
    //    if (GameManager.instance != null)
    //    {
    //        //GameManager.instance.StartTimer();
    //    }
    //}

    //private void HideTitle()
    //{
    //    if (isHidden)
    //    {
    //        return;
    //    }

    //    isHidden = true;
    //    enabled = false;
    //    if (moveTarget != null)
    //    {
    //        moveTarget.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        gameObject.SetActive(false);
    //    }
    //}

    //public void ShowTitle()
    //{
    //    TitlePosition = 1;
    //    if (moveTarget != null)
    //    {
    //        moveTarget.gameObject.SetActive(true);
    //        moveTarget.position = initialPosition;
    //        DOTween.Kill(moveTarget);minSwipeDistance
    //    }
    //    else
    //    {
    //        gameObject.SetActive(true);
    //    }

    //    isStarted = false;
    //    isHidden = false;
    //    enabled = true;
    //}

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

        // 横移動が最小距離未満 → 無効
        if (Mathf.Abs(delta.x) < minSwipeDistance)
            return SwipeDirection.None;

        // 縦移動のほうが大きい → 無効
        if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            return SwipeDirection.None;

        return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
    }
}
