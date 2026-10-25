namespace Xunit.SharedExamples.Tests;

// Tests of the library's own API, as opposed to the shared examples it runs.
public class LibraryTests
{
	[Fact]
	public void QualifiedMethodNameOmitsAssemblyVersions()
	{
		var name = SharedExampleDiscoverer.QualifiedMethodName(
			typeof(ICollectionExamples<List<int>>),
			nameof(ICollectionExamples<List<int>>.StartsOutEmpty)
		);

		Assert.Equal(
			"Xunit.SharedExamples.Tests.ICollectionExamples`1[System.Collections.Generic.List`1[System.Int32]].StartsOutEmpty",
			name
		);
	}

	[Fact]
	public void FormatterRejectsNull() =>
		Assert.Throws<ArgumentNullException>(() => SharedExampleNaming.Formatter = null!);
}
