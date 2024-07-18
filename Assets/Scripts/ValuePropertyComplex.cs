using System;
using System.Globalization;
using System.Runtime.CompilerServices; 
using UnityEngine;

namespace Manager.Tool
{
    public struct 
        ValuePropertyComplex : IFormattable, IDisposable
    {
        [SerializeField] private float _value;

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public float BaseValue { get; private set; }

        private YuoNativeHasMap<int, float> baseValueMap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AddBaseValue(int key, float value)
        {
            if (baseValueMap.ContainsKey(key))
            {
                var old = baseValueMap[key];
                BaseValue -= old;
                BaseValue += value;
                baseValueMap[key] = value;
            }
            else
            {
                baseValueMap[key] = value;
                BaseValue += value;
            }

            UpdateValue();
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RemoveBaseValue(int key)
        {
            if (baseValueMap.ContainsKey(key))
            {
                var old = baseValueMap[key];
                BaseValue -= old;
                baseValueMap.Remove(key);
                UpdateValue();
            }

            return Value;
        }

        public float ExtraValue { get; private set; }

        private YuoNativeHasMap<int, float> extraValueMap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AddExtraValue(int key, float value)
        {
            if (extraValueMap.ContainsKey(key))
            {
                var old = extraValueMap[key];
                ExtraValue -= old;
                extraValueMap[key] = value;
                ExtraValue += value;
            }
            else
            {
                extraValueMap[key] = value;
                ExtraValue += value;
            }

            UpdateValue();
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RemoveExtraValue(int key)
        {
            if (extraValueMap.ContainsKey(key))
            {
                var old = extraValueMap[key];
                ExtraValue -= old;
                extraValueMap.Remove(key);
                UpdateValue();
            }

            return Value;
        }

        public float BaseRate { get; private set; }


        private YuoNativeHasMap<int, float> baseRateMap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AddBaseRate(int key, float value)
        {
            if (baseRateMap.ContainsKey(key))
            {
                var old = baseRateMap[key];
                BaseRate -= old;
                BaseRate += value;
                baseRateMap[key] = value;
            }
            else
            {
                baseRateMap[key] = value;
                BaseRate += value;
            }

            UpdateValue();
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RemoveBaseRate(int key)
        {
            if (baseRateMap.ContainsKey(key))
            {
                baseRateMap.Remove(key);
                BaseRate = 0;
                foreach (var kvPair in baseRateMap)
                {
                    BaseRate += kvPair.Value;
                }

                UpdateValue();
            }

            return Value;
        }

        public float ExtraRate { get; private set; }

        private YuoNativeHasMap<int, float> extraRateMap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AddExtraRate(int key, float value)
        {
            if (extraRateMap.ContainsKey(key))
            {
                var old = extraRateMap[key];
                extraRateMap[key] = value;
                ExtraRate -= old;
                ExtraRate += value;
            }
            else
            {
                extraRateMap[key] = value;
                ExtraRate += value;
            }

            UpdateValue();
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RemoveExtraRate(int key)
        {
            if (extraRateMap.ContainsKey(key))
            {
                var old = extraRateMap[key];
                extraRateMap.Remove(key);
                ExtraRate -= old;
                UpdateValue();
            }

            return Value;
        }

        public float TotalRate { get; private set; }

        private YuoNativeHasMap<int, float> totalRateMap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float AddTotalRate(int key, float value)
        {
            if (totalRateMap.ContainsKey(key))
            {
                var old = totalRateMap[key];
                TotalRate -= old;
                TotalRate += value;
                totalRateMap[key] = value;
            }
            else
            {
                totalRateMap[key] = value;
                TotalRate += value;
            }

            UpdateValue();
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RemoveTotalRate(int key)
        {
            if (totalRateMap.ContainsKey(key))
            {
                var old = totalRateMap[key];
                TotalRate -= old;
                UpdateValue();
            }

            return Value;
        }

        /// <summary>
        /// 更新数值,重新计算
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue()
        {
            Value = (BaseValue * BaseRate + ExtraValue * ExtraRate) * TotalRate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValuePropertyComplex(float baseValue = 0, float extraValue = 0,
            float baseRate = 1,
            float extraRate = 1,
            float totalRate = 1)
        {
            baseValueMap = new(8);
            extraValueMap = new(8);
            baseRateMap = new(8);
            extraRateMap = new(8);
            totalRateMap = new(8);
            BaseValue = baseValue;
            ExtraValue = extraValue;
            BaseRate = baseRate;
            ExtraRate = extraRate;
            TotalRate = totalRate;

            baseValueMap.SetValue(Int32.MinValue, BaseValue);
            extraValueMap.SetValue(Int32.MinValue, ExtraValue);
            baseRateMap.SetValue(Int32.MinValue, BaseRate);
            extraRateMap.SetValue(Int32.MinValue, ExtraRate);
            totalRateMap.SetValue(Int32.MinValue, TotalRate);

            _value = 0;
            UpdateValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValuePropertyComplex(ValuePropertyComplex copyData)
        {
            baseValueMap = new(8);
            extraValueMap = new(8);
            baseRateMap = new(8);
            extraRateMap = new(8);
            totalRateMap = new(8);
            BaseValue = copyData.BaseValue;
            ExtraValue = copyData.ExtraValue;
            BaseRate = copyData.BaseRate;
            ExtraRate = copyData.ExtraRate;
            TotalRate = copyData.TotalRate;

            foreach (var kvPair in copyData.baseValueMap)
            {
                baseValueMap.SetValue(kvPair.Key, kvPair.Value);
            }

            foreach (var kvPair in copyData.extraValueMap)
            {
                extraValueMap.SetValue(kvPair.Key, kvPair.Value);
            }

            foreach (var kvPair in copyData.baseRateMap)
            {
                baseRateMap.SetValue(kvPair.Key, kvPair.Value);
            }

            foreach (var kvPair in copyData.extraRateMap)
            {
                extraRateMap.SetValue(kvPair.Key, kvPair.Value);
            }

            foreach (var kvPair in copyData.totalRateMap)
            {
                totalRateMap.SetValue(kvPair.Key, kvPair.Value);
            }

            _value = 0;
            UpdateValue();
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(ValuePropertyComplex value) => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator +(ValuePropertyComplex a, ValuePropertyComplex b) => a.Value + b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator -(ValuePropertyComplex a, ValuePropertyComplex b) => a.Value - b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(ValuePropertyComplex a, ValuePropertyComplex b) => a.Value * b.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator /(ValuePropertyComplex a, ValuePropertyComplex b) => a.Value / b.Value;

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
            if (obj is ValuePropertyComplex other)
            {
                return Math.Abs(Value - other.Value) < 0.000001f;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public void Dispose()
        {
            baseValueMap.Dispose();
            extraValueMap.Dispose();
            baseRateMap.Dispose();
            extraRateMap.Dispose();
            totalRateMap.Dispose();
        }
    }
}