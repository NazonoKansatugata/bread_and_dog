using System.Collections;
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

    [Header("Wrong Swipe Animation")]
    public float wrongMoveDuration = 0.3f;
    public float wrongInputLockSeconds = 3f;
    public float minSwipeDistance = 60f;
    public float sortedMoveDistance = 2f;
    public float sortedMoveDuration = 0.25f;

    [Header("Scoring")]
    public int maxFreshScore = 100;
    public int minFreshScore = 20;
    public float maxFreshTime = 2.5f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successCorgiClip;
    public AudioClip successBreadClip;
    public AudioClip successMixedClip;
    public AudioClip failClip;
    public AudioSource bgmAudioSource;
    public AudioClip bgmClip;

    [Header("Fail Animation")]
    public GameObject failCorgiAnimation;
    public GameObject failBreadAnimation;
    public GameObject failMixedAnimation;

    private float inputLockTimer;
    private bool gameEnded;
    private Vector2 swipeStart;
    private bool isSwipeTracking;
    private Coroutine bgmPauseRoutine;

    private enum SwipeDirection
    {
        None,
        Left,
        Right,
        Down
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.instance;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        InitializeFailAnimations();
        StartGame();
    }

    private void InitializeFailAnimations()
    {
        if (failCorgiAnimation != null)
        {
            failCorgiAnimation.SetActive(false);
            SetSortingOrder(failCorgiAnimation, 15);
        }
        if (failBreadAnimation != null)
        {
            failBreadAnimation.SetActive(false);
            SetSortingOrder(failBreadAnimation, 15);
        }
        if (failMixedAnimation != null)
        {
            failMixedAnimation.SetActive(false);
            SetSortingOrder(failMixedAnimation, 15);
        }
    }

    private void SetSortingOrder(GameObject obj, int order)
    {
        var canvas = obj.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = order;
        }
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

        if (swipe == SwipeDirection.Down)
        {
            TrySort(SortableType.Mixed);
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
        PlayBGM();
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
            PlaySuccessSound(item.itemType);

            item.MarkSorted();
            var removed = spawner.RemoveBottomAndShift();
            if (removed != null)
            {
                var direction = targetType == SortableType.Bread ? Vector2.left : (targetType == SortableType.Corgi ? Vector2.right : Vector2.down);
                removed.PlaySortAndDespawn(direction, sortedMoveDistance, sortedMoveDuration);
            }
        }
        else
        {
            Debug.Log($"Sort Fail: expected {targetType}, got {item.itemType}");
            gameManager.Miss(true);
            inputLockTimer = wrongInputLockSeconds;
            PlayFailSound();
            PauseBGMForFail();
            ShowFailAnimation(item.itemType);

            item.MarkSorted();
            var removed = spawner.RemoveBottomAndShift();
            if (removed != null)
            {
                var direction = targetType == SortableType.Bread ? Vector2.left : (targetType == SortableType.Corgi ? Vector2.right : Vector2.down);
                removed.PlaySortAndDespawn(direction, sortedMoveDistance, wrongMoveDuration);
            }
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
        var absDeltaX = Mathf.Abs(delta.x);
        var absDeltaY = Mathf.Abs(delta.y);

        if (Mathf.Max(absDeltaX, absDeltaY) < minSwipeDistance)
        {
            return SwipeDirection.None;
        }

        if (absDeltaX > absDeltaY)
        {
            return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
        }
        else
        {
            return delta.y < 0f ? SwipeDirection.Down : SwipeDirection.None;
        }
    }

    private void PlaySuccessSound(SortableType itemType)
    {
        if (audioSource == null)
        {
            return;
        }

        var clip = itemType switch
        {
            SortableType.Corgi => successCorgiClip,
            SortableType.Bread => successBreadClip,
            SortableType.Mixed => successMixedClip,
            _ => null
        };

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayFailSound()
    {
        if (audioSource == null || failClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(failClip);
    }

    private void PlayBGM()
    {
        if (bgmAudioSource == null || bgmClip == null)
        {
            return;
        }

        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    private void PauseBGMForFail()
    {
        if (bgmAudioSource == null || !bgmAudioSource.isPlaying)
        {
            return;
        }

        if (bgmPauseRoutine != null)
        {
            StopCoroutine(bgmPauseRoutine);
        }

        bgmPauseRoutine = StartCoroutine(BGMPauseRoutine());
    }

    private void ShowFailAnimation(SortableType itemType)
    {
        StartCoroutine(ShowFailAnimationRoutine(itemType));
    }

    private IEnumerator ShowFailAnimationRoutine(SortableType itemType)
    {
        GameObject animObject = null;
        
        if (itemType == SortableType.Corgi && failCorgiAnimation != null)
        {
            animObject = failCorgiAnimation;
        }
        else if (itemType == SortableType.Bread && failBreadAnimation != null)
        {
            animObject = failBreadAnimation;
        }
        else if (itemType == SortableType.Mixed && failMixedAnimation != null)
        {
            animObject = failMixedAnimation;
        }

        if (animObject != null)
        {
            animObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            animObject.SetActive(false);
        }
    }

    private IEnumerator BGMPauseRoutine()
    {
        bgmAudioSource.Pause();
        yield return new WaitForSeconds(wrongInputLockSeconds);
        bgmAudioSource.UnPause();
    }
}
