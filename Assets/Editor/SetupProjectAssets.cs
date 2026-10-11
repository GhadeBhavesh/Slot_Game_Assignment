#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using SlotGame.Core;
using SlotGame.Data;
using SlotGame.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SetupProjectAssets
{
    private const string SpritesFolder = "Assets/Sprites";
    private const string DataFolder = "Assets/Data";
    private const string ScenesFolder = "Assets/Scenes";
    private const string ScenePath = "Assets/Scenes/SlotGame.unity";

    [MenuItem("SlotGame/1. Configure All Sprites")]
    public static void ConfigureAllSprites()
    {
        if (!Directory.Exists(SpritesFolder))
        {
            Debug.LogError("Assets/Sprites folder not found.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            string fileName = Path.GetFileName(path);
            if (fileName.StartsWith("slot_machine_buttons-"))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                SpriteMetaData[] metas = new SpriteMetaData[4];
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string[] stateNames = { "Normal", "Highlighted", "Pressed", "Disabled" };
                for (int i = 0; i < 4; i++)
                {
                    metas[i] = new SpriteMetaData
                    {
                        name = $"{baseName}_{stateNames[i]}",
                        rect = new Rect(0, (3 - i) * 256, 256, 256),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                }
                importer.spritesheet = metas;
            }
            else
            {
                importer.spriteImportMode = SpriteImportMode.Single;
            }

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished configuring all sprites!");
    }

    [MenuItem("SlotGame/2. Create Game Data Assets")]
    public static SlotGameConfig CreateDataAssets()
    {
        if (!Directory.Exists(DataFolder))
        {
            Directory.CreateDirectory(DataFolder);
            AssetDatabase.Refresh();
        }

        // Load Symbol Sprites
        Sprite s1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-symbol1.png");
        Sprite s2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-symbol2.png");
        Sprite s3Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-symbol3.png");
        Sprite s4Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-symbol4.png");

        // Create 4 Symbol ScriptableObjects
        SymbolData sym1 = CreateOrUpdateSymbol("Symbol_Cherry", "Cherry", s1Sprite, 5, 10);
        SymbolData sym2 = CreateOrUpdateSymbol("Symbol_Bell", "Bell", s2Sprite, 10, 8);
        SymbolData sym3 = CreateOrUpdateSymbol("Symbol_Bar", "Bar", s3Sprite, 25, 5);
        SymbolData sym4 = CreateOrUpdateSymbol("Symbol_Seven", "Seven", s4Sprite, 50, 2);

        // Create SlotGameConfig Asset
        string configPath = $"{DataFolder}/SlotGameConfig.asset";
        SlotGameConfig config = AssetDatabase.LoadAssetAtPath<SlotGameConfig>(configPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<SlotGameConfig>();
            AssetDatabase.CreateAsset(config, configPath);
        }

        List<SymbolData> symbolList = new List<SymbolData> { sym1, sym2, sym3, sym4 };
        config.SetSymbols(symbolList);
        EditorUtility.SetDirty(config);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Game Data Assets successfully!");
        return config;
    }

    private static SymbolData CreateOrUpdateSymbol(string assetName, string displayName, Sprite sprite, int multiplier, int weight)
    {
        string path = $"{DataFolder}/{assetName}.asset";
        SymbolData asset = AssetDatabase.LoadAssetAtPath<SymbolData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SymbolData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.Initialize(displayName, displayName, sprite, multiplier, weight);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    [MenuItem("SlotGame/3. Build Slot Game Scene")]
    public static void BuildSlotGameScene()
    {
        ConfigureAllSprites();
        SlotGameConfig config = CreateDataAssets();
        TMP_FontAsset defaultFont = GetDefaultFont();
        Debug.Log($"[SetupProjectAssets] Loaded default font: {(defaultFont != null ? defaultFont.name : "NULL")}");

        // Load Sprites
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bg_gradient.png");
        Sprite cabinetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cabinet_base.png") ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-machine1.png");
        Sprite leverUpSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/lever_up.png");
        Sprite leverMidSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-machine2.png");
        Sprite leverDownSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/slot-machine3.png");
        Sprite reelBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/reel_window_bg.png");
        Sprite popupSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/popup.png");

        // Create New Empty Scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Camera
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();

        // 2. Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // 3. EventSystem
        GameObject eventSysGo = new GameObject("EventSystem");
        eventSysGo.AddComponent<EventSystem>();
        eventSysGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // 4. Background
        GameObject bgGo = CreateUIObject("Background", canvasGo.transform);
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        SetStretchAll(bgRect);
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = Color.white;

        // 5. Header Title
        GameObject headerGo = CreateUIObject("HeaderTitle", canvasGo.transform);
        RectTransform headerRect = headerGo.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -35);
        headerRect.sizeDelta = new Vector2(900, 60);
        TMP_Text headerText = headerGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) headerText.font = defaultFont;
        headerText.text = "CLASSIC 3-REEL SLOTS";
        headerText.fontSize = 44;
        headerText.fontStyle = FontStyles.Bold;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = new Color(1f, 0.84f, 0.2f); // Gold

        // 6. Slot Machine Cabinet Root
        GameObject cabinetGo = CreateUIObject("Cabinet", canvasGo.transform);
        RectTransform cabinetRect = cabinetGo.GetComponent<RectTransform>();
        cabinetRect.anchorMin = new Vector2(0.5f, 0.5f);
        cabinetRect.anchorMax = new Vector2(0.5f, 0.5f);
        cabinetRect.pivot = new Vector2(0.5f, 0.5f);
        cabinetRect.anchoredPosition = new Vector2(0, 50);
        cabinetRect.sizeDelta = new Vector2(816, 624);
        Image cabinetImg = cabinetGo.AddComponent<Image>();
        cabinetImg.sprite = cabinetSprite;

        // 7. Reel Window Background (Layer 0 behind reels)
        GameObject reelBgGo = CreateUIObject("ReelWindowBg", cabinetGo.transform);
        RectTransform rbgRect = reelBgGo.GetComponent<RectTransform>();
        rbgRect.anchorMin = new Vector2(0.5f, 0.5f);
        rbgRect.anchorMax = new Vector2(0.5f, 0.5f);
        rbgRect.pivot = new Vector2(0.5f, 0.5f);
        rbgRect.anchoredPosition = new Vector2(4.5f, -38.5f);
        rbgRect.sizeDelta = new Vector2(368, 210);
        Image rbgImg = reelBgGo.AddComponent<Image>();
        rbgImg.sprite = reelBgSprite;
        rbgImg.raycastTarget = false;

        // 8. Reel Panel (Layer 1 holds 3 reels)
        GameObject reelPanelGo = CreateUIObject("ReelPanel", cabinetGo.transform);
        RectTransform rpRect = reelPanelGo.GetComponent<RectTransform>();
        rpRect.anchorMin = new Vector2(0.5f, 0.5f);
        rpRect.anchorMax = new Vector2(0.5f, 0.5f);
        rpRect.pivot = new Vector2(0.5f, 0.5f);
        rpRect.anchoredPosition = new Vector2(4.5f, -38.5f);
        rpRect.sizeDelta = new Vector2(368, 210);

        ReelController[] reelControllers = new ReelController[3];
        float[] reelXPositions = { -130f, 0f, 130f };

        for (int i = 0; i < 3; i++)
        {
            GameObject reelGo = CreateUIObject($"Reel_{i + 1}", reelPanelGo.transform);
            RectTransform rRect = reelGo.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(reelXPositions[i], 0);
            rRect.sizeDelta = new Vector2(108, 210);
            reelGo.AddComponent<RectMask2D>();

            // Strip container
            GameObject stripGo = CreateUIObject("Strip", reelGo.transform);
            RectTransform stripRect = stripGo.GetComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0.5f, 0.5f);
            stripRect.anchorMax = new Vector2(0.5f, 0.5f);
            stripRect.pivot = new Vector2(0.5f, 0.5f);
            stripRect.anchoredPosition = Vector2.zero;
            stripRect.sizeDelta = new Vector2(108, 300);

            // 3 symbol slots: Top (+96), Center (0), Bottom (-96)
            Image topImg = CreateSymbolSlot("TopSymbol", stripGo.transform, 96f, config.Symbols[0].Icon);
            Image centerImg = CreateSymbolSlot("CenterSymbol", stripGo.transform, 0f, config.Symbols[i % config.Symbols.Count].Icon);
            Image bottomImg = CreateSymbolSlot("BottomSymbol", stripGo.transform, -96f, config.Symbols[(i + 1) % config.Symbols.Count].Icon);

            ReelController rc = reelGo.AddComponent<ReelController>();
            SerializedObject soRc = new SerializedObject(rc);
            soRc.FindProperty("stripContainer").objectReferenceValue = stripRect;
            soRc.FindProperty("centerSymbolImage").objectReferenceValue = centerImg;
            soRc.FindProperty("topSymbolImage").objectReferenceValue = topImg;
            soRc.FindProperty("bottomSymbolImage").objectReferenceValue = bottomImg;
            soRc.ApplyModifiedPropertiesWithoutUndo();

            reelControllers[i] = rc;
        }

        // 9. Interactive Lever Overlay (Layer 2 on top of cabinet)
        GameObject leverGo = CreateUIObject("Lever", cabinetGo.transform);
        RectTransform leverRect = leverGo.GetComponent<RectTransform>();
        leverRect.anchorMin = new Vector2(0.5f, 0.5f);
        leverRect.anchorMax = new Vector2(0.5f, 0.5f);
        leverRect.pivot = new Vector2(0.5f, 0.5f);
        leverRect.anchoredPosition = Vector2.zero;
        leverRect.sizeDelta = new Vector2(816, 624);
        Image leverImg = leverGo.AddComponent<Image>();
        leverImg.sprite = leverUpSprite;
        leverImg.raycastTarget = true;
        Button leverBtn = leverGo.AddComponent<Button>();
        leverBtn.transition = Selectable.Transition.None;

        // 9. Marquee Status Message Banner
        GameObject statusGo = CreateUIObject("StatusBanner", canvasGo.transform);
        RectTransform statusRect = statusGo.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0, -260);
        statusRect.sizeDelta = new Vector2(720, 50);

        Image statusBg = statusGo.AddComponent<Image>();
        statusBg.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject statusTextGo = CreateUIObject("StatusText", statusGo.transform);
        RectTransform stRect = statusTextGo.GetComponent<RectTransform>();
        SetStretchAll(stRect);
        TMP_Text statusText = statusTextGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) statusText.font = defaultFont;
        statusText.text = "WELCOME! PRESS SPIN TO PLAY";
        statusText.fontSize = 26;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = new Color(1f, 0.95f, 0.5f);

        // 10. Bottom Dashboard / Control Bar
        GameObject controlBarGo = CreateUIObject("ControlBar", canvasGo.transform);
        RectTransform cbRect = controlBarGo.GetComponent<RectTransform>();
        cbRect.anchorMin = new Vector2(0.5f, 0.5f);
        cbRect.anchorMax = new Vector2(0.5f, 0.5f);
        cbRect.pivot = new Vector2(0.5f, 0.5f);
        cbRect.anchoredPosition = new Vector2(0, -360);
        cbRect.sizeDelta = new Vector2(850, 100);

        // Balance Card
        GameObject balanceCardGo = CreateUIObject("BalanceCard", controlBarGo.transform);
        RectTransform balCardRect = balanceCardGo.GetComponent<RectTransform>();
        balCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        balCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        balCardRect.pivot = new Vector2(0.5f, 0.5f);
        balCardRect.anchoredPosition = new Vector2(-280, 0);
        balCardRect.sizeDelta = new Vector2(240, 70);
        Image balBg = balanceCardGo.AddComponent<Image>();
        balBg.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);

        GameObject balTextGo = CreateUIObject("BalanceText", balanceCardGo.transform);
        RectTransform btRect = balTextGo.GetComponent<RectTransform>();
        SetStretchAll(btRect);
        TMP_Text balanceText = balTextGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) balanceText.font = defaultFont;
        balanceText.text = "BALANCE: 1000";
        balanceText.fontSize = 24;
        balanceText.fontStyle = FontStyles.Bold;
        balanceText.alignment = TextAlignmentOptions.Center;
        balanceText.color = Color.white;

        // Bet Card
        GameObject betCardGo = CreateUIObject("BetCard", controlBarGo.transform);
        RectTransform betCardRect = betCardGo.GetComponent<RectTransform>();
        betCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        betCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        betCardRect.pivot = new Vector2(0.5f, 0.5f);
        betCardRect.anchoredPosition = new Vector2(0, 0);
        betCardRect.sizeDelta = new Vector2(260, 70);
        Image betBg = betCardGo.AddComponent<Image>();
        betBg.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);

        // Bet Minus Button
        Button betDownBtn = CreateSimpleButton("BetMinusBtn", betCardGo.transform, new Vector2(-85, 0), new Vector2(50, 50), "-", new Color(0.7f, 0.2f, 0.2f), 28, defaultFont);
        // Bet Text
        GameObject betTextGo = CreateUIObject("BetText", betCardGo.transform);
        RectTransform betTextRect = betTextGo.GetComponent<RectTransform>();
        betTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        betTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        betTextRect.pivot = new Vector2(0.5f, 0.5f);
        betTextRect.anchoredPosition = Vector2.zero;
        betTextRect.sizeDelta = new Vector2(100, 50);
        TMP_Text betText = betTextGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) betText.font = defaultFont;
        betText.text = "BET: 10";
        betText.fontSize = 24;
        betText.fontStyle = FontStyles.Bold;
        betText.alignment = TextAlignmentOptions.Center;
        betText.color = Color.white;
        // Bet Plus Button
        Button betUpBtn = CreateSimpleButton("BetPlusBtn", betCardGo.transform, new Vector2(85, 0), new Vector2(50, 50), "+", new Color(0.2f, 0.6f, 0.3f), 28, defaultFont);

        // Spin Button
        Button spinBtn = CreateSimpleButton("SpinButton", controlBarGo.transform, new Vector2(280, 0), new Vector2(180, 75), "SPIN", new Color(0.9f, 0.55f, 0.05f), 32, defaultFont);

        // 11. Paytable Toggle Button (Top Right)
        Button paytableToggleBtn = CreateSimpleButton("PaytableToggleBtn", canvasGo.transform, new Vector2(0, 0), new Vector2(160, 50), "PAYTABLE", new Color(0.25f, 0.35f, 0.6f), 20, defaultFont);
        RectTransform ptToggleRect = paytableToggleBtn.GetComponent<RectTransform>();
        ptToggleRect.anchorMin = new Vector2(1f, 1f);
        ptToggleRect.anchorMax = new Vector2(1f, 1f);
        ptToggleRect.pivot = new Vector2(1f, 1f);
        ptToggleRect.anchoredPosition = new Vector2(-40, -40);

        // 12. Paytable Modal Panel
        GameObject paytablePanelGo = CreateUIObject("PaytablePanel", canvasGo.transform);
        RectTransform ptRect = paytablePanelGo.GetComponent<RectTransform>();
        ptRect.anchorMin = new Vector2(0.5f, 0.5f);
        ptRect.anchorMax = new Vector2(0.5f, 0.5f);
        ptRect.pivot = new Vector2(0.5f, 0.5f);
        ptRect.anchoredPosition = Vector2.zero;
        ptRect.sizeDelta = new Vector2(650, 460);
        Image ptBg = paytablePanelGo.AddComponent<Image>();
        ptBg.color = new Color(0.06f, 0.08f, 0.15f, 0.96f);

        // Paytable Title
        GameObject ptTitleGo = CreateUIObject("Title", paytablePanelGo.transform);
        RectTransform ptTitleRect = ptTitleGo.GetComponent<RectTransform>();
        ptTitleRect.anchorMin = new Vector2(0.5f, 1f);
        ptTitleRect.anchorMax = new Vector2(0.5f, 1f);
        ptTitleRect.pivot = new Vector2(0.5f, 1f);
        ptTitleRect.anchoredPosition = new Vector2(0, -25);
        ptTitleRect.sizeDelta = new Vector2(500, 50);
        TMP_Text ptTitle = ptTitleGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) ptTitle.font = defaultFont;
        ptTitle.text = "PAYTABLE & RULES";
        ptTitle.fontSize = 32;
        ptTitle.fontStyle = FontStyles.Bold;
        ptTitle.alignment = TextAlignmentOptions.Center;
        ptTitle.color = new Color(1f, 0.85f, 0.2f);

        // Paytable Rows
        string[] ruleLines = {
            "3x SEVEN (Diamond)  ==>  50x BET",
            "3x BAR (Star)       ==>  25x BET",
            "3x BELL (Fruit)     ==>  10x BET",
            "3x CHERRY           ==>   5x BET",
            "\nWIN RULE: All 3 reels must match on the payline!"
        };
        GameObject ptBodyGo = CreateUIObject("RulesText", paytablePanelGo.transform);
        RectTransform ptBodyRect = ptBodyGo.GetComponent<RectTransform>();
        ptBodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        ptBodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        ptBodyRect.pivot = new Vector2(0.5f, 0.5f);
        ptBodyRect.anchoredPosition = new Vector2(0, 10);
        ptBodyRect.sizeDelta = new Vector2(580, 260);
        TMP_Text ptBody = ptBodyGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) ptBody.font = defaultFont;
        ptBody.text = string.Join("\n", ruleLines);
        ptBody.fontSize = 22;
        ptBody.alignment = TextAlignmentOptions.Center;
        ptBody.color = Color.white;

        // Paytable Close Button
        Button ptCloseBtn = CreateSimpleButton("CloseBtn", paytablePanelGo.transform, new Vector2(0, -180), new Vector2(150, 48), "CLOSE", new Color(0.8f, 0.25f, 0.25f), 22, defaultFont);

        // 13. Win Popup Dialog
        GameObject winPopupGo = CreateUIObject("WinPopup", canvasGo.transform);
        RectTransform wpRect = winPopupGo.GetComponent<RectTransform>();
        wpRect.anchorMin = new Vector2(0.5f, 0.5f);
        wpRect.anchorMax = new Vector2(0.5f, 0.5f);
        wpRect.pivot = new Vector2(0.5f, 0.5f);
        wpRect.anchoredPosition = Vector2.zero;
        wpRect.sizeDelta = new Vector2(700, 380);
        Image wpBg = winPopupGo.AddComponent<Image>();
        wpBg.sprite = popupSprite;

        GameObject wpTextGo = CreateUIObject("WinText", winPopupGo.transform);
        RectTransform wptRect = wpTextGo.GetComponent<RectTransform>();
        wptRect.anchorMin = new Vector2(0.5f, 0.5f);
        wptRect.anchorMax = new Vector2(0.5f, 0.5f);
        wptRect.pivot = new Vector2(0.5f, 0.5f);
        wptRect.anchoredPosition = new Vector2(0, 30);
        wptRect.sizeDelta = new Vector2(550, 160);
        TMP_Text wpText = wpTextGo.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) wpText.font = defaultFont;
        wpText.text = "BIG WIN!\n+500 COINS";
        wpText.fontSize = 36;
        wpText.fontStyle = FontStyles.Bold;
        wpText.alignment = TextAlignmentOptions.Center;
        wpText.color = new Color(1f, 0.9f, 0.2f);

        Button wpCloseBtn = CreateSimpleButton("CollectBtn", winPopupGo.transform, new Vector2(0, -110), new Vector2(180, 52), "COLLECT", new Color(0.9f, 0.55f, 0.05f), 24, defaultFont);

        // 14. SlotMachine Game Controller Object
        GameObject machineControllerGo = new GameObject("SlotMachine");
        SlotMachine slotMachine = machineControllerGo.AddComponent<SlotMachine>();
        SlotMachineUI slotMachineUi = machineControllerGo.AddComponent<SlotMachineUI>();

        // Wire SlotMachineUI
        SerializedObject soUi = new SerializedObject(slotMachineUi);
        soUi.FindProperty("balanceText").objectReferenceValue = balanceText;
        soUi.FindProperty("betText").objectReferenceValue = betText;
        soUi.FindProperty("statusText").objectReferenceValue = statusText;
        soUi.FindProperty("spinButton").objectReferenceValue = spinBtn;
        soUi.FindProperty("betDownButton").objectReferenceValue = betDownBtn;
        soUi.FindProperty("betUpButton").objectReferenceValue = betUpBtn;
        soUi.FindProperty("winPopupPanel").objectReferenceValue = winPopupGo;
        soUi.FindProperty("winPopupText").objectReferenceValue = wpText;
        soUi.FindProperty("winPopupCloseButton").objectReferenceValue = wpCloseBtn;
        soUi.FindProperty("paytablePanel").objectReferenceValue = paytablePanelGo;
        soUi.FindProperty("paytableToggleButton").objectReferenceValue = paytableToggleBtn;
        soUi.FindProperty("paytableCloseButton").objectReferenceValue = ptCloseBtn;
        soUi.ApplyModifiedPropertiesWithoutUndo();

        // Wire SlotMachine
        SerializedObject soSm = new SerializedObject(slotMachine);
        soSm.FindProperty("config").objectReferenceValue = config;
        SerializedProperty reelsProp = soSm.FindProperty("reels");
        reelsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            reelsProp.GetArrayElementAtIndex(i).objectReferenceValue = reelControllers[i];
        }
        soSm.FindProperty("ui").objectReferenceValue = slotMachineUi;
        soSm.FindProperty("cabinetImage").objectReferenceValue = cabinetImg;
        soSm.FindProperty("leverImage").objectReferenceValue = leverImg;
        soSm.FindProperty("leverUpSprite").objectReferenceValue = leverUpSprite;
        soSm.FindProperty("leverMidSprite").objectReferenceValue = leverMidSprite;
        soSm.FindProperty("leverDownSprite").objectReferenceValue = leverDownSprite;
        soSm.FindProperty("cabinetBaseSprite").objectReferenceValue = cabinetSprite;
        soSm.FindProperty("reelWindowBgSprite").objectReferenceValue = reelBgSprite;
        soSm.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(slotMachine);
        EditorUtility.SetDirty(slotMachineUi);
        EditorUtility.SetDirty(machineControllerGo);
        EditorSceneManager.MarkSceneDirty(scene);

        // Save Scene
        if (!Directory.Exists(ScenesFolder)) Directory.CreateDirectory(ScenesFolder);
        EditorSceneManager.SaveScene(scene, ScenePath);

        // Add to EditorBuildSettings as Scene 0
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully built and saved {ScenePath}!");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetStretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Image CreateSymbolSlot(string name, Transform parent, float yPos, Sprite sprite)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(96, 96);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        return img;
    }

    private static TMP_FontAsset GetDefaultFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }
        return font;
    }

    private static Button CreateSimpleButton(string name, Transform parent, Vector2 pos, Vector2 size, string text, Color bgColor, int fontSize = 24, TMP_FontAsset fontAsset = null)
    {
        GameObject btnGo = CreateUIObject(name, parent);
        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnGo.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textGo = CreateUIObject("Text", btnGo.transform);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        SetStretchAll(trt);
        TMP_Text tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font = fontAsset != null ? fontAsset : GetDefaultFont();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }
}
#endif
