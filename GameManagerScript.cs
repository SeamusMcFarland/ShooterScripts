using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq.Expressions;
using UnityEngine.UIElements;

public class GameManagerScript : MonoBehaviour
{
    public AudioClip bulletClang;
    public AudioClip gunshot;
    public AudioClip reload;
    public AudioClip bulletStrike;
    public AudioClip switchWeapons;
    public AudioClip shotgun;
    public AudioClip minigun;
    public AudioClip playerHit;
    public AudioClip killsound;
    public AudioClip playerKill;
    public AudioClip click;
    public AudioClip boom;

    int score;
    int lives;
    float timer;

    int defaultScore;
    int defaultLives;

    bool beatLevel;
    bool canBeatLevel;
    int beatLevelScore;

    bool timedLevel;
    float startTime;

    GameObject player;
    bool playerEnabled;

    public AudioSource backgroundMusic;
    public AudioSource soundEffects;
    AudioSource gameOverSFX, beatLevelSFX;
    bool backgroundMusicOver;

    enum GameStates { Playing, Death, GameOver, BeatLevel };
    GameStates gameState;
    bool gameIsOver;
    bool playerIsDead;

    public Canvas menuCanvas, HUDCanvas, endScreenCanvas, footerCanvas;

    public String endMessageWinS, endMessageLoseS, gameOverS, gameTitleS, gameCreditsS, gameCopyrightS, timerTitleS, scoreS, livesS, scoreVS, livesVS, timerVS;
    public Text endMessageT, gameOverT, gameTitleT, gameCreditsT, gameCopyrightT, timerTitleT, scoreT, livesT, timerT, scoreVT, livesVT, timerVT;

    public String firstLevel, nextLevel, levelToLoad, currentLevel;

    float currentTime;

    bool gameStarted;
    bool replay;

    String copyrightTextAquire;

    AudioSource audioSource;

    float hurtTimer;
    const float HURT_COOLDOWN = 0.5f;

    GameObject camera;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Reset()
    {
        endMessageT.text = "";
        gameOverT.text = "";
        gameTitleT.text = "";
        gameCreditsT.text = "";
        gameCopyrightT.text = "";
        timerTitleT.text = "";
        scoreT.text = "";
        livesT.text = "";
        timerT.text = "";
        scoreVT.text = "";
        livesVT.text = "";
        timerVT.text = "";
    }

    public void HideMenu()
    {
        menuCanvas.enabled = false;
        footerCanvas.enabled = false;
        endScreenCanvas.enabled = false;
        print("hidden at: " + Time.time);
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Main Camera");
        HideMenu();
        levelToLoad = firstLevel;
        MainMenu();
        //PlayGame();
    }

    public void MainMenu()
    {
        HUDCanvas.enabled = false;

        defaultScore = 0;
        defaultLives = 3;

        gameTitleS = "Unblockable";
        gameCreditsS = "by Seamus McFarland";
        gameCopyrightS = "Music is OdM EP from Hidden Traffic by Massa";

        gameTitleT.text = gameTitleS;
        gameCreditsT.text = gameCreditsS;
        gameCopyrightT.text = gameCopyrightS;

        if (menuCanvas != null)
            menuCanvas.enabled = true;
        if (footerCanvas != null)
            footerCanvas.enabled = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        playerEnabled = true;
        HideMenu();
        if (HUDCanvas != null)
            HUDCanvas.enabled = true;

        if (timedLevel)
        {
            currentTime = startTime;

            timerTitleS = "Timer: ";
            timerTitleT.text = timerTitleS;
        }

        if (scoreT != null)
        {
            scoreT.text = scoreS;
        }

        if (livesT != null)
        {
            livesT.text = livesS;
        }

        gameStarted = true;
        gameState = GameStates.Playing;
        playerIsDead = false;
        camera.SetActive(false);
        SceneManager.LoadScene(levelToLoad, LoadSceneMode.Additive);
        backgroundMusic.volume = 1f;
        lives = defaultLives;
        timerT.enabled = false;
        timerVT.enabled = false;
        backgroundMusic.Play();
        currentLevel = levelToLoad;
    }

