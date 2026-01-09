Unityでの使いやすい関数、属性、コンポーネントをまとめ、開発効率の向上を図ります

- [SetupPanel](#setuppanel)
  - [基本的な使用方法](#基本的な使用方法)
- [UTLogとUTConsole](#utlogとutconsole)
  - [基本的な使用方法](#基本的な使用方法-1)
- [追加方法](#追加方法)

# SetupPanel

このパッケージは将来的にUniTaskやUniRxを依存関係としたものになる予定です
そのためそれぞれのインストールを楽にするセットアップパネルを作成しました

## 基本的な使用方法

コンソールは以下の階層にあります
>Tools/UniTLib/Setup

開いたらUniTask,UniRxにチェックを入れて下部のInstallボタンを押してください

おまけでフォルダを自動で作成する項目も追加されています
作成されるフォルダは以下の階層になっています
```
Assets/
├── Scripts/
├── Resources/
│   ├── Images/
│   ├── Sounds/
│   │   ├── BGM/
│   │   └── SE/
│   └── Materials/
└── Prefabs/
```


# UTLogとUTConsole

従来のDebug.Logはタグ付けが不可能ゆえに多量化して追えないことが起こりやすいのでタグ付けを可能にし、かつ情報量を減らし欲しい情報が素早く見れるような改善を行いました

## 基本的な使用方法

コンソールは以下の階層にあります
>Tools/UniTLib/UTConsole

ログの出し方は
>UTLog.Log(メッセージ).Tag(タグ);
- .Logを.Warningや.Errorにすることで警告度を変更できます
- タグは任意のためつけないとDefaultタグが付きます

```cysharp
using UniTLib.Debug;
~
void Hoge
{
    UTLog.Log("test1").Tag("Game");
    UTLog.Log("test2").Tag("Game");
    UTLog.Warning("Attack").Tag("Player");
    UTLog.Error("Error406").Tag("System");
}
```

# 追加方法
- 1.Window > Package Manager > 左上の＋マークから Install package from git url
- 2.https://github.com/ShouyaTanaka/UniTLib.git を貼り付け
- 3.インストール完了