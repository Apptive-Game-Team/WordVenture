using Combat.UI;
using Core;
using UnityEngine;

namespace Combat.Enemies
{

    public class SelectableObject : MonoBehaviour
    {
        private Vector3 scale;
        private bool selectable = false;
        CombineZone combineZone;

        private void Start()
        {
            scale = transform.localScale;
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
            if (bigSide)
                transform.localScale = scale * 1.2f;
            else
                transform.localScale = scale;
        }
    }

}
