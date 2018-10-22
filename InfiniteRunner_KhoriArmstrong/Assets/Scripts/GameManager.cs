using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
#if DEBUG
    if (Input.GetKey(KeyCode.Escape))
    {
    Application.Quit();
    }
#endif
*/


public class GameManager : MonoBehaviour {

    public float PLAYERMOVEMENTSPEED = 15f; // 15
    public float PLAYERSPEEDMULT = 0.04f;
    public int LEVELUPTHRESHOLD = 10;
    public int LEVELCAP = 100;

    int SECOND;

    public GameObject layer_menu;
    public GameObject layer_game;
    public GameObject layer_score;

    public Player player;
    public GameCam cam;
    private List<Backdrop> allBackdrops;

    public GameObject geography;
    public GameObject gameplayZone;
    ObstacleCourse currentCourse;
    ObstacleCourse lastCourse;
    ObstacleCourse lastLastCourse;

    public Text levelText;
    public UI_HealthBar playerHealth;
    public Slider gameProgress;

    public int obstacleCounter = 0;
    public int zoneHeight = 0;

    public float runSpeed;

    public class Obstacle
    {
        public string id;
        public int offsetting;


        public Obstacle(string i, int off)
        {
            id = i;
            offsetting = off;
        }
    }

    public Dictionary<string,Obstacle> courseDictionary;
    List<string> allCourses;
    public List<List<string>> obstacleCourses;


    private float gameTime;

    public string[] DomainList;
    bool checkDomain = true;
    bool illegalCopy = true;
    bool fullURL = true;

	// Use this for initialization
	void Start () {
        print(Application.absoluteURL);
        Debug.Log(Application.platform);
        if (checkDomain) // ??? <-- Not working... // Application.platform == RuntimePlatform.WebGLPlayer && 
        {
            int i = 0;
            for (i=0; i<DomainList.Length; i++)
            {
                Debug.Log(DomainList[i]);
                if (Application.absoluteURL == DomainList[i])
                {
                    illegalCopy = false;
                }
                else if (Application.absoluteURL.Contains(DomainList[i]) && !fullURL)
                {
                    illegalCopy = false;
                }
            }
        }

        // ==================================================================================================================================


        float jist;
        float count = 10;
        for (int i = 0; i < count; i++)
        {
            jist = (i / (count - 1));
            //
            Backdrop Q = ((GameObject)Instantiate(Resources.Load("Prefabs/Floor"), gameplayZone.transform)).GetComponent<Backdrop>();
            Q.distMult = 1 + (3 - 1) * jist;
            Q.transform.position = (new Vector3(0, -(0 + (5 - 0) * jist), 5 - 1 * jist)) * 0.1f;
        }


        // ==================================================================================================================================

        allBackdrops = new List<Backdrop>();
        foreach (Backdrop Q in FindObjectsOfType<Backdrop>())
        {
            allBackdrops.Add(Q);
        }



        SECOND = 60; // ??? <-- I don't actually recall how to "programmatically" find the frame rate...

        layer_menu.SetActive(false);
        layer_game.SetActive(false);
        layer_score.SetActive(false);


        courseDictionary = new Dictionary<string, Obstacle>() {
            { "line", new Obstacle("line", 0) },
            { "bumpy", new Obstacle("bumpy", 0) },
            { "godown", new Obstacle("godown", -1) },
            { "goup", new Obstacle("goup", +1) },
            { "complex_pitfall", new Obstacle("complex_pitfall", 0) },
            { "drop", new Obstacle("drop", -2) },
            { "complex_ascend", new Obstacle("complex_ascend", -2) },
            { "complex_chamber", new Obstacle("complex_chamber", 0) },
        };
        //
        allCourses = new List<string>();
        foreach (KeyValuePair<string, Obstacle> Q in courseDictionary)
        {
            allCourses.Add(Q.Value.id);
        }
        //
        obstacleCourses = new List<List<string>>() {
            new List<string>() {
                "line",
                "bumpy",
                "godown",
                "godown",
                "goup",
                "goup",
            },
            new List<string>() {
                "line",
                "bumpy",
                "bumpy",
                "godown",
                "godown",
                "goup",
                "goup",
            },
            new List<string>() {
                "line",
                "bumpy",
                "bumpy",
                "bumpy",
                "godown",
                "goup",
            },
            new List<string>() {
                "line",
                "bumpy",
                "bumpy",
                "bumpy",
                "godown",
                "godown",
                "goup",
                "goup",
                "complex_pitfall",
            },
            new List<string>() {
                "line",
                "bumpy",
                "bumpy",
                "bumpy",
                "godown",
                "goup",
                "complex_pitfall",
            },
            new List<string>() {
                "bumpy",
                "bumpy",
                "godown",
                "goup",
                "complex_pitfall",
            },
            new List<string>() {
                "bumpy",
                "bumpy",
                "godown",
                "goup",
                "complex_pitfall",
                "complex_pitfall",
            },
            new List<string>() {
                "bumpy",
                "godown",
                "goup",
                "complex_pitfall",
                "complex_pitfall",
                "complex_chamber",
            },
        };



        if (illegalCopy)
        {
            // *** We'll go REEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEALLY slow.
            PLAYERMOVEMENTSPEED /= 4;
            PLAYERSPEEDMULT /= 4;
        }
    }

