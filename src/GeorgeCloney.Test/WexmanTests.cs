using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace GeorgeCloney.Test
{
    [TestFixture]
    public class WexmanTests
    {
        [Test]
        [Description("GitHub Pull Request #4")]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void TestCloneOfAbstractClass()
        {
            BaseClass obj1 = new DerivedClass();

            var clone = obj1.DeepCloneWithoutSerialization();

            var clone2 = obj1.DeepCloneWithSerialization();
        }

        [Serializable]
        public abstract class BaseClass
        {
            public string BaseMember { get; set; }
        }

        [Serializable]
        public class DerivedClass : BaseClass
        {
            public string DerivedMember { get; set; }
        }
    }
}
