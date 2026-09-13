namespace Xunit.SharedExamples.Tests;

/// <summary>
/// The shared examples themselves: every <see cref="ICollection{T}"/> is expected to behave
/// this way. This is the analog of RSpec's <c>shared_examples "a collection"</c>.
/// </summary>
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

	[SharedExample("counts the items that were added")]
	void CountsAddedItems()
	{
		var subject = CreateSubject();

		subject.Add(1);
		subject.Add(2);

		Assert.Equal(2, subject.Count);
	}

	[SharedExample("is empty again after being cleared")]
	void IsEmptyAfterClear()
	{
		var subject = CreateSubject();
		subject.Add(1);

		subject.Clear();

		Assert.Empty(subject);
	}
}

/// <summary>
/// A second, independent set of shared examples, to show that a class can include more
/// than one at a time.
/// </summary>
[SharedExamples("a set")]
public interface ISetExamples<TSet> : ISharedExamplesFor<TSet>
	where TSet : ICollection<int>
{
	[SharedExample("ignores an item that was already added")]
	void IgnoresDuplicates()
	{
		var subject = CreateSubject();

		subject.Add(7);
		subject.Add(7);

		Assert.Single(subject);
	}
}
