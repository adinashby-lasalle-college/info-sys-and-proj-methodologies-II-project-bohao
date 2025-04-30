    using UnityEngine;

[System.Serializable]
public class Mission
{
    public enum MissionStatus
    {
        NotAccepted,
        InProgress,
        Completed,
        Delivered
    }

    public string missionId;
    public string missionTitle;
    public string description;
    public int currentProgress;
    public int targetProgress;
    public string targetTag;
    public MissionStatus status = MissionStatus.NotAccepted;

    public Mission(string id, string title, string desc, int target, string tag)
    {
        missionId = id;
        missionTitle = title;
        description = desc;
        targetProgress = target;
        targetTag = tag;
        currentProgress = 0;
    }

    public bool IsCompleted()
    {
        return currentProgress >= targetProgress;
    }

    public void UpdateProgress(int amount = 1)
    {
        currentProgress += amount;
        
        // 确保进度不超过目标
        if (currentProgress > targetProgress)
        {
            currentProgress = targetProgress;
        }
        
        // 状态更新
        if (currentProgress >= targetProgress && status == MissionStatus.InProgress)
        {
            status = MissionStatus.Completed;
            Debug.Log($"Mission {missionTitle} completed! Current: {currentProgress}/{targetProgress}");
        }
        else
        {
            Debug.Log($"Mission {missionTitle} progress updated: {currentProgress}/{targetProgress}");
        }
    }

    public string GetProgressText()
    {
        return $"{currentProgress}/{targetProgress}";
    }
}