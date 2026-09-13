namespace Xunit.SharedExamples;

/// <summary>
/// Marks an interface as a set of shared examples (the xUnit.net analog of RSpec's
/// <c>shared_examples</c>). A test class "includes" the examples simply by implementing
/// the interface, which is the analog of RSpec's <c>it_behaves_like</c>.
/// </summary>
/// <param name="name">
/// The human-readable name of the shared examples, phrased so that it reads well after
/// "behaves like" (e.g. <c>"a collection"</c>).
/// </param>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class SharedExamplesAttribute(string name) : Attribute
{
	/// <summary>
	/// Gets the human-readable name of the shared examples.
	/// </summary>
	public string Name { get; } = name;
}
