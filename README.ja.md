# Xunit.SharedExamples

[![CI](https://github.com/ryuchan00/XunitSharedExamples/actions/workflows/ci.yml/badge.svg)](https://github.com/ryuchan00/XunitSharedExamples/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Xunit.SharedExamples.svg)](https://www.nuget.org/packages/Xunit.SharedExamples)

[English](README.md) | 日本語

[xUnit.net v3](https://xunit.net/) で RSpec の shared examples を書けるようにするライブラリです。
C# の既定インターフェースメソッドを使います。

共有したいテストをインターフェースに一度だけ書き、テストクラスはそのインターフェースを実装して取り込みます。
取り込んだテストは、そのクラスの通常の xUnit.net のテストとして実行されます。
コンストラクタ、`IDisposable`、`IAsyncLifetime`、フィクスチャもそのまま使えます。

```csharp
// shared_examples "a collection"
[SharedExamples("a collection")]
public interface ICollectionExamples<TCollection> : ISharedExamplesFor<TCollection>
    where TCollection : ICollection<int>
{
    [SharedExample("starts out empty")]
    void StartsOutEmpty() =>
        Assert.Empty(CreateSubject());

    [SharedExample("contains an item once it has been added")]
    void ContainsAddedItem()
    {
        var subject = CreateSubject();

        subject.Add(42);

        Assert.Contains(42, subject);
    }
}

// it_behaves_like "a collection"
public class ListSpec : ICollectionExamples<List<int>>
{
    public List<int> CreateSubject() => [];
}
```

テストの出力:

```
ListSpec behaves like a collection: starts out empty
ListSpec behaves like a collection: contains an item once it has been added
```

紹介記事: [xUnit.net v3 で RSpec の shared examples を書けるようにした](https://leokun0210.hatenablog.com/entry/xunit-shared-examples)

## インストール

```
dotnet add package Xunit.SharedExamples
```

xUnit.net v3（4.0.1 以降）と .NET (Core) のターゲットが必要です。
詳しくは[動作要件](#動作要件)を参照してください。

## RSpec との対応

| RSpec | Xunit.SharedExamples |
|---|---|
| `shared_examples "a collection" do … end` | `[SharedExamples("a collection")] interface ICollectionExamples<T>` |
| `it "starts out empty" do … end` | `[SharedExample("starts out empty")] void StartsOutEmpty() => …` |
| `it_behaves_like "a collection"` | `class ListSpec : ICollectionExamples<List<int>>` |
| `subject { … }` | `ISharedExamplesFor<TSubject>` の `CreateSubject()` |
| `before { … }` | 取り込む側のクラスのコンストラクタ、または `IAsyncLifetime.InitializeAsync` |
| `after { … }` | `IDisposable.Dispose` / `IAsyncLifetime.DisposeAsync` |
| `context "when …" do … end` | 入れ子のクラス |
| `let(:name) { … }` | 取り込む側のクラスのプロパティやメソッド（共有テストから使う場合はインターフェースにも宣言する） |
| 引数付きの shared examples | ジェネリック型パラメーター、またはインターフェースの抽象メンバー |

## 複数のセットを取り込む

1 つのクラスで、複数のインターフェースを実装できます。
抽象基底クラスは 1 つしか継承できないので、ここがインターフェースで書く利点です。

```csharp
public class HashSetSpec : ICollectionExamples<HashSet<int>>, ISetExamples<HashSet<int>>
{
    public HashSet<int> CreateSubject() => [];
}
```

## 入れ子の context

RSpec の入れ子の `context` は、入れ子のクラスで書きます。
同じテストを、テスト対象を変えて複数回取り込むときもこの形を使います。

```csharp
public class SortedSetSpec
{
    public class WhenDefault : ICollectionExamples<SortedSet<int>>
    {
        public SortedSet<int> CreateSubject() => [];
    }

    public class WhenReverseOrdered : ICollectionExamples<SortedSet<int>>
    {
        public SortedSet<int> CreateSubject() =>
            new(Comparer<int>.Create((left, right) => right.CompareTo(left)));
    }
}
```

表示名も入れ子になります（`SortedSetSpec+WhenReverseOrdered behaves like a collection: starts out empty`）。

## 前処理と後処理

取り込む側のクラスは通常の xUnit.net のテストクラスなので、いつもの前処理と後処理がそのまま使えます。
クラス自身のテストだけでなく、取り込んだ各テストの前後でも実行されます。

```csharp
public class RepositorySpec : IRepositoryExamples, IAsyncLifetime
{
    Repository subject = null!;

    public ValueTask InitializeAsync()     // RSpec の before
    {
        subject = Repository.Open(":memory:");
        return default;
    }

    public ValueTask DisposeAsync()        // RSpec の after
    {
        subject.Dispose();
        return default;
    }

    public Repository CreateSubject() => subject;
}
```

## 非同期のテスト

共有テストは `Task` や `ValueTask` を返せます。
`await` のあとで失敗しても、通常どおり報告されます。

```csharp
[SharedExample("starts out empty")]
async Task StartsOutEmptyAsync()
{
    await Task.Yield();

    Assert.Empty(CreateSubject());
}
```

## データ駆動のテスト

`[SharedTheory]` は `[Theory]` の共有版です。
通常のデータ属性をそのまま使え、データの各行が取り込む側のクラスの 1 つのテストになります。

```csharp
[SharedExamples("a collection of numbers")]
public interface INumberCollectionExamples<TCollection> : ISharedExamplesFor<TCollection>
    where TCollection : ICollection<int>
{
    [SharedTheory("contains an item once it has been added")]
    [InlineData(0)]
    [InlineData(42)]
    void ContainsAddedItem(int item)
    {
        var subject = CreateSubject();

        subject.Add(item);

        Assert.Contains(item, subject);
    }
}
```

```
NumberListSpec behaves like a collection of numbers: contains an item once it has been added(item: 0)
NumberListSpec behaves like a collection of numbers: contains an item once it has been added(item: 42)
```

`[MemberData]` は、テストを宣言したインターフェースからメンバーを探します。
データは、そのインターフェースの static メンバーに置いてください。
テスト名は、探索時にデータを列挙するかどうか（pre-enumeration）によらず同じになります。
`[Theory]` と同じく、`TheoryDataRow` で `TestDisplayName` を指定した行は、その名前だけで表示されます。

## テスト対象の無いテスト

`ISharedExamplesFor<TSubject>` は慣例であり、必須ではありません。
テスト対象が要らないテストは、これを実装しなくてかまいません。

```csharp
[SharedExamples("a pure rule")]
public interface IPureRuleExamples
{
    [SharedExample("holds without a subject")]
    void HoldsWithoutSubject() => Assert.Equal(4, 2 + 2);
}
```

## 表示名の変更

表示名の組み立て方は `SharedExampleNaming.Formatter` で変えられます。
表示名はテストの探索時に決まるので、それより前に設定する必要があります。
確実なのはモジュール初期化子です。

```csharp
using System.Runtime.CompilerServices;

static class TestSetup
{
    [ModuleInitializer]
    internal static void Init() =>
        SharedExampleNaming.Formatter =
            context => $"{context.TestClassName} ({context.SharedExamplesName}) {context.Description}";
}
```

context には、取り込む側のクラス名、`[SharedExamples]` の名前、`[SharedExample]` または `[SharedTheory]` の説明が入ります。
上の例では `ListSpec (a collection) starts out empty` になります。

## 動作要件

- xUnit.net v3 の 4.0.1 以降
- .NET (Core) のターゲット。共有テストは既定インターフェースメソッドなので .NET Core 3.0 以降が必要で、.NET Framework では使えません。
- リフレクションによるテストの探索。Native AOT（`xunit.v3.core.aot`）では使えません。AOT 用の Source Generator はクラスの宣言をたどり、継承したメソッドは基底クラスからしか集めないので、インターフェースに書いたテストが生成されません。

## 補足

2 つのセットに、同じメソッド名のテスト（`[SharedTheory]` を含む）があってもかまいません。
xUnit.net はテストメソッドの ID を「クラスの ID + メソッド名」から作り、ID が重複したテストを警告なしに除外します。
そこでこのパッケージは、宣言したインターフェースを ID に含めて、両方のテストが実行されるようにしています。

## ライセンス

MIT
