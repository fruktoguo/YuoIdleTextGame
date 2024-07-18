using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Manager.Tool
{
    public struct ValueProperty : IFormattable
    {
        [SerializeField] private float value;

        public float Value
        {
            get => value;
            private set => this.value = value;
        }

        [SerializeField] private float baseValue;

        /// <summary>
        /// 基础值
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set
            {
                baseValue = value;
                UpdateValue();
            }
        }

        [SerializeField] private float extraValue;

        /// <summary>
        /// 附加值
        /// </summary>
        public float ExtraValue
        {
            get => extraValue;
            set
            {
                extraValue = value;
                UpdateValue();
            }
        }

        [SerializeField] private float baseRate;

        /// <summary>
        /// 基础值倍率
        /// </summary>
        public float BaseRate
        {
            get => baseRate;
            set
            {
                baseRate = value;
                UpdateValue();
            }
        }

        [SerializeField] private float extraRate;

        /// <summary>
        /// 附加值倍率
        /// </summary>
        public float ExtraRate
        {
            get => extraRate;
            set
            {
                extraRate = value;
                UpdateValue();
            }
        }

        [SerializeField] private float totalRate;

        /// <summary>
        /// 总倍率
        /// </summary>
        public float TotalRate
        {
            get => totalRate;
            set
            {
                totalRate = value;
                UpdateValue();
            }
        }

        /// <summary>
        /// 更新数值,重新计算
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue()
        {
            Value = (BaseValue * BaseRate + ExtraValue * ExtraRate) * TotalRate;
        }

        public ValueProperty(float baseValue = 0, float extraValue = 0, float baseRate = 1, float extraRate = 1,
            float totalRate = 1)
        {
            this.baseValue = baseValue;
            this.extraValue = extraValue;
            this.baseRate = baseRate;
            this.extraRate = extraRate;
            this.totalRate = totalRate;
            value = 0;
            UpdateValue();
        }
        
        public ValueProperty(ValueProperty copyData)
        {
            baseValue = copyData.baseValue;
            extraValue = copyData.extraValue;
            baseRate = copyData.baseRate;
            extraRate = copyData.extraRate;
            totalRate = copyData.totalRate;
            value = copyData.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(ValueProperty value) => value.Value;

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static implicit operator RoleProperty(float value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator +(ValueProperty a, ValueProperty b) => a.Value + b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator -(ValueProperty a, ValueProperty b) => a.Value - b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(ValueProperty a, ValueProperty b) => a.Value * b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator /(ValueProperty a, ValueProperty b) => a.Value / b.Value;

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Value.ToString(format, formatProvider);
        }

        public override bool Equals(object obj)
        {
            if (obj is ValueProperty other)
            {
                return Math.Abs(Value - other.Value) < 0.000001f;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}