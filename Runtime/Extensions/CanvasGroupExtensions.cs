//=============================
#region
//=============================

#endregion

using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniTLib.Extensions
{
    public static class CanvasGroupExtensions
    {
        //=============================
        #region DoTween代用
        //=============================

        /// <summary>
        /// [Fade処理]
        /// </summary>
        public static async UniTask UniFade(this CanvasGroup cg, float a, float duration, CancellationToken token = default)
        {
            if (duration <= 0f)
            {
                cg.alpha = a;
                return;
            }

            float startAlpha = cg.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cg.alpha = Mathf.Lerp(startAlpha, a, t);
                await UniTask.Yield(cancellationToken: token);
            }

            cg.alpha = a;
        }

        #endregion
    }
}