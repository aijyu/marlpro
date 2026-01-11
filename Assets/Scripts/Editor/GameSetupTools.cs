using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Events;

public class GameSetupTools
{
    [MenuItem("Tools/Create 2D Game Scene")]
    public static void SetupScene()
    {
        // 0. Sprite Generation
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        // 1 Unity Unit = 1 Pixel (using 1.0f)
        Sprite whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1.0f);

        // 0b. Physics Material
        PhysicsMaterial2D noFrictionMat = new PhysicsMaterial2D("NoFriction");
        noFrictionMat.friction = 0f;
        noFrictionMat.bounciness = 0f;

        // 1. Camera
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 6; // Zoom out slightly

        // 2. Ground (Extended)
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(10, -3, 0); // Shifted right
        ground.transform.localScale = new Vector3(40, 1, 1); // Longer
        SpriteRenderer groundSr = ground.AddComponent<SpriteRenderer>();
        groundSr.sprite = whiteSprite;
        groundSr.color = new Color(0.3f, 0.7f, 0.3f); 
        ground.AddComponent<BoxCollider2D>();
        ground.layer = LayerMask.NameToLayer("Default");

        // 2b. Platforms
        CreatePlatform(new Vector3(3, -0.5f, 0), new Vector3(4, 0.5f, 1), whiteSprite);
        CreatePlatform(new Vector3(10, 1f, 0), new Vector3(3, 0.5f, 1), whiteSprite);
        CreatePlatform(new Vector3(18, 0f, 0), new Vector3(3, 0.5f, 1), whiteSprite);
        CreatePlatform(new Vector3(24, -1f, 0), new Vector3(4, 0.5f, 1), whiteSprite);

        // 2c. Walls
        GameObject wallLeft = new GameObject("WallLeft");
        wallLeft.transform.position = new Vector3(-9, 0, 0); 
        wallLeft.transform.localScale = new Vector3(1, 10, 1);
        SpriteRenderer wallLSr = wallLeft.AddComponent<SpriteRenderer>();
        wallLSr.sprite = whiteSprite;
        wallLSr.color = new Color(0.5f, 0.5f, 0.5f);
        wallLeft.AddComponent<BoxCollider2D>();
        wallLeft.layer = LayerMask.NameToLayer("Default");

        // 3. Player
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(-5, -1, 0);
        player.layer = 2; // Ignore Raycast

        SpriteRenderer playerSr = player.AddComponent<SpriteRenderer>();
        playerSr.sprite = whiteSprite;
        playerSr.color = Color.white; 

        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.freezeRotation = true;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.sharedMaterial = noFrictionMat;
        
        PlayerController playerCtrl = player.AddComponent<PlayerController>();
        playerCtrl.moveSpeed = 8f;
        playerCtrl.jumpForce = 15f; 
        
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.parent = player.transform;
        groundCheck.transform.localPosition = new Vector3(0, -0.55f, 0); 
        playerCtrl.groundCheck = groundCheck.transform;
        playerCtrl.groundLayer = 1 << 0; 
        player.tag = "Player";

        CameraFollow camFollow = Camera.main.gameObject.AddComponent<CameraFollow>();
        camFollow.target = player.transform;

        // 4. Enemy
        CreateEnemy(new Vector3(5, -2, 0), whiteSprite, noFrictionMat);
        CreateEnemy(new Vector3(15, -2, 0), whiteSprite, noFrictionMat);
        CreateEnemy(new Vector3(10, 2.5f, 0), whiteSprite, noFrictionMat); // On platform

        // 5. Goal
        GameObject goal = new GameObject("Goal");
        goal.transform.position = new Vector3(28, -2f, 0);
        goal.transform.localScale = new Vector3(1, 4, 1); // Pole
        SpriteRenderer goalSr = goal.AddComponent<SpriteRenderer>();
        goalSr.sprite = whiteSprite;
        goalSr.color = new Color(1f, 0.9f, 0f); // Yellow
        BoxCollider2D goalCol = goal.AddComponent<BoxCollider2D>();
        goalCol.isTrigger = true;
        goal.AddComponent<Goal>();

        // 6. Manager
        GameObject gm = new GameObject("GameManager");
        GameManager gmScript = gm.AddComponent<GameManager>();

        // 7. UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        Font uiFont = Font.CreateDynamicFontFromOSFont("Arial", 24);

        // Score
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(canvasObj.transform, false);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = uiFont;
        scoreText.fontSize = 32;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.color = Color.black; // Visible against sky
        scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
        scoreText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);
        scoreRect.anchoredPosition = new Vector2(20, -20);
        gmScript.scoreText = scoreText;

        // UI Builder Helper
        gmScript.gameOverPanel = CreatePanel(canvasObj, "GameOverPanel", "GAME OVER", Color.black, uiFont, gmScript.RestartScene);
        gmScript.gameClearPanel = CreatePanel(canvasObj, "GameClearPanel", "STAGE CLEAR!", new Color(1f, 0.8f, 0f, 0.8f), uiFont, gmScript.RestartScene);

        Debug.Log("Level Extension Setup Complete!");
    }

    private static void CreatePlatform(Vector3 pos, Vector3 scale, Sprite sprite)
    {
        GameObject plat = new GameObject("Platform");
        plat.transform.position = pos;
        plat.transform.localScale = scale;
        SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.6f, 0.4f, 0.2f); // Brown
        plat.AddComponent<BoxCollider2D>();
        plat.layer = LayerMask.NameToLayer("Default");
    }

    private static void CreateEnemy(Vector3 pos, Sprite sprite, PhysicsMaterial2D mat)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = pos;
        enemy.layer = 0; 
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.red; 
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.sharedMaterial = mat;
        
        EnemyController ctrl = enemy.AddComponent<EnemyController>();
        GameObject wallCheck = new GameObject("WallCheck");
        wallCheck.transform.parent = enemy.transform;
        wallCheck.transform.localPosition = new Vector3(0.8f, 0, 0); 
        ctrl.wallCheck = wallCheck.transform;
        ctrl.collisionLayer = 1 << 0; 
    }

    private static GameObject CreatePanel(GameObject canvas, string name, string label, Color bgColor, Font font, UnityEngine.Events.UnityAction action)
    {
        GameObject panelObj = new GameObject(name);
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = bgColor;
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Title");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = font;
        text.text = label;
        text.fontSize = 64;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(panelObj.transform, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = Color.white;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -80);
        btnRect.sizeDelta = new Vector2(200, 50);

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = font;
        btnText.text = "REPLAY";
        btnText.fontSize = 24;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.black;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        UnityEventTools.AddPersistentListener(btn.onClick, action);
        panelObj.SetActive(false);
        return panelObj;
    }
}
