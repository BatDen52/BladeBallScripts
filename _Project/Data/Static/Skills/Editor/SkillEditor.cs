using System;
using System.Linq;
using System.Reflection;
using _Project.Data.Static.Skills;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Editor
{
    [CustomEditor(typeof(Skill), true)]
    public class SkillEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            Skill skill = (Skill)target;

            FieldInfo[] soundFields = typeof(Skill).GetFields()
                .Where(field => field.FieldType == typeof(Sound))
                .ToArray();

            foreach (var soundField in soundFields)
            {
                Sound sound = (Sound)soundField.GetValue(skill);
                if (sound == null)
                {
                    sound = new Sound();
                    soundField.SetValue(skill, sound);
                }

                EditorGUILayout.LabelField(soundField.Name, EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Play Min"))
                {
                    PlaySoundWithPitch(sound, sound.MinPitch);
                }

                if (GUILayout.Button("Play Max"))
                {
                    PlaySoundWithPitch(sound, sound.MaxPitch);
                }

                if (GUILayout.Button("Play Random"))
                {
                    PlayRandomSound(sound);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PlaySoundWithPitch(Sound sound, float pitch)
        {
            if (sound != null && sound.AudioClips != null && sound.AudioClips.Length > 0)
            {
                int randomIndex = Random.Range(0, sound.AudioClips.Length);
                AudioClip clip = sound.AudioClips[randomIndex];

                PlaySound(clip, pitch);
            }
        }

        private void PlayRandomSound(Sound sound)
        {
            if (sound != null && sound.AudioClips != null && sound.AudioClips.Length > 0)
            {
                int randomIndex = Random.Range(0, sound.AudioClips.Length);
                AudioClip clip = sound.AudioClips[randomIndex];
                float pitch = Random.Range(sound.MinPitch, sound.MaxPitch);

                PlaySound(clip, pitch);
            }
        }

        private async void PlaySound(AudioClip clip, float pitch)
        {
            AudioSource audioSource = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length));
            DestroyImmediate(audioSource.gameObject);
        }
    }
}
