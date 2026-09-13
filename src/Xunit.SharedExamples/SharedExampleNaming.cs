using System.Reflection;
using Xunit.v3;

namespace Xunit.SharedExamples;

/// <summary>
/// Builds the display names for shared examples.
/// </summary>
public static class SharedExampleNaming
{
	/// <summary>
	/// Gets or sets the formatter used to build the display name of a shared example. The
	/// default produces <c>Namespace.ListSpec behaves like a collection: is empty when created</c>.
	/// </summary>
	public static Func<SharedExampleNameContext, string> Formatter { get; set; } =
		context => string.Format(
			CultureInfo.CurrentCulture,
			"{0} behaves like {1}: {2}",
			context.TestClassName,
			context.SharedExamplesName,
			context.Description
		);

	/// <summary>
	/// Gets the display name for a test method, falling back to <paramref name="defaultDisplayName"/>
	/// when the method does not come from a <see cref="SharedExamplesAttribute"/> interface.
	/// </summary>
	public static string GetDisplayName(
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute,
		string defaultDisplayName)
	{
		ArgumentNullException.ThrowIfNull(testMethod);
		ArgumentNullException.ThrowIfNull(factAttribute);

		var sharedExamples =
			testMethod.Method.DeclaringType?.GetCustomAttribute<SharedExamplesAttribute>();

		if (sharedExamples is null)
			return defaultDisplayName;

		var description =
			(factAttribute as SharedExampleAttribute)?.Description
				?? factAttribute.DisplayName
				?? testMethod.MethodName;

		return Formatter(new(testMethod.TestClass.TestClassName, sharedExamples.Name, description));
	}
}

/// <summary>
/// The values available when formatting the display name of a shared example.
/// </summary>
/// <param name="TestClassName">The fully qualified name of the class that includes the examples.</param>
/// <param name="SharedExamplesName">The name from <see cref="SharedExamplesAttribute"/>.</param>
/// <param name="Description">The description of the individual example.</param>
public sealed record SharedExampleNameContext(
	string TestClassName,
	string SharedExamplesName,
	string Description
);
