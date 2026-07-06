using Cysharp.Threading.Tasks;
using UniTLib.Extensions;
using UnityEngine;

namespace UniTLib.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : MonoBehaviour, IUIView
    {
        public UIState State { get; private set; } = UIState.Hidden;
        [SerializeField] private UIAnimationSettings aniSet = new();

        protected CanvasGroup cg;
        protected RectTransform rt;

        protected virtual void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            rt = transform as RectTransform;
        }

        public async UniTask ShowAsync()
        {
            if (State == UIState.Visible || State == UIState.Showing) return;

            State = UIState.Showing;

            await OnShowAsync();

            State = UIState.Visible;
        }

        public async UniTask HideAsync()
        {
            if (State == UIState.Hidden || State == UIState.Hiding) return;

            State = UIState.Hiding;

            await OnHideAsync();

            State = UIState.Hidden;
        }

        protected virtual async UniTask OnShowAsync()
        {
            gameObject.SetActive(true);

            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;

            await UIAnimationExecutor.ShowAsync(cg, rt, aniSet);

            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        protected virtual async UniTask OnHideAsync()
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;

            await UIAnimationExecutor.HideAsync(cg, rt, aniSet);

            gameObject.SetActive(false);
        }
    }
}