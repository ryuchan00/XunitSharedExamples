namespace Xunit.SharedExamples.Tests;

/// <summary>
/// Shared examples may be asynchronous. Both <see cref="Task"/> and <see cref="ValueTask"/>
/// work, and a failed assertion after an await is reported normally.
/// </summary>
[SharedExamples("an async store")]
public interface IAsyncStoreExamples<TStore> : ISharedExamplesFor<TStore>
	where TStore : ICollection<int>
{
	[SharedExample("starts out empty")]
	async Task StartsOutEmptyAsync()
	{
		await Task.Yield();

		Assert.Empty(CreateSubject());
	}

	[SharedExample("contains an item once it has been added")]
	async ValueTask ContainsAddedItemAsync()
	{
		var subject = CreateSubject();

		await Task.Yield();
		subject.Add(42);

		Assert.Contains(42, subject);
	}
}

public class AsyncStoreSpec : IAsyncStoreExamples<List<int>>
{
	public List<int> CreateSubject() => [];
}

/// <summary>
/// <see cref="ISharedExamplesFor{TSubject}"/> is a convention, not a requirement: shared
/// examples that need no system under test simply do not implement it.
/// </summary>
[SharedExamples("a pure rule")]
public interface IPureRuleExamples
{
	[SharedExample("holds without a subject")]
	void HoldsWithoutSubject() =>
		Assert.Equal(4, 2 + 2);
}

public class PureRuleSpec : IPureRuleExamples
{
}

/// <summary>
/// The including class is an ordinary test class, so <see cref="IAsyncLifetime"/> — the
/// closest analog of RSpec's <c>before</c>/<c>after</c> — runs around every shared example.
/// </summary>
public class AsyncLifetimeSpec : ISharedExamplesFor<List<int>>, IPreparedSubjectExamples, IAsyncLifetime
{
	List<int> subject = [];

	public ValueTask InitializeAsync()
	{
		subject = [1, 2, 3];
		return default;
	}

	public ValueTask DisposeAsync()
	{
		subject = [];
		return default;
	}

	public List<int> CreateSubject() => subject;
}

[SharedExamples("a prepared subject")]
public interface IPreparedSubjectExamples : ISharedExamplesFor<List<int>>
{
	[SharedExample("sees the state prepared before the example ran")]
	void SeesPreparedState() =>
		Assert.Equal([1, 2, 3], CreateSubject());
}
