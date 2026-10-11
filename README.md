# Slot Game Assignment — Classic 3-Reel Slot Machine

https://github.com/user-attachments/assets/01d9228b-bf6b-4313-a44b-a0896b8cd813

A 3-reel slot machine prototype developed in **Unity 6 (6000.6.4f1)** using C# and Unity UI (uGUI), following clean OOP architecture and data-driven design. Built as an educational game prototype using the supplied assignment art assets.

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

---

## 🏗️ Architecture & OOP Design

The codebase adheres to Single Responsibility Principle (SRP) and separation of concerns:

```
Assets/Scripts/
├── Core/
│   ├── IRNGService.cs          # Random outcome generation abstraction
│   ├── RNGService.cs           # Centralized System.Random implementation with seed support
│   ├── IPayoutCalculator.cs    # Payout logic abstraction
│   ├── PayoutCalculator.cs     # 3-match win evaluator and payout calculator
│   ├── SlotResult.cs           # Immutable outcome result data struct
│   ├── ReelController.cs       # Vertical strip scroll, deceleration, bounce, and alignment
│   └── SlotMachine.cs          # Main orchestrator (balance, betting, spin sequencing, state)
├── Data/
│   ├── SymbolData.cs           # ScriptableObject for symbol ID, display name, sprite, multiplier, weight
│   └── SlotGameConfig.cs       # ScriptableObject for starting balance, bet limits, timing, and symbols
└── UI/
    └── SlotMachineUI.cs        # UI management for balance, bet, status text, paytable, and win popup
```

### Key Technical Highlights:
1. **Decoupled Business Logic**: `PayoutCalculator` and `RNGService` are pure C# services independent of `MonoBehaviour`, making them 100% unit testable in EditMode.
2. **Outcome Pre-computation**: Random outcomes are generated once before reels start. Animations land on that outcome; intermediate spinning frames are presentation-only.
3. **State Integrity**: All coroutines and UI interactivity use `try / finally` blocks to ensure controls and spinning flags (`IsSpinning`) are never left locked if an exception occurs.
4. **Self-Healing Fallbacks**: `SlotMachine`, `SlotMachineUI`, and `ReelController` include automated runtime discovery (`Resources.Load<SlotGameConfig>`, child hierarchy lookups) ensuring zero null-reference exceptions.

---

## 📁 Project Folder Structure

```
SlotGameAssignment/
├── Assets/
│   ├── Animations/             # Animation assets
│   ├── Data/                   # ScriptableObject data assets (Symbols, SlotGameConfig)
│   ├── Editor/                 # Automation scripts (SetupProjectAssets, WebGLBuilder, Tests)
│   │   └── Tests/              # EditMode NUnit Unit Tests
│   ├── Prefabs/                # Reusable UI prefabs
│   ├── Resources/              # Runtime-loadable backup game configuration & symbols
│   ├── Scenes/
│   │   └── SlotGame.unity      # Main playable slot machine scene (Scene 0)
│   ├── Scripts/
│   │   ├── Core/               # Business logic, state orchestration, reel controllers
│   │   ├── Data/               # ScriptableObject definitions
│   │   └── UI/                 # UI controllers
│   ├── Sounds/                 # Audio assets
│   └── Sprites/                # Configured 2D sprites (Cabinet, symbols, buttons, background)
├── Build/
│   └── WebGL/                  # Tested, production-ready WebGL build
│       ├── Build/              # WebGL.wasm.br, WebGL.data.br, WebGL.framework.js.br
│       ├── TemplateData/       # WebGL web template styling and icons
│       └── index.html          # WebGL entry page
├── Packages/
│   └── manifest.json           # Package manifest
├── ProjectSettings/            # Unity project and build settings
├── serve_webgl.py              # Local HTTP server with Brotli (.br) header support
├── README.md                   # This documentation
└── IMPLEMENTATION_GUIDE.md     # Step-by-step setup and verification guide
```

---

## 🎨 Asset Integration Mapping

All 17 supplied assignment assets are integrated:

