# Slot Game Assignment — Classic 3-Reel Slot Machine

A 3-reel slot machine prototype developed in **Unity 6 (6000.6.4f1)** using C# and Unity UI (uGUI), following clean OOP architecture and data-driven design. Built as an educational game prototype using the supplied assignment art assets.

<img width="1377" height="832" alt="Image" src="https://github.com/user-attachments/assets/025ddb9b-4465-4b4d-8294-7195edcf6ec9" />

<img width="1371" height="815" alt="Image" src="https://github.com/user-attachments/assets/9b045446-3296-4dc1-b6d6-ff93d7f9132a" />

<img width="1375" height="832" alt="Image" src="https://github.com/user-attachments/assets/151a23a4-49cf-4070-bdf3-a5da3b679ca3" />

---

## 🎮 Game Overview & Controls

The player spins a mechanical 3-reel slot machine to match symbols on the central payline. The game features interactive betting, smooth vertical symbol reel scrolling with ease-out deceleration and bounce settling, animated cabinet lever feedback, paytable modal, and win celebration popup dialogs.

### Controls:
- **SPIN Button**: Deducts the current bet stake and initiates the spin animation across all 3 reels.
- **BET + / BET - Buttons**: Adjusts bet amount in increments of 5 (Min Bet: 5, Max Bet: 100).
- **PAYTABLE Button**: Opens an interactive modal displaying symbol payouts and game rules.
- **COLLECT Button**: Dismisses the win dialog.

---

## 📜 Rules & Win Condition

- **Reels**: 3 reels with 1 active center payline.
- **Outcome Generation**: Determined **once** before reel animations begin via a centralized random number generator service (`IRNGService`). Reel strips visually scroll, cycle random intermediate frames during motion, decelerate, and settle on the preselected outcome.
- **Win Rule**: The player wins **only** when **ALL THREE reels land on the exact same symbol** (`reel[0] == reel[1] == reel[2]`).
- **Loss Condition**: Any combination where symbols do not match all 3 reels awards **0 coins** (Loss). No near-win or partial payouts exist.
- **Bet Stake**: The bet is deducted exactly once upon spin initiation. Spins are blocked if current balance is lower than the selected bet.

---

## 💰 Payout Formula & Table

### Formula:
$$\text{Awarded Payout} = \text{Bet Stake} \times \text{Symbol Multiplier}$$

*Stake is deducted at spin start. On a win, the payout is credited back to the player's balance.*

| Symbol Name | Visual Asset | Multiplier | Example Payout (Bet = 10) | Example Payout (Bet = 50) |
| :--- | :--- | :---: | :---: | :---: |
| **Seven / Diamond** | `slot-symbol4.png` | **50×** | 500 Coins | 2,500 Coins |
| **Bar / Star** | `slot-symbol3.png` | **25×** | 250 Coins | 1,250 Coins |
| **Bell / Fruit** | `slot-symbol2.png` | **10×** | 100 Coins | 500 Coins |
| **Cherry** | `slot-symbol1.png` | **5×** | 50 Coins | 250 Coins |
| **Non-Match** | *Mixed symbols* | **0×** | 0 Coins | 0 Coins |

## 🚀 How to Open and Run the Project

### Requirements:
- **Unity Version**: Unity 6 (`6000.6.4f1` or compatible).
- **Modules**: WebGL Build Support (optional, for builds).

### In Unity Editor:
1. Open **Unity Hub** and click **Add project from disk**.
2. Select the `SlotGameAssignment` root folder.
3. Open the project with Unity 6000.6.4f1.
4. In the Project window, navigate to `Assets/Scenes/` and double-click `SlotGame.unity`.
5. Press **Play** in the Unity toolbar.
6. Use the **BET +/-** buttons to select your stake and click **SPIN** to play!

---

## 🌐 How to Run the WebGL Build

Unity WebGL uses Brotli compression (`.br`). Running directly via `file://index.html` is prevented by browser CORS policies. Use the included Python HTTP server:

1. Open PowerShell or Terminal in the repository root:
   ```powershell
   python serve_webgl.py
   ```
2. Open your browser and navigate to:
   ```
   http://localhost:8088/index.html
   ```
3. The game will load and run with full interactivity, animations, and responsive layout.
