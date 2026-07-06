using Cysharp.Threading.Tasks;

namespace UniTLib.UI
{
    public interface IUIView
    {
        UIState State { get; }

        UniTask ShowAsync();
        UniTask HideAsync();
    }
}