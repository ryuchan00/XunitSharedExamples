using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.SharedExamples;

/// <summary>
/// Discoverer for <see cref="SharedTheoryAttribute"/>. Identical to <see cref="TheoryDiscoverer"/>
/// apart from the display name, which is built by <see cref="SharedExampleNaming"/>, and the
/// unique ID, which is qualified by the declaring interface as in <see cref="SharedExampleDiscoverer"/>.
/// </summary>
public class SharedTheoryDiscoverer : TheoryDiscoverer
{
	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		ArgumentNullException.ThrowIfNull(testMethod);
		ArgumentNullException.ThrowIfNull(factAttribute);

		// See SharedExampleDiscoverer for why the ID is qualified. Every test case is created
		// from this qualified method, so data rows inherit the distinct ID too.
		var qualifiedTestMethod = SharedExampleDiscoverer.QualifyByDeclaringInterface(testMethod);

		// xUnit.net uses the attribute's display name as the base name of every test case it
		// creates for the theory: data rows (with their arguments appended, whether enumerated
		// during discovery or at run time), a skipped theory, and the error reported for a
		// theory without data. Supplying the shared name there keeps all of them consistent.
		if (factAttribute is ITheoryAttribute theoryAttribute
			&& SharedExampleNaming.GetSharedDisplayName(testMethod, factAttribute) is { } sharedDisplayName)
			factAttribute = new DisplayNamedTheoryAttribute(theoryAttribute, sharedDisplayName);

		return base.Discover(discoveryOptions, qualifiedTestMethod, factAttribute);
	}

	/// <inheritdoc/>
	protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments,
		string? index)
	{
		ArgumentNullException.ThrowIfNull(discoveryOptions);
		ArgumentNullException.ThrowIfNull(testMethod);
		ArgumentNullException.ThrowIfNull(theoryAttribute);
		ArgumentNullException.ThrowIfNull(dataRow);
		ArgumentNullException.ThrowIfNull(testMethodArguments);

		var genericTypes = testMethod.ResolveGenericTypes(testMethodArguments);
		if (genericTypes is null)
			return base.CreateTestCasesForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments, index);

		// For a generic method, xUnit.net closes the method into a new test method whose ID is
		// built from the class and method name alone. Qualify that method again, and derive the
		// test case ID from it, so that same-named generic theories stay distinct.
		var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments, index);
		var resolvedTestMethod = SharedExampleDiscoverer.QualifyByDeclaringInterface(details.ResolvedTestMethod);

		IXunitTestCase testCase = new XunitTestCase(
			resolvedTestMethod,
			details.TestCaseDisplayName,
			UniqueIDGenerator.ForTestCase(resolvedTestMethod.UniqueID, genericTypes, testMethodArguments),
			details.Explicit,
			dataRow.Label,
			dataRow.DisableParallelization ?? false,
			details.SkipExceptions,
			details.SkipReason,
			details.SkipType,
			details.SkipUnless,
			details.SkipWhen,
			TestIntrospectionHelper.GetTraits(testMethod, dataRow),
			testMethodArguments,
			sourceFilePath: details.SourceFilePath,
			sourceLineNumber: details.SourceLineNumber,
			timeout: details.Timeout
		);

		return new([testCase]);
	}

	/// <summary>
	/// An <see cref="ITheoryAttribute"/> that reports a different display name and otherwise
	/// forwards to the attribute on the method.
	/// </summary>
	sealed class DisplayNamedTheoryAttribute(ITheoryAttribute inner, string displayName) : ITheoryAttribute
	{
		public bool DisableDiscoveryEnumeration => inner.DisableDiscoveryEnumeration;
		public bool DisableParallelization => inner.DisableParallelization;
		public string? DisplayName => displayName;
		public bool Explicit => inner.Explicit;
		public bool IncludeTestCaseIndex => inner.IncludeTestCaseIndex;
		public string? Skip => inner.Skip;
		public Type[]? SkipExceptions => inner.SkipExceptions;
		public bool SkipTestWithoutData => inner.SkipTestWithoutData;
		public Type? SkipType => inner.SkipType;
		public string? SkipUnless => inner.SkipUnless;
		public string? SkipWhen => inner.SkipWhen;
		public string? SourceFilePath => inner.SourceFilePath;
		public int? SourceLineNumber => inner.SourceLineNumber;
		public int Timeout => inner.Timeout;
	}
}
