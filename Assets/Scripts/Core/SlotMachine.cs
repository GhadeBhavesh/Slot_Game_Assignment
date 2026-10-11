using System;
using System.Collections;
using System.Collections.Generic;
using SlotGame.Data;
using SlotGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.Core
{
    /// <summary>
    /// Main slot machine orchestrator. Coordinates betting, balance, spin execution,
    /// reel animations, RNG outcome determination, payout evaluation, and UI updates.
    /// </summary>
    public class SlotMachine : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Slot game configuration asset containing symbols, balance, and timing rules.")]
        [SerializeField] private SlotGameConfig config;

        [Header("Reel Columns")]
        [Tooltip("The three reel controllers (left to right).")]
        [SerializeField] private ReelController[] reels;

        [Header("UI Controller")]
        [Tooltip("UI manager managing text labels and interactive buttons.")]
        [SerializeField] private SlotMachineUI ui;

        [Header("Cabinet Artwork")]
        [Tooltip("Image component displaying the permanent slot machine cabinet.")]
        [SerializeField] private Image cabinetImage;

        [Header("Cabinet / Lever Animation (Optional)")]
        [Tooltip("Image component displaying the animated lever overlay.")]
        [SerializeField] private Image leverImage;

        [Tooltip("Lever sprite upright / idle.")]
        [SerializeField] private Sprite leverUpSprite;

        [Tooltip("Lever sprite pulled midway (optional).")]
        [SerializeField] private Sprite leverMidSprite;

        [Tooltip("Lever sprite pulled fully down.")]
        [SerializeField] private Sprite leverDownSprite;

        [Tooltip("Cabinet base image sprite (hollow window).")]
        [SerializeField] private Sprite cabinetBaseSprite;

        [Tooltip("Reel window background sprite.")]
        [SerializeField] private Sprite reelWindowBgSprite;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spinSound;
        [SerializeField] private AudioClip winSound;

        private Coroutine _activeLeverRoutine;

        // Dependencies (injected or default)
        private IRNGService _rngService;
        private IPayoutCalculator _payoutCalculator;

        // Runtime state
        private int _balance;
        private int _currentBet;
        private bool _isSpinning;
        private Coroutine _activeSpinRoutine;

        public int Balance => _balance;
        public int CurrentBet => _currentBet;
        public bool IsSpinning => _isSpinning;
        public SlotGameConfig Config => config;

        private void Awake()
        {
            if (config == null)
            {
                config = Resources.Load<SlotGameConfig>("SlotGameConfig");
            }
            if (reels == null || reels.Length == 0)
            {
                reels = FindObjectsByType<ReelController>(FindObjectsSortMode.None);
                if (reels != null && reels.Length > 1)
                {
                    System.Array.Sort(reels, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
                }
            }
            if (ui == null)
            {
                ui = GetComponent<SlotMachineUI>() ?? FindFirstObjectByType<SlotMachineUI>();
            }

            _rngService = new RNGService();
            _payoutCalculator = new PayoutCalculator();
        }

        private void Start()
        {
            ValidateDependencies();
            SetupVisuals();
            InitializeGame();
        }

        private void OnEnable()
        {
            if (ui != null)
            {
                ui.OnSpinClicked += HandleSpinRequest;
                ui.OnBetUpClicked += HandleBetUp;
                ui.OnBetDownClicked += HandleBetDown;
            }
        }

        private void OnDisable()
        {
            if (ui != null)
            {
                ui.OnSpinClicked -= HandleSpinRequest;
                ui.OnBetUpClicked -= HandleBetUp;
                ui.OnBetDownClicked -= HandleBetDown;
            }

            // Cancel active lever animation and restore cabinet sprite
            if (_activeLeverRoutine != null)
            {
                StopCoroutine(_activeLeverRoutine);
                _activeLeverRoutine = null;
            }

            // Cancel active spin routine
            if (_activeSpinRoutine != null)
            {
                StopCoroutine(_activeSpinRoutine);
                _activeSpinRoutine = null;
                _isSpinning = false;
            }

            // Restore the lever to its default upright sprite
            if (leverImage != null && leverUpSprite != null && leverImage != cabinetImage)
            {
                leverImage.sprite = leverUpSprite;
                leverImage.enabled = true;
            }

            if (cabinetImage != null)
            {
                cabinetImage.enabled = true;
            }
        }

        /// <summary>
        /// Validates Inspector wiring and logs clear warnings if essential references are unassigned.
        /// </summary>
        private void ValidateDependencies()
        {
            if (config == null)
            {
                Debug.LogError("[SlotMachine] SlotGameConfig is not assigned in the Inspector!");
            }
            if (reels == null || reels.Length < 3)
            {
                Debug.LogError("[SlotMachine] At least 3 ReelControllers must be assigned!");
            }
            if (ui == null)
            {
                Debug.LogError("[SlotMachine] SlotMachineUI is not assigned in the Inspector!");
            }
        }

        /// <summary>
        /// Initializes balance, bet, reel symbols, and UI display.
        /// </summary>
        private void InitializeGame()
        {
            if (config == null) return;

            _balance = config.StartingBalance;
            _currentBet = config.DefaultBet;

            // Initialize each reel with the symbol catalog
            if (reels != null)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                    {
                        reels[i].Initialize(config.Symbols, config.SymbolHeight);
                        // Stagger initial displayed symbols for visual interest
                        if (config.Symbols.Count > 0)
                        {
                            int initialIndex = i % config.Symbols.Count;
                            reels[i].SetDisplayedSymbol(config.Symbols[initialIndex]);
                        }
                    }
                }
            }

            if (ui != null)
            {
                ui.UpdateBalance(_balance);
                ui.UpdateBet(_currentBet);
                ui.UpdateStatus("WELCOME! PRESS SPIN TO PLAY");
                ui.SetControlsInteractable(true);
            }

            SetLeverSprite(leverUpSprite);
        }

        /// <summary>
        /// Validates, repairs, and enforces pixel-perfect alignment for the cabinet, lever, and reel windows.
        /// Resolves issues where symbols appear outside the reels or the cabinet artwork disappears.
        /// </summary>
        public void SetupVisuals()
        {
            // 1. Ensure fallback sprites are loaded from Resources if unassigned
            if (cabinetBaseSprite == null)
            {
                cabinetBaseSprite = Resources.Load<Sprite>("cabinet_base") ?? Resources.Load<Sprite>("Sprites/cabinet_base");
            }
            if (leverUpSprite == null)
            {
                leverUpSprite = Resources.Load<Sprite>("lever_up") ?? Resources.Load<Sprite>("Sprites/lever_up");
            }
            if (leverMidSprite == null)
            {
                leverMidSprite = Resources.Load<Sprite>("slot-machine2") ?? Resources.Load<Sprite>("Sprites/slot-machine2");
            }
            if (leverDownSprite == null)
            {
                leverDownSprite = Resources.Load<Sprite>("slot-machine3") ?? Resources.Load<Sprite>("Sprites/slot-machine3");
            }
            if (reelWindowBgSprite == null)
            {
                reelWindowBgSprite = Resources.Load<Sprite>("reel_window_bg") ?? Resources.Load<Sprite>("Sprites/reel_window_bg");
            }

            // 2. Setup Cabinet GameObject with hollow-window base artwork
            if (cabinetImage == null)
            {
                var cabObj = GameObject.Find("Cabinet");
                if (cabObj != null) cabinetImage = cabObj.GetComponent<Image>();
            }

            if (cabinetImage != null)
            {
                cabinetImage.enabled = true;
                cabinetImage.gameObject.SetActive(true);
                if (cabinetBaseSprite != null)
                {
                    cabinetImage.sprite = cabinetBaseSprite;
                }
            }

            // 3. Deactivate obsolete MiddleBoxFrame
            var middleBox = GameObject.Find("MiddleBoxFrame");
            if (middleBox != null)
            {
                middleBox.SetActive(false);
            }

            Transform cabinetTransform = cabinetImage != null ? cabinetImage.transform : (transform.Find("Cabinet") ?? GameObject.Find("Cabinet")?.transform);

            // 4. Setup ReelWindowBackground behind the reels
            if (cabinetTransform != null)
            {
                Transform bgChild = cabinetTransform.Find("ReelWindowBg");
                if (bgChild == null)
                {
                    GameObject bgGo = new GameObject("ReelWindowBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    bgGo.transform.SetParent(cabinetTransform, false);
                    bgChild = bgGo.transform;
                }

                RectTransform bgRt = bgChild.GetComponent<RectTransform>();
                bgRt.anchorMin = new Vector2(0.5f, 0.5f);
                bgRt.anchorMax = new Vector2(0.5f, 0.5f);
                bgRt.pivot = new Vector2(0.5f, 0.5f);
                bgRt.anchoredPosition = new Vector2(4.5f, -38.5f);
                bgRt.sizeDelta = new Vector2(368f, 210f);

                Image bgImg = bgChild.GetComponent<Image>();
                bgImg.raycastTarget = false;
                if (reelWindowBgSprite != null)
                {
                    bgImg.sprite = reelWindowBgSprite;
                    bgImg.color = Color.white;
                }
                else
                {
                    bgImg.color = new Color(0.85f, 0.95f, 0.97f, 1f);
                }
                bgChild.SetSiblingIndex(0); // Place under ReelPanel
            }

            // 5. Align ReelPanel precisely with the machine's visible reel window (width=368, height=210, pos=(4.5, -38.5))
            var reelPanelObj = GameObject.Find("ReelPanel");
            if (reelPanelObj != null)
            {
                RectTransform panelRt = reelPanelObj.GetComponent<RectTransform>();
                if (panelRt != null)
                {
                    panelRt.anchorMin = new Vector2(0.5f, 0.5f);
                    panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                    panelRt.pivot = new Vector2(0.5f, 0.5f);
                    panelRt.anchoredPosition = new Vector2(4.5f, -38.5f);
                    panelRt.sizeDelta = new Vector2(368f, 210f);
                }

                if (cabinetTransform != null && reelPanelObj.transform.parent != cabinetTransform)
                {
                    reelPanelObj.transform.SetParent(cabinetTransform, false);
                }
                reelPanelObj.transform.SetSiblingIndex(1); // Above ReelWindowBg
            }

            // 6. Align the three individual reels and guarantee clipping via RectMask2D
            if (reels != null && reels.Length >= 3)
            {
                float[] xOffsets = { -130f, 0f, 130f };
                for (int i = 0; i < 3; i++)
                {
                    if (reels[i] != null)
                    {
                        RectTransform rRt = reels[i].GetComponent<RectTransform>();
                        if (rRt != null)
                        {
                            rRt.anchorMin = new Vector2(0.5f, 0.5f);
                            rRt.anchorMax = new Vector2(0.5f, 0.5f);
                            rRt.pivot = new Vector2(0.5f, 0.5f);
                            rRt.anchoredPosition = new Vector2(xOffsets[i], 0f);
                            rRt.sizeDelta = new Vector2(108f, 210f);
                        }

                        if (reels[i].GetComponent<RectMask2D>() == null)
                        {
                            reels[i].gameObject.AddComponent<RectMask2D>();
                        }
                    }
                }
            }

            // 7. Decouple Lever as a dedicated overlay with interactive pull support
            if (cabinetTransform != null)
            {
                Transform leverChild = cabinetTransform.Find("Lever");
                if (leverChild == null)
                {
                    GameObject leverGo = new GameObject("Lever", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                    leverGo.transform.SetParent(cabinetTransform, false);
                    leverChild = leverGo.transform;
                }

                RectTransform rt = leverChild.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(816f, 624f);

                Image img = leverChild.GetComponent<Image>();
                img.raycastTarget = true;
                leverImage = img;

                Button leverBtn = leverChild.GetComponent<Button>();
                if (leverBtn != null)
                {
                    leverBtn.transition = Selectable.Transition.None;
                    leverBtn.onClick.RemoveListener(HandleSpinRequest);
                    leverBtn.onClick.AddListener(HandleSpinRequest);
                }

                leverChild.SetAsLastSibling();
            }

            // 8. Ensure lever sprite is set to upright idle
            if (leverImage != null && leverUpSprite != null && leverImage != cabinetImage)
            {
                leverImage.sprite = leverUpSprite;
                leverImage.enabled = true;
            }
        }

        /// <summary>
        /// Dependency injection hook for automated tests or custom RNG / payout logic.
        /// </summary>
        public void SetServices(IRNGService rngService, IPayoutCalculator payoutCalculator)
        {
            _rngService = rngService ?? new RNGService();
            _payoutCalculator = payoutCalculator ?? new PayoutCalculator();
        }

        private void HandleSpinRequest()
        {
            if (_isSpinning) return;

            if (_balance < _currentBet)
            {
                if (ui != null)
                    ui.UpdateStatus("INSUFFICIENT BALANCE! LOWER YOUR BET");
                return;
            }

            StartSpin();
        }

        private void HandleBetUp()
        {
            if (_isSpinning || config == null) return;

            if (_currentBet + config.BetStep <= config.MaxBet)
            {
                _currentBet += config.BetStep;
                if (ui != null) ui.UpdateBet(_currentBet);
            }
        }

        private void HandleBetDown()
        {
            if (_isSpinning || config == null) return;

            if (_currentBet - config.BetStep >= config.MinBet)
            {
                _currentBet -= config.BetStep;
                if (ui != null) ui.UpdateBet(_currentBet);
            }
        }

        /// <summary>
        /// Executes a full spin sequence with randomized outcome.
        /// </summary>
        public void StartSpin()
        {
            if (_isSpinning || config == null || reels == null) return;

            int[] outcome = _rngService.GenerateOutcome(reels.Length, config.Symbols);
            ExecuteSpinRoutine(outcome);
        }

        /// <summary>
        /// Test-only method allowing deterministic execution with specified outcomes for automated validation.
        /// </summary>
        public void TestSpinWithOutcome(int[] forcedOutcome)
        {
            if (_isSpinning || config == null || reels == null) return;
            ExecuteSpinRoutine(forcedOutcome);
        }

        private void ExecuteSpinRoutine(int[] outcome)
        {
            if (_activeSpinRoutine != null)
            {
                StopCoroutine(_activeSpinRoutine);
            }

            _activeSpinRoutine = StartCoroutine(SpinSequence(outcome));
        }

        private IEnumerator SpinSequence(int[] outcome)
        {
            _isSpinning = true;

            try
            {
                // 1. Deduct bet stake exactly once
                _balance -= _currentBet;
                if (ui != null)
                {
                    ui.UpdateBalance(_balance);
                    ui.UpdateStatus("SPINNING...");
                    ui.SetControlsInteractable(false);
                }

                // 2. Play spin sound and lever pull animation
                PlayAudio(spinSound);
                if (_activeLeverRoutine != null)
                    StopCoroutine(_activeLeverRoutine);
                _activeLeverRoutine = StartCoroutine(AnimateLeverPull());

                // 3. Pre-evaluate the final outcome using PayoutCalculator
                SlotResult result = _payoutCalculator.Evaluate(outcome, config.Symbols, _currentBet);

                // 4. Trigger reel spins with staggered stop delays
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                    {
                        float stopDuration = config.BaseSpinDuration + (i * config.ReelStopStagger);
                        reels[i].StartSpin(outcome[i], stopDuration, config.ReelSpinSpeed);
                    }
                }

                // 5. Wait until all reels have completed their spin animation
                bool allReelsStopped = false;
                while (!allReelsStopped)
                {
                    allReelsStopped = true;
                    for (int i = 0; i < reels.Length; i++)
                    {
                        if (reels[i] != null && reels[i].IsSpinning)
                        {
                            allReelsStopped = false;
                            break;
                        }
                    }
                    yield return null;
                }

                // 6. Process evaluated outcome
                if (result.IsWin)
                {
                    _balance += result.Payout;
                    PlayAudio(winSound);

                    if (ui != null)
                    {
                        ui.UpdateBalance(_balance);
                        string winMsg = $"MATCH! 3x {result.WinningSymbol.DisplayName}! +{result.Payout} COINS";
                        ui.UpdateStatus(winMsg);
                        ui.ShowWinPopup(result.Payout, result.WinningSymbol.DisplayName);
                    }
                }
                else
                {
                    if (ui != null)
                    {
                        ui.UpdateStatus("NO MATCH — TRY AGAIN!");
                    }
                }
            }
            finally
            {
                // 7. Guarantee state restoration regardless of early coroutine exits
                _isSpinning = false;
                _activeSpinRoutine = null;

                if (ui != null)
                {
                    ui.SetControlsInteractable(true);
                }
            }
        }

        private IEnumerator AnimateLeverPull()
        {
            if (leverImage == null) yield break;

            try
            {
                // 1. Midway pull down
                if (leverMidSprite != null)
                {
                    SetLeverSprite(leverMidSprite);
                    yield return new WaitForSeconds(0.09f);
                }

                // 2. Full down pull
                if (leverDownSprite != null)
                {
                    SetLeverSprite(leverDownSprite);
                    yield return new WaitForSeconds(0.18f);
                }

                // 3. Midway release up
                if (leverMidSprite != null)
                {
                    SetLeverSprite(leverMidSprite);
                    yield return new WaitForSeconds(0.09f);
                }
            }
            finally
            {
                // 4. Return to upright resting position
                if (leverUpSprite != null)
                {
                    SetLeverSprite(leverUpSprite);
                }

                if (cabinetImage != null)
                {
                    cabinetImage.enabled = true;
                }
                _activeLeverRoutine = null;
            }
        }

        private void SetLeverSprite(Sprite sprite)
        {
            if (leverImage != null && sprite != null)
            {
                leverImage.sprite = sprite;
                leverImage.enabled = true;
            }
        }

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
