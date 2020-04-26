using FluentAssertions;
using System.Collections.Generic;
using TradingSystem.Domain.Common;
using Xunit;

namespace TradingSystem.Tests.Domain
{
    public class ValueObjectTest
    {
        private class C1 : ValueObject
        {
            public int I { get; set; }
            public string S { get; set; }

            protected override IEnumerable<object> GetAllValues()
            {
                yield return I;
                yield return S;
            }
        }

        [Fact]
        public void ComparisonEqual()
        {
            var c1 = new C1() { I = 10, S = "a" };
            var c2 = new C1() { I = 10, S = "a" };
            (c1.Equals(c2)).Should().BeTrue();

            c1.GetHashCode().Should().Be(c2.GetHashCode());
        }

        [Fact]
        public void ComparisonNotEqual()
        {
            var c1 = new C1() { I = 10, S = "a" };
            var c2 = new C1() { I = 10, S = "b" };
            (c1.Equals(c2)).Should().BeFalse();

            c2 = new C1() { I = 10, S = null };
            (c1.Equals(c2)).Should().BeFalse();

            c2 = null;
            (c1.Equals(c2)).Should().BeFalse();

            c1 = new C1() { I = 10, S = null };
            c2 = new C1() { I = 10, S = "b" };
            (c1.Equals(c2)).Should().BeFalse();
        }
    }
}
