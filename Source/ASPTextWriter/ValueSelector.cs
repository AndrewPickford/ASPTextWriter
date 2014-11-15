﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public interface IValueField<T>
    {
        string ToString(T value);
        T Parse(string text);
        bool TryParse(string text, out T value);
        bool CheckLimits(T value, T min, T max);
        void Clamp(ref T value, T min, T max);
        void Add(ref T value, T step);
        void Sub(ref T value, T step);
        bool NotEqual(T lhs, T rhs);
    }

    public class IntField : IValueField<int>
    {
        public string ToString(int value)
        {
            return value.ToString();
        }

        public int Parse(string text)
        {
            return int.Parse(text);
        }

        public bool TryParse(string text, out int value)
        {
            return int.TryParse(text, out value);
        }

        public bool CheckLimits(int value, int min, int max)
        {
            if (value >= min && value <= max) return true;
            else return false;
        }

        public void Clamp(ref int value, int min, int max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
        }

        public void Add(ref int value, int step)
        {
            value += step;
        }

        public void Sub(ref int value, int step)
        {
            value -= step;
        }

        public bool NotEqual(int lhs, int rhs)
        {
            if (lhs != rhs) return true;
            else return false;
        }
    }

    public class FloatField : IValueField<float>
    {
        public string ToString(float value)
        {
            return value.ToString();
        }

        public float Parse(string text)
        {
            return float.Parse(text);
        }

        public bool TryParse(string text, out float value)
        {
            return float.TryParse(text, out value);
        }

        public bool CheckLimits(float value, float min, float max)
        {
            if (value >= min && value <= max) return true;
            else return false;
        }

        public void Clamp(ref float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
        }

        public void Add(ref float value, float step)
        {
            value += step;
        }

        public void Sub(ref float value, float step)
        {
            value -= step;
        }

        public bool NotEqual(float lhs, float rhs)
        {
            if (lhs != rhs) return true;
            else return false;
        }
    }

    public class ValueSelector<T, U>
        where T : new()
        where U : IValueField<T>, new()
    {
        private U _field;
        private T _value;
        private T _min;
        private T _max;
        private T _step;
        private string _label;
        private Color _color;
        private string _valueText;
        private float _lastRepeat;
        private float _autoRepeatGap;
        private float _lastButtonPress;
        private bool _changed;

        public T value() { return _value; }

        public ValueSelector(T value, T min, T max, T step, string label, Color color)
        {
            _field = new U();

            _value = value;
            _min = min;
            _max = max;
            _step = step;
            _label = label;
            _color = color;

            _valueText = _field.ToString(_value);

            _lastRepeat = Time.time;
            _autoRepeatGap = 0.4f;
            _lastButtonPress = _lastRepeat;
        }


        public bool draw()
        {
            Color contentColor = GUI.contentColor;
            bool repeatOK = false;
            if ((Time.time - _lastRepeat) > _autoRepeatGap) repeatOK = true;

            _changed = false;

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUI.contentColor = _color;
            GUILayout.Label(_label);
            GUI.contentColor = contentColor;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            string oldValueText = _valueText;
            _valueText = GUILayout.TextField(_valueText, GUILayout.Height(50), GUILayout.Width(50));
            _valueText = System.Text.RegularExpressions.Regex.Replace(_valueText, @"[^0-9.]", "");

            if (oldValueText != _valueText)
            {
                _lastButtonPress = Time.time;
                T v;
                bool ok = _field.TryParse(_valueText, out v);
                if (ok)
                {
                    if (_field.CheckLimits(v, _min, _max))
                    {
                        _value = v;
                        _changed = true;
                    }
                }
            }

            GUILayout.BeginVertical();
            if (GUILayout.RepeatButton("+", GUILayout.Height(25), GUILayout.Width(20)))
            {
                _lastButtonPress = Time.time;
                if (repeatOK)
                {
                    T v = _value;
                    _field.Add(ref v, _step);
                    _field.Clamp(ref v, _min, _max);
                    if (_field.NotEqual(v, _value))
                    {
                        _value = v;
                        _valueText = _field.ToString(_value);
                        _changed = true;
                    }
                }
            }

            if (GUILayout.RepeatButton("-", GUILayout.Height(25), GUILayout.Width(20)))
            {
                _lastButtonPress = Time.time;
                if (repeatOK)
                {
                    T v = _value;
                    _field.Sub(ref v, _step);
                    _field.Clamp(ref v, _min, _max);
                    if (_field.NotEqual(v, _value))
                    {
                        _value = v;
                        _valueText = _field.ToString(_value);
                        _changed = true;
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (_changed)
            {
                _lastRepeat = _lastButtonPress;
                _autoRepeatGap = _autoRepeatGap * 0.8f;
                if (_autoRepeatGap < 0.04f) _autoRepeatGap = 0.04f;
            }

            if ((Time.time - _lastButtonPress) > 0.3f) _autoRepeatGap = 0.4f;

            return _changed;
        }
    }
}
