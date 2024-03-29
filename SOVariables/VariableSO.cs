﻿using SO.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace SO
{
    //  [CreateAssetMenu(fileName = "SOEvent", menuName = "SO/Event")]
    public abstract class VariableSO<T> : IVariableSO, ISerializationCallbackReceiver
    {

        //Value
        [HideInInspector]
        public T Value { get { return GetValue(); } set { SetValue(value); } }
        [SerializeField]
        protected T m_value;

        //When the game starts, the starting Value we use (so we can reset if need be)
        [SerializeField]
        private T startingValue = default(T);

        public static implicit operator T(VariableSO<T> v)
        {
            return v.Value;
        }
        //public static implicit operator VariableSO<T>(T v)
        //{
        //    this.value = v;
        //    return this;
        //}

        public virtual void SetValue(T newValue, bool forceUpdate = false, bool log = false)
        {
            if (log) Debuger.Log("SetValue: " + newValue + " on " + this.name);
            if ((m_value == null && newValue != null) || forceUpdate || (m_value != null && !m_value.Equals(newValue)))
            {
                m_value = newValue;

                if (allowCache)
                {
                    CasheValue();
                }
                RaisEvents();
            }
        }

        public virtual void SetValue(VariableSO<T> variableSo)
        {
            SetValue(variableSo.Value);
        }



        public virtual T GetValue()
        {
            if (!isCacheRetrived && allowCache)
            {
                RetriveCache();
            }
            return m_value;
        }

        // <summary>
        // Recieve callback after unity deseriallzes the object
        // </summary>
        public void OnAfterDeserialize()
        {
            //if (!Application.isPlaying)
            //{
            //    _value = startingValue;
            //    UnSubscripeAll();
            //}
        }
        public void OnBeforeSerialize()
        {
            //if (!Application.isPlaying)
            //{
            //    UnSubscripeAll(); ResetValue();
            //}
        }


        /// <summary>
        /// Reset the Value to it's inital Value if it's resettable
        /// </summary>
        public override void ResetValue()
        {
            Value = startingValue;
            UnSubscripeAll();
        }
        public T GetDefultValue()
        {
            return Value;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Value);
        }

    }

    public abstract class IVariableSO : ManagedScriptableObject, IFormattable, System.Runtime.Serialization.ISerializable
    {
        public EventSO OnChanged;
        [Header("Cache value if changed \"dont use in update\"")]
        public bool allowCache = false;

        protected List<EventHandler> valChanged;
        List<EventHandler> supEvents = new List<EventHandler>();
        static List<IVariableSO> refToSoVars = new List<IVariableSO>();
        protected override void OnEnable()
        {
            base.OnEnable();
            Initialize();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (allowCache)
            {
                CasheValue();
            }
            else
                ResetValue();
        }
        protected virtual void Awake()
        {
            Initialize();
        }
        protected void Initialize()
        {
            if (refToSoVars == null) { refToSoVars = new List<IVariableSO>(); }
            if (!refToSoVars.Contains(this))
            {
                refToSoVars.Add(this);
            }

            if (!isCacheRetrived && allowCache)
            {
                RetriveCache();
            }

        }

        protected virtual void RaisEvents()
        {
            if (this.valChanged != null)
            {
                for (int i = 0; i < valChanged.Count; i++)
                {
                    try
                    {
                        valChanged[i].Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

            }
            if (OnChanged != null)
            {
                try
                {
                    OnChanged.Raise();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Subscripe(System.EventHandler onValChanged)
        {
            if (valChanged == null) valChanged = new List<EventHandler>();
            valChanged.Add(onValChanged);
            supEvents.Add(onValChanged);
        }
        public void UnSubscripe(System.EventHandler onValChanged)
        {
            if (valChanged != null) valChanged.Remove(onValChanged);
            supEvents.Remove(onValChanged);
        }
        public void UnSubscripeAll()
        {
            if (valChanged != null) valChanged.Clear();
            supEvents.Clear();
        }

        public abstract string ToString(string format, IFormatProvider formatProvider);

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public abstract void SetValue(string value);




        public static IVariableSO Parse(string inistanceID)
        {
            int id = 0;
            if (int.TryParse(inistanceID, out id))
            {
#if UNITY_EDITOR
                try
                {
                    return (IVariableSO)EditorUtility.InstanceIDToObject(id);
                }
                catch (Exception)
                {
                    Debug.LogError("cant find inistanceID: " + inistanceID);
                }
#endif
            }
            else
            {
                Debug.LogError("string is not inistanceID: " + inistanceID);
            }
            return null;
        }

        public static bool TryParse(string inistanceID, out IVariableSO variableSO)
        {
            try
            {
                variableSO = IVariableSO.Parse(inistanceID);
                return true;
            }
            catch (Exception)
            {

                variableSO = null;
                return false;
            }
        }

        protected override void OnBegin(bool isEditor)
        {
            if (allowCache)
            {
                RetriveCache();
            }
        }

        protected override void OnEnd(bool isEditor)
        {
            if (allowCache)
            {
                CasheValue();
            }

        }


        protected bool isCacheRetrived = false;
        protected void RetriveCache()
        {

            //if (PlayerPrefs.HasKey($"SOV{this.name}"))
            //{
            //    SetValue(PlayerPrefs.GetString($"SOV{this.name}"));
            //    //  Debug.Log("RetriveCache of " + this.name + "is " + PlayerPrefs.GetString($"SOV{this.name}"));
            //}
            //else
            //    ResetValue();

            if (!PlayerPrefs.HasKey($"SOV{this.name}"))
            {
                ResetValue();
                PlayerPrefs.SetString($"SOV{this.name}", null);
            }else
            {
                SetValue(PlayerPrefs.GetString($"SOV{this.name}"));
            }

            isCacheRetrived = true;
        }
        protected void CasheValue()
        {
            if (!PlayerPrefs.HasKey($"SOV{this.name}"))
            {
                ResetValue();
            }else
             PlayerPrefs.SetString($"SOV{this.name}", this.ToString());
            // Debug.Log("CashValue of "+ this.name+ "is " + PlayerPrefs.GetString($"SOV{this.name}"));
        }

        public void CopyToText(Text textComponent)
        {
            textComponent.text = this.ToString();
        }

        public void CopyToTMP_Text(TMPro.TMP_Text TMP_textComponent)
        {
            TMP_textComponent.text = this.ToString();
        }

        public void CopyToInputField(InputField InputFieldComponent)
        {
            InputFieldComponent.text = this.ToString();
        }

        public void CopyToScrollbar(Scrollbar ScrollbarComponent)
        {
            ScrollbarComponent.value = float.Parse(this.ToString());
        }

        public void CopyToSlider(Slider SliderComponent)
        {
            SliderComponent.value = float.Parse(this.ToString());
        }

        public void CopyToTMP_InputField(TMPro.TMP_InputField TMP_InputFieldComponent)
        {
            TMP_InputFieldComponent.text = this.ToString();
        }

        /// <summary>
        /// Reset the Value to it's inital Value if it's resettable
        /// </summary>
        public abstract void ResetValue();
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }

}

