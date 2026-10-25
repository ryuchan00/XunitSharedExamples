using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.SharedExamples;

/// <summary>
/// Discoverer for <see cref="SharedExampleAttribute"/>. Identical to <see cref="FactDiscoverer"/>
/// apart from the display name, which is built by <see cref="SharedExampleNaming"/>.
/// </summary>
public class SharedExampleDiscoverer : FactDiscoverer
{
	/// <inheritdoc/>
	protected override IXunitTestCase CreateTestCase(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute)
	{
		ArgumentNullException.ThrowIfNull(discoveryOptions);
		ArgumentNullException.ThrowIfNull(testMethod);
		ArgumentNullException.ThrowIfNull(factAttribute);

		// Two sets of shared examples may declare examples with the same method name. The default
		// unique ID for a test method is derived from the class ID plus the method name, so those
		// examples would collide and xUnit.net would silently drop all but the first of them. We
		// qualify the ID with the declaring interface to keep them distinct.
		var resolvedTestMethod = QualifyByDeclaringInterface(testMethod);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, resolvedTestMethod, factAttribute);

		return new XunitTestCase(
			details.ResolvedTestMethod,
			SharedExampleNaming.GetDisplayName(testMethod, factAttribute, details.TestCaseDisplayName),
			details.UniqueID,
			details.Explicit,
			details.SkipExceptions,
			details.SkipReason,
			details.SkipType,
			details.SkipUnless,
			details.SkipWhen,
			ToReadWrite(testMethod.Traits),
			sourceFilePath: details.SourceFilePath,
			sourceLineNumber: details.SourceLineNumber,
			timeout: details.Timeout
		);
	}

	internal static IXunitTestMethod QualifyByDeclaringInterface(IXunitTestMethod testMethod)
	{
		var declaringType = testMethod.Method.DeclaringType;

		if (declaringType is null || !declaringType.IsInterface)
			return testMethod;

		return new XunitTestMethod(
			testMethod.TestClass,
			testMethod.Method,
			testMethod.TestMethodArguments,
			uniqueID: UniqueIDGenerator.ForTestMethod(
				testMethod.TestClass.UniqueID,
				QualifiedMethodName(declaringType, testMethod.MethodName)
			)
		);
	}

	// Type.ToString() names closed generic types by their type arguments alone, whereas FullName
	// adds each argument's assembly-qualified name, so IDs built from it would change whenever an
	// argument's assembly version does (for example, on a runtime upgrade).
	internal static string QualifiedMethodName(Type declaringType, string methodName) =>
		declaringType + "." + methodName;

	static Dictionary<string, HashSet<string>> ToReadWrite(IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits)
	{
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var kvp in traits)
			result[kvp.Key] = new HashSet<string>(kvp.Value, StringComparer.OrdinalIgnoreCase);

		return result;
	}
}
