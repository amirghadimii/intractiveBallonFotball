using UnityEngine;
using System.Collections.Generic;

namespace GoalRush
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int teamIndex;
        public int score;
        public int goldHits;
        public int totalClicks;
        public float accuracy;
        public string date;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        public void AddEntry(LeaderboardEntry entry)
        {
            entries.Add(entry);
            entries.Sort((a, b) => b.score.CompareTo(a.score));
            if (entries.Count > 10)
                entries = entries.GetRange(0, 10);
        }

        public static void Save(LeaderboardData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("GoalRush_Leaderboard", json);
            PlayerPrefs.Save();
        }

        public static LeaderboardData Load()
        {
            if (!PlayerPrefs.HasKey("GoalRush_Leaderboard"))
                return new LeaderboardData();
            string json = PlayerPrefs.GetString("GoalRush_Leaderboard");
            return JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey("GoalRush_Leaderboard");
            PlayerPrefs.Save();
        }
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public string name = "";
        public int teamIndex = 0;

        public static void Save(string name, int teamIndex)
        {
            PlayerPrefs.SetString("GoalRush_PlayerName", name);
            PlayerPrefs.SetInt("GoalRush_TeamIndex", teamIndex);
            PlayerPrefs.Save();
        }

        public static string LoadName()
        {
            return PlayerPrefs.GetString("GoalRush_PlayerName", "");
        }

        public static int LoadTeamIndex()
        {
            return PlayerPrefs.GetInt("GoalRush_TeamIndex", 0);
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey("GoalRush_PlayerName");
            PlayerPrefs.DeleteKey("GoalRush_TeamIndex");
            PlayerPrefs.Save();
        }
    }
}
