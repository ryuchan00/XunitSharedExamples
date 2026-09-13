namespace Xunit.SharedExamples;

/// <summary>
/// The conventional way for a set of shared examples to obtain the system under test.
/// This is the analog of RSpec's <c>subject</c>: the including test class supplies the
/// instance, and the shared examples exercise it.
/// </summary>
/// <remarks>
/// Default interface methods cannot hold state, so each example asks for a fresh subject
/// rather than sharing one. Per-example setup that does need state belongs in the
/// including class's constructor, which xUnit.net runs normally.
/// </remarks>
/// <typeparam name="TSubject">The type of the system under test.</typeparam>
public interface ISharedExamplesFor<out TSubject>
{
	/// <summary>
	/// Creates the system under test. Called once per example.
	/// </summary>
	TSubject CreateSubject();
}
