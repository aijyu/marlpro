using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Text scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public GameObject gameOverPanel;
    public GameObject stageClearPanel;
    public Text finalScoreText; // クリア時のスコア表示用

    private int bonusScore = 0; // 敵撃破などのボーナス
    private int maxHeightScore = 0; // 到達高度スコア
    private bool isGameOver = false;

    private Transform playerTransform;

    void Start()
    {
        // Failsafe: 参照が切れていたら名前で探す
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        if (stageClearPanel == null) stageClearPanel = GameObject.Find("StageClearPanel");
        if (scoreText == null)
        {
            GameObject stObj = GameObject.Find("ScoreText");
            if (stObj != null) scoreText = stObj.GetComponent<Text>();
        }
        if (finalScoreText == null)
        {
            GameObject ftObj = GameObject.Find("FinalScoreText");
            if (ftObj != null) finalScoreText = ftObj.GetComponent<Text>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        UpdateScoreUI();
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (stageClearPanel != null) stageClearPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        if (playerTransform != null)
        {
            // 高さをスコアに換算 (Y=1 -> 10点)
            int currentHeightScore = (int)(playerTransform.position.y * 10);
            if (currentHeightScore > maxHeightScore)
            {
                maxHeightScore = currentHeightScore;
                UpdateScoreUI();
            }
        }
    }

    public void AddScore(int visibleScore)
    {
        if (isGameOver) return;
        bonusScore += visibleScore;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            int totalScore = maxHeightScore + bonusScore;
            scoreText.text = "Score: " + totalScore;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        Debug.Log("Game Over!");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void StageClear()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        AddScore(1000); // クリアボーナス

        Debug.Log("Stage Clear!");
        if (stageClearPanel != null)
        {
            stageClearPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + (maxHeightScore + bonusScore);
            }
        }
    }



    // Goal.csからの互換性用
    public void GameWin()
    {
        StageClear();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextStage()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // 次のシーンがあるか確認
        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            // なければ最初に戻る（あるいはエンディング）
            Debug.Log("All Stages Cleared! Returning to first stage.");
            SceneManager.LoadScene(0);
        }
    }
}