    // Update is called once per frame
    void Update()
    {
        hurtTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();

        //TestCommands();

        UpdateScoreAndLives();

        switch (gameState)
        {
            case GameStates.Playing:
                if (playerIsDead)
                {
                    if (lives > 0)
                    {
                        lives--;
                        ResetLevel();
                    }
                    else
                    {
                        gameState = GameStates.Death;
                    }
                }
                if (canBeatLevel && score >= beatLevelScore)
                {
                    gameState = GameStates.BeatLevel;
                }
                if (timedLevel)
                {
                    if (currentTime < 0)
                        gameState = GameStates.GameOver;
                    else if (timerVT != null)
                    {
                        timer -= Time.deltaTime;
                        timerVS = "" + timer;
                        timerVT.text = timerVS;
                    }
                }
                break;

            case GameStates.Death:
                if (backgroundMusic != null)
                {
                    if (backgroundMusic.volume <= 0)
                        backgroundMusicOver = true;
                    else
                        backgroundMusic.volume -= 0.002f;

                    if (backgroundMusicOver || backgroundMusic.clip == null)
                    {
                        if (gameOverSFX != null)
                            audioSource = gameOverSFX;
                        endMessageT.text = endMessageLoseS;
                        gameState = GameStates.GameOver;
                    }
                }
                break;

            case GameStates.BeatLevel:
                if (backgroundMusic != null)
                {
                    if (backgroundMusic.volume <= 0)
                        backgroundMusicOver = true;
                    else
                        backgroundMusic.volume -= 0.01f;

                    if (backgroundMusicOver || backgroundMusic.clip == null)
                    {
                        if (beatLevelSFX != null)
                            audioSource = beatLevelSFX;

                        if (nextLevel != null)
                            StartNextLevel();
                        else
                        {
                            endMessageT.text = "" + endMessageWinS;
                            gameState = GameStates.GameOver;
                        }
                    }
                }
                break;

            case GameStates.GameOver:
                if (playerEnabled)
                {
                    GameObject.FindGameObjectWithTag("player").SetActive(false);
                    playerEnabled = false;
                }
                HideMenu();
                if (endScreenCanvas != null)
                {
                    endScreenCanvas.enabled = true;
                    print("shown at: " + Time.time);
                    gameOverT.text = gameOverS;
                }
                break;

            default:
                print("ERROR! INVALID GAMESTATE!");
                break;
        }

    }

    private void TestCommands()
    {
        if (Input.GetKeyDown(KeyCode.U))
            print("playerIsDead: " + playerIsDead);
        if (Input.GetKeyDown(KeyCode.I))
            print("gameIsOver: " + gameIsOver);
        if (Input.GetKeyDown(KeyCode.O))
            print("playerIsDead: " + canBeatLevel);
    }

    private void UpdateScoreAndLives()
    {
        if (scoreVT != null)
        {
            scoreVS = "" + score;
            scoreVT.text = scoreVS;
        }
        if (livesVT != null)
        {
            livesVS = "" + lives;
            livesVT.text = livesVS;
        }
    }

    public void ResetLevel()
    {
        print("resetlevel called");
        playerIsDead = false;
        SceneManager.UnloadSceneAsync(currentLevel);
        PlayGame();
    }

    public void StartNextLevel()
    {
        print("startnextlevel called");
        backgroundMusicOver = false;
        lives = defaultLives;
        SceneManager.UnloadSceneAsync(currentLevel);
        levelToLoad = nextLevel;
        PlayGame();
    }

    public void RestartGame()
    {
        print("restart called");
        score = defaultScore;
        lives = defaultLives;
        SceneManager.UnloadSceneAsync(currentLevel);
        levelToLoad = firstLevel;
        PlayGame();
    }

    public void Win()
    {
        gameState = GameStates.BeatLevel;
    }


    public void Lose()
    {
        lives = 0;
        gameState = GameStates.GameOver;
    }

    public void Hurt()
    {
        if (hurtTimer < 0)
        {
            hurtTimer = HURT_COOLDOWN;
            lives--;
            if (lives < 1)
                gameState = GameStates.GameOver;
        }
    }

    public void ScorePoint()
    {
        score++;
    }

    public void PlayBulletClang()
    {
        soundEffects.PlayOneShot(bulletClang);
    }

    public void PlayGunshot()
    {
        soundEffects.PlayOneShot(gunshot);
    }

    public void PlayShotgun()
    {
        soundEffects.PlayOneShot(shotgun);
    }

    public void PlayMinigun()
    {
        soundEffects.PlayOneShot(minigun);
    }

    public void PlayReload()
    {
        soundEffects.PlayOneShot(reload);
    }

    public void PlayBulletStrike()
    {
        soundEffects.PlayOneShot(bulletStrike);
    }

    public void PlaySwitchWeapons()
    {
        soundEffects.PlayOneShot(switchWeapons);
    }

    public void PlayPlayerHit()
    {
        soundEffects.PlayOneShot(playerHit);
    }

    public void PlayKillsound()
    {
        soundEffects.PlayOneShot(killsound);
    }

    public void PlayPlayerKill()
    {
        soundEffects.PlayOneShot(playerKill);
    }

    public void PlayClick()
    {
        soundEffects.PlayOneShot(click);
    }

    public void PlayBoom()
    {
        soundEffects.PlayOneShot(boom);
    }

}
