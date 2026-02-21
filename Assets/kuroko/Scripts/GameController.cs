using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameManager gameManager;
    public ItemSpawner spawner;

    [Header("Input")]
    public bool enableKeyboardInput = true;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode leftKeyAlt = KeyCode.A;
    public KeyCode rightKeyAlt = KeyCode.D;
    public float wrongInputLockSeconds = 1f;
    public float minSwipeDistance = 60f;
    public float sortedMoveDistance = 2f;
    public float sortedMoveDuration = 0.25f;

    [Header("Scoring")]
    public int maxFreshScore = 100;
    public int minFreshScore = 20;
    public float maxFreshTime = 2.5f;

    private float inputLockTimer;
    private bool gameEnded;
    private Vector2 swipeStart;
    private bool isSwipeTracking;

    private enum SwipeDirection
    {
        None,
        Left,
        Right
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.instance;
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (gameManager.timer <= 0f)
        {
            if (!gameEnded)
            {
                gameEnded = true;
                if (spawner != null)
                {
                    spawner.StopSpawning();
                }
            }

            return;
        }

        if (inputLockTimer > 0f)
        {
            inputLockTimer -= Time.deltaTime;
            return;
        }

        var swipe = GetSwipeDirection();
        if (swipe == SwipeDirection.Left)
        {
            TrySort(SortableType.Bread);
            return;
        }

        if (swipe == SwipeDirection.Right)
        {
            TrySort(SortableType.Corgi);
            return;
        }

        if (enableKeyboardInput)
        {
            if (GetLeftInput())
            {
                TrySort(SortableType.Bread);
            }
            else if (GetRightInput())
            {
                TrySort(SortableType.Corgi);
            }
        }
    }

    private void StartGame()
    {
        if (gameManager != null)
        {
            gameManager.StartTimer();
        }

        if (spawner != null)
        {
            spawner.StartSpawning();
        }

        gameEnded = false;
    }

    private bool GetLeftInput()
    {
        return Input.GetKeyDown(leftKey) || Input.GetKeyDown(leftKeyAlt);
    }

    private bool GetRightInput()
    {
        return Input.GetKeyDown(rightKey) || Input.GetKeyDown(rightKeyAlt);
    }

    private void TrySort(SortableType targetType)
    {
        if (spawner == null)
        {
            return;
        }

        var item = spawner.GetBottomItem();
        if (item == null)
        {
            return;
        }

        if (item.itemType == targetType)
        {
            var score = CalculateFreshScore(item.FreshnessElapsed);
            ApplyFreshScore(score);
            Debug.Log($"Sort Success: {item.itemType}");

            item.MarkSorted();
            var removed = spawner.RemoveBottomAndShift();
            if (removed != null)
            {
                var direction = targetType == SortableType.Bread ? Vector2.left : Vector2.right;
                removed.PlaySortAndDespawn(direction, sortedMoveDistance, sortedMoveDuration);
            }
        }
        else
        {
            Debug.Log($"Sort Fail: expected {targetType}, got {item.itemType}");
            gameManager.Miss(true);
            inputLockTimer = wrongInputLockSeconds;
        }
    }

    private void ApplyFreshScore(int baseScore)
    {
        if (gameManager == null || gameManager.timer <= 0f)
        {
            return;
        }

        if (gameManager.feverTimer > 0f)
        {
            gameManager.AddScore();
        }
        else
        {
            gameManager.AddScore();
        }
    }

    private int CalculateFreshScore(float elapsed)
    {
        if (maxFreshTime <= 0f)
        {
            return maxFreshScore;
        }

        var t = Mathf.Clamp01(elapsed / maxFreshTime);
        return Mathf.RoundToInt(Mathf.Lerp(maxFreshScore, minFreshScore, t));
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

        return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
    }
}
