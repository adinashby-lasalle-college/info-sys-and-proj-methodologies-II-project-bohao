using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Interaction Prompts")]
    [SerializeField] private GameObject acceptMissionPrompt;
    [SerializeField] private TextMeshProUGUI acceptMissionText;
    [SerializeField] private GameObject completeMissionPrompt;
    [SerializeField] private TextMeshProUGUI completeMissionText;
    
    [Header("Mission Tracker")]
    [SerializeField] private GameObject missionTrackerPanel;
    [SerializeField] private TextMeshProUGUI missionTitleText;
    [SerializeField] private TextMeshProUGUI missionProgressText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;

    [Header("Mission Complete UI")]
    [SerializeField] private CanvasGroup missionCompleteGroup;
    [SerializeField] private TextMeshProUGUI missionCompleteText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip missionCompleteSound;
    // 当前任务引用，用于快速更新进度
    private Mission currentMission;
    
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
    
    void Start()
    {
        // 初始化时隐藏所有UI元素
        HideAllUI();
        
        // 设置提示文本
        if (acceptMissionText != null)
        {
            acceptMissionText.text = "按 E 接受任务";
        }
        
        if (completeMissionText != null)
        {
            completeMissionText.text = "按 E 提交任务";
        }
        
        // 检查UI组件是否存在
        if (missionProgressText == null)
        {
            Debug.LogError("Mission progress text is missing! Make sure to assign it in the inspector.");
        }
        
        if (missionTitleText == null)
        {
            Debug.LogError("Mission title text is missing! Make sure to assign it in the inspector.");
        }
    }
    
    public void HideAllUI()
    {
        if (acceptMissionPrompt != null) acceptMissionPrompt.SetActive(false);
        if (completeMissionPrompt != null) completeMissionPrompt.SetActive(false);
        if (missionTrackerPanel != null) missionTrackerPanel.SetActive(false);
    }
    
    public void ShowAcceptPrompt(bool show)
    {
        if (acceptMissionPrompt != null)
        {
            acceptMissionPrompt.SetActive(show);
        }
    }
    
    public void ShowCompletePrompt(bool show)
    {
        if (completeMissionPrompt != null)
        {
            completeMissionPrompt.SetActive(show);
        }
    }
    
    // 初始显示或完全更新任务追踪器
    public void UpdateMissionTracker(Mission mission, bool show)
    {
        if (missionTrackerPanel == null) return;
        
        // 保存对当前任务的引用
        currentMission = mission;
        
        if (show && mission != null)
        {
            // 更新任务信息
            if (missionTitleText != null)
            {
                missionTitleText.text = $"Current Mission: {mission.missionTitle}";
            }
            
            if (missionProgressText != null)
            {
                string progressText = mission.GetProgressText();
                missionProgressText.text = progressText;
                Debug.Log($"Updated mission progress UI to: {progressText}");
            }
            
            if (missionDescriptionText != null)
            {
                missionDescriptionText.text = mission.description;
            }
            
            // 显示任务追踪器面板
            if (!missionTrackerPanel.activeSelf)
            {
                missionTrackerPanel.SetActive(true);
            }
        }
        else
        {
            // 隐藏任务追踪器面板
            missionTrackerPanel.SetActive(false);
            currentMission = null;
        }
    }
    
    // 仅更新进度文本 - 为效率更高的UI更新提供
    public void UpdateProgressText()
    {
        if (currentMission != null && missionProgressText != null && missionTrackerPanel.activeSelf)
        {
            string progressText = currentMission.GetProgressText();
            missionProgressText.text = progressText;
            Debug.Log($"Progress text updated to: {progressText}");
        }
    }
    
    // 直接更新进度文本 - 供MissionSystem直接调用
    public void UpdateProgressText(string progressText)
    {
        if (missionProgressText != null && missionTrackerPanel.activeSelf)
        {
            missionProgressText.text = progressText;
            Debug.Log($"Progress text directly updated to: {progressText}");
        }
    }
    public void ShowMissionCompleteMessage()
    {
        StopAllCoroutines(); // 清除前一个UI协程
        StartCoroutine(FadeMissionCompleteUI());
    }
    private IEnumerator FadeMissionCompleteUI()
    {
        missionCompleteText.text = "MISSION COMPLETE";
        missionCompleteGroup.alpha = 0;
        missionCompleteGroup.gameObject.SetActive(true);

        if (audioSource != null && missionCompleteSound != null)
            audioSource.PlayOneShot(missionCompleteSound);

        // 渐显
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            missionCompleteGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // 渐隐
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            missionCompleteGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        missionCompleteGroup.gameObject.SetActive(false);
    }

}