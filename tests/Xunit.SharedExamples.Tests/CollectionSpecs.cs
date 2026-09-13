namespace Xunit.SharedExamples.Tests;

// "it_behaves_like 'a collection'" is spelled as implementing the interface.
public class ListSpec : ICollectionExamples<List<int>>
{
	public List<int> CreateSubject() => [];

	// Shared examples live alongside the class's own tests.
	[Fact]
	public void IndexerReturnsTheItemThatWasAdded()
	{
		var subject = CreateSubject();

		subject.Add(42);

		Assert.Equal(42, subject[0]);
	}
}

// Two sets of shared examples included at once.
public class HashSetSpec : ICollectionExamples<HashSet<int>>, ISetExamples<HashSet<int>>
{
	public HashSet<int> CreateSubject() => [];
}

// RSpec's nested "context" blocks map onto nested classes, which is also how the same
// shared examples get included more than once with different subjects.
public class SortedSetSpec
{
	public class WhenDefault : ICollectionExamples<SortedSet<int>>, ISetExamples<SortedSet<int>>
	{
		public SortedSet<int> CreateSubject() => [];
	}

	public class WhenReverseOrdered : ICollectionExamples<SortedSet<int>>, ISetExamples<SortedSet<int>>
	{
		public SortedSet<int> CreateSubject() =>
			new(Comparer<int>.Create((left, right) => right.CompareTo(left)));
	}
}

// The including class is a perfectly ordinary xUnit.net test class, so per-example setup
// and teardown (RSpec's before/after) work exactly as they always do.
public class ObservableCollectionSpec : ICollectionExamples<ObservableCollection<int>>, IDisposable
{
	readonly ObservableCollection<int> subject = [];
	int changeCount;

	public ObservableCollectionSpec() =>
		subject.CollectionChanged += (_, _) => changeCount++;

	public ObservableCollection<int> CreateSubject() => subject;

	public void Dispose() =>
		GC.SuppressFinalize(this);

	[Fact]
	public void RaisesChangeNotifications()
	{
		subject.Add(1);

		Assert.Equal(1, changeCount);
	}
}
