using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Combat.UI;
using WordVenture.Core;

namespace WordVenture.Cards
{
    public class Order : MonoBehaviour
    {
        [SerializeField] Renderer[] backRenderers;
        [SerializeField] Renderer[] middleRenderers;
        [SerializeField] string sortingLayerName;
        int originOrder;

        public void SetOriginOrder(int originOrder)
        {
            this.originOrder = originOrder;
            SetOrder(originOrder);
        }

        public void SetMostFrontOrder(bool isMostFront)
        {
            SetOrder(isMostFront ? 100 : originOrder);
        }

        public void SetOrder(int order)
        {
            int mulOrder = order * 10;

            foreach (var renderer in backRenderers)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = mulOrder;
            }

            foreach (var renderer in middleRenderers)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = mulOrder + 1;
            }
        }
    }
}
