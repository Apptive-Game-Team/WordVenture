using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WordVenture.Tests
{
    /// <summary>
    /// 게임 코드는 predefined assembly(Assembly-CSharp)에 있어서 테스트 어셈블리가 타입을
    /// 참조할 수 없다. 그래서 에셋을 <see cref="SerializedObject"/>로 열어 필드 이름으로 검사한다.
    /// 필드 이름이 바뀌면 테스트가 깨지는데, 그게 바로 잡으려는 회귀다. 이름이 바뀌면
    /// 기존 에셋의 값이 조용히 사라진다.
    /// </summary>
    public static class ProjectAssets
    {
        public const string WaveDataFolder = "Assets/ScriptableObjects/Combat";
        public const string StageFolder = "Assets/ScriptableObjects/Stages";
        public const string EnemyDataContainerPath =
            "Assets/ScriptableObjects/Combat/EnemyDataContainer.asset";

        public static IEnumerable<string> WaveDataPaths()
        {
            return ScriptableObjectPaths(WaveDataFolder)
                .Where(path => path.EndsWith("BattleWaveData.asset"));
        }

        public static IEnumerable<string> StagePaths()
        {
            return ScriptableObjectPaths(StageFolder);
        }

        public static IEnumerable<string> ScriptableObjectPaths(string folder)
        {
            return AssetDatabase.FindAssets("t:ScriptableObject", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(path => path);
        }

        public static SerializedObject Load(string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            Assert(asset != null, path + " 을(를) 로드하지 못했다");
            return new SerializedObject(asset);
        }

        /// <summary>
        /// 프리팹에서 <paramref name="propertyName"/> 필드를 가진 컴포넌트를 찾는다.
        /// 타입을 직접 참조할 수 없으니 필드 이름으로 식별한다.
        /// </summary>
        public static SerializedObject FindComponentWithProperty(GameObject prefab, string propertyName)
        {
            foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                SerializedObject serialized = new SerializedObject(behaviour);
                if (serialized.FindProperty(propertyName) != null)
                {
                    return serialized;
                }
            }

            return null;
        }

        static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException(message);
            }
        }
    }
}
