using System;
using System.Collections;
using System.Collections.Generic;
using SlotGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.Core
{
    /// <summary>
    /// Controls a single slot reel's vertical scrolling animation, random symbol presentation during spin,
    /// smooth deceleration, mechanical bounce settling, and alignment on the predetermined target symbol.
    /// </summary>
    public class ReelController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The central visible image that displays the final landed symbol.")]
        [SerializeField] private Image centerSymbolImage;

        [Tooltip("Top symbol image (visible partially or during scrolling).")]
        [SerializeField] private Image topSymbolImage;

        [Tooltip("Bottom symbol image (visible partially or during scrolling).")]
        [SerializeField] private Image bottomSymbolImage;

        [Tooltip("Transform containing the symbol images that moves vertically during spin.")]
        [SerializeField] private RectTransform stripContainer;

        [Header("Audio & Feedback")]
        [Tooltip("AudioSource for playing reel stop click/thud.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Optional audio clip played when this reel snaps to a stop.")]
        [SerializeField] private AudioClip stopSound;

        // Runtime state
        private bool _isSpinning;
        private Coroutine _spinCoroutine;
        private IReadOnlyList<SymbolData> _symbols;
        private float _symbolHeight = 96f;

        public bool IsSpinning => _isSpinning;

        public event Action<ReelController> OnSpinCompleted;

        private void OnDisable()
        {
            // Cancel any running spin when the reel is disabled to prevent orphaned coroutines
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }
            _isSpinning = false;

            // Reset strip to neutral position so symbols don't appear offset
            if (stripContainer != null)
            {
                stripContainer.anchoredPosition = Vector2.zero;
            }
        }

        private void Awake()
        {
            if (GetComponent<RectMask2D>() == null)
            {
                gameObject.AddComponent<RectMask2D>();
            }

            if (stripContainer == null)
            {
                stripContainer = transform.Find("Strip")?.GetComponent<RectTransform>() ?? GetComponent<RectTransform>();
            }
            if (centerSymbolImage == null)
            {
                centerSymbolImage = transform.Find("Strip/CenterSymbol")?.GetComponent<Image>() ?? transform.Find("CenterSymbol")?.GetComponent<Image>();
            }
            if (topSymbolImage == null)
            {
                topSymbolImage = transform.Find("Strip/TopSymbol")?.GetComponent<Image>() ?? transform.Find("TopSymbol")?.GetComponent<Image>();
            }
            if (bottomSymbolImage == null)
            {
                bottomSymbolImage = transform.Find("Strip/BottomSymbol")?.GetComponent<Image>() ?? transform.Find("BottomSymbol")?.GetComponent<Image>();
            }

            // Ensure symbol sprites strictly maintain their 1:1 aspect ratio
            if (centerSymbolImage != null) centerSymbolImage.preserveAspect = true;
            if (topSymbolImage != null) topSymbolImage.preserveAspect = true;
            if (bottomSymbolImage != null) bottomSymbolImage.preserveAspect = true;
        }

        /// <summary>
        /// Initializes the reel with the symbol catalog and initial symbol display.
        /// </summary>
        public void Initialize(IReadOnlyList<SymbolData> symbols, float symbolHeight = 96f)
        {
            _symbols = symbols;
            _symbolHeight = symbolHeight > 0 ? symbolHeight : 96f;

            if (_symbols != null && _symbols.Count > 0)
            {
                SetDisplayedSymbol(_symbols[0]);
            }
        }

        /// <summary>
        /// Sets the symbol displayed on the center payline.
        /// </summary>
        public void SetDisplayedSymbol(SymbolData symbol)
        {
            if (symbol == null || symbol.Icon == null) return;

            if (centerSymbolImage != null)
            {
                centerSymbolImage.sprite = symbol.Icon;
                centerSymbolImage.enabled = true;
            }

            // Fill top and bottom with valid adjacent symbols for realistic visuals
            if (_symbols != null && _symbols.Count > 1)
            {
                int index = -1;
                for (int i = 0; i < _symbols.Count; i++)
                {
                    if (_symbols[i] == symbol) { index = i; break; }
                }

                if (index >= 0)
                {
                    int prevIndex = (index - 1 + _symbols.Count) % _symbols.Count;
                    int nextIndex = (index + 1) % _symbols.Count;

                    if (topSymbolImage != null && _symbols[prevIndex].Icon != null)
                        topSymbolImage.sprite = _symbols[prevIndex].Icon;

                    if (bottomSymbolImage != null && _symbols[nextIndex].Icon != null)
                        bottomSymbolImage.sprite = _symbols[nextIndex].Icon;
                }
            }
        }

        /// <summary>
        /// Starts the reel spinning toward the predetermined target symbol index.
        /// </summary>
        /// <param name="targetIndex">Index into the symbols collection determined by RNG before spin started.</param>
        /// <param name="spinDuration">Duration before this reel begins deceleration.</param>
        /// <param name="spinSpeed">Pixels-per-second scrolling speed.</param>
        public void StartSpin(int targetIndex, float spinDuration, float spinSpeed = 1400f)
        {
            if (_isSpinning)
            {
                Debug.LogWarning("Reel is already spinning. Ignoring duplicate spin call.");
                return;
            }

            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
            }

            _spinCoroutine = StartCoroutine(SpinRoutine(targetIndex, spinDuration, spinSpeed));
        }

        private IEnumerator SpinRoutine(int targetIndex, float spinDuration, float spinSpeed)
        {
            _isSpinning = true;

            try
            {
                if (_symbols == null || _symbols.Count == 0 || targetIndex < 0 || targetIndex >= _symbols.Count)
                {
                    Debug.LogError($"[ReelController] Invalid target symbol index {targetIndex}. Aborting spin.");
                    yield break;
                }

                SymbolData targetSymbol = _symbols[targetIndex];
                float elapsed = 0f;
                float currentY = 0f;
                float stepHeight = _symbolHeight;

                // Fast spinning phase: continuously cycle random intermediate symbols
                while (elapsed < spinDuration)
                {
                    float dt = Time.deltaTime;
                    elapsed += dt;
                    currentY -= spinSpeed * dt;

                    // As strip scrolls past one full symbol height, randomize intermediate presentation
                    if (currentY <= -stepHeight)
                    {
                        currentY += stepHeight;

                        int randTop = UnityEngine.Random.Range(0, _symbols.Count);
                        int randCenter = UnityEngine.Random.Range(0, _symbols.Count);
                        int randBottom = UnityEngine.Random.Range(0, _symbols.Count);

                        if (topSymbolImage != null && _symbols[randTop].Icon != null)
                            topSymbolImage.sprite = _symbols[randTop].Icon;
                        if (centerSymbolImage != null && _symbols[randCenter].Icon != null)
                            centerSymbolImage.sprite = _symbols[randCenter].Icon;
                        if (bottomSymbolImage != null && _symbols[randBottom].Icon != null)
                            bottomSymbolImage.sprite = _symbols[randBottom].Icon;
                    }

                    if (stripContainer != null)
                    {
                        stripContainer.anchoredPosition = new Vector2(0f, currentY);
                    }

                    yield return null;
                }

                // Deceleration and landing phase:
                // Pre-assign the exact predetermined target symbol to the center slot
                int prevTarget = (targetIndex - 1 + _symbols.Count) % _symbols.Count;
                int nextTarget = (targetIndex + 1) % _symbols.Count;

                if (centerSymbolImage != null) centerSymbolImage.sprite = targetSymbol.Icon;
                if (topSymbolImage != null) topSymbolImage.sprite = _symbols[prevTarget].Icon;
                if (bottomSymbolImage != null) bottomSymbolImage.sprite = _symbols[nextTarget].Icon;

                // Smooth deceleration into payline position
                float landDuration = 0.25f;
                float landElapsed = 0f;
                float startY = stripContainer != null ? stripContainer.anchoredPosition.y : 0f;

                while (landElapsed < landDuration)
                {
                    landElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(landElapsed / landDuration);
                    // Smooth ease out cubic
                    float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                    float y = Mathf.Lerp(startY, 0f, easeOut);

                    if (stripContainer != null)
                    {
                        stripContainer.anchoredPosition = new Vector2(0f, y);
                    }

                    yield return null;
                }

                // Mechanical settling bounce: bounce down slightly and snap firmly back
                float bounceDuration = 0.12f;
                float bounceElapsed = 0f;
                float bounceDepth = -10f;

                while (bounceElapsed < bounceDuration)
                {
                    bounceElapsed += Time.deltaTime;
                    float bt = Mathf.Clamp01(bounceElapsed / bounceDuration);
                    float bounceY = Mathf.Sin(bt * Mathf.PI) * bounceDepth;

                    if (stripContainer != null)
                    {
                        stripContainer.anchoredPosition = new Vector2(0f, bounceY);
                    }

                    yield return null;
                }

                // Final snap perfectly at 0
                if (stripContainer != null)
                {
                    stripContainer.anchoredPosition = Vector2.zero;
                }

                // Play stop sound feedback if audio source is assigned
                PlayStopSound();
            }
            finally
            {
                // Guarantee IsSpinning is reset on every exit path
                _isSpinning = false;
                _spinCoroutine = null;
                OnSpinCompleted?.Invoke(this);
            }
        }

        private void PlayStopSound()
        {
            if (audioSource != null && stopSound != null)
            {
                audioSource.PlayOneShot(stopSound);
            }
        }

        /// <summary>
        /// Forces immediate stop at the specified symbol without playing animation (useful for tests or scene init).
        /// </summary>
        public void ForceStop(SymbolData symbol)
        {
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }

            _isSpinning = false;
            if (stripContainer != null) stripContainer.anchoredPosition = Vector2.zero;
            SetDisplayedSymbol(symbol);
            OnSpinCompleted?.Invoke(this);
        }
    }
}
