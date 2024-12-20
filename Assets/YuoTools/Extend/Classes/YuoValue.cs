using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YuoTools
{
    [Serializable]
    [HideReferenceObjectPicker]
    public partial class YuoValue
    {
        /// <summary>
        /// 自定义显示值的事件,要返回一个字符串
        /// </summary>
        /// <returns></returns>
        [HideInInspector] public StringAction<YuoValue> CustomDisplay;

        public string GetValueOnInspector()
        {
            if (CustomDisplay != null)
            {
                return CustomDisplay.Invoke(this);
            }

            return Value.ToString("f2");
        }

        #region Core Values

        [BoxGroup("Core Values")]
        [HorizontalGroup("Core Values/Split")]
        [VerticalGroup("Core Values/Split/Left")]
        [LabelText("最终值")]
        [PropertyOrder(-1)]
        [ReadOnly]
        [ShowInInspector]
        public double Value { get; private set; }

        [VerticalGroup("Core Values/Split/Left")]
        [LabelText("基础值")]
        [ShowInInspector]
        public double BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                UpdateValue();
            }
        }

        private double baseValue;

        [VerticalGroup("Core Values/Split/Right")]
        [LabelText("附加值")]
        [ReadOnly]
        [ShowInInspector]
        public double AdditionalValues { get; private set; }

        [VerticalGroup("Core Values/Split/Right")]
        [LabelText("比率值")]
        [ReadOnly]
        [ShowInInspector]
        public double RatioValue { get; private set; }

        #endregion

        #region Modifiers

        [TabGroup("Modifiers", "数值修改器")]
        [LabelText("额外值")]
        [ShowInInspector]
        [ReadOnly]
        public double AddValue => CalculateSum(addValue);

        [TabGroup("Modifiers", "数值修改器")]
        [LabelText("额外值乘数")]
        [ShowInInspector]
        [ReadOnly]
        public double MulAddValue => CalculateMultiplier(mulAddValue);

        [TabGroup("Modifiers", "数值修改器")]
        [LabelText("基础值乘数")]
        [ShowInInspector]
        [ReadOnly]
        public double MulBaseValue => CalculateMultiplier(mulBaseValue);

        [TabGroup("Modifiers", "数值修改器")]
        [LabelText("最终值乘数")]
        [ShowInInspector]
        [ReadOnly]
        public double MulValue => CalculateMultiplier(mulValue);

        #endregion

        #region Modifier Lists

        [PropertyOrder(2)]
        [TabGroup("Modifiers", "修改器列表")]
        [TableList(ShowIndexLabels = true)]
        [LabelText("额外值列表")]
        [ShowInInspector]
        private Dictionary<int, double> addValue = new();

        [PropertyOrder(2)]
        [TabGroup("Modifiers", "修改器列表")]
        [TableList(ShowIndexLabels = true)]
        [LabelText("额外值乘数列表")]
        [ShowInInspector]
        private Dictionary<int, double> mulAddValue = new();

        [PropertyOrder(2)]
        [TabGroup("Modifiers", "修改器列表")]
        [TableList(ShowIndexLabels = true)]
        [LabelText("基础值乘数列表")]
        [ShowInInspector]
        private Dictionary<int, double> mulBaseValue = new();

        [PropertyOrder(2)]
        [TabGroup("Modifiers", "修改器列表")]
        [TableList(ShowIndexLabels = true)]
        [LabelText("最终值乘数列表")]
        [ShowInInspector]
        private Dictionary<int, double> mulValue = new();

        #endregion

        #region Constructors

        public YuoValue(double value)
        {
            baseValue = value;
            UpdateValue();
        }

        public YuoValue(float value)
        {
            baseValue = value;
            UpdateValue();
        }

        #endregion

        #region Value Change Actions

        private Dictionary<int, DoubleAction<YuoValue>> valueChange = new();

        public void AddValueChangeAction(int id, DoubleAction<YuoValue> action)
        {
            valueChange[id] = action;
            UpdateValue();
        }

        public void RemoveValueChangeAction(int id)
        {
            if (valueChange.ContainsKey(id))
            {
                valueChange.Remove(id);
            }
        }

        [HideInInspector] public Action<double, double> OnValueChange;

        #endregion

        #region Modifier Methods

        public void AddAddValue(int id, double value)
        {
            addValue[id] = value;
            UpdateValue();
        }

        public void RemoveAddValue(int id)
        {
            if (addValue.ContainsKey(id))
            {
                addValue.Remove(id);
                UpdateValue();
            }
        }

        public void AddMulAddValue(int id, double value)
        {
            mulAddValue[id] = value;
            UpdateValue();
        }

        public void RemoveMulAddValue(int id)
        {
            if (mulAddValue.ContainsKey(id))
            {
                mulAddValue.Remove(id);
                UpdateValue();
            }
        }

        public void AddMulBaseValue(int id, double value)
        {
            mulBaseValue[id] = value;
            UpdateValue();
        }

        public void RemoveMulBaseValue(int id)
        {
            if (mulBaseValue.ContainsKey(id))
            {
                mulBaseValue.Remove(id);
                UpdateValue();
            }
        }

        public void AddMulValue(int id, double value)
        {
            mulValue[id] = value;
            UpdateValue();
        }

        public void RemoveMulValue(int id)
        {
            if (mulValue.ContainsKey(id))
            {
                mulValue.Remove(id);
                UpdateValue();
            }
        }

        #endregion

        #region Update and Calculate Methods

        public void UpdateValue()
        {
            var oldValue = Value;
            Value = (baseValue * MulBaseValue + AddValue * MulAddValue) * MulValue;

            foreach (var action in valueChange.Values)
            {
                Value += action(this);
            }

            AdditionalValues = Value - baseValue;
            RatioValue = ValueToPercent(Value, RatioDivisor);
            OnValueChange?.Invoke(oldValue, Value);
        }

        [HideInInspector] public double RatioDivisor = 100;

        private double ValueToPercent(double value, double divisor = 100)
        {
            if (value >= 0)
                return 1 - value / (value + divisor);
            value = -value;
            return -(1 - value / (value + divisor));
        }

        private double CalculateSum(Dictionary<int, double> dict)
        {
            double sum = 0;
            foreach (var value in dict.Values)
            {
                sum += value;
            }

            return sum;
        }

        private double CalculateMultiplier(Dictionary<int, double> dict)
        {
            double multiplier = 1;
            foreach (var value in dict.Values)
            {
                multiplier += value;
            }

            return multiplier;
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            YuoValue other = (YuoValue)obj;
            return Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString("F1");
        }

        #endregion

        #region operator

        public static implicit operator double(YuoValue value) => value.Value;

        public static implicit operator float(YuoValue value) => (float)value.Value;

        public static implicit operator YuoValue(int value) => new(value);

        public static implicit operator YuoValue(double value) => new(value);

        public static implicit operator YuoValue(float value) => new(value);

        public static double operator +(YuoValue a, YuoValue b)
        {
            return a.Value + b.Value;
        }

        public static double operator -(YuoValue a, YuoValue b)
        {
            return a.Value - b.Value;
        }

        public static double operator *(YuoValue a, YuoValue b)
        {
            return a.Value * b.Value;
        }

        public static double operator /(YuoValue a, YuoValue b)
        {
            return a.Value / b.Value;
        }

        public static bool operator >(YuoValue a, YuoValue b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(YuoValue a, YuoValue b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >=(YuoValue a, YuoValue b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <=(YuoValue a, YuoValue b)
        {
            return a.Value <= b.Value;
        }

        #endregion

        #region operator double

        public static double operator +(YuoValue a, double b)
        {
            return a.Value + b;
        }

        public static double operator -(YuoValue a, double b)
        {
            return a.Value - b;
        }

        public static double operator *(YuoValue a, double b)
        {
            return a.Value * b;
        }

        public static double operator /(YuoValue a, double b)
        {
            return a.Value / b;
        }

        public static double operator +(double a, YuoValue b)
        {
            return a + b.Value;
        }

        public static double operator -(double a, YuoValue b)
        {
            return a - b.Value;
        }

        public static double operator *(double a, YuoValue b)
        {
            return a * b.Value;
        }

        public static double operator /(double a, YuoValue b)
        {
            return a / b.Value;
        }

        public static bool operator >(YuoValue a, double b)
        {
            return a.Value > b;
        }

        public static bool operator <(YuoValue a, double b)
        {
            return a.Value < b;
        }

        public static bool operator >=(YuoValue a, double b)
        {
            return a.Value >= b;
        }

        public static bool operator <=(YuoValue a, double b)
        {
            return a.Value <= b;
        }

        public static bool operator >(double a, YuoValue b)
        {
            return a > b.Value;
        }

        public static bool operator <(double a, YuoValue b)
        {
            return a < b.Value;
        }

        public static bool operator >=(double a, YuoValue b)
        {
            return a >= b.Value;
        }

        public static bool operator <=(double a, YuoValue b)
        {
            return a <= b.Value;
        }

        #endregion

        #region operator int

        public static bool operator >(int a, YuoValue b)
        {
            return a > b.Value;
        }

        public static bool operator <(int a, YuoValue b)
        {
            return a < b.Value;
        }

        public static bool operator >=(int a, YuoValue b)
        {
            return a >= b.Value;
        }

        public static bool operator <=(int a, YuoValue b)
        {
            return a <= b.Value;
        }

        public static bool operator >(YuoValue a, int b)
        {
            return a.Value > b;
        }

        public static bool operator <(YuoValue a, int b)
        {
            return a.Value < b;
        }

        public static bool operator >=(YuoValue a, int b)
        {
            return a.Value >= b;
        }

        public static bool operator <=(YuoValue a, int b)
        {
            return a.Value <= b;
        }

        #endregion

        #region operator float

        public static bool operator >(float a, YuoValue b)
        {
            return a > b.Value;
        }

        public static bool operator <(float a, YuoValue b)
        {
            return a < b.Value;
        }

        public static bool operator >=(float a, YuoValue b)
        {
            return a >= b.Value;
        }

        public static bool operator <=(float a, YuoValue b)
        {
            return a <= b.Value;
        }

        public static bool operator >(YuoValue a, float b)
        {
            return a.Value > b;
        }

        public static bool operator <(YuoValue a, float b)
        {
            return a.Value < b;
        }

        public static bool operator >=(YuoValue a, float b)
        {
            return a.Value >= b;
        }

        public static bool operator <=(YuoValue a, float b)
        {
            return a.Value <= b;
        }

        public static double operator +(YuoValue a, float b)
        {
            return a.Value + b;
        }

        public static double operator -(YuoValue a, float b)
        {
            return a.Value - b;
        }

        public static double operator *(YuoValue a, float b)
        {
            return a.Value * b;
        }

        public static double operator /(YuoValue a, float b)
        {
            return a.Value / b;
        }

        public static double operator +(float a, YuoValue b)
        {
            return a + b.Value;
        }

        public static double operator -(float a, YuoValue b)
        {
            return a - b.Value;
        }

        public static double operator *(float a, YuoValue b)
        {
            return a * b.Value;
        }

        public static double operator /(float a, YuoValue b)
        {
            return a / b.Value;
        }

        #endregion

        #region operator long

        public static bool operator >(long a, YuoValue b)
        {
            return a > b.Value;
        }

        public static bool operator <(long a, YuoValue b)
        {
            return a < b.Value;
        }

        public static bool operator >=(long a, YuoValue b)
        {
            return a >= b.Value;
        }

        public static bool operator <=(long a, YuoValue b)
        {
            return a <= b.Value;
        }

        public static bool operator >(YuoValue a, long b)
        {
            return a.Value > b;
        }

        public static bool operator <(YuoValue a, long b)
        {
            return a.Value < b;
        }

        public static bool operator >=(YuoValue a, long b)
        {
            return a.Value >= b;
        }

        public static bool operator <=(YuoValue a, long b)
        {
            return a.Value <= b;
        }

        #endregion
    }
}