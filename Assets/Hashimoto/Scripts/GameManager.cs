using System.Threading;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector] public int score;
    [HideInInspector] public float timer;
    [HideInInspector] public int combo;
    [HideInInspector] public float feverTimer;
    Text scoreText, timerText;
    Image comboImage;
    [SerializeField] int maxCombo, TimeLimit, feverTime;
    [SerializeField] Sprite[] comboSprites = new Sprite[11];
    GameObject
    public bool isStartGame, isResult;

    Image feverImage;

    private void Awake()
    {
        if(instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        feverImage = GameObject.Find("FeverImage").GetComponent<Image>();
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if(isStartGame)
            {
                isResult = true;
            }
            timer = 0;
        }
        if (feverTimer > 0)
        {
            feverTimer -= Time.deltaTime;
            feverImage.color = Color.red;
        }
        else
        {
            feverTimer = 0;
            feverImage.color = new Color(0, 0, 0, 0);
        }
        if (combo == maxCombo)
        {
            feverTimer = feverTime;
            combo = 0;
        }
        SetText();
    }
    public void ResetGame()
    {
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        comboImage = GameObject.Find("ComboImage").GetComponent<Image>();
        score = 0;
        timer = 0;
        combo = 0;
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
            }
            else if (feverTimer > 0)
            {
                score += 3;
            }
        }
    }
    public void StartTimer()
    {
        timer = 30;
        isStartGame = true;
        isResult = false;
    }

    public void Miss(bool isWrongDog)
    {
        combo = 0;
    }
    public void SetText()
    {
        scoreText.text = score.ToString() + "‘K";
        timerText.text = timer.ToString() + "•b";
        comboImage.sprite = comboSprites[combo];
        Debug.Log("Combo : " + combo);
        GameObject.Find("ComboText").GetComponent<Text>().text = combo.ToString();
    }
}
