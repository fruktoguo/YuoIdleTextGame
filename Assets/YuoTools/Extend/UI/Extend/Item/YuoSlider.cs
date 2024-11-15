using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YuoTools.UI
{
    public class YuoSlider : MonoBehaviour, IGenerateCode
    {
        public Slider Slider;
        public TextMeshProUGUI Title;
        public TMP_InputField InputField;

        public string Format = "F2";

        public UnityEvent<float> OnValueChange;

        // Start is called before the first frame update
        void Awake()
        {
            Slider.onValueChanged.AddListener(OnSliderChange);
            InputField.onValueChanged.AddListener(OnInputFieldChange);
            OnSliderChange(Slider.value);
        }

        private void OnInputFieldChange(string value)
        {
            Slider.value = float.Parse(value);
        }

        void OnSliderChange(float value)
        {
            InputField.SetTextWithoutNotify(value.ToString(Format));
            OnValueChange?.Invoke(value);
        }
    }
}