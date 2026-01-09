using UnityEngine;

namespace UniTLib.Extensions
{
    public static class TransformExtensions
    {
        //=============================
        #region Position操作系（ワールド）
        //=============================

        /// <summary>
        /// [ワールド座標のXのみ変更]
        /// </summary>
        public static void SetPositionX(this Transform posA, float x)
        {
            Vector3 pos = posA.position;
            pos.x = x;
            posA.position = pos;
        }

        /// <summary>
        /// [ワールド座標のYのみ変更]
        /// </summary>
        public static void SetPositionY(this Transform PosA, float y)
        {
            Vector3 pos = PosA.position;
            pos.y = y;
            PosA.position = pos;
        }

        /// <summary>
        /// [ワールド座標のZのみ変更]
        /// </summary>
        public static void SetPositionZ(this Transform PosA, float z)
        {
            Vector3 pos = PosA.position;
            pos.z = z;
            PosA.position = pos;
        }

        #endregion

        //=============================
        #region Position操作系（ローカル）
        //=============================

        /// <summary>
        /// [ローカル座標のXのみ変更]
        /// </summary>
        public static void SetLocalPositionX(this Transform PosA, float x)
        {
            Vector3 pos = PosA.localPosition;
            pos.x = x;
            PosA.localPosition = pos;
        }

        /// <summary>
        /// [ローカル座標のYのみ変更]
        /// </summary>
        public static void SetLocalPositionY(this Transform PosA, float y)
        {
            Vector3 pos = PosA.localPosition;
            pos.y = y;
            PosA.localPosition = pos;
        }

        /// <summary>
        /// [ローカル座標のZのみ変更]
        /// </summary>
        public static void SetLocalPositionZ(this Transform PosA, float z)
        {
            Vector3 pos = PosA.localPosition;
            pos.z = z;
            PosA.localPosition = pos;
        }

        #endregion

        //=============================
        #region リセット・指定系
        //=============================

        /// <summary>
        /// [position, rotationをすべてリセット]
        /// </summary>
        public static void ResetWorldTransform(this Transform PosA)
        {
            PosA.position = Vector3.zero;
            PosA.rotation = Quaternion.identity;
        }

        /// <summary>
        /// [position, rotationをすべてリセット]
        /// </summary>
        public static void ResetLocalTransform(this Transform PosA)
        {
            PosA.localPosition = Vector3.zero;
            PosA.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// [ワールド座標を直接指定して位置を設定]
        /// </summary>
        public static void SetWorldPosition(this Transform PosA, Vector3 pos)
        {
            PosA.position = pos;
        }

        /// <summary>
        /// [ローカル座標を直接指定して位置を設定]
        /// </summary>
        public static void SetLocalPosition(this Transform PosA, Vector3 pos)
        {
            PosA.localPosition = pos;
        }

        #endregion

        //=============================
        #region Transform相互操作系
        //=============================

        /// <summary>
        /// [他のTransformと position / rotation を交換]
        /// </summary>
        public static void SwapTransform(this Transform PosA, Transform other)
        {
            Vector3 pos = PosA.position;
            Quaternion rot = PosA.rotation;
            Vector3 scl = PosA.localScale;

            PosA.position = other.position;
            PosA.rotation = other.rotation;
            PosA.localScale = other.localScale;

            other.position = pos;
            other.rotation = rot;
            other.localScale = scl;
        }

        /// <summary>
        /// [他のTransformの position / rotation をコピー]
        /// </summary>
        public static void CopyTransformFrom(this Transform PosA, Transform source)
        {
            PosA.position = source.position;
            PosA.rotation = source.rotation;
            PosA.localScale = source.localScale;
        }

        #endregion
    }
}