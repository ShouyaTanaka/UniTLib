using Cysharp.Threading.Tasks;
using UniTLib.Extensions;
using UnityEngine;

namespace UniTLib.UI
{
    public static class UIAnimationExecutor
    {
        public static async UniTask ShowAsync(CanvasGroup cg, RectTransform rt, UIAnimationSettings aniSet)
        {
            switch (aniSet.ShowType)
            {
                case UIAnimationType.None:
                    break;

                case UIAnimationType.Fade:

                    cg.alpha = 0f;

                    await cg.UniFade(1f, aniSet.Duration);

                    break;

                case UIAnimationType.Scale:

                    cg.alpha = 1f;
                    rt.localScale = Vector3.zero;

                    await rt.UniScale(Vector3.one, aniSet.Duration);

                    break;

                case UIAnimationType.FadeScale:

                    cg.alpha = 0f;
                    rt.localScale = Vector3.zero;

                    await UniTask.WhenAll(
                        cg.UniFade(1f, aniSet.Duration),
                        rt.UniScale(Vector3.one, aniSet.Duration)
                    );

                    break;
            }
        }

        public static async UniTask HideAsync(CanvasGroup cg, RectTransform rt, UIAnimationSettings aniSet)
        {
            switch (aniSet.HideType)
            {
                case UIAnimationType.None:
                    break;

                case UIAnimationType.Fade:

                    await cg.UniFade(0f, aniSet.Duration);

                    break;

                case UIAnimationType.Scale:

                    cg.alpha = 1f;
                    await rt.UniScale(Vector3.zero, aniSet.Duration);

                    break;

                case UIAnimationType.FadeScale:

                    await UniTask.WhenAll(
                        cg.UniFade(0f, aniSet.Duration),
                        rt.UniScale(Vector3.zero, aniSet.Duration)
                    );

                    break;
            }
        }
    }
}