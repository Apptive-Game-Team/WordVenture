using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public enum Language
    {
        Korean,
        English,
    }

    /// <summary>
    /// 한국어 원문을 key 로 삼아 지금 언어의 문구를 돌려준다.
    ///
    /// 대사 에셋과 씬에는 한국어 원문만 두고, 번역은 Resources/Localization 의 JSON 표에서 찾는다.
    /// 표에 없는 문장은 원문 그대로 보여 주므로, 번역이 늦은 대사도 한국어로는 계속 읽힌다.
    /// </summary>
    public static class Localization
    {
        public const string LanguageKey = "Language";
        public const string EnglishTablePath = "Localization/English";

        static Language? current;
        static Dictionary<string, string> english;

        public static Language Current
        {
            get
            {
                if (current == null)
                    current = PlayerPrefs.HasKey(LanguageKey)
                        ? (Language)PlayerPrefs.GetInt(LanguageKey)
                        : Application.systemLanguage == SystemLanguage.Korean ? Language.Korean : Language.English;
                return current.Value;
            }
        }

        public static void SetLanguage(Language language)
        {
            current = language;
            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.Save();
        }

        public static string Translate(string korean)
        {
            if (Current == Language.Korean || string.IsNullOrEmpty(korean)) return korean;
            return EnglishTable.TryGetValue(korean.Trim(), out string translated) ? translated : korean;
        }

        public static IReadOnlyDictionary<string, string> EnglishTable
        {
            get
            {
                if (english == null) english = Load(Resources.Load<TextAsset>(EnglishTablePath));
                return english;
            }
        }

        public static Dictionary<string, string> Load(TextAsset asset)
        {
            var table = new Dictionary<string, string>();
            if (asset == null) return table;
            foreach (Entry entry in JsonUtility.FromJson<Table>(asset.text).entries)
                if (!string.IsNullOrEmpty(entry.ko) && !string.IsNullOrEmpty(entry.en))
                    table[entry.ko.Trim()] = entry.en;
            return table;
        }

        // 씬에 직렬화된 고정 문구(크레딧, 안내 문구 등)는 씬을 열 때 한 번 바꾼다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void TranslateLoadedScenes()
        {
            current = null;
            SceneManager.sceneLoaded -= TranslateScene;
            SceneManager.sceneLoaded += TranslateScene;
        }

        static void TranslateScene(Scene scene, LoadSceneMode mode)
        {
            if (Current == Language.Korean) return;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
                    label.text = Translate(label.text);
        }

        [Serializable]
        struct Entry
        {
            public string ko;
            public string en;
        }

        [Serializable]
        struct Table
        {
            public Entry[] entries;
        }
    }
}
