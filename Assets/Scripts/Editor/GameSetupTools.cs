using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSetupTools
{
    [MenuItem("Tools/Create 2D Game Scene")]
    public static void CreateScene()
    {
        // 1. タグの登録 (Enemy, Goal)
        CreateTag("Enemy");
        CreateTag("Goal");

        // 2. 既存のオブジェクトを掃除 (Canvas, EventSystem, Camera除く...いや、全削除推奨だが、カメラは残すか再利用)
        // カメラはセットアップし直す
        if (Camera.main != null)
        {
            Object.DestroyImmediate(Camera.main.gameObject);
        }
        
        // 3. Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f; // 視野を広く
        cam.backgroundColor = new Color(0.5f, 0.7f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 0, -10);

        // 4. Ground (地面)
        GameObject ground = CreateSpriteObject("Ground", new Color(0.2f, 0.8f, 0.2f), new Vector3(0, -4, 0), new Vector3(500, 2, 1)); // 幅をさらに広く
        ground.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;

        // 5. Platforms (空中の足場) & 7. Enemy loop
        // スタート付近からゴール付近までループで生成
        for (int x = 5; x < 100; x += 8)
        {
            // ランダムな高さオフセット (-1 to 2) 簡易的にパターン化
            float yOffset = (x % 3) * 1.5f - 1f; 
            CreateSpriteObject($"Platform_{x}", new Color(0.6f, 0.4f, 0.2f), new Vector3(x, yOffset, 0), new Vector3(3, 1, 1));

            // 一定間隔で敵を配置 (足場の上)
            if (x % 16 == 5) // 適当な頻度
            {
                 CreateEnemy(new Vector3(x, yOffset + 1.5f, 0));
            }
        }
        
        // 地上の敵も追加
        for (int x = 10; x < 100; x += 15)
        {
             CreateEnemy(new Vector3(x, -2, 0));
        }

        // 6. Player
        GameObject player = CreateSpriteObject("Player", Color.white, new Vector3(-5, -2, 0), Vector3.one);
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // ガクつき防止
        
        // 壁張り付き防止のマテリアル
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
        noFriction.friction = 0;
        player.GetComponent<BoxCollider2D>().sharedMaterial = noFriction;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;
        pc.jumpForce = 12f;
        
        // GroundCheck用の子オブジェクト
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.6f, 0); 
        pc.groundCheck = groundCheck.transform;
        
        // レイヤーマスク設定
        pc.groundLayer = -1; 

        // カメラ追従設定
        CameraFollow cf = cameraObj.AddComponent<CameraFollow>();
        cf.target = player.transform;

        // 8. Goal
        GameObject goal = CreateSpriteObject("Goal", Color.yellow, new Vector3(110, -2, 0), new Vector3(1, 4, 1));
        goal.tag = "Goal";
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<Goal>(); // Goalスクリプトをアタッチ

        // 9. GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // 10. UI Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem (必須)
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // Score Text
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(canvasObj.transform, false);
        Text scoreText = scoreTextObj.AddComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 30;
        scoreText.color = Color.black;
        scoreText.rectTransform.anchorMin = new Vector2(0, 1);
        scoreText.rectTransform.anchorMax = new Vector2(0, 1);
        scoreText.rectTransform.pivot = new Vector2(0, 1);
        scoreText.rectTransform.anchoredPosition = new Vector2(20, -20);
        scoreText.rectTransform.sizeDelta = new Vector2(200, 50);
        gm.scoreText = scoreText;

        // Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", Color.black, 0.8f);
        Text goText = CreateText(gameOverPanel.transform, "GAME OVER", 50, Color.red, 50);
        Button restartBtn = CreateButton(gameOverPanel.transform, "Restart", 50, gm);
        gm.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // Stage Clear Panel
        GameObject clearPanel = CreatePanel(canvasObj.transform, "StageClearPanel", Color.white, 0.8f);
        Text scText = CreateText(clearPanel.transform, "STAGE CLEAR!", 50, Color.blue, 100);
        
        GameObject finalScoreObj = new GameObject("FinalScoreText");
        finalScoreObj.transform.SetParent(clearPanel.transform, false);
        Text fsText = finalScoreObj.AddComponent<Text>();
        fsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fsText.fontSize = 30;
        fsText.color = Color.black;
        fsText.alignment = TextAnchor.MiddleCenter;
        fsText.rectTransform.anchoredPosition = new Vector2(0, 0);
        fsText.rectTransform.sizeDelta = new Vector2(400, 50);
        gm.finalScoreText = fsText;

        Button clearRestartBtn = CreateButton(clearPanel.transform, "Restart", -100, gm);
        gm.stageClearPanel = clearPanel;
        clearPanel.SetActive(false); // 初期は非表示

        // 変更をUnityエディタに認識させる（重要）
        EditorUtility.SetDirty(gm);

        Debug.Log("Scene Created Successfully!");
    }

    private static GameObject CreateSpriteObject(string name, Color color, Vector3 position, Vector3 scale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(color);
        
        obj.AddComponent<BoxCollider2D>();
        
        return obj;
    }

    private static Sprite CreateSquareSprite(Color color)
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    private static void CreateEnemy(Vector3 position)
    {
        GameObject enemy = CreateSpriteObject("Enemy", Color.red, position, Vector3.one);
        enemy.tag = "Enemy";
        
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // ガクつき防止

        EnemyController ec = enemy.AddComponent<EnemyController>();
        
        // Checks
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = enemy.transform;
        // 判定位置を内側(0.3f)に寄せて、崖ギリギリまで進めるように緩和
        groundCheck.transform.localPosition = new Vector3(0.3f, -0.6f, 0); 
        ec.groundCheck = groundCheck.transform;

        GameObject wallCheck = new GameObject("WallCheck");
        wallCheck.transform.parent = enemy.transform;
        wallCheck.transform.localPosition = new Vector3(0.6f, 0, 0); 
        ec.wallCheck = wallCheck.transform;

        ec.groundLayer = -1; 
    }

    private static void CreateTag(string tagName)
    {
        Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if ((asset != null) && (asset.Length > 0))
        {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagName)
                {
                    return; 
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, float alpha)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image img = panel.AddComponent<Image>();
        color.a = alpha;
        img.color = color;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero; 
        
        return panel;
    }

    private static Text CreateText(Transform parent, string content, int fontSize, Color color, float yOffset)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.rectTransform.anchoredPosition = new Vector2(0, yOffset);
        txt.rectTransform.sizeDelta = new Vector2(400, 100);
        return txt;
    }

    private static Button CreateButton(Transform parent, string label, float yOffset, GameManager gm)
    {
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(parent, false);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        
        Button btn = btnObj.AddComponent<Button>();
        
        // 重要: エディタスクリプトで永続的なリスナーを追加する
        // UnityEventToolsを使用するにはUnityEditor.Events名前空間が必要だが、
        // このスクリプトはEditorフォルダにあるのでOK
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, gm.RestartGame);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = new Vector2(0, yOffset);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.resizeTextForBestFit = true;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }
}
