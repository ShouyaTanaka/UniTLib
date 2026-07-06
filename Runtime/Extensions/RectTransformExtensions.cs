using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniTLib.Extensions
{
    public static class RectTransformExtensions
    {
        //=============================
        #region DoTween代用
        //=============================

        /// <summary>
        /// [Scale処理]
        /// </summary>
        public static async UniTask UniScale(this RectTransform rt, Vector3 target, float duration, CancellationToken token = default)
        {
            if (duration <= 0f)
            {
                rt.localScale = target;
                return;
            }

            Vector3 start = rt.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                elapsed += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(elapsed / duration);

                rt.localScale =
                    Vector3.Lerp(start, target, t);

                await UniTask.Yield(token);
            }

            rt.localScale = target;
        }
        #endregion
    }
}