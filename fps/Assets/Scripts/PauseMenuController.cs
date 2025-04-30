using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
        
        // 确保面板初始时是隐藏的
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // 设置按钮监听
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    private void Update()
    {
        // 检测ESC键，但仅在玩家未死亡时响应
        if (Input.GetKeyDown(KeyCode.Escape) && Player_Controller.Instance != null && !Player_Controller.Instance.IsDead())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ContinueGame();
        }
    }

    private void PauseGame()
    {
        // 显示暂停菜单
        pauseMenuPanel.SetActive(true);
        
        // 冻结游戏时间
        Time.timeScale = 0f;
        
        // 显示并解锁鼠标
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // 通知Player_Controller暂停
        if (Player_Controller.Instance != null)
        {
            Player_Controller.Instance.PauseGame();
        }
    }

    private void ContinueGame()
    {
        // 隐藏暂停菜单
        pauseMenuPanel.SetActive(false);
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        // 隐藏并锁定鼠标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // 通知Player_Controller恢复
        if (Player_Controller.Instance != null)
        {
            Player_Controller.Instance.ResumeGame();
        }
        
        isPaused = false;
    }

    private void ExitGame()
    {
        // 恢复时间比例，以防止在加载新场景时出现问题
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        // 如果你有一个主菜单场景，用它的名称替换"MainMenu"
        SceneManager.LoadScene("Title");
        
        // 如果你想直接退出游戏（在构建中有效，编辑器中无效）
        // Application.Quit();
    }
}