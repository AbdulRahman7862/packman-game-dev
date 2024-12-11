using UnityEngine;
using UnityEngine.UI;
using TMPro; 

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private TextMeshProUGUI gameOverText; // Updated
    [SerializeField] private TextMeshProUGUI scoreText;    // Updated
    [SerializeField] private TextMeshProUGUI livesText;    // Updated
    [SerializeField] private TextMeshProUGUI pauseButtonText;

    public AudioSource munch1;
    public AudioSource munch2;

     private bool isPaused = false; // Pause state
    public int currentMunch = 0; 

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private int ghostMultiplier = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1; 
    }

    private void Start()
{
    gameOverText.gameObject.SetActive(false); // Hide Game Over text initially
    NewGame();
}


    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(true);
        }

        this.pacman.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }

        this.pacman.gameObject.SetActive(false);

         gameOverText.text = "Game Over!";
    gameOverText.gameObject.SetActive(true);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "Lives: " + this.lives.ToString(); // Update UI
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = "Score: " + this.score.ToString(); // Update UI
    }

   public void PacmanEaten()
{
    this.pacman.gameObject.SetActive(false);

    SetLives(lives - 1);

    if (this.lives > 0)
    {
        // Move ghosts to their home positions to avoid overlap
        foreach (Ghost ghost in ghosts)
        {
            ghost.SetPosition(ghost.home.inside.position); // Send ghosts home
            ghost.ResetState();
        }

        // Respawn Pacman at a safe location
        pacman.transform.position = pacman.movement.startingPosition;
        Invoke(nameof(ResetState), 3.0f);
    }
    else
    {
        GameOver();
    }
}


    public void GhostEaten(Ghost ghost)
    {
        SetScore(this.score + ghost.points);
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

   public void collectedPellet()
{
    if (currentMunch == 0)
    {
        munch1.Play();
        currentMunch = 1; // Alternate to munch2 next time
    }
    else
    {
        munch2.Play();
        currentMunch = 0; // Alternate to munch1 next time
    }
}

 // Pause/Resume game logic
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0; // Pause the game
            pauseButtonText.text = "Resume"; // Change button text
        }
        else
        {
            Time.timeScale = 1; // Resume the game
            pauseButtonText.text = "Pause"; // Change button text
        }
    }



    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }
}
