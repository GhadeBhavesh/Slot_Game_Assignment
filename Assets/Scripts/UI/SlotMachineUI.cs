using System;
using System.Collections.Generic;
using SlotGame.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.UI
{
    /// <summary>
    /// UI manager for the slot machine. Updates balance, bet, and status labels,
    /// manages control interactability, and displays win dialogs.
    /// </summary>
    public class SlotMachineUI : MonoBehaviour
    {
        [Header("Text Displays")]
        [Tooltip("Text displaying player's current credit balance.")]
        [SerializeField] private TMP_Text balanceText;

        [Tooltip("Text displaying current bet stake.")]
        [SerializeField] private TMP_Text betText;

        [Tooltip("Text displaying win amount or status messages.")]
        [SerializeField] private TMP_Text statusText;

        [Header("Interactive Buttons")]
        [Tooltip("Primary button to trigger a spin.")]
        [SerializeField] private Button spinButton;

        [Tooltip("Button to decrease the bet amount.")]
        [SerializeField] private Button betDownButton;

        [Tooltip("Button to increase the bet amount.")]
        [SerializeField] private Button betUpButton;

        [Header("Win Popup Dialog")]
        [Tooltip("Modal window shown on large wins.")]
        [SerializeField] private GameObject winPopupPanel;

        [Tooltip("Text inside the win popup showing the payout.")]
        [SerializeField] private TMP_Text winPopupText;

        [Tooltip("Dismiss button inside the win popup.")]
        [SerializeField] private Button winPopupCloseButton;

        [Header("Paytable Display")]
        [Tooltip("Panel displaying symbol payouts and win rules.")]
        [SerializeField] private GameObject paytablePanel;

        [Tooltip("Button to toggle the paytable panel.")]
        [SerializeField] private Button paytableToggleButton;

        [Tooltip("Close button for the paytable panel.")]
        [SerializeField] private Button paytableCloseButton;

        // Events
        public event Action OnSpinClicked;
        public event Action OnBetUpClicked;
        public event Action OnBetDownClicked;

        private void Awake()
        {
            // Auto-resolve references from hierarchy if unassigned
            if (balanceText == null) balanceText = GameObject.Find("BalanceText")?.GetComponent<TMP_Text>();
            if (betText == null) betText = GameObject.Find("BetText")?.GetComponent<TMP_Text>();
            if (statusText == null) statusText = GameObject.Find("StatusText")?.GetComponent<TMP_Text>();
            if (spinButton == null) spinButton = GameObject.Find("SpinButton")?.GetComponent<Button>();
            if (betUpButton == null) betUpButton = GameObject.Find("BetPlusBtn")?.GetComponent<Button>();
            if (betDownButton == null) betDownButton = GameObject.Find("BetMinusBtn")?.GetComponent<Button>();
            if (winPopupPanel == null) winPopupPanel = GameObject.Find("WinPopup");
            if (winPopupText == null && winPopupPanel != null) winPopupText = winPopupPanel.transform.Find("WinText")?.GetComponent<TMP_Text>();
            if (winPopupCloseButton == null && winPopupPanel != null) winPopupCloseButton = winPopupPanel.transform.Find("CollectBtn")?.GetComponent<Button>();
            if (paytablePanel == null) paytablePanel = GameObject.Find("PaytablePanel");
            if (paytableToggleButton == null) paytableToggleButton = GameObject.Find("PaytableToggleBtn")?.GetComponent<Button>();
            if (paytableCloseButton == null && paytablePanel != null) paytableCloseButton = paytablePanel.transform.Find("CloseBtn")?.GetComponent<Button>();

            if (spinButton != null)
                spinButton.onClick.AddListener(() => OnSpinClicked?.Invoke());

            if (betUpButton != null)
                betUpButton.onClick.AddListener(() => OnBetUpClicked?.Invoke());

            if (betDownButton != null)
                betDownButton.onClick.AddListener(() => OnBetDownClicked?.Invoke());

            if (winPopupCloseButton != null)
                winPopupCloseButton.onClick.AddListener(HideWinPopup);

            if (paytableToggleButton != null)
                paytableToggleButton.onClick.AddListener(TogglePaytable);

            if (paytableCloseButton != null)
                paytableCloseButton.onClick.AddListener(() => SetPaytableVisible(false));

            if (winPopupPanel != null)
                winPopupPanel.SetActive(false);

            if (paytablePanel != null)
                paytablePanel.SetActive(false);
        }

        public void UpdateBalance(int balance)
        {
            if (balanceText != null)
            {
                balanceText.text = $"BALANCE: {balance}";
            }
        }

        public void UpdateBet(int bet)
        {
            if (betText != null)
            {
                betText.text = $"BET: {bet}";
            }
        }

        public void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void SetControlsInteractable(bool interactable)
        {
            if (spinButton != null)
                spinButton.interactable = interactable;

            if (betUpButton != null)
                betUpButton.interactable = interactable;

            if (betDownButton != null)
                betDownButton.interactable = interactable;
        }

        public void ShowWinPopup(int payout, string symbolName)
        {
            if (winPopupPanel != null)
            {
                if (winPopupText != null)
                {
                    winPopupText.text = $"BIG WIN!\n3x {symbolName}\n+{payout} COINS";
                }
                winPopupPanel.SetActive(true);
            }
        }

        public void HideWinPopup()
        {
            if (winPopupPanel != null)
            {
                winPopupPanel.SetActive(false);
            }
        }

        public void TogglePaytable()
        {
            if (paytablePanel != null)
            {
                paytablePanel.SetActive(!paytablePanel.activeSelf);
            }
        }

        public void SetPaytableVisible(bool visible)
        {
            if (paytablePanel != null)
            {
                paytablePanel.SetActive(visible);
            }
        }
    }
}
