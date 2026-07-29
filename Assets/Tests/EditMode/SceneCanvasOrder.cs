using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace WordVenture.Tests
{
    /// <summary>
    /// 씬을 열지 않고 씬 파일에서 캔버스 정렬 순서를 읽는다. EditMode 테스트에서 씬을 열면
    /// 편집 중이던 씬이 바뀌기 때문에 텍스트로 읽는다.
    /// </summary>
    public static class SceneCanvasOrder
    {
        static readonly Regex SortingOrder = new Regex(@"^\s*m_SortingOrder:\s*(-?\d+)\s*$",
            RegexOptions.Multiline);

        public static int Highest()
        {
            int highest = int.MinValue;

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!File.Exists(scene.path))
                {
                    continue;
                }

                foreach (Match match in SortingOrder.Matches(File.ReadAllText(scene.path)))
                {
                    int order = int.Parse(match.Groups[1].Value);
                    if (order > highest)
                    {
                        highest = order;
                    }
                }
            }

            return highest == int.MinValue ? 0 : highest;
        }
    }
}
