using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeorgeCloney.Test
{
    [TestClass]
    public class GitHubIssues
    {
        [TestMethod]
        [Description("GitHub Issue #1")]
        public void CloneObjectPropertiesAsCorrectType()
        {
            var foo = new Foo() { SomeObject = new Bar() { SomeValue = 13 } };
            var clone = foo.DeepClone();

            Assert.IsInstanceOfType(clone.SomeObject, typeof(Bar));
        }


        class Foo
        {
            public object SomeObject { get; set; }
        }

        class Bar
        {
            public int SomeValue { get; set; }
        }
    }
}
