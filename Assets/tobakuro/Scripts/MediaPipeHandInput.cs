using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

/// <summary>
/// MediaPipeのハンドランドマーク結果を受け取り、
/// 手の動き（スワイプ）を検出してTobakuroGameControllerに伝達するひな型。
///
/// 使い方:
///   1. このコンポーネントをGameObjectにアタッチする。
///   2. gameController フィールドに TobakuroGameController を設定する。
///   3. HandLandmarkerRunner の OnHandLandmarkDetectionOutput から
///      OnHandLandmarkResult() を呼び出すよう拡張する。
/// </summary>
public class MediaPipeHandInput : MonoBehaviour
{
    [Header("References")]
    public TobakuroGameController gameController;

    [Header("Swipe Detection")]
    [Tooltip("スワイプと判定する最小移動量（正規化座標、0〜1）")]
    public float minSwipeDistance = 0.15f;

    [Tooltip("スワイプ判定に使う手首ランドマークのインデックス（0 = 手首）")]
    public int wristLandmarkIndex = 0;

    // 前フレームの手首X座標（正規化）
    private float? _prevWristX;

    // 1フレームで判定した後、次フレームまでリセット待ちフラグ
    private bool _swipeConsumed;

    private void LateUpdate()
    {
        // スワイプ判定はOnHandLandmarkResult()で行う。
        // LateUpdate でフレームごとに consumed フラグをリセットする。
        _swipeConsumed = false;
    }

    /// <summary>
    /// HandLandmarkerRunnerのコールバックからこのメソッドを呼び出す。
    /// 例:
    ///   private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
    ///   {
    ///       _handInput.OnHandLandmarkResult(result);
    ///   }
    /// </summary>
    public void OnHandLandmarkResult(HandLandmarkerResult result)
    {
        if (_swipeConsumed)
        {
            return;
        }

        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            _prevWristX = null;
            return;
        }

        var landmarks = result.handLandmarks[0].landmarks;
        if (landmarks == null || landmarks.Count <= wristLandmarkIndex)
        {
            _prevWristX = null;
            return;
        }

        var wristX = landmarks[wristLandmarkIndex].x;

        if (_prevWristX.HasValue)
        {
            var delta = wristX - _prevWristX.Value;
            if (Mathf.Abs(delta) >= minSwipeDistance)
            {
                _swipeConsumed = true;
                _prevWristX = wristX;

                if (delta < 0f)
                {
                    // 手が左へ動いた → コルギを選択
                    OnSwipeLeft();
                }
                else
                {
                    // 手が右へ動いた → パンを選択
                    OnSwipeRight();
                }

                return;
            }
        }

        _prevWristX = wristX;
    }

    // ---------- 入力イベント ----------

    private void OnSwipeLeft()
    {
        if (gameController == null)
        {
            return;
        }

        gameController.TrySort(SortableType.Corgi);
    }

    private void OnSwipeRight()
    {
        if (gameController == null)
        {
            return;
        }

        gameController.TrySort(SortableType.Bread);
    }
}
