using Combat.UI;
using Core;
using UnityEngine;

namespace Combat.Enemies
{

    public class SelectableObject : MonoBehaviour
    {
        // 적은 시간이 지나 분노하면 몸이 커지므로, 처음 크기가 아니라 커지기 직전 크기로 되돌린다.
        private Vector3 scaleBeforeHover;
        private bool enlarged;
        private bool selectable = false;
        CombineZone combineZone;

        private void Start()
        {
            combineZone = CombineZone.Instance;
        }

        public bool GetSelectable()
        {
            return selectable;
        }

        public void SetSelectable(bool selectable)
        {
            this.selectable = selectable;
        }

        private void OnMouseEnter()
        {
            if (selectable && !InteractionLock.IsLocked)
            {
                ChangeSize(true);
            }
        }

        private void OnMouseDown()
        {
            // 대화창 뒤의 대상 선택을 막는다. UI는 콜라이더 클릭을 가리지 못한다.
            if (selectable && !InteractionLock.IsLocked)
            {
                combineZone.SetTarget(this);
            }
        }

        private void OnMouseExit()
        {
            ChangeSize(false);
        }


        private void ChangeSize(bool bigSide)
        {
            if (bigSide == enlarged) return;
            enlarged = bigSide;
            if (bigSide)
            {
                scaleBeforeHover = transform.localScale;
                transform.localScale = scaleBeforeHover * 1.2f;
            }
            else
                transform.localScale = scaleBeforeHover;
        }
    }

}
