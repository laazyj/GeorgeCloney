using NUnit.Framework;

namespace GeorgeCloney.Test
{
    [TestFixture]
    public class GitHubIssues
    {
        [Test]
        [Description("GitHub Issue #1")]
        public void CloneObjectPropertiesAsCorrectType()
        {
            var foo = new Foo() { SomeObject = new Bar() { SomeValue = 13 } };
            var clone = foo.DeepClone();

            Assert.IsInstanceOf<Bar>(clone.SomeObject);
        }

        [Test]
        [Description("GitHub Issue #2")]
        public void ClonePropertiesFromBaseClasses()
        {
            var q = new BarRoomQueen
                {
                    SomeValue = 15,
                    FromMemphis = true
                };

            var clone = q.DeepCloneWithoutSerialization();

            Assert.AreEqual(q.FromMemphis, clone.FromMemphis);
            Assert.AreEqual(q.SomeValue, clone.SomeValue);
        }

        class Foo
        {
            public object SomeObject { get; set; }
        }

        class Bar
        {
            public int SomeValue { get; set; }
        }

        class BarRoomQueen : Bar
        {
            public bool FromMemphis { get; set; }
        }
    }
}
