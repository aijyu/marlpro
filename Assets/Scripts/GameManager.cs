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

    private int score = 0;
    private bool isGameOver = false;

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

        UpdateScoreUI();
        // 初期化時に非表示にするが、シーンロード直後はオブジェクトがアクティブな可能性があるので
        // 取得後に非表示にする
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (stageClearPanel != null) stageClearPanel.SetActive(false);
    }

    public void AddScore(int visibleScore)
    {
        if (isGameOver) return;
        score += visibleScore;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
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
                finalScoreText.text = "Final Score: " + score;
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
