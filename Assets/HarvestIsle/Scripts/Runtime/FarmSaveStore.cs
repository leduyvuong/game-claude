using System;
using System.IO;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed class FarmSaveStore
    {
        private readonly string path;
        public string LastError { get; private set; }
        public FarmSaveStore(string directory) { path = Path.Combine(directory, "harvest-isle-v1.json"); }

        public FarmState Load(double now,FarmState defaultState=null)
        {
            LastError = null;
            bool existed = File.Exists(path) || File.Exists(path + ".bak");
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    if (new FileInfo(candidate).Length > 1024 * 1024) continue;
                    // An envelope prevents an empty/partial JSON object becoming a valid default state.
                    SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(candidate));
                    string reason;
                    if (envelope != null && envelope.format == "harvest-isle-v1" && FarmModel.Validate(envelope.farm, out reason))
                    {
                        if (candidate != path) LastError = "Đã khôi phục bản lưu dự phòng.";
                        return envelope.farm;
                    }
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
                {
                    Debug.LogWarning("Harvest Isle save read: " + e.Message);
                }
            }
            if (existed) LastError = "Bản lưu bị lỗi. Phiên này dùng nông trại mới; bản cũ được giữ lại.";
            return defaultState??FarmModel.NewState(now);
        }

        public bool Save(FarmState state)
        {
            string reason;
            if (!FarmModel.Validate(state, out reason)) { LastError = reason; return false; }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // Preserve invalid input before writing a clean session.
                if (File.Exists(path) && !IsValidFile(path))
                    File.Move(path, path + ".invalid-" + DateTime.UtcNow.Ticks);
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(new SaveEnvelope { format = "harvest-isle-v1", farm = state }, true));
                if (File.Exists(path))
                {
                    try { File.Replace(temporary, path, path + ".bak"); }
                    catch (PlatformNotSupportedException)
                    {
                        File.Copy(path, path + ".bak", true);
                        File.Copy(temporary, path, true);
                        File.Delete(temporary);
                    }
                }
                else File.Move(temporary, path);
                LastError = null;
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            {
                LastError = "Chưa lưu được nông trại. Kiểm tra quyền ghi và dung lượng máy.";
                Debug.LogWarning("Harvest Isle save write: " + e.Message);
                return false;
            }
        }

        private static bool IsValidFile(string file)
        {
            try
            {
                SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(file));
                string reason;
                return envelope != null && envelope.format == "harvest-isle-v1" && FarmModel.Validate(envelope.farm, out reason);
            }
            catch (Exception e) when (e is IOException || e is ArgumentException) { return false; }
        }

        [Serializable]
        private sealed class SaveEnvelope { public string format; public FarmState farm; }
    }
}
