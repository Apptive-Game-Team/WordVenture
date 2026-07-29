using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 코드가 이름으로 부르는 씬은 전부 빌드 설정에 들어 있어야 한다. 빠지면 컴파일은
    /// 통과하고 그 씬으로 넘어가는 순간 런타임에서 죽는다.
    /// </summary>
    public sealed class BuildSettingsTests
    {
        static readonly Regex LoadSceneCall = new Regex(@"LoadScene\s*\(\s*""([^""]+)""");

        [Test]
        public void 코드가_부르는_씬이_모두_빌드_설정에_있다()
        {
            HashSet<string> registered = new HashSet<string>(
                EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => Path.GetFileNameWithoutExtension(scene.path)));

            List<string> missing = new List<string>();

            foreach (string file in Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories))
            {
                foreach (Match match in LoadSceneCall.Matches(File.ReadAllText(file)))
                {
                    string sceneName = match.Groups[1].Value;
                    if (!registered.Contains(sceneName))
                    {
                        missing.Add(sceneName + " (" + file + ")");
                    }
                }
            }

            Assert.That(missing, Is.Empty,
                "빌드 설정에 없는 씬을 로드한다: " + string.Join(", ", missing));
        }

        [Test]
        public void 빌드_설정의_씬_파일이_모두_존재한다()
        {
            List<string> missing = EditorBuildSettings.scenes
                .Select(scene => scene.path)
                .Where(path => !File.Exists(path))
                .ToList();

            Assert.That(missing, Is.Empty, "빌드 설정이 없는 씬을 가리킨다: " + string.Join(", ", missing));
        }
    }
}
