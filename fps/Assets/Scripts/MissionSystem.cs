using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance;

    [Header("Mission Settings")]
    [SerializeField] private float interactionDistance = 3f;
    
    [Header("Mission 1 - Zombie Hunter")]
    [SerializeField] private int zombieMissionTarget = 5;
    [SerializeField] private string zombieMissionTitle = "Zombie Hunter";
    [SerializeField] private string zombieMissionDesc = "Eliminate zombies in the area";
    private string zombieTag = "Zombie"; // 注意大写Z
    
    [Header("Mission 2 - Boss Hunter")]
    [SerializeField] private int bossMissionTarget = 1; // 只需击杀一个僵尸王
    [SerializeField] private string bossMissionTitle = "Zombie King";
    [SerializeField] private string bossMissionDesc = "Eliminate the Huge Scary Zombie";
    [SerializeField] private string bossTag = "Boss"; // 僵尸王标签
    
    // 任务状态跟踪
    private Mission currentMission;
    private bool isMission1Available = true;   // 第一个任务默认可用
    private bool isMission1Completed = false;  // 第一个任务是否已完成并提交
    private bool isMission2Available = false;  // 第二个任务初始不可用
    
    [Header("Mission Indicators")]
    [SerializeField] private GameObject exclamationMark; // 可接受任务的标记
    [SerializeField] private GameObject questionMark;    // 可提交任务的标记

    private Transform playerTransform;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 获取玩家引用
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
        }
        
        // 初始化任务标记状态
        if (exclamationMark != null) exclamationMark.SetActive(true);
        if (questionMark != null) questionMark.SetActive(false);
        
        Debug.Log("MissionSystem initialized. First mission available: " + isMission1Available);
        Debug.Log($"Zombie mission will track enemies with tag: '{zombieTag}'");
        Debug.Log($"Boss mission will track enemies with tag: '{bossTag}'");
    }

    private void Update()
    {
        // 检查玩家是否在交互范围内
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distanceToPlayer <= interactionDistance;
            
            // 如果玩家刚刚进入范围，更新UI
            if (isPlayerInRange && !wasInRange)
            {
                UpdateUIPrompts();
            }
            // 如果玩家刚刚离开范围，隐藏UI
            else if (!isPlayerInRange && wasInRange)
            {
                HideAllPrompts();
            }
            
            // 处理玩家输入
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                HandleInteraction();
            }
        }
    }

    private void UpdateUIPrompts()
    {
        if (UIManager.Instance == null) return;
        
        if (currentMission == null)
        {
            // 有可接任务时显示接受任务提示
            if (isMission1Available || isMission2Available)
            {
                UIManager.Instance.ShowAcceptPrompt(true);
                UIManager.Instance.ShowCompletePrompt(false);
            }
        }
        else if (currentMission.status == Mission.MissionStatus.Completed)
        {
            // 任务完成但未提交时显示提交任务提示
            UIManager.Instance.ShowAcceptPrompt(false);
            UIManager.Instance.ShowCompletePrompt(true);
        }
        else
        {
            // 任务进行中，不显示提示
            HideAllPrompts();
        }
    }

    private void HideAllPrompts()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowAcceptPrompt(false);
            UIManager.Instance.ShowCompletePrompt(false);
        }
    }

    private void HandleInteraction()
    {
        if (currentMission == null)
        {
            // 接受任务 - 优先提供任务1，只有任务1完成后才提供任务2
            if (isMission1Available)
            {
                isMission1Available = false; // 任务已被接受
                
                // 先隐藏感叹号，再接受任务
                if (exclamationMark != null)
                {
                    exclamationMark.SetActive(false);
                    Debug.Log("Accepting mission 1. Hiding exclamation mark.");
                }
                
                // 注意这里使用了正确的僵尸标签 (大写Z)
                AcceptMission("mission1", zombieMissionTitle, zombieMissionDesc, zombieMissionTarget, zombieTag);
            }
            else if (isMission2Available)
            {
                isMission2Available = false; // 任务已被接受
                
                // 先隐藏感叹号，再接受任务
                if (exclamationMark != null)
                {
                    exclamationMark.SetActive(false);
                    Debug.Log("Accepting mission 2. Hiding exclamation mark.");
                }
                
                // 使用Boss标签的僵尸王任务
                AcceptMission("mission2", bossMissionTitle, bossMissionDesc, bossMissionTarget, bossTag);
            }
        }
        else if (currentMission.status == Mission.MissionStatus.Completed)
        {
            // 提交任务
            string completedMissionId = currentMission.missionId;
            
            // 隐藏问号，再提交任务
            if (questionMark != null)
            {
                questionMark.SetActive(false);
                Debug.Log("Completing mission. Hiding question mark.");
            }
            
            CompleteMission();
            
            // 如果是第一个任务，完成后解锁第二个任务
            if (completedMissionId == "mission1")
            {
                isMission1Completed = true;
                isMission2Available = true;
                
                // 任务2解锁，显示感叹号
                if (exclamationMark != null)
                {
                    exclamationMark.SetActive(true);
                    Debug.Log("Mission 2 unlocked! Showing exclamation mark.");
                }
            }
        }
    }

    public void AcceptMission(string id, string title, string description, int target, string tag)
    {
        currentMission = new Mission(id, title, description, target, tag);
        currentMission.status = Mission.MissionStatus.InProgress;
        Debug.Log($"Mission accepted: {title} (Target tag: {tag}, Count: {target})");
        
        // 确保感叹号隐藏（添加额外保障）
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
        
        // 更新UI - 完整更新
        HideAllPrompts();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionTracker(currentMission, true);
        }
    }

    public void CompleteMission()
    {
        if (currentMission != null && currentMission.status == Mission.MissionStatus.Completed)
        {
            Debug.Log($"Mission delivered: {currentMission.missionTitle}");

            // 隐藏问号图标
            if (questionMark != null)
            {
                questionMark.SetActive(false);
            }

            // 更新 UI
            HideAllPrompts();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateMissionTracker(null, false);
            }

            // 保存当前任务 ID 后清除任务状态
            string completedId = currentMission.missionId;
            currentMission.status = Mission.MissionStatus.Delivered;
            currentMission = null;

            // 如果是 mission1，解锁 mission2
            if (completedId == "mission1")
            {
                isMission1Completed = true;
                isMission2Available = true;

                if (exclamationMark != null)
                {
                    exclamationMark.SetActive(true);
                    Debug.Log("Mission 2 unlocked! Showing exclamation mark.");
                }
            }

            // ✅ 如果是 mission2（Boss任务），播放完成UI并延迟退出游戏
            if (completedId == "mission2")
            {
                Debug.Log("🎯 Boss mission completed! Showing Mission Complete UI.");

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMissionCompleteMessage();
                }

                // 开始延迟结束游戏
                StartCoroutine(EndGameAfterDelay(3f));
            }
        }
    }
    private IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    public void UpdateMissionProgress(string tag, int amount = 1)
    {
        if (currentMission == null || 
            currentMission.status != Mission.MissionStatus.InProgress)
        {
            return;
        }
        
        // 检查标签是否匹配当前任务
        if (currentMission.targetTag == tag)
        {
            int oldProgress = currentMission.currentProgress;
            
            // 更新任务进度
            currentMission.UpdateProgress(amount);
            
            // 如果进度确实变化了，更新UI
            if (oldProgress != currentMission.currentProgress)
            {
                Debug.Log($"Progress changed from {oldProgress} to {currentMission.currentProgress}. Updating UI...");
                
                // 更新UI - 仅更新进度文本
                if (UIManager.Instance != null)
                {
                    // 使用两种方法确保UI更新
                    UIManager.Instance.UpdateProgressText();
                    UIManager.Instance.UpdateProgressText(currentMission.GetProgressText());
                    
                    // 如果上面两种方法都不起作用，尝试完整更新
                    UIManager.Instance.UpdateMissionTracker(currentMission, true);
                }
                
                // 如果任务完成，显示问号
                if (currentMission.status == Mission.MissionStatus.Completed && questionMark != null)
                {
                    questionMark.SetActive(true);
                    Debug.Log("Mission completed! Showing question mark.");
                }
            }
        }
        else
        {
            Debug.Log($"Tag '{tag}' doesn't match current mission target '{currentMission.targetTag}'");
        }
    }
}