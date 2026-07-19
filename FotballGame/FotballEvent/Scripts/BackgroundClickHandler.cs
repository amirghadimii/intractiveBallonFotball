using UnityEngine;
using UnityEngine.EventSystems;

namespace GoalRush
{
    public class BackgroundClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            gm.HandleBackgroundClick();
        }
    }
}
