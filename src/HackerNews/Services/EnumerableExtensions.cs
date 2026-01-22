using System.Collections;

namespace HackerNews;

static class EnumerableExtensions
{
	public static bool IsNullOrEmpty(this IEnumerable? enumerable)
	{
		if (enumerable is null)
			return false;

		var enumerator = enumerable.GetEnumerator() ?? throw new InvalidOperationException("Enumerator not found");
		using var disposable = (IDisposable)enumerator;
		return !enumerator.MoveNext();
	}
}