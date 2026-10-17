# Xunit.SharedExamples

[![CI](https://github.com/ryuchan00/XunitSharedExamples/actions/workflows/ci.yml/badge.svg)](https://github.com/ryuchan00/XunitSharedExamples/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Xunit.SharedExamples.svg)](https://www.nuget.org/packages/Xunit.SharedExamples)

RSpec-style shared examples for [xUnit.net v3](https://xunit.net/), built on C# default
interface methods.

Write a set of examples once as an interface; any test class includes them by implementing
that interface. The examples then run as ordinary xUnit.net tests of the including class —
with its constructor, `IDisposable`, `IAsyncLifetime` and fixtures all working as usual.

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

Test output:

```
ListSpec behaves like a collection: starts out empty
ListSpec behaves like a collection: contains an item once it has been added
```

## Installation

```
dotnet add package Xunit.SharedExamples
```

Requires xUnit.net v3 (4.0.1 or later) and a .NET (Core) target. See
[Requirements](#requirements).

## How it maps to RSpec

| RSpec | Xunit.SharedExamples |
|---|---|
| `shared_examples "a collection" do … end` | `[SharedExamples("a collection")] interface ICollectionExamples<T>` |
| `it "starts out empty" do … end` | `[SharedExample("starts out empty")] void StartsOutEmpty() => …` |
| `it_behaves_like "a collection"` | `class ListSpec : ICollectionExamples<List<int>>` |
| `subject { … }` | `CreateSubject()` from `ISharedExamplesFor<TSubject>` |
| `before { … }` | the including class's constructor, or `IAsyncLifetime.InitializeAsync` |
| `after { … }` | `IDisposable.Dispose` / `IAsyncLifetime.DisposeAsync` |
| `context "when …" do … end` | a nested class |
| `let(:name) { … }` | a property or method on the including class (declared on the interface when the examples need it) |
| shared examples with parameters | generic type parameters, or abstract members on the interface |

## Including more than one set

A class can behave like several things at once, which is where the interface-based approach
pays off over an abstract base class:

```csharp
public class HashSetSpec : ICollectionExamples<HashSet<int>>, ISetExamples<HashSet<int>>
{
    public HashSet<int> CreateSubject() => [];
}
```

## Nested contexts

RSpec's nested `context` blocks map onto nested classes. This is also how the same examples
get included more than once with different subjects:

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

Display names nest along with them: `SortedSetSpec+WhenReverseOrdered behaves like a
collection: starts out empty`.

## Setup and teardown

The including class is a perfectly ordinary xUnit.net test class, so everything you already
use keeps working — and it runs around every shared example, not just the class's own tests:

```csharp
public class RepositorySpec : IRepositoryExamples, IAsyncLifetime
{
    Repository subject = null!;

    public ValueTask InitializeAsync()     // RSpec's before
    {
        subject = Repository.Open(":memory:");
        return default;
    }

    public ValueTask DisposeAsync()        // RSpec's after
    {
        subject.Dispose();
        return default;
    }

    public Repository CreateSubject() => subject;
}
```

## Async examples

Shared examples may return `Task` or `ValueTask`; failures after an `await` are reported
normally.

```csharp
[SharedExample("starts out empty")]
async Task StartsOutEmptyAsync()
{
    await Task.Yield();

    Assert.Empty(CreateSubject());
}
```

## Examples without a subject

`ISharedExamplesFor<TSubject>` is a convention, not a requirement. Examples that need no
system under test simply do not implement it:

```csharp
[SharedExamples("a pure rule")]
public interface IPureRuleExamples
{
    [SharedExample("holds without a subject")]
    void HoldsWithoutSubject() => Assert.Equal(4, 2 + 2);
}
```

## Customizing display names

`SharedExampleNaming.Formatter` controls how names are built. Display names are baked in
during discovery, so it has to be set before that happens — a module initializer is the
reliable place:

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

The context carries the including class's name, the name from `[SharedExamples]`, and the
description from `[SharedExample]`. The example above produces
`ListSpec (a collection) starts out empty`.

## Requirements

- **xUnit.net v3**, version 4.0.1 or later.
- **A .NET (Core) target.** Shared examples are default interface methods, which require
  .NET Core 3.0+; **.NET Framework is not supported**.
- **Reflection-based discovery.** Native AOT (`xunit.v3.core.aot`) is not supported: the AOT
  source generator walks class declarations and picks up inherited methods only from base
  classes, so examples declared on an interface are never emitted.

## Notes

Two sets of shared examples may declare examples with the same method name. xUnit.net derives
a test method's unique ID from the class ID plus the method name, and silently drops test
cases whose IDs collide, so this package qualifies the ID with the declaring interface to keep
such examples distinct.

## License

MIT
