using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forge
{
    public enum GameLanguage
    {
        Vietnamese,
        English
    }

    public static class LocalizationService
    {
        public static GameLanguage CurrentLanguage { get; private set; } = GameLanguage.Vietnamese;
        public static event Action OnLanguageChanged;

        private const string PREF_KEY = "Forge_Language";

        private static readonly Dictionary<string, string> _vi = new Dictionary<string, string>()
        {
            // HUD
            { "ORE_LABEL", "Quặng: {0}" },
            { "DPS_LABEL", "+{0}/giây" },
            { "RANK_LABEL", "Danh hiệu: {0}" },
            { "SHOP_BUTTON", "CÔNG XƯỞNG" },
            { "SETTINGS_BUTTON", "CÀI ĐẶT" },

            // Shop
            { "SHOP_TITLE", "CÔNG XƯỞNG TỰ ĐỘNG HÓA" },
            { "UPGRADE_BTN", "Nâng Cấp" },
            { "LEVEL_LABEL", "Cấp: {0}" },
            { "RATE_LABEL", "+{0}/s" },
            { "COST_LABEL", "Giá: {0}" },
            { "LOCKED_ORE", "Chưa mở — cần {0} Quặng" },
            { "LOCKED_GEN", "Chưa mở — cần {0} cấp {1}" },
            { "LOCKED_SUFFIX", "(Khóa)" },

            // Generator Names
            { "GEN_G1", "Tay Búa Tí Hon" },
            { "GEN_G2", "Cũi Đào Quặng" },
            { "GEN_G3", "Lò Than Tự Đun" },
            { "GEN_G4", "Cối Xay Kim" },
            { "GEN_G5", "Robot Thợ Cả" },

            // Ranks
            { "RANK_APPRENTICE", "Học việc" },
            { "RANK_TRAINEE", "Tập sự" },
            { "RANK_NOVICE", "Thợ mới" },
            { "RANK_MINER", "Thợ mỏ" },
            { "RANK_SMELTER", "Thợ lò" },
            { "RANK_CHIEF", "Chủ lò" },
            { "RANK_MASTER", "Đại sư phụ" },
            { "RANK_GRANDMASTER", "Đại sư rèn" },

            // Tutorial Hint
            { "TUTORIAL_INITIAL", "👆 Chạm vào Lò hoặc Đe để rèn quặng!" },
            { "TUTORIAL_PROGRESS", "👆 Đập thêm {0} lần nữa để gom quặng!" },

            // Offline Popup
            { "OFFLINE_TITLE", "CÔNG XƯỞNG TỰ HOẠT ĐỘNG" },
            { "OFFLINE_BODY", "Lò chạy hộ bạn {0}.\n+{1} quặng!" },
            { "OFFLINE_CLAIM", "Thu Nhận" },
            { "TIME_HOURS_MINUTES", "{0} giờ {1} phút" },
            { "TIME_MINUTES_SECONDS", "{0} phút {1} giây" },
            { "TIME_SECONDS", "{0} giây" },

            // Settings
            { "SETTINGS_TITLE", "CÀI ĐẶT HỆ THỐNG" },
            { "SETTING_ORIENTATION", "Chế Độ Màn Hình:" },
            { "ORIENTATION_LANDSCAPE", "🖥️ Ngang (16:9)" },
            { "ORIENTATION_PORTRAIT", "📱 Dọc (9:16)" },
            { "SETTING_SFX", "Âm Thanh (SFX)" },
            { "SETTING_BGM", "Nhạc Nền (BGM)" },
            { "SETTING_VFX", "Hiệu Ứng Hình Ảnh (VFX)" },
            { "SETTING_HAPTICS", "Rung & Lắc Màn Hình" },
            { "SETTING_LANG", "Ngôn Ngữ:" },
            { "LANG_VI", "🇻🇳 Tiếng Việt" },
            { "LANG_EN", "🇬🇧 English" },
            { "BTN_RESET_SAVE", "⚠️ XÓA DỮ LIỆU (RESET GAME)" },
            { "RESET_CONFIRM_TITLE", "XÁC NHẬN RESET" },
            { "RESET_CONFIRM_MSG", "Toàn bộ quặng và máy móc sẽ quay về 0.\nBạn có chắc muốn chơi lại từ đầu?" },
            { "BTN_CONFIRM", "ĐỒNG Ý (RESET)" },
            { "BTN_CANCEL", "HỦY BỎ" },
            { "STATE_ON", "BẬT" },
            { "STATE_OFF", "TẮT" },

            // Main Menu
            { "MENU_TITLE", "HUYỀN THOẠI LÒ RÈN" },
            { "MENU_TAGLINE", "🔥 Công Xưởng Hắc Thiết Cổ Đại 🔥" },
            { "MENU_VERSION", "v1.0 • Phiên Bản Clicker Rèn Thần Binh" },
            { "MENU_BTN_PLAY", "🔨 BẮT ĐẦU CHƠI" },
            { "MENU_BTN_CONTINUE", "🔨 BẮT ĐẦU CHƠI" },
            { "MENU_BTN_SETTINGS", "⚙️ CÀI ĐẶT" },
            { "MENU_BTN_CREDITS", "📜 THÔNG TIN" },
            { "MENU_BTN_QUIT", "🚪 THOÁT GAME" },
            { "MENU_PROGRESS_INFO", "Tiến trình: {0} • Quặng: {1}" },
            { "MENU_NEW_JOURNEY", "Khởi đầu hành trình rèn thần binh huyền thoại!" },
            { "MENU_CREDITS_TITLE", "GIỚI THIỆU & BẢN QUYỀN" },
            { "MENU_CREDITS_BODY", "Chào mừng bạn đến với Huyền Thoại Lò Rèn!\nTrải nghiệm clicker mô phỏng công xưởng rèn hắc thiết vô tận.\n\nPhát triển & Thiết kế bởi Trường Giang ✨" },
            { "MENU_BTN_STUDIO", "🌐 Tác giả: Trường Giang ✨" },
            { "MENU_BTN_RETURN_MAIN", "🏠 VỀ MENU CHÍNH" },
            { "STUDIO_CREDIT", "Phát triển & Thiết kế bởi Trường Giang ✨" }
        };

        private static readonly Dictionary<string, string> _en = new Dictionary<string, string>()
        {
            // HUD
            { "ORE_LABEL", "Ore: {0}" },
            { "DPS_LABEL", "+{0}/sec" },
            { "RANK_LABEL", "Rank: {0}" },
            { "SHOP_BUTTON", "WORKSHOP" },
            { "SETTINGS_BUTTON", "SETTINGS" },

            // Shop
            { "SHOP_TITLE", "AUTOMATION WORKSHOP" },
            { "UPGRADE_BTN", "Upgrade" },
            { "LEVEL_LABEL", "Lv: {0}" },
            { "RATE_LABEL", "+{0}/s" },
            { "COST_LABEL", "Cost: {0}" },
            { "LOCKED_ORE", "Locked — requires {0} Ore" },
            { "LOCKED_GEN", "Locked — requires {0} Lv {1}" },
            { "LOCKED_SUFFIX", "(Locked)" },

            // Generator Names
            { "GEN_G1", "Tiny Hammer" },
            { "GEN_G2", "Ore Cart" },
            { "GEN_G3", "Coal Smelter" },
            { "GEN_G4", "Steam Mill" },
            { "GEN_G5", "Master Automaton" },

            // Ranks
            { "RANK_APPRENTICE", "Apprentice" },
            { "RANK_TRAINEE", "Trainee" },
            { "RANK_NOVICE", "Novice Smith" },
            { "RANK_MINER", "Miner" },
            { "RANK_SMELTER", "Smelter Smith" },
            { "RANK_CHIEF", "Furnace Master" },
            { "RANK_MASTER", "Master Artisan" },
            { "RANK_GRANDMASTER", "Grandmaster Smith" },

            // Tutorial Hint
            { "TUTORIAL_INITIAL", "👆 Tap Furnace or Anvil to forge ore!" },
            { "TUTORIAL_PROGRESS", "👆 Tap {0} more times to gather ore!" },

            // Offline Popup
            { "OFFLINE_TITLE", "OFFLINE FORGING REPORT" },
            { "OFFLINE_BODY", "Your workshop produced for {0}.\n+{1} Ore collected!" },
            { "OFFLINE_CLAIM", "Claim" },
            { "TIME_HOURS_MINUTES", "{0}h {1}m" },
            { "TIME_MINUTES_SECONDS", "{0}m {1}s" },
            { "TIME_SECONDS", "{0}s" },

            // Settings
            { "SETTINGS_TITLE", "GAME SETTINGS" },
            { "SETTING_ORIENTATION", "Screen Orientation:" },
            { "ORIENTATION_LANDSCAPE", "🖥️ Landscape (16:9)" },
            { "ORIENTATION_PORTRAIT", "📱 Portrait (9:16)" },
            { "SETTING_SFX", "Sound Effects (SFX)" },
            { "SETTING_BGM", "Music (BGM)" },
            { "SETTING_VFX", "Visual Effects (VFX)" },
            { "SETTING_HAPTICS", "Vibration & Screen Shake" },
            { "SETTING_LANG", "Language:" },
            { "LANG_VI", "🇻🇳 Tiếng Việt" },
            { "LANG_EN", "🇬🇧 English" },
            { "BTN_RESET_SAVE", "⚠️ RESET GAME DATA" },
            { "RESET_CONFIRM_TITLE", "CONFIRM RESET" },
            { "RESET_CONFIRM_MSG", "All progress, ore and machines will be wiped to zero.\nStart fresh from scratch?" },
            { "BTN_CONFIRM", "CONFIRM (RESET)" },
            { "BTN_CANCEL", "CANCEL" },
            { "STATE_ON", "ON" },
            { "STATE_OFF", "OFF" },

            // Main Menu
            { "MENU_TITLE", "LEGEND OF THE FORGE" },
            { "MENU_TAGLINE", "🔥 Ancient Blacksmith Workshop 🔥" },
            { "MENU_VERSION", "v1.0 • Idle Blacksmith Clicker Edition" },
            { "MENU_BTN_PLAY", "🔨 START GAME" },
            { "MENU_BTN_CONTINUE", "🔨 START GAME" },
            { "MENU_BTN_SETTINGS", "⚙️ SETTINGS" },
            { "MENU_BTN_CREDITS", "📜 CREDITS" },
            { "MENU_BTN_QUIT", "🚪 QUIT GAME" },
            { "MENU_PROGRESS_INFO", "Progress: {0} • Ore: {1}" },
            { "MENU_NEW_JOURNEY", "Begin your legendary blacksmith journey!" },
            { "MENU_CREDITS_TITLE", "ABOUT & CREDITS" },
            { "MENU_CREDITS_BODY", "Welcome to Legend of the Forge!\nAn endless idle blacksmith clicker adventure.\n\nDesigned & Developed by Truong Giang ✨" },
            { "MENU_BTN_STUDIO", "🌐 Author: Truong Giang ✨" },
            { "MENU_BTN_RETURN_MAIN", "🏠 MAIN MENU" },
            { "STUDIO_CREDIT", "Designed & Developed by Truong Giang ✨" }
        };

        public static void Initialize()
        {
            string savedLang = PlayerPrefs.GetString(PREF_KEY, "VI");
            CurrentLanguage = savedLang == "EN" ? GameLanguage.English : GameLanguage.Vietnamese;
        }

        public static void SetLanguage(GameLanguage lang)
        {
            CurrentLanguage = lang;
            PlayerPrefs.SetString(PREF_KEY, lang == GameLanguage.English ? "EN" : "VI");
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }

        public static string Get(string key, params object[] args)
        {
            var dict = CurrentLanguage == GameLanguage.English ? _en : _vi;
            if (!dict.TryGetValue(key, out string val))
            {
                if (!_vi.TryGetValue(key, out val))
                {
                    val = key;
                }
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(val, args);
                }
                catch
                {
                    return val;
                }
            }
            return val;
        }

        public static string GetGeneratorName(string genId)
        {
            return Get($"GEN_{genId}");
        }
    }
}
