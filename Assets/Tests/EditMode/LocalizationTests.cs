using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    public sealed class LocalizationTests
    {
        const string FontPath = "Assets/Art/Fonts/NeoDunggeunmoPro-Regular SDF.asset";
        static readonly Regex Hangul = new Regex("[가-힣]");
        static readonly Regex RichTextTag = new Regex("<[^>]+>");

        static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name)).First(type => type != null);

        static IReadOnlyDictionary<string, string> EnglishTable() =>
            (IReadOnlyDictionary<string, string>)RuntimeType("Core.Localization")
                .GetProperty("EnglishTable").GetValue(null);

        [Test]
        public void 영어_문구는_폰트_atlas_에_있는_글자만_쓴다()
        {
            SerializedProperty characters = ProjectAssets.Load(FontPath).FindProperty("m_CharacterTable");
            var glyphs = new HashSet<int>();
            for (int i = 0; i < characters.arraySize; i++)
                glyphs.Add((int)characters.GetArrayElementAtIndex(i).FindPropertyRelative("m_Unicode").longValue);

            IReadOnlyDictionary<string, string> table = EnglishTable();
            Assert.That(table, Is.Not.Empty, "Resources/Localization/English.json 을 읽지 못했다");
            var missing = new List<string>();
            foreach (string english in table.Values)
                foreach (char letter in RichTextTag.Replace(english, ""))
                    if (!glyphs.Contains(letter)) missing.Add("'" + letter + "' in \"" + english + "\"");
            Assert.That(missing, Is.Empty, "폰트 atlas 에 없는 글자는 네모로 보인다");
        }

        [Test]
        public void 모든_대사_에셋의_한국어_문장에_영어_번역이_있다()
        {
            IReadOnlyDictionary<string, string> table = EnglishTable();
            var missing = new List<string>();
            foreach (string path in ProjectAssets.ScriptableObjectPaths("Assets"))
            {
                if (path.StartsWith("Assets/ThirdParty/")) continue;
                foreach (string korean in KoreanOnly(DialogueStrings(ProjectAssets.Load(path))))
                    if (!table.ContainsKey(korean.Trim())) missing.Add(path + ": " + korean);
            }
            Assert.That(missing, Is.Empty, "Resources/Localization/English.json 에 번역을 추가해야 한다");
        }

        [Test]
        public void 빌드_씬의_한국어_고정_문구에_영어_번역이_있다()
        {
            IReadOnlyDictionary<string, string> table = EnglishTable();
            var missing = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                foreach (Match text in Regex.Matches(System.IO.File.ReadAllText(scene.path), "m_text: \"(.*)\""))
                {
                    string korean = Regex.Unescape(text.Groups[1].Value);
                    if (Hangul.IsMatch(korean) && !table.ContainsKey(korean.Trim()))
                        missing.Add(scene.path + ": " + korean);
                }
            Assert.That(missing, Is.Empty, "Resources/Localization/English.json 에 번역을 추가해야 한다");
        }

        // 대사 컨테이너(script)와 지역 대화(chapters)에서 화면에 나오는 한국어 문자열을 모은다.
        static IEnumerable<string> DialogueStrings(SerializedObject asset)
        {
            SerializedProperty script = asset.FindProperty("script");
            if (script != null && script.isArray)
                for (int i = 0; i < script.arraySize; i++)
                    foreach (string field in new[] { "name", "text" })
                        yield return script.GetArrayElementAtIndex(i).FindPropertyRelative(field)?.stringValue;

            SerializedProperty chapters = asset.FindProperty("chapters");
            if (chapters == null || !chapters.isArray) yield break;
            for (int i = 0; i < chapters.arraySize; i++)
            {
                SerializedProperty chapter = chapters.GetArrayElementAtIndex(i);
                yield return chapter.FindPropertyRelative("title").stringValue;
                yield return chapter.FindPropertyRelative("speakerName").stringValue;
                SerializedProperty lines = chapter.FindPropertyRelative("lines");
                for (int j = 0; j < lines.arraySize; j++)
                    yield return lines.GetArrayElementAtIndex(j).FindPropertyRelative("text").stringValue;
            }
        }

        static IEnumerable<string> KoreanOnly(IEnumerable<string> values) =>
            values.Where(value => value != null && Hangul.IsMatch(value));
    }
}
