using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;
    public float turnDelay = 0.1f;
    public static GameManager instance = null;
    public BoardManager boarScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    private Text recordText;
    private Text levelText;
    private GameObject levelImage;
    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup;
    private InterstitialAd interstitial;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
        MobileAds.Initialize(initStatus => { });
        RequestInterstitial();
        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boarScript = GetComponent<BoardManager>();
        InitGame();
    }
    private void RequestInterstitial()
    {
        string adUnitId = "ca-app-pub-6210545297762687/6388796299";
        interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        interstitial.LoadAd(request);
    }
    private void OnLevelWasLoaded()
    {
        level++;
        InitGame();
    }
    void Update()
    {
        if (playersTurn || enemiesMoving || doingSetup)
        {
            return;
        }
        StartCoroutine(MoveEnemies());
    }
    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }
    public void GameOver()
    {
        if (interstitial.IsLoaded() && level > 4)
        {
            interstitial.Show();
        }
        levelText.text = "After " + level + " days, you dead!";
        int lvlPrevRecord = PlayerPrefs.GetInt("LvlMax");
        if (lvlPrevRecord < level)
        {
            recordText.text = "You Record : " + level + " days, you dead!";
            PlayerPrefs.SetInt("LvlMax", level);
        }
        else
        {
            recordText.text = "You Record : " + lvlPrevRecord;
        }
        levelImage.SetActive(true);
        Invoke("RestartLvl", levelStartDelay);
        //enabled = false;
    }

    [System.Obsolete]
    public void RestartLvl()
    {
        Application.LoadLevel(Application.loadedLevel);
        recordText.text = null;
        level = 0;
        SoundManager.instance.musicSource.Play();
        InitGame();
    }
    void InitGame()
    {
        doingSetup = true;
        recordText = GameObject.Find("RecordText").GetComponent<Text>();
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);
        enemies.Clear();
        boarScript.SetupScene(level);
    }
    void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        playersTurn = true;
        enemiesMoving = false;
    }
}
