using UnityEngine;
using TMPro;

public class HorizontalGlowEffect : MonoBehaviour
{
    public Color glowColor = new Color(0.5f, 0.5f, 1f, 1f);
    public float glowIntensity = 1.0f;
    public float glowWidth = 0.3f;
    public float speed = 1.0f;

    private TextMeshProUGUI textMesh;
    private Material textMaterial;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        
        // 确保文本使用自定义材质
        textMesh.fontMaterial = new Material(Shader.Find("TextMeshPro/Distance Field"));
        textMaterial = textMesh.fontMaterial;
        
        // 设置初始参数
        textMaterial.EnableKeyword("GLOW_ON");
        textMaterial.SetColor("_GlowColor", glowColor);
        textMaterial.SetFloat("_GlowPower", glowIntensity);
        textMaterial.SetFloat("_GlowOuter", glowWidth);
    }
    
    void Update()
    {
        // 创建水平线动画效果
        float glowOffset = Mathf.PingPong(Time.time * speed, 1.0f);
        textMaterial.SetFloat("_GlowOffset", glowOffset);
    }
}