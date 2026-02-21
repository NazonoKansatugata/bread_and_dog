using System.Collections;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Core;
using Mediapipe.Unity;
using UnityEngine;

/// <summary>
/// WebCam の映像を MediaPipe Hand Landmarker に渡し、
/// 手の左右スワイプを検出して TobakuroGameController に通知する。
/// </summary>
public class MediaPipeHandInput : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("シーン内の TobakuroGameController をアサインしてください")]
    public TobakuroGameController gameController;

    [Header("スワイプ判定")]
    [Tooltip("スワイプとみなす手首の移動量（0〜1 の正規化座標）")]
    public float swipeThreshold = 0.08f;

    [Tooltip("スワイプ判定後のクールダウン秒数")]
    public float swipeCooldown = 0.4f;

    // --- 内部状態 ---
    private WebCamTexture _webCamTexture;
    private HandLandmarker _handLandmarker;
    private Texture2D _inputTexture;

    private float _prevWristX = -1f;
    private float _cooldownTimer = 0f;
    private bool _isRunning = false;

    private enum SwipeDir { Left, Right }

    private IEnumerator Start()
    {
        yield return SetupWebCam();
        yield return SetupHandLandmarker();

        _isRunning = true;
        StartCoroutine(DetectionLoop());
    }

    private IEnumerator SetupWebCam()
    {
        _webCamTexture = new WebCamTexture();
        _webCamTexture.Play();

        yield return new WaitUntil(() => _webCamTexture.width > 16);

        _inputTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height, TextureFormat.RGBA32, false);
        Debug.Log($"[MediaPipeHandInput] WebCam started: {_webCamTexture.width}x{_webCamTexture.height}");
    }

    private IEnumerator SetupHandLandmarker()
    {
        // カスタムリゾルバーを有効化し、モデルファイルのパスを登録する
        var modelPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            "hand_landmarker.task");

        Mediapipe.ResourceUtil.EnableCustomResolver();
        Mediapipe.ResourceUtil.SetAssetPath("hand_landmarker.task", modelPath);

        yield return null;

        var options = new HandLandmarkerOptions(
            baseOptions: new BaseOptions(
                delegateCase: BaseOptions.Delegate.CPU,
                modelAssetPath: modelPath),
            runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM,
            numHands: 1,
            minHandDetectionConfidence: 0.5f,
            minHandPresenceConfidence: 0.5f,
            minTrackingConfidence: 0.5f,
            resultCallback: OnHandLandmarkerResult
        );

        _handLandmarker = HandLandmarker.CreateFromOptions(options);
        Debug.Log("[MediaPipeHandInput] HandLandmarker initialized.");
    }

    private IEnumerator DetectionLoop()
    {
        var waitForEndOfFrame = new WaitForEndOfFrame();

        while (_isRunning)
        {
            yield return waitForEndOfFrame;

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            _inputTexture.SetPixels32(_webCamTexture.GetPixels32());
            _inputTexture.Apply();

            using var image = new Mediapipe.Image(
                Mediapipe.ImageFormat.Types.Format.Srgba,
                _inputTexture);

            var timestamp = (long)(Time.realtimeSinceStartup * 1000);
            _handLandmarker.DetectAsync(image, timestamp);
        }
    }

    private void OnHandLandmarkerResult(HandLandmarkerResult result, Mediapipe.Image image, long timestamp)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            _prevWristX = -1f;
            return;
        }

        var wristX = result.handLandmarks[0].landmarks[0].x;

        if (_prevWristX >= 0f && _cooldownTimer <= 0f)
        {
            var delta = wristX - _prevWristX;

            if (delta > swipeThreshold)
            {
                TriggerSort(SwipeDir.Right);
            }
            else if (delta < -swipeThreshold)
            {
                TriggerSort(SwipeDir.Left);
            }
        }

        _prevWristX = wristX;
    }

    private void TriggerSort(SwipeDir dir)
    {
        if (gameController == null) return;

        _cooldownTimer = swipeCooldown;

        var targetType = dir == SwipeDir.Left ? SortableType.Corgi : SortableType.Bread;
        gameController.TrySort(targetType);

        Debug.Log($"[MediaPipeHandInput] Hand swipe {dir} → {targetType}");
    }

    private void OnDestroy()
    {
        _isRunning = false;
        _webCamTexture?.Stop();
        _handLandmarker?.Close();
    }
}
