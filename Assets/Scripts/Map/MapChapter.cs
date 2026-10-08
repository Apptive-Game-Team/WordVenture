using UnityEngine;

namespace Map
{
    // 맵 한 장(2부 등)의 배경과 지점 위치. 1부 맵은 씬에 놓인 지점 오브젝트를 그대로 쓴다.
    [CreateAssetMenu(menuName = "Map/Map Chapter")]
    public sealed class MapChapter : ScriptableObject
    {
        [Tooltip("이 장의 첫 스테이지 번호. 지점은 이 번호부터 순서대로 놓인다.")]
        public int firstStageID;
        public Sprite background;
        [Tooltip("지점의 월드 좌표. 배경 그림의 픽셀 (px, py)는 ((px - 768) / 100 * 1.1913, (512 - py) / 100)이다.")]
        public Vector2[] stagePoints;
        [Tooltip("전투 데이터가 있어 들어갈 수 있는 마지막 스테이지 번호. 그 뒤의 지점은 보이기만 한다.")]
        public int lastPlayableStageID;

        public int LastStageID => firstStageID + stagePoints.Length - 1;
    }
}
