using System.Threading;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score;
    [HideInInspector] public float timer;
    [HideInInspector] public int combo;
    [HideInInspector] public float feverTimer;
    Text scoreText, timerText;
    Image comboImage;
    [SerializeField] int maxCombo, feverTime;
    [SerializeField] float TimeLimit;
    [SerializeField] Sprite[] comboSprites = new Sprite[11];
    [SerializeField] GameObject plusTextPrefab;
    [SerializeField] Transform scoreBack;
    public bool isStartGame, isResult;
    GameObject resultCanvas;

    GameObject feverObject;

    public bool isChangeImage;

    public int TP;

    public Vector3 bottomPosition;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            
            // 初期状態を設定
            score = 0;
            timer = 0;
            combo = 0;
            feverTimer = 0;
            isStartGame = false;
            isResult = false;
            
            // シーン読み込みイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン読み込み時にUI参照を再取得
        RefreshUIReferences();
    }
    
    private void RefreshUIReferences()
    {
        feverObject = GameObject.Find("Fiver_corgi_L");
        resultCanvas = GameObject.Find("ResultCanvas");
        // resultCanvasは常にアクティブにしておく（内部のResultScriptが動作するため）
        if (resultCanvas != null)
        {
            resultCanvas.SetActive(true);
        }
        
        GameObject scoreObj = GameObject.Find("ScoreText");
        if (scoreObj != null) scoreText = scoreObj.GetComponent<Text>();
        
        GameObject timerObj = GameObject.Find("TimerText");
        if (timerObj != null) timerText = timerObj.GetComponent<Text>();
        
        GameObject comboObj = GameObject.Find("ComboImage");
        if (comboObj != null) comboImage = comboObj.GetComponent<Image>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshUIReferences();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            if (isStartGame)
            {
                timer -= Time.deltaTime;
            }
        }
        else
        {
            if(isStartGame)
            {
                isResult = true;
                // resultCanvasのSetActiveは削除（ResultScriptが内部のresultImageを管理）
            }
            timer = 0;
        }
        if (feverTimer > 0)
        {
            feverTimer -= Time.deltaTime;
            if (feverObject != null)
            {
                feverObject.SetActive(true);
            }
        }
        else
        {
            feverTimer = 0;
            if (feverObject != null)
            {
                feverObject.SetActive(false);
            }
        }
        if (combo == maxCombo)
        {
            feverTimer = feverTime;
            combo = 0;
        }
        if(score >= 100)
        {
            isChangeImage = true;
        }
        SetText();
    }
    public void ResetGame()
    {
        resultCanvas = GameObject.Find("ResultCanvas");
        // resultCanvasは常にアクティブにしておく（ResultScriptが常に動作するため）
        
        GameObject scoreObj = GameObject.Find("ScoreText");
        if (scoreObj != null) scoreText = scoreObj.GetComponent<Text>();
        
        GameObject timerObj = GameObject.Find("TimerText");
        if (timerObj != null) timerText = timerObj.GetComponent<Text>();
        
        GameObject comboObj = GameObject.Find("ComboImage");
        if (comboObj != null) comboImage = comboObj.GetComponent<Image>();
        
        score = 0;
        timer = 0;
        combo = 0;
        feverTimer = 0;
        isStartGame = false;
        isResult = false;
        SetText();
    }
    public void AddScore()
    {
        if(timer > 0)
        {
            if (feverTimer == 0)
            {
                score += 1;
                combo += 1;
                Instantiate(plusTextPrefab, scoreBack.position + new Vector3(0.5f ,-1, 0), Quaternion.identity, scoreBack).GetComponent<Text>().text = "+1";
            }
            else if (feverTimer > 0)
            {
                score += 3;
                Instantiate(plusTextPrefab, scoreBack.position + new Vector3(0.5f, -1, 0), Quaternion.identity, scoreBack).GetComponent<Text>().text = "<color=red>+3</color>";
            }
        }
    }
    public void StartTimer()
    {
        // ゲーム開始時に全ての状態をリセット
        score = 0;
        combo = 0;
        feverTimer = 0;
        timer = TimeLimit;
        isStartGame = true;
        isResult = false;
        SetText();
    }

    public void Miss(bool isWrongDog)
    {
        combo = 0;
    }
    public void SetText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        if (timerText != null)
        {
            timerText.text = (Mathf.Round(timer * 10f) / 10f).ToString("F1");
        }
        if (comboImage != null && comboSprites != null && combo < comboSprites.Length)
        {
            comboImage.sprite = comboSprites[combo];
        }
        //GameObject.Find("ComboText").GetComponent<Text>().text = combo.ToString();
    }
}
