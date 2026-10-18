namespace Xunit.SharedExamples.Tests;

/// <summary>
/// Data-driven shared examples. <see cref="SharedTheoryAttribute"/> takes the same data
/// attributes as <see cref="TheoryAttribute"/>, and each data row becomes its own test.
/// </summary>
[SharedExamples("a collection of numbers")]
public interface INumberCollectionExamples<TCollection> : ISharedExamplesFor<TCollection>
	where TCollection : ICollection<int>
{
	[SharedTheory("contains an item once it has been added")]
	[InlineData(0)]
	[InlineData(42)]
	[InlineData(-1)]
	void ContainsAddedItem(int item)
	{
		var subject = CreateSubject();

		subject.Add(item);

		Assert.Contains(item, subject);
	}

	// Data members are looked up on the declaring interface, so a static member works.
	static TheoryData<int[]> DistinctItems => new([1], [1, 2], [3, 1, 2]);

	[SharedTheory("counts distinct items")]
	[MemberData(nameof(DistinctItems))]
	void CountsDistinctItems(int[] items)
	{
		var subject = CreateSubject();

		foreach (var item in items)
			subject.Add(item);

		Assert.Equal(items.Length, subject.Count);
	}

	// A non-serializable argument makes xUnit.net enumerate the data when the test runs
	// rather than during discovery; the display name is built the same way either way.
	static TheoryData<Func<int, int>> Doublers => new(x => x * 2, x => x + x);

	[SharedTheory("contains a computed item")]
	[MemberData(nameof(Doublers))]
	void ContainsComputedItem(Func<int, int> compute)
	{
		var subject = CreateSubject();

		subject.Add(compute(21));

		Assert.Contains(42, subject);
	}

	[SharedTheory("accepts items given as any type")]
	[InlineData(7)]
	[InlineData("7")]
	void AcceptsItemsOfAnyType<T>(T value)
	{
		var subject = CreateSubject();

		subject.Add(int.Parse(value!.ToString()!, CultureInfo.InvariantCulture));

		Assert.Contains(7, subject);
	}
}

/// <summary>
/// Declares a theory with the same name and data as <see cref="INumberCollectionExamples{TCollection}"/>.
/// Both must still run when one class includes the two sets.
/// </summary>
[SharedExamples("a growable collection")]
public interface IGrowableCollectionExamples<TCollection> : ISharedExamplesFor<TCollection>
	where TCollection : ICollection<int>
{
	[SharedTheory("grows by one for each added item")]
	[InlineData(0)]
	[InlineData(42)]
	[InlineData(-1)]
	void ContainsAddedItem(int item)
	{
		var subject = CreateSubject();
		var before = subject.Count;

		subject.Add(item);

		Assert.Equal(before + 1, subject.Count);
	}

	// Generic theories are closed over their type arguments during discovery, which builds a
	// new test method; it has to keep the qualified ID as well.
	[SharedTheory("grows when items are given as any type")]
	[InlineData(7)]
	[InlineData("7")]
	void AcceptsItemsOfAnyType<T>(T value)
	{
		var subject = CreateSubject();

		subject.Add(int.Parse(value!.ToString()!, CultureInfo.InvariantCulture));

		Assert.Single(subject);
	}
}

public class NumberListSpec : INumberCollectionExamples<List<int>>, IGrowableCollectionExamples<List<int>>
{
	public List<int> CreateSubject() => [];
}
