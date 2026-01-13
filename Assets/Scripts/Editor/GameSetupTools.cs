using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSetupTools
{
    // Stage 1-1
    [MenuItem("Tools/Create Stage 1-1")]
    public static void CreateStage1_1()
    {
        StageSettings settings = new StageSettings
        {
            stageName = "Stage 1-1",
            skyColor = new Color(0.5f, 0.7f, 1f), // Blue Sky
            groundColor = new Color(0.2f, 0.8f, 0.2f), // Green Ground
            platformColor = new Color(0.6f, 0.4f, 0.2f), // Brown
            holeFrequency = 0, // No big holes in ground, just continuous
            enemyFrequency = 20,
            platformGap = 10,
            goalX = 110
        };
        CreateScene(settings);
    }

    // Stage 1-2
    [MenuItem("Tools/Create Stage 1-2")]
    public static void CreateStage1_2()
    {
        StageSettings settings = new StageSettings
        {
            stageName = "Stage 1-2",
            skyColor = new Color(1f, 0.6f, 0.4f), // Sunset/Orange
            groundColor = new Color(0.5f, 0.5f, 0.5f), // Gray/Stone Ground
            platformColor = new Color(0.4f, 0.4f, 0.6f), // Blue-ish platforms
            holeFrequency = 5, // Holes appear
            enemyFrequency = 12, // More enemies
            platformGap = 8, // Tighter jumps
            goalX = 150 // Longer level
        };
        CreateScene(settings);
    }

    // Stage 1-3
    [MenuItem("Tools/Create Stage 1-3")]
    public static void CreateStage1_3()
    {
        StageSettings settings = new StageSettings
        {
            stageName = "Stage 1-3",
            skyColor = new Color(0.1f, 0.1f, 0.3f), // Night Blue
            groundColor = new Color(0.2f, 0.2f, 0.4f), // Dark Ground
            platformColor = new Color(0.6f, 0.6f, 0.8f), // Pale platforms
            holeFrequency = 10, // More holes
            enemyFrequency = 10, // More enemies
            platformGap = 9, 
            goalX = 180 
        };
        CreateScene(settings);
    }

    // Stage 1-4
    [MenuItem("Tools/Create Stage 1-4")]
    public static void CreateStage1_4()
    {
        StageSettings settings = new StageSettings
        {
            stageName = "Stage 1-4",
            skyColor = new Color(0.3f, 0.0f, 0.0f), // Dark Red/Lava Sky
            groundColor = new Color(0.4f, 0.2f, 0.2f), // Reddish Ground
            platformColor = new Color(0.3f, 0.3f, 0.3f), // Dark platforms
            holeFrequency = 8, 
            enemyFrequency = 8, // Very frequent enemies
            platformGap = 11, // Far jumps
            goalX = 200 // Long stage
        };
        CreateScene(settings);
    }

    // Babel Tower
    [MenuItem("Tools/Create Babel Tower")]
    public static void CreateBabelTower()
    {
        StageSettings settings = new StageSettings
        {
            stageName = "Babel Tower",
            skyColor = new Color(0.1f, 0.1f, 0.2f), // Dark Blue
            groundColor = new Color(0.3f, 0.3f, 0.3f), // Stone
            platformColor = new Color(0.6f, 0.5f, 0.3f), // Wood/Stone
            holeFrequency = 0, 
            enemyFrequency = 3, // Y軸間隔
            platformGap = 3, // Y軸間隔
            goalX = 0, 
            goalY = 150 // 高さ
        };
        CreateTowerScene(settings);
    }

    private struct StageSettings
    {
        public string stageName;
        public Color skyColor;
        public Color groundColor;
        public Color platformColor;
        public int holeFrequency; // 0=none, higher=more holes
        public int enemyFrequency; // lower=more frequent
        public int platformGap;
        public float goalX;
        public float goalY;
    }

    private static void CreateScene(StageSettings settings)
    {
        CreateTag("Enemy");
        CreateTag("Goal");

        if (Camera.main != null)
        {
            Object.DestroyImmediate(Camera.main.gameObject);
        }
        
        // 3. Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f; 
        cam.backgroundColor = settings.skyColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 0, -10);

        // 4. Ground (地面) - 穴を作るために分割生成するか、一枚板にするか
        // 1-2で穴を作りたいので、ループで生成する方式に変えます
        CreateGround(settings);

        // 5. Platforms & Enemies
        for (int x = 10; x < settings.goalX - 10; x += settings.platformGap)
        {
            float yOffset = (x % 3) * 1.5f - 1f; 
            CreateSpriteObject($"Platform_{x}", settings.platformColor, new Vector3(x, yOffset, 0), new Vector3(4.5f, 1.5f, 1));

            if (x % settings.enemyFrequency == 5) 
            {
                 CreateEnemy(new Vector3(x, yOffset + 2f, 0));
            }
        }
        
        // 6. Player
        GameObject player = CreateSpriteObject("Player", Color.white, new Vector3(-5, -1, 0), Vector3.one * 1.5f);
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
        noFriction.friction = 0;
        player.GetComponent<BoxCollider2D>().sharedMaterial = noFriction;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;
        pc.jumpForce = 22f; 
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.6f, 0); 
        pc.groundCheck = groundCheck.transform;
        
        pc.groundLayer = -1; 

        CameraFollow cf = cameraObj.AddComponent<CameraFollow>();
        cf.target = player.transform;

        // 8. Goal
        GameObject goal = CreateSpriteObject("Goal", Color.yellow, new Vector3(settings.goalX, -2, 0), new Vector3(1, 4, 1));
        goal.tag = "Goal";
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<Goal>();

        // 9. UI setup (GameManager, Canvas...)
        SetupUI(settings);

        Debug.Log($"{settings.stageName} Created Successfully!");
    }

    private static void CreateTowerScene(StageSettings settings)
    {
        CreateTag("Enemy");
        CreateTag("Goal");

        if (Camera.main != null) Object.DestroyImmediate(Camera.main.gameObject);
        
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 10f; // 縦長なので視野広めに
        cam.backgroundColor = settings.skyColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cameraObj.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0, 5, -10); // スタート位置少し上

        // 左右の壁 (塔の壁)
        CreateSpriteObject("LeftWall", settings.groundColor, new Vector3(-12, settings.goalY / 2, 0), new Vector3(2, settings.goalY + 20, 1));
        CreateSpriteObject("RightWall", settings.groundColor, new Vector3(12, settings.goalY / 2, 0), new Vector3(2, settings.goalY + 20, 1));

        // 地面 (スタート地点)
        CreateSpriteObject("Ground", settings.groundColor, new Vector3(0, -4, 0), new Vector3(24, 2, 1));

        // 縦方向の足場
        for (float y = 0; y < settings.goalY; y += settings.platformGap)
        {
            // 足場のX座標をランダムに (壁の内側 -10 ~ 10)
            float x = Random.Range(-9f, 9f);
            
            // ランダム幅
            float width = Random.Range(3f, 6f);
            
            CreateSpriteObject($"Platform_{y}", settings.platformColor, new Vector3(x, y, 0), new Vector3(width, 1, 1));

            // 敵配置
            if ((int)y % settings.enemyFrequency == 0 && y > 5)
            {
                 CreateEnemy(new Vector3(x, y + 1.5f, 0));
            }
        }

        // 6. Player
        GameObject player = CreateSpriteObject("Player", Color.white, new Vector3(0, -2, 0), Vector3.one * 1.5f);
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 
        
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction");
        noFriction.friction = 0;
        player.GetComponent<BoxCollider2D>().sharedMaterial = noFriction;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 10f; // 左右移動は大事
        pc.jumpForce = 22f; 
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.6f, 0); 
        pc.groundCheck = groundCheck.transform;
        
        pc.groundLayer = -1; 

        CameraFollow cf = cameraObj.AddComponent<CameraFollow>();
        cf.target = player.transform;
        cf.scrollSpeed = 1.5f; // スクロール速度設定

        // 8. Goal
        GameObject goal = CreateSpriteObject("Goal", Color.yellow, new Vector3(0, settings.goalY + 2, 0), new Vector3(20, 1, 1)); // ゴールは横長バー
        goal.tag = "Goal";
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<Goal>();

        SetupUI(settings);
    }

    private static void CreateGround(StageSettings settings)
    {
        // 穴あき地面生成ロジック
        // -10からゴール先(+20)まで
        int startX = -10;
        int endX = (int)settings.goalX + 20;
        int currentX = startX;

        while (currentX < endX)
        {
            // 穴を作るかどうか
            bool makeHole = false;
            if (settings.holeFrequency > 0 && currentX > 10 && currentX < settings.goalX - 10)
            {
                // 固定周期で穴をあける簡易ロジック (例: holeFrequencyが5なら、ある程度ランダムではなく固定パタンでもよいが)
                // ここでは単純にランダム要素を入れると再現性がなくなるので、
                // X座標に基づいて穴を決める
                if (currentX % 30 < 5) // 30ブロックごとに5ブロック分の穴
                {
                    if (settings.holeFrequency >= 5) makeHole = true;
                }
            }

            if (makeHole)
            {
                currentX += 5; // 穴の幅
            }
            else
            {
                // 地面ブロック生成
                // 幅20のブロックを置く
                int width = 20;
                // 次の穴までの距離を確認して調整すべきだが、簡易的に重ねて配置
                CreateSpriteObject($"Ground_{currentX}", settings.groundColor, new Vector3(currentX + width/2f, -4, 0), new Vector3(width, 2, 1));
                
                // 地上の敵
                if (currentX > 5 && currentX < settings.goalX)
                {
                     if (currentX % settings.enemyFrequency * 2 == 0) // 適当な頻度
                     {
                         CreateEnemy(new Vector3(currentX, -1.5f, 0));
                     }
                }
                
                currentX += width;
            }
        }
    }

    private static void SetupUI(StageSettings settings)
    {
        GameManager gm = new GameObject("GameManager").AddComponent<GameManager>();
        
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

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
        CreateText(gameOverPanel.transform, "GAME OVER", 50, Color.red, 50);
        CreateButton(gameOverPanel.transform, "Restart", 50, gm.RestartGame);
        gm.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // Stage Clear Panel
        GameObject clearPanel = CreatePanel(canvasObj.transform, "StageClearPanel", Color.white, 0.8f);
        CreateText(clearPanel.transform, "STAGE CLEAR!", 50, Color.blue, 100);
        
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

        // Next Stage Button
        CreateButton(clearPanel.transform, "Next Stage", -50, gm.NextStage);
        // Restart Button (Retry)
        CreateButton(clearPanel.transform, "Replay", -120, gm.RestartGame);

        gm.stageClearPanel = clearPanel;
        clearPanel.SetActive(false);
        
        EditorUtility.SetDirty(gm);
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
        GameObject enemy = CreateSpriteObject("Enemy", Color.red, position, Vector3.one * 1.5f);
        enemy.tag = "Enemy";
        
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; 

        EnemyController ec = enemy.AddComponent<EnemyController>();
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = enemy.transform;
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
                if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return;
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

    // UnityActionを受け取るように変更
    private static Button CreateButton(Transform parent, string label, float yOffset, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        
        Button btn = btnObj.AddComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action); // 汎用化

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
