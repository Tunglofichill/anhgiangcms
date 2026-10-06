using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Forge
{
    public class SaveService : MonoBehaviour
    {
        private const string SAVE_FILE_NAME = "forge_save.json";
        private const string TMP_FILE_NAME = "forge_save.tmp";

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private string TmpPath => Path.Combine(Application.persistentDataPath, TMP_FILE_NAME);

        private float _autoSaveTimer = 0f;
        private const float AUTO_SAVE_INTERVAL = 10f;

        private void Update()
        {
            _autoSaveTimer += Time.unscaledDeltaTime;
            if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                _autoSaveTimer = 0f;
                if (GameManager.Instance != null)
                {
                    SaveCurrentState();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && GameManager.Instance != null)
            {
                SaveCurrentState();
            }
        }

        private void OnApplicationQuit()
        {
            if (GameManager.Instance != null)
            {
                SaveCurrentState();
            }
        }

        public void SaveCurrentState()
        {
            if (GameManager.Instance == null) return;
            SaveData data = GameManager.Instance.ExportSaveData();
            Save(data);
        }

        public void Save(SaveData data)
        {
            try
            {
                data.lastSaveUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                string json = JsonUtility.ToJson(data, true);

                string savePath = SavePath;
                string tmpPath = TmpPath;
                string bakPath = savePath + ".bak";

                // Atomic write via temp file
                File.WriteAllText(tmpPath, json);

                if (File.Exists(savePath))
                {
                    try { File.Copy(savePath, bakPath, true); } catch { }
                    File.Delete(savePath);
                }
                File.Move(tmpPath, savePath);

                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to save game: {ex.Message}");
            }
        }

        public SaveData Load()
        {
            string savePath = SavePath;
            string bakPath = savePath + ".bak";

            if (!File.Exists(savePath) && !File.Exists(bakPath))
            {
                Debug.Log("[SaveService] No save file found. Initializing new game.");
                return null;
            }

            try
            {
                string targetPath = File.Exists(savePath) ? savePath : bakPath;
                string json = File.ReadAllText(targetPath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveService] Loaded game successfully from: {targetPath}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Failed to load main save, attempting backup. Error: {ex.Message}");
                if (File.Exists(bakPath))
                {
                    try
                    {
                        string json = File.ReadAllText(bakPath);
                        SaveData data = JsonUtility.FromJson<SaveData>(json);
                        Debug.Log("[SaveService] Loaded game successfully from backup!");
                        return data;
                    }
                    catch (Exception exBak)
                    {
                        Debug.LogError($"[SaveService] Backup also failed: {exBak.Message}");
                    }
                }
                return null;
            }
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(TmpPath)) File.Delete(TmpPath);
                string bakPath = SavePath + ".bak";
                if (File.Exists(bakPath)) File.Delete(bakPath);
                Debug.Log("[SaveService] Save file deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to delete save: {ex.Message}");
            }
        }

        public string GetSavePath()
        {
            return SavePath;
        }
    }
}
