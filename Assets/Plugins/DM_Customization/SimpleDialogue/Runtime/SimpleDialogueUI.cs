using System;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.Common.UnityUI;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace DM_Customization
{
    [AddComponentMenu("Game Creator/UI/Simple Dialogue UI")]
    [Icon(DMPaths.DMPATH + "SimpleDialogue/Editor/GizmoSimpleDialogue.png")]
    public class SimpleDialogueUI : MonoBehaviour
    {
#if UNITY_EDITOR

        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            IsOpen = false;
            EventOpen = null;
            EventClose = null;
        }
#endif
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        [SerializeField] private CollectorListVariable m_DialogueText = new CollectorListVariable();
        [SerializeField] private PropertyGetDecimal m_IntervalTime = GetDecimalDecimal.Create(3f);
        [SerializeField] private GameObject m_Container;
        [SerializeField] private TextReference m_Text = new TextReference();
        [SerializeField] private TextReference m_CurrentSpeaker = new TextReference();
        [SerializeField] private PropertyGetBool m_UseBillboard = new PropertyGetBool(true);
        [SerializeField] private TimeMode m_UpdateTime;

        // MEMBERS: -----------------------------------------------------------------------
        private Args m_Args;
        private Camera m_Camera;

        // PROPERTIES: ----------------------------------------------------------------------------
        [field: NonSerialized] public static bool IsOpen { get; private set; }
        [field: NonSerialized] public static bool IsTalking { get; private set; }

        // EVENTS. --------------------------------------------------------------------------------

        public static event Action EventOpen;
        public static event Action EventClose;
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            IsOpen = true;
            EventOpen?.Invoke();
        }
        private void Awake()
        {
            m_Camera = Camera.main;
            
            m_Container.gameObject.SetActive(false);
            if (m_DialogueText.TypeID.ToString().ToUpper() != "STRING") Debug.LogError($"{m_DialogueText} variable list in {this.name} is not of type STRING or is not assigned");

        }

        private void OnDisable()
        {
            IsOpen = false;
            EventClose?.Invoke();
        }


        // PUBLIC METHODS: ------------------------------------------------------------------------
        public void StartDialogue(string currentSpeaker = "")
        {
            IsOpen = true;
            IsTalking = true;
            if (m_Container.gameObject.activeInHierarchy == true) return;

            m_Container.gameObject.SetActive(true);

            if(m_CurrentSpeaker != null)
                m_CurrentSpeaker.Text = currentSpeaker;


            m_Text.Text = m_DialogueText.Get()[0].ToString();
            OnContinue();
        }

        public void EndDialogue()
        {
            m_Container.gameObject.SetActive(false);
            IsOpen = false;
            IsTalking = false;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        public static SimpleDialogueUI CreateFrom(GameObject gameObject, GameObject image, Text text)
        {
            SimpleDialogueUI simpleDialogueUI = gameObject.gameObject.AddComponent<SimpleDialogueUI>();
            simpleDialogueUI.m_Text = new TextReference(text);
            simpleDialogueUI.m_Container = image;

            return simpleDialogueUI;
        }
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void isUseBillboard()
        {
            this.transform.rotation = m_Camera.transform.rotation;
            this.transform.rotation = Quaternion.Euler(0f, this.transform.rotation.eulerAngles.y, 0f);
        }
        private async void OnContinue()
        {
            float duration = (float)m_IntervalTime.Get(m_Args);

            for (int i = 0; i < m_DialogueText.Get().Count; i++)
            {
                m_Text.Text = m_DialogueText.Get()[i].ToString();
                await this.Time(duration, m_UpdateTime);
            }
            
            EndDialogue();
        }
        private async Task Yield()
        {
            await Task.Yield();
        }

        protected async Task Time(float duration, TimeMode time)
        {
            float startTime = time.Time;
            while (time.Time < startTime + duration)
            {
                await Yield();
            }
        }
        private void LateUpdate()
        {
            if (IsTalking)
                if (this.m_UseBillboard.Get(this.m_Args))
                    isUseBillboard();
        }
    }
}