| Asset File | Resolution | Project Location | Role in Game |
| :--- | :--- | :--- | :--- |
| `bg_gradient.png` | 1920 × 1080 | `Assets/Sprites/` | Fullscreen background canvas |
| `slot-machine1.png` | 816 × 624 | `Assets/Sprites/` | Slot machine cabinet housing (lever upright) |
| `slot-machine2.png` | 816 × 624 | `Assets/Sprites/` | Cabinet lever midway pull animation frame |
| `slot-machine3.png` | 816 × 624 | `Assets/Sprites/` | Cabinet lever fully pulled down animation frame |
| `slot-machine4.png` | 816 × 624 | `Assets/Sprites/` | Cabinet frame variant |
| `slot-machine5.png` | 816 × 624 | `Assets/Sprites/` | Bezel bezel cutout |
| `slot_machine_Middle_box.png` | 658 × 277 | `Assets/Sprites/` | Three-reel window display housing |
| `slot-symbol1.png` | 96 × 96 | `Assets/Sprites/` | Symbol 1: Cherry (5× multiplier) |
| `slot-symbol2.png` | 96 × 96 | `Assets/Sprites/` | Symbol 2: Bell / Fruit (10× multiplier) |
| `slot-symbol3.png` | 96 × 96 | `Assets/Sprites/` | Symbol 3: Bar / Star (25× multiplier) |
| `slot-symbol4.png` | 96 × 96 | `Assets/Sprites/` | Symbol 4: Seven / Diamond (50× multiplier) |
| `slot_machine_buttons-02.png` | 256 × 1024 | `Assets/Sprites/` | Sliced 4-state button sheet (Spin button) |
| `slot_machine_buttons-03.png` | 256 × 1024 | `Assets/Sprites/` | Sliced 4-state button sheet (Bet - button) |
| `slot_machine_buttons-04.png` | 256 × 1024 | `Assets/Sprites/` | Sliced 4-state button sheet (Bet + button) |
| `popup.png` | 1730 × 873 | `Assets/Sprites/` | Win celebration dialog frame |
| `Yes_No_Btn.png` | 990 × 690 | `Assets/Sprites/` | Dialog confirmation button sprite |
| `4tXlXs.gif` | 813 × 620 | `Assets/Sprites/` | Reference layout and animation mockup |

---

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

---

## 🧪 Testing & Validation Results

Automated unit tests were executed via the Unity Test Runner in EditMode (**Window → General → Test Runner**):

| Test Suite | Test Case | Status | Description |
| :--- | :--- | :---: | :--- |
| `PayoutCalculatorTests` | `ThreeMatchingSymbols_AwardsWinningPayout` | **PASSED** | Verifies 3 matching symbols award `bet × multiplier`. |
| `PayoutCalculatorTests` | `Jackpots_SevenMatch_AwardsTopMultiplier` | **PASSED** | Verifies 3 Sevens award top 50× multiplier. |
| `PayoutCalculatorTests` | `TwoMatchingSymbols_AwardsZeroPayout` | **PASSED** | Verifies near-win (2 matching, 1 different) pays 0. |
| `PayoutCalculatorTests` | `AllDifferentSymbols_AwardsZeroPayout` | **PASSED** | Verifies 3 different symbols pay 0. |
| `RNGServiceTests` | `GenerateOutcome_ReturnsCorrectReelCount` | **PASSED** | Verifies outcome array length equals requested reel count (3). |
| `RNGServiceTests` | `GenerateOutcome_IndicesAreWithinValidRange` | **PASSED** | Verifies all generated symbol indices are within `[0, symbols.Count - 1]`. |
| `RNGServiceTests` | `GenerateOutcome_WithSeed_IsDeterministic` | **PASSED** | Verifies identical seeds yield identical reproducible outcomes. |

**Total Tests: 7 | Passed: 7 | Failed: 0**

---

## 🏆 Bonus Features Implemented
- **Cabinet Lever Pull Animation**: The physical slot machine lever cycles through upright, mid-pull, and full-down states (`slot-machine1` → `slot-machine2` → `slot-machine3` → `slot-machine1`) when SPIN is triggered.
- **Paytable Modal**: Built-in paytable dialog displays all symbol payouts and win conditions on demand.
- **Settling Bounce Feedback**: Deceleration incorporates a mechanical overshoot bounce curve before snapping into final payline alignment.
- **Big Win Popup**: Dedicated celebration popup with collect action for high payouts.
- **Local Brotli WebGL Server**: Ready-to-run Python server script with proper compression header support.

---

## 📝 Commit History

The repository maintains a clean, descriptive, staged Git history:
1. `chore: create Unity project structure`
2. `feat: integrate provided slot assets`
3. `feat: add slot symbols and game configuration`
4. `feat: implement randomized slot results and payout logic with tests`
5. `feat: animate reels and orchestrate spin state`
6. `feat: build playable slot scene with full UI integration`
7. `polish: add self-healing runtime fallbacks and local server utility`
8. `build: add tested WebGL build`
9. `docs: add comprehensive documentation and implementation guide`
