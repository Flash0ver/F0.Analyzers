using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using F0.Analyzers.Example.Shared;

namespace F0.Analyzers.Example.CodeAnalysis.CodeFixes
{
	[SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "Examples")]
	internal sealed class UsePatternMatchingNullCheckInsteadOfComparisonWithNullExamples
	{
		public void PatternMatching(Record instance)
		{
			_ = instance is null;
			_ = instance is not null;
		}

		public void Equality_Inequality(Record instance)
		{
			_ = instance == null;
			_ = instance != null;

			_ = null == instance;
			_ = null != instance;

			_ = (object)instance == null;
			_ = (object)instance != null;
		}

		public void Equals(Record instance, IEquatable<Record> equatable)
		{
			_ = instance.Equals(null);
			_ = !instance.Equals(null);

			_ = ((object)instance).Equals(null);
			_ = !((object)instance).Equals(null);

			_ = Object.Equals(instance, null);
			_ = !Object.Equals(instance, null);

			_ = Object.Equals(null, instance);
			_ = !Object.Equals(null, instance);

			_ = equatable.Equals(null);
			_ = !equatable.Equals(null);

			_ = ((IEquatable<Record>)instance).Equals(null);
			_ = !((IEquatable<Record>)instance).Equals(null);
		}

		public void ReferenceEquals(Record instance)
		{
			_ = Object.ReferenceEquals(instance, null);
			_ = !Object.ReferenceEquals(instance, null);

			_ = Object.ReferenceEquals(null, instance);
			_ = !Object.ReferenceEquals(null, instance);
		}

		public void EqualityComparer(Record instance, IEqualityComparer<Record> comparer)
		{
			_ = comparer.Equals(instance, null);
			_ = !comparer.Equals(instance, null);

			_ = EqualityComparer<Record>.Default.Equals(instance, null);
			_ = !EqualityComparer<Record>.Default.Equals(instance, null);

			_ = ReferenceEqualityComparer.Instance.Equals(instance, null);
			_ = !ReferenceEqualityComparer.Instance.Equals(instance, null);

			_ = RecordEqualityComparer.Instance.Equals(instance, null);
			_ = !RecordEqualityComparer.Instance.Equals(instance, null);
		}
	}
}
