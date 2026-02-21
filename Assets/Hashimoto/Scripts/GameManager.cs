using System.Threading;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score;
    public float timer;
    public int combo;
    public float feverTime;

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
        timer = 0;
        score = 0;
        combo = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer >= 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 0;
        }
    }
    public void ResetGame()
    {
        score = 0;
        timer = 30;
        combo = 0;
    }
    public void AddScore(int plusScore)
    {
        combo += 1;
        score += plusScore;
    }
    public void StartTimer()
    {
        timer = 30;
    }

    public void Miss(bool isWrongDog)
    {
        combo = 0;
    }
}
