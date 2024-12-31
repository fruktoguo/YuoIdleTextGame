using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YuoTools;

public class YuoBar : MonoBehaviour
{
    [BoxGroup("Value Settings")]
    [HorizontalGroup("Value Settings/Split")]
    [VerticalGroup("Value Settings/Split/Left")]
    [LabelText("最小值")]
    public float minValue = 0;

    [VerticalGroup("Value Settings/Split/Right")] [LabelText("最大值")]
    public float maxValue = 1;

    [BoxGroup("Value Settings")]
    [PropertyRange(nameof(minValue), nameof(maxValue))]
    [OnValueChanged(nameof(OnSliderValueChanged))]
    [LabelText("当前值")]
    [SerializeField]
    private float sliderValue;

    public float SliderValue
    {
        get => sliderValue;
        set
        {
            sliderValue = Mathf.Clamp(value, minValue, maxValue);
            OnSliderValueChanged(sliderValue);
        }
    }

    [BoxGroup("Display Settings")] [LabelText("文本格式")] [Tooltip("使用标准字符串格式。例如：\nf2 - 保留2位小数\np0 - 百分比格式\nn0 - 数字格式")]
    public string textFormat = "f2";

    [FoldoutGroup("References")] [Required] [SerializeField]
    public Image background;

    [FoldoutGroup("References")] [Required] [SerializeField]
    public Image bar;

    [FoldoutGroup("References")] [Required] [SerializeField]
    public TextMeshProUGUI text;

    [FoldoutGroup("References")] [ReadOnly] [SerializeField]
    public RectTransform rectTransform;

    private void OnValidate()
    {
        rectTransform ??= GetComponent<RectTransform>();
        background ??= transform.FindChildContains("BG")?.GetComponent<Image>();
        bar ??= transform.FindChildContains("Bar")?.GetComponent<Image>();
        text ??= transform.FindChildContains("Text")?.GetComponent<TextMeshProUGUI>();
    }

    void OnSliderValueChanged(float value)
    {
        value = (value - minValue) / (maxValue - minValue);
        var width = rectTransform.rect.width;
        var targetWidth = width * value;
        if (targetWidth < rectTransform.rect.height)
        {
            var scale = targetWidth / rectTransform.rect.height;
            bar.rectTransform.localScale = Vector3.one * scale;
            bar.rectTransform.SetSizeX(rectTransform.rect.height);
        }
        else
        {
            bar.rectTransform.SetSizeX(width * value);
            bar.rectTransform.localScale = Vector3.one;
        }

        text.text = sliderValue.ToString(textFormat);
    }
}