    public enum GameStates
    {
        Init,
        Menu,
        Game,
        Score,
    };
    GameStates state = GameStates.Init;
    public void ChangeState(GameStates gs)
    {
        // *** Ending state...
        switch (state)
        {
            case GameStates.Init:
                PlayerPrefs.DeleteKey("highScore"); // *** Debug only
                break;
            case GameStates.Menu:
                layer_menu.SetActive(false);
                break;
            case GameStates.Game:
                layer_game.SetActive(false);
                //
                Gameplay_HighScore();
                break;
            case GameStates.Score:
                layer_score.SetActive(false);
                break;
        }
        //
        state = gs;
        //
        // *** Beginning state...
        switch (state)
        {
            case GameStates.Init:
                break;
            case GameStates.Menu:
                layer_menu.SetActive(true);
                break;
            case GameStates.Game:
                layer_game.SetActive(true);
                break;
            case GameStates.Score:
                layer_score.SetActive(true);
                break;
        }
    }

    bool gameIsOver = false;
    int gameoverTime = 0;
    internal void GameOver()
    {
        gameIsOver = true;
    }

    internal void InitializeGame()
    {
        if (currentCourse != null)
        {
            Destroy(currentCourse.gameObject);
            currentCourse = null;
        }
        if (lastCourse != null)
        {
            Destroy(lastCourse.gameObject);
            lastCourse = null;
        }
        if (lastLastCourse != null)
        {
            Destroy(lastLastCourse.gameObject);
            lastLastCourse = null;
        }

        obstacleCounter = 0;
        zoneHeight = 0;
        runSpeed = 0;
        gameoverTime = 0;
        gameIsOver = false;
        //
        player.isDead = false;
        if (illegalCopy) { player.maxHealth = 10; }
        player.health = 100;
        player.canJump = 0;
        player.transform.position = (new Vector3(0, 1.5f, 1))*0.1f;
        //
        //
        foreach (Backdrop Q in allBackdrops)
        {
            // Q.transform.position = new Vector3(0, 0, Q.transform.position.z);
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        switch (state)
        {
            case GameStates.Init:
                Gameplay_MenuMode();
                return;
            case GameStates.Menu:
                break;
            case GameStates.Game:
                if (gameIsOver)
                {
                    gameoverTime++;
                    if (gameoverTime > SECOND*3)
                    {
                        ChangeState(GameStates.Score);
                    }
                }
                else
                {
                    RunGameplayRunner();
                }
                break;
        }
        
        
    }

    public void Gameplay_MenuMode()
    {
        ChangeState(GameStates.Menu);
    }

    private void RunGameplayRunner()
    {
        gameTime += Time.deltaTime;


        runSpeed = (PLAYERMOVEMENTSPEED + PLAYERSPEEDMULT * obstacleCounter);
        runSpeed *= Mathf.Sqrt(1 + Mathf.Min(obstacleCounter, LEVELCAP) / LEVELUPTHRESHOLD);
        //
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(0, rb.velocity.y);
        //
        cam.transform.position = new Vector3(cam.transform.position.x, Mathf.Max(0, player.transform.position.y), player.transform.position.z);
        //
        Vector3 shiftVec = new Vector3(-runSpeed * Time.deltaTime * Time.deltaTime, 0, 0);
        if (currentCourse != null)
        {
            currentCourse.transform.Translate(shiftVec);
        }
        if (lastCourse != null)
        {
            lastCourse.transform.Translate(shiftVec);
        }
        if (lastLastCourse != null)
        {
            lastLastCourse.transform.Translate(shiftVec);
        }
        //
        foreach (Backdrop Q in allBackdrops)
        {
            Q.transform.Translate(shiftVec * Q.distMult);
            //
            Vector3 bounds = Q.GetComponent<MeshRenderer>().bounds.size;
            if (Q.transform.position.x < -bounds.x/4)
            {
                Q.transform.Translate(new Vector3(2*(bounds.x / 4), 0, 0));
            }
        }



        if (currentCourse == null)
        {
            currentCourse = ((GameObject)Instantiate(Resources.Load("Prefabs/layout_line"), player.transform.position + (new Vector3(0, -0.1f, 0)), Quaternion.identity)).GetComponent<ObstacleCourse>();
            //
            currentCourse.transform.SetParent(geography.transform);
            //
            currentCourse.transform.localScale = Vector3.one;
            currentCourse.gameObject.SetActive(true);
        }
        else if (Mathf.Max(cam.transform.position.x, player.transform.position.x) > currentCourse.start.transform.position.x)
        {
            if (lastLastCourse != null)
            {
                Destroy(lastLastCourse.gameObject);
                lastLastCourse = null;
            }
            //
            lastLastCourse = lastCourse;
            //
            lastCourse = currentCourse;
            //
            currentCourse = ((GameObject)Instantiate(Resources.Load("Prefabs/layout_" + SelectCourse()), lastCourse.end.pos, Quaternion.identity)).GetComponent<ObstacleCourse>();
            //
            currentCourse.transform.SetParent(geography.transform);
            //
            currentCourse.transform.localScale = Vector3.one;
            currentCourse.gameObject.SetActive(true);
            //
            //
            // ### <-- ... I don't even know why this works.
            Vector3 Q = lastCourse.end.pos - currentCourse.start.pos;
            Q.x *= currentCourse.transform.lossyScale.x;
            Q.y *= currentCourse.transform.lossyScale.y;
            Q.z *= currentCourse.transform.lossyScale.z;
            currentCourse.transform.position = lastCourse.transform.position + Q;


            obstacleCounter++;
            //
            if ((obstacleCounter) % LEVELUPTHRESHOLD == 0 && obstacleCounter < LEVELCAP)
            {
                Debug.Log("Speed up!");
            }
            levelText.text = "Lv." + obstacleCounter.ToString("000");
        }

        playerHealth.SetValue(player.health / 100);
        float missionprg = (obstacleCounter % LEVELUPTHRESHOLD) * 1.0f / LEVELUPTHRESHOLD;
        float playerprg;
        if (lastCourse == null) { playerprg = 0; }
        else
        {
            playerprg = ((cam.transform.position.x - lastCourse.start.transform.position.x) / (lastCourse.end.transform.position.x - lastCourse.start.transform.position.x));
        }
        gameProgress.value = missionprg + (playerprg / LEVELUPTHRESHOLD);
    }

    public string SelectCourse()
    {
        Obstacle coursepick = null;
        if ((obstacleCounter+2) % LEVELUPTHRESHOLD == 0)
        {
            coursepick = courseDictionary["line"]; // *** Always do a line when we're speeding up.
        }
        else
        {
            while (coursepick == null)
            {
                List<string> courseSet;
                if (obstacleCounter / LEVELUPTHRESHOLD >= obstacleCourses.Count) {
                    courseSet = allCourses;
                }
                else {
                    courseSet = obstacleCourses[Mathf.Min(obstacleCounter / LEVELUPTHRESHOLD, obstacleCourses.Count - 1)];
                }
                coursepick = courseDictionary[courseSet[UnityEngine.Random.Range(0, courseSet.Count)]];
                //
                if (zoneHeight+coursepick.offsetting < 0) { coursepick = null; } // *** Don't go down if we're at the bottom...
                else if (zoneHeight + coursepick.offsetting >= 5) { coursepick = null; } // *** Don't go up if we're at the top...
                else if (coursepick.id == "line" && obstacleCounter < 20) { coursepick = null; } // *** No lines if we're going slowly.
            };
        }
        //
        zoneHeight += coursepick.offsetting;
        //
        return coursepick.id;
    }





    public void Gameplay_StartGame()
    {
        InitializeGame();
        //
        ChangeState(GameStates.Game);
    }


    public Text scoreTextMessage;
    public void Gameplay_HighScore()
    {
        int lastScore;
        if (PlayerPrefs.HasKey("highScore"))
        {
            lastScore = PlayerPrefs.GetInt("highScore");
            //
            scoreTextMessage.text = "The best runner got to Level " + lastScore + ".\nYou reached Level " + obstacleCounter + ".";
        }
        else
        {
            lastScore = -1;
            //
            scoreTextMessage.text = "You reached Level " + obstacleCounter + "!";
        }
        //
        if (obstacleCounter > lastScore)
        {
            PlayerPrefs.SetInt("highScore", obstacleCounter);
        }
    }








    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    private static GameManager _instance;
    public static GameManager Instance()
    {
        return _instance;
    }
}
