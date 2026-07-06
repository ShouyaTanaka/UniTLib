using System;

namespace UniTLib.UI
{
    [Serializable]
    public class UIAnimationSettings
    {
        public UIAnimationType ShowType = UIAnimationType.Fade;
        public UIAnimationType HideType = UIAnimationType.Fade;
        public float Duration = 0.25f;
    }
}