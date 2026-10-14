using Combat.UI;
using Core;
using UnityEngine;

namespace Combat.Enemies
{

    public class SelectableObject : MonoBehaviour
    {
        private Vector3 scale;

        private void Start()
        {
            scale = transform.localScale;
        }

        // 주문을 시전할 때 고를 수 있는 대상을 미리 모아 두면, 그 사이에 다음 wave 로
        // 나온 적은 목록에 없어 클릭해도 대상이 되지 않았다. 그러면 주문이 대상을 끝없이
        // 기다려 조합창과 턴 종료 버튼이 잠긴 채로 남는다. 그래서 클릭하는 순간에
        // 조합창이 대상을 기다리는지 직접 묻는다.
        public bool GetSelectable()
        {
            return CombineZone.Instance != null && CombineZone.Instance.IsAwaitingTarget;
        }

        private void OnMouseEnter()
        {
            if (GetSelectable() && !InteractionLock.IsLocked)
            {
                ChangeSize(true);
            }
        }

        private void OnMouseDown()
        {
            // 대화창 뒤의 대상 선택을 막는다. UI는 콜라이더 클릭을 가리지 못한다.
            if (GetSelectable() && !InteractionLock.IsLocked)
            {
                CombineZone.Instance.SetTarget(this);
            }
        }

        private void OnMouseExit()
        {
            ChangeSize(false);
        }


        private void ChangeSize(bool bigSide)
        {
            if (bigSide)
                transform.localScale = scale * 1.2f;
            else
                transform.localScale = scale;
        }
    }

}
