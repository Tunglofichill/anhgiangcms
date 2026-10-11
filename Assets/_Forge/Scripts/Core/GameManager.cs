using System.Collections.Generic;
using UnityEngine;

namespace Forge
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private double ore = 0.0;
        [SerializeField] private double totalEarned = 0.0;

        [Header("Config")]
        public BalanceConfig balanceConfig;
        public List<GeneratorDefinition> generatorDefinitions = new List<GeneratorDefinition>();

        [Header("Systems")]
        public SaveService saveService;
        public OfflineService offlineService;
        public EconomyService economyService;
        public MilestoneService milestoneService;
        public AudioService audioService;
        public SettingsService settingsService;
        public TickRunner tickRunner;

        [Header("Views")]
        public WorldView worldView;
        public ForgeLightController lightController;
        public HudView hudView;
        public ShopView shopView;
        public SettingsView settingsView;
        public OfflinePopupView offlinePopupView;
        public MainMenuView mainMenuView;

        private bool _hasPendingOfflineProgress = false;
        private string _pendingOfflineDuration = "";
        private double _pendingOfflineGain = 0.0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // 1. Initialize Economy Service
            if (economyService != null)
            {
                economyService.Initialize(balanceConfig, generatorDefinitions);
            }

            // 2. Load Save Data
            SaveData save = saveService != null ? saveService.Load() : null;

            if (save != null)
            {
                ore = save.ore;
                totalEarned = save.totalEarned;

                if (save.generators != null && economyService != null)
                {
                    foreach (var entry in save.generators)
                    {
                        economyService.SetGeneratorLevel(entry.id, entry.level);
                    }
                }

                if (milestoneService != null && save.unlockedMilestones != null)
                {
                    milestoneService.LoadUnlockedMilestones(save.unlockedMilestones);
                }

                if (economyService != null)
                {
                    economyService.HammerLevel = Mathf.Max(1, save.hammerLevel);
                    economyService.FurnaceLevel = Mathf.Max(1, save.furnaceLevel);
                    economyService.GlovesLevel = Mathf.Clamp(save.glovesLevel, 0, 10);
                    economyService.OilLevel = Mathf.Clamp(save.oilLevel, 0, 10);
                    if (lightController != null) lightController.UpdateFurnaceLevel(economyService.FurnaceLevel);
                }

                // 3. Calculate Offline Progress BEFORE applying new gains
                if (offlineService != null && balanceConfig != null)
                {
                    double dpsAtLoad = economyService != null ? economyService.CalculateFinalDps() : 0.0;
                    double offlineGain = offlineService.CalculateOfflineProgress(
                        save.lastSaveUtc,
                        dpsAtLoad,
                        balanceConfig.maxOfflineSeconds,
                        out string durationStr,
                        out _);

                    if (offlineGain > 0.0)
                    {
                        ore += offlineGain;
                        totalEarned += offlineGain;
                        _hasPendingOfflineProgress = true;
                        _pendingOfflineDuration = durationStr;
                        _pendingOfflineGain = offlineGain;
                    }
                }
            }
            else
            {
                ore = 0.0;
                totalEarned = 0.0;
            }

            // 4. Initialize UI Callbacks
            if (shopView != null)
            {
                shopView.Initialize(OnBuyRequested);
            }

            if (offlinePopupView != null)
            {
                offlinePopupView.Initialize(OnOfflinePopupClaimed);
            }

            if (settingsView != null)
            {
                settingsView.Initialize();
            }

            if (milestoneService != null)
            {
                milestoneService.OnMilestoneUnlocked += OnMilestoneUnlocked;
                milestoneService.CheckMilestones(totalEarned, economyService);
            }

            // 5. Initialize Main Menu
            if (mainMenuView != null)
            {
                mainMenuView.Initialize(OnStartGameRequested);
                mainMenuView.Open();

                // Hide gameplay HUD & shop button while in Main Menu
                if (hudView != null) hudView.gameObject.SetActive(false);
                if (shopView != null && shopView.btnOpenShop != null) shopView.btnOpenShop.gameObject.SetActive(false);
                if (TutorialHintController.Instance != null) TutorialHintController.Instance.gameObject.SetActive(false);
            }
            else
            {
                // Fallback: If no Main Menu in scene, show offline popup immediately
                if (_hasPendingOfflineProgress && offlinePopupView != null)
                {
                    offlinePopupView.Show(_pendingOfflineDuration, _pendingOfflineGain);
                    _hasPendingOfflineProgress = false;
                }
            }

            // 6. Start Tick Loop
            if (tickRunner != null && balanceConfig != null)
            {
                tickRunner.StartTicking(balanceConfig.tickRate);
            }

            // 7. Refresh Views
            RefreshAllViews();
        }

        private void OnStartGameRequested()
        {
            if (hudView != null) hudView.gameObject.SetActive(true);
            if (shopView != null && shopView.btnOpenShop != null) shopView.btnOpenShop.gameObject.SetActive(true);
            if (TutorialHintController.Instance != null) TutorialHintController.Instance.gameObject.SetActive(true);
            if (audioService != null) audioService.PlayHit();

            RefreshAllViews();

            // Trigger offline earnings popup after entering gameplay
            if (_hasPendingOfflineProgress && offlinePopupView != null)
            {
                offlinePopupView.Show(_pendingOfflineDuration, _pendingOfflineGain);
                _hasPendingOfflineProgress = false;
            }
        }

        public void ReturnToMainMenu()
        {
            // Auto save state before returning to menu
            if (saveService != null)
            {
                saveService.SaveCurrentState();
            }

            // Close gameplay popups
            if (shopView != null) shopView.Close();
            if (settingsView != null) settingsView.Close();
            if (offlinePopupView != null) offlinePopupView.Close();

            // Hide in-game HUD & shop button
            if (hudView != null) hudView.gameObject.SetActive(false);
            if (shopView != null && shopView.btnOpenShop != null) shopView.btnOpenShop.gameObject.SetActive(false);
            if (TutorialHintController.Instance != null) TutorialHintController.Instance.gameObject.SetActive(false);

            // Open Main Menu
            if (mainMenuView != null)
            {
                mainMenuView.Open();
            }
        }

        public void Tick(float dt)
        {
            if (economyService == null) return;

            double finalDps = economyService.CalculateFinalDps();
            double gain = finalDps * dt;

            if (gain > 0)
            {
                ore += gain;
                totalEarned += gain;

                if (milestoneService != null)
                {
                    milestoneService.CheckMilestones(totalEarned, economyService);
                }
            }

            RefreshHUD();

            if (shopView != null && shopView.IsOpen)
            {
                shopView.RefreshShop(economyService, ore, totalEarned);
            }
        }

        public void OnFurnaceClicked(double tapPower = -1.0)
        {
            if (economyService == null) return;

            if (tapPower <= 0.0)
            {
                tapPower = economyService.CalculateFinalTapPower();
            }
            ore += tapPower;
            totalEarned += tapPower;

            if (milestoneService != null)
            {
                milestoneService.CheckMilestones(totalEarned, economyService);
            }

            RefreshHUD();

            if (shopView != null && shopView.IsOpen)
            {
                shopView.RefreshShop(economyService, ore, totalEarned);
            }
        }

        private void OnBuyRequested(string genOrUpgradeId)
        {
            if (genOrUpgradeId.StartsWith("UP_"))
            {
                TryBuyToolUpgrade(genOrUpgradeId);
            }
            else
            {
                TryBuyGenerator(genOrUpgradeId);
            }
        }

        public bool TryBuyToolUpgrade(string upgradeId)
        {
            if (economyService == null) return false;
            if (!economyService.CanAffordUpgrade(upgradeId, ore)) return false;

            double cost = economyService.GetUpgradeCost(upgradeId);
            int curLv = economyService.GetUpgradeLevel(upgradeId);
            ore -= cost;
            int newLv = curLv + 1;
            economyService.SetUpgradeLevel(upgradeId, newLv);

            if (audioService != null)
            {
                audioService.PlayUnlock();
            }

            if (worldView != null)
            {
                worldView.PlayUnlockBurst();
            }

            if (upgradeId == "UP_FURNACE" && lightController != null)
            {
                lightController.UpdateFurnaceLevel(newLv);
            }

            if (milestoneService != null)
            {
                milestoneService.CheckMilestones(totalEarned, economyService);
            }

            if (saveService != null)
            {
                saveService.SaveCurrentState();
            }

            RefreshAllViews();
            return true;
        }

        public bool TryBuyGenerator(string genId)
        {
            if (economyService == null) return false;

            if (!economyService.IsGeneratorUnlocked(genId, totalEarned)) return false;
            if (!economyService.CanAfford(genId, ore)) return false;

            double cost = economyService.GetGeneratorCost(genId);
            int currentLevel = economyService.GetGeneratorLevel(genId);

            ore -= cost;
            int newLevel = currentLevel + 1;
            economyService.SetGeneratorLevel(genId, newLevel);

            // Audio feedback
            if (audioService != null)
            {
                if (currentLevel == 0) audioService.PlayUnlock();
                else audioService.PlayBuy();
            }

            // Visual FX unlock
            if (currentLevel == 0 && worldView != null)
            {
                worldView.PlayUnlockBurst();
            }

            // Check milestones
            if (milestoneService != null)
            {
                milestoneService.CheckMilestones(totalEarned, economyService);
            }

            // Save immediately after purchase
            if (saveService != null)
            {
                saveService.SaveCurrentState();
            }

            RefreshAllViews();
            return true;
        }

        private void OnMilestoneUnlocked(string milestoneId)
        {
            if (audioService != null) audioService.PlayUnlock();
            if (worldView != null)
            {
                worldView.PlayUnlockBurst();
                worldView.Refresh(economyService, milestoneService);
            }
            RefreshHUD();
        }

        private void OnOfflinePopupClaimed()
        {
            // Save state after offline claim
            if (saveService != null)
            {
                saveService.SaveCurrentState();
            }
        }

        public void RefreshAllViews()
        {
            RefreshHUD();
            if (worldView != null && economyService != null && milestoneService != null)
            {
                worldView.Refresh(economyService, milestoneService);
            }
            if (shopView != null && economyService != null)
            {
                shopView.RefreshShop(economyService, ore, totalEarned);
            }
        }

        private void RefreshHUD()
        {
            if (hudView != null)
            {
                double dps = economyService != null ? economyService.CalculateFinalDps() : 0.0;
                string rank = milestoneService != null ? milestoneService.GetCurrentRank() : "Học việc";
                hudView.UpdateHUD(ore, dps, rank);

                bool canAffordAny = false;
                if (economyService != null)
                {
                    if (generatorDefinitions != null)
                    {
                        foreach (var gen in generatorDefinitions)
                        {
                            if (gen != null && economyService.IsGeneratorUnlocked(gen.id, totalEarned) && economyService.CanAfford(gen.id, ore))
                            {
                                canAffordAny = true;
                                break;
                            }
                        }
                    }

                    if (!canAffordAny)
                    {
                        string[] uIds = new[] { "UP_HAMMER", "UP_FURNACE", "UP_GLOVES", "UP_OIL" };
                        foreach (var uid in uIds)
                        {
                            if (economyService.CanAffordUpgrade(uid, ore))
                            {
                                canAffordAny = true;
                                break;
                            }
                        }
                    }
                }
                hudView.SetShopNotification(canAffordAny);
            }
        }

        public SaveData ExportSaveData()
        {
            SaveData data = new SaveData
            {
                version = 1,
                ore = ore,
                totalEarned = totalEarned,
                tapLevel = economyService != null ? economyService.HammerLevel : 1,
                hammerLevel = economyService != null ? economyService.HammerLevel : 1,
                furnaceLevel = economyService != null ? economyService.FurnaceLevel : 1,
                glovesLevel = economyService != null ? economyService.GlovesLevel : 0,
                oilLevel = economyService != null ? economyService.OilLevel : 0
            };

            if (economyService != null)
            {
                data.generators = new List<GeneratorSaveEntry>();
                var levels = economyService.GetAllLevels();
                foreach (var kvp in levels)
                {
                    data.generators.Add(new GeneratorSaveEntry(kvp.Key, kvp.Value));
                }
            }

            if (milestoneService != null)
            {
                data.unlockedMilestones = milestoneService.GetUnlockedList();
            }

            return data;
        }

        // Debug Helpers
        public void DebugAddOre(double amount)
        {
            ore += amount;
            totalEarned += amount;
            if (milestoneService != null && economyService != null)
            {
                milestoneService.CheckMilestones(totalEarned, economyService);
            }
            RefreshAllViews();
            Debug.Log($"[Debug] Added {amount} Ore. Current: {ore}");
        }

        public void DebugUnlockAllLv10()
        {
            if (economyService != null)
            {
                economyService.SetGeneratorLevel("G1", 10);
                economyService.SetGeneratorLevel("G2", 10);
                economyService.SetGeneratorLevel("G3", 10);
                economyService.SetGeneratorLevel("G4", 10);
                economyService.SetGeneratorLevel("G5", 10);
            }
            totalEarned = 1500000.0;
            ore += 50000.0;
            if (milestoneService != null && economyService != null)
            {
                milestoneService.CheckMilestones(totalEarned, economyService);
            }
            RefreshAllViews();
            if (saveService != null) saveService.SaveCurrentState();
            Debug.Log("[Debug] Unlocked all generators to Level 10!");
        }

        public double GetOre() => ore;
        public double GetDps() => economyService != null ? economyService.CalculateFinalDps() : 0.0;
        public double GetTotalEarned() => totalEarned;
        public string GetRank() => milestoneService != null ? milestoneService.GetCurrentRank() : "Học việc";
    }
}
