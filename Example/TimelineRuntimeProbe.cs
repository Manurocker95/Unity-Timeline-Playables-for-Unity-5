using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace VirtualPhenix.Testing
{

    public class TimelineRuntimeProbe : MonoBehaviour
    {
        [Header("Animation Probe")]
        [SerializeField] private AnimationClip m_TestClip;
        [SerializeField] private Animator m_TestAnimator;

        [Header("Audio Probe")]
        [SerializeField] private AudioClip m_TestAudioClip;
        [SerializeField] private AudioMixerGroup m_TestAudioMixerGroup;
        [SerializeField] private bool m_LoopAudio = true;

        private PlayableGraph m_Graph;
        private TimelineAsset m_TimelineAsset;
        private PlayableHandle m_TimelineRoot;
        private PlayableHandle m_CallbackPlayable;
        private ScriptPlayableOutput m_Output;
        private PlayableHandle m_AnimationClipHandle;
        private PlayableHandle m_AnimationMixerHandle;
        private AnimationPlayableOutput m_AnimationOutput;

        private PlayableHandle m_AudioClipHandle;
        private PlayableHandle m_AudioMixerHandle;
        private AudioPlayableOutput m_AudioOutput;

        private int m_Frame;

        private void Start()
        {
            try
            {
                m_TimelineAsset =
                    ScriptableObject.CreateInstance<TimelineAsset>();

                Debug.Log(
                    "[TimelineProbe] TimelineAsset created: " +
                    (m_TimelineAsset != null));

                m_Graph = PlayableGraph.Create();

                Debug.Log(
                    "[TimelineProbe] Graph created. Valid: " +
                    m_Graph.IsValid());

                m_TimelineRoot =
                    m_TimelineAsset.CreatePlayable(
                        m_Graph,
                        gameObject);

                Debug.Log(
                    "[TimelineProbe] Timeline root valid: " +
                    m_TimelineRoot.IsValid());

                m_CallbackPlayable =
                    m_Graph.CreateScriptPlayable<CallbackProbePlayable>();

                Debug.Log(
                    "[TimelineProbe] Callback playable valid: " +
                    m_CallbackPlayable.IsValid());

                m_CallbackPlayable.duration = 2.0;
                m_CallbackPlayable.speed = 1.0;

                m_Output = m_Graph.CreateScriptOutput("CallbackProbe");
                m_Output.sourcePlayable = m_CallbackPlayable;
                m_Output.sourceInputPort = 0;
                m_Output.userData = gameObject;

                Debug.Log(
                    "[TimelineProbe] Output valid: " +
                    m_Output.IsValid());

                CreateAnimationProbe();
                CreateAudioProbe();

                Debug.Log(
                    "[TimelineProbe] Playable count: " +
                    m_Graph.playableCount);

                Debug.Log(
                    "[TimelineProbe] Root count: " +
                    m_Graph.rootPlayableCount);

                m_Graph.Play();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[TimelineProbe] Failed:\n" +
                    exception);
            }
        }

        private void Update()
        {
            if (!m_Graph.IsValid())
                return;

            m_Graph.Evaluate(Time.deltaTime);
            ++m_Frame;

            if (m_Frame % 30 == 0 &&
                m_CallbackPlayable.IsValid())
            {
                Debug.Log(
                    "[TimelineProbe] Time: " +
                    m_CallbackPlayable.time.ToString("F3") +
                    " / " +
                    m_CallbackPlayable.duration.ToString("F3") +
                    " | State: " +
                    m_CallbackPlayable.playState +
                    " | Done: " +
                    m_CallbackPlayable.isDone);

                if (m_AnimationClipHandle.IsValid())
                {
                    Debug.Log(
                        "[TimelineProbe] Animation clip time: " +
                        m_AnimationClipHandle.time.ToString("F3") +
                        " / " +
                        m_AnimationClipHandle.duration.ToString("F3") +
                        " | State: " +
                        m_AnimationClipHandle.playState +
                        " | Done: " +
                        m_AnimationClipHandle.isDone);
                }

                if (m_AudioClipHandle.IsValid())
                {
                    AudioClipPlayable audioPlayable =
                        m_AudioClipHandle.GetObject<AudioClipPlayable>();

                    Debug.Log(
                        "[TimelineProbe] Audio clip time: " +
                        m_AudioClipHandle.time.ToString("F3") +
                        " / " +
                        m_AudioClipHandle.duration.ToString("F3") +
                        " | State: " +
                        m_AudioClipHandle.playState +
                        " | Done: " +
                        m_AudioClipHandle.isDone +
                        " | IsPlaying: " +
                        (audioPlayable != null &&
                         audioPlayable.isPlaying));
                }
            }
        }


        private void CreateAnimationProbe()
        {
            if (m_TestClip == null)
            {
                Debug.LogWarning(
                    "[TimelineProbe] No AnimationClip assigned. " +
                    "Animation test skipped.");
                return;
            }

            m_AnimationClipHandle =
                m_Graph.CreateAnimationClipPlayable(m_TestClip);

            Debug.Log(
                "[TimelineProbe] Animation clip handle valid: " +
                m_AnimationClipHandle.IsValid());

            AnimationClipPlayable clipPlayable =
                m_AnimationClipHandle.GetObject<AnimationClipPlayable>();

            Debug.Log(
                "[TimelineProbe] Animation clip object created: " +
                (clipPlayable != null));

            if (clipPlayable != null)
            {
                Debug.Log(
                    "[TimelineProbe] Animation clip matches: " +
                    (clipPlayable.clip == m_TestClip));

                Debug.Log(
                    "[TimelineProbe] Animation clip duration: " +
                    m_AnimationClipHandle.duration.ToString("F3"));

                clipPlayable.speed = 1f;
                clipPlayable.applyFootIK = false;
            }

            m_AnimationMixerHandle =
                m_Graph.CreateAnimationMixerPlayable(2, false);

            Debug.Log(
                "[TimelineProbe] Animation mixer valid: " +
                m_AnimationMixerHandle.IsValid());

            Debug.Log(
                "[TimelineProbe] Animation mixer input count: " +
                m_AnimationMixerHandle.inputCount);

            bool connected = m_Graph.Connect(
                m_AnimationClipHandle,
                0,
                m_AnimationMixerHandle,
                0);

            Debug.Log(
                "[TimelineProbe] Animation clip connected: " +
                connected);

            m_AnimationMixerHandle.SetInputWeight(0, 1f);

            Debug.Log(
                "[TimelineProbe] Mixer input matches clip: " +
                (m_AnimationMixerHandle.GetInput(0) ==
                 m_AnimationClipHandle));

            Debug.Log(
                "[TimelineProbe] Mixer weight 0: " +
                m_AnimationMixerHandle.GetInputWeight(0).ToString("F3"));

            m_AnimationOutput = m_Graph.CreateAnimationOutput(
                "AnimationProbe",
                m_TestAnimator);

            Debug.Log(
                "[TimelineProbe] Animation output valid: " +
                m_AnimationOutput.IsValid());

            m_AnimationOutput.sourcePlayable =
                m_AnimationMixerHandle;
            m_AnimationOutput.sourceInputPort = 0;
            m_AnimationOutput.weight = 1f;
            m_AnimationOutput.userData = gameObject;

            Debug.Log(
                "[TimelineProbe] Animation target matches: " +
                (m_AnimationOutput.target == m_TestAnimator));

            Debug.Log(
                "[TimelineProbe] Animation source matches mixer: " +
                (m_AnimationOutput.sourcePlayable ==
                 m_AnimationMixerHandle));

            Debug.Log(
                "[TimelineProbe] Animation output count: " +
                m_Graph.GetAnimationOutputCount());

            if (m_Graph.GetAnimationOutputCount() > 0)
            {
                AnimationPlayableOutput output =
                    m_Graph.GetAnimationOutput(0);

                Debug.Log(
                    "[TimelineProbe] Retrieved output valid: " +
                    output.IsValid());

                Debug.Log(
                    "[TimelineProbe] Retrieved output source matches: " +
                    (output.sourcePlayable ==
                     m_AnimationMixerHandle));
            }
        }


        private void CreateAudioProbe()
        {
            if (m_TestAudioClip == null)
            {
                Debug.LogWarning(
                    "[TimelineProbe] No AudioClip assigned. " +
                    "Audio test skipped.");
                return;
            }

            m_AudioClipHandle =
                m_Graph.CreateAudioClipPlayable(
                    m_TestAudioClip,
                    m_LoopAudio);

            Debug.Log(
                "[TimelineProbe] Audio clip handle valid: " +
                m_AudioClipHandle.IsValid());

            if (!m_AudioClipHandle.IsValid())
                return;

            AudioClipPlayable clipPlayable =
                m_AudioClipHandle.GetObject<AudioClipPlayable>();

            Debug.Log(
                "[TimelineProbe] AudioClipPlayable object created: " +
                (clipPlayable != null));

            if (clipPlayable != null)
            {
                Debug.Log(
                    "[TimelineProbe] Audio clip matches: " +
                    (clipPlayable.clip == m_TestAudioClip));

                Debug.Log(
                    "[TimelineProbe] Audio loop matches: " +
                    (clipPlayable.looped == m_LoopAudio));

                Debug.Log(
                    "[TimelineProbe] Audio clip duration: " +
                    m_AudioClipHandle.duration.ToString("F3"));
            }

            m_AudioMixerHandle =
                m_Graph.CreateAudioMixerPlayable(
                    1,
                    false);

            Debug.Log(
                "[TimelineProbe] Audio mixer valid: " +
                m_AudioMixerHandle.IsValid());

            if (!m_AudioMixerHandle.IsValid())
                return;

            Debug.Log(
                "[TimelineProbe] Audio mixer input count: " +
                m_AudioMixerHandle.inputCount);

            bool connected = m_Graph.Connect(
                m_AudioClipHandle,
                0,
                m_AudioMixerHandle,
                0);

            Debug.Log(
                "[TimelineProbe] Audio clip connected: " +
                connected);

            m_AudioMixerHandle.SetInputWeight(0, 1f);

            Debug.Log(
                "[TimelineProbe] Audio mixer input matches clip: " +
                (m_AudioMixerHandle.GetInput(0) ==
                 m_AudioClipHandle));

            Debug.Log(
                "[TimelineProbe] Audio mixer weight 0: " +
                m_AudioMixerHandle
                    .GetInputWeight(0)
                    .ToString("F3"));

            m_AudioOutput =
                m_Graph.CreateAudioOutput(
                    "AudioProbe",
                    m_TestAudioMixerGroup);

            Debug.Log(
                "[TimelineProbe] Audio output valid: " +
                m_AudioOutput.IsValid());

            if (!m_AudioOutput.IsValid())
                return;

            m_AudioOutput.sourcePlayable =
                m_AudioMixerHandle;

            m_AudioOutput.sourceInputPort = 0;
            m_AudioOutput.userData = gameObject;

            Debug.Log(
                "[TimelineProbe] Audio output target matches: " +
                (m_AudioOutput.target ==
                 m_TestAudioMixerGroup));

            Debug.Log(
                "[TimelineProbe] Audio output source matches mixer: " +
                (m_AudioOutput.sourcePlayable ==
                 m_AudioMixerHandle));

            Debug.Log(
                "[TimelineProbe] Audio clip should begin when graph plays.");
        }

        private void OnDestroy()
        {
            if (m_Graph.IsValid())
                m_Graph.Stop();

            if (m_Graph.IsValid() && m_AudioOutput.IsValid())
                m_Graph.DestroyOutput(m_AudioOutput);

            if (m_Graph.IsValid() && m_AnimationOutput.IsValid())
                m_Graph.DestroyOutput(m_AnimationOutput);

            if (m_Graph.IsValid() && m_Output.IsValid())
                m_Graph.DestroyOutput(m_Output);

            if (m_Graph.IsValid() && m_AudioMixerHandle.IsValid())
                m_Graph.DestroyPlayable(m_AudioMixerHandle);

            if (m_Graph.IsValid() && m_AudioClipHandle.IsValid())
                m_Graph.DestroyPlayable(m_AudioClipHandle);

            if (m_Graph.IsValid() && m_AnimationMixerHandle.IsValid())
                m_Graph.DestroyPlayable(m_AnimationMixerHandle);

            if (m_Graph.IsValid() && m_AnimationClipHandle.IsValid())
                m_Graph.DestroyPlayable(m_AnimationClipHandle);

            if (m_Graph.IsValid() && m_CallbackPlayable.IsValid())
                m_Graph.DestroyPlayable(m_CallbackPlayable);

            if (m_Graph.IsValid() && m_TimelineRoot.IsValid())
                m_Graph.DestroyPlayable(m_TimelineRoot);

            if (m_Graph.IsValid())
                m_Graph.Destroy();

            if (m_TimelineAsset != null)
                Destroy(m_TimelineAsset);
        }

        private sealed class CallbackProbePlayable :
            IPlayable,
            IScriptPlayable
        {
            private PlayableHandle m_PlayableHandle;

            public PlayableHandle playableHandle
            {
                get { return m_PlayableHandle; }
                set { m_PlayableHandle = value; }
            }

            public void OnGraphStart()
            {
                Debug.Log("[TimelineProbe] Callback: OnGraphStart");
            }

            public void OnGraphStop()
            {
                Debug.Log("[TimelineProbe] Callback: OnGraphStop");
            }

            public void PrepareFrame(FrameData info)
            {
                if (info.frameId % 30UL == 0UL)
                {
                    Debug.Log(
                        "[TimelineProbe] Callback: PrepareFrame " +
                        "Frame=" + info.frameId +
                        " Delta=" + info.deltaTime.ToString("F4") +
                        " Speed=" + info.effectiveSpeed.ToString("F2"));
                }
            }

            public void ProcessFrame(FrameData info, object playerData)
            {
                if (info.frameId % 30UL == 0UL)
                {
                    Debug.Log(
                        "[TimelineProbe] Callback: ProcessFrame " +
                        "Frame=" + info.frameId +
                        " UserData=" +
                        (playerData != null));
                }
            }

            public void OnPlayStateChanged(
                FrameData info,
                PlayState newState)
            {
                Debug.Log(
                    "[TimelineProbe] Callback: OnPlayStateChanged -> " +
                    newState);
            }
        }
    }

}