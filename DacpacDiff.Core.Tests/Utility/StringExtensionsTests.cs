using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DacpacDiff.Core.Utility.Tests
{
    [TestClass]
    public class StringExtensions
    {
        [TestMethod]
        [DataRow("a", "a")]
        [DataRow("a()", "a()")]
        [DataRow("(a)", "a")]
        [DataRow("((a))", "a")]
        [DataRow("(a),(b)", "(a),(b)")]
        [DataRow("((a),(b))", "(a),(b)")]
        [DataRow("(a),(b),(c)", "(a),(b),(c)")]
        [DataRow("((((a),(b),(c))))", "(a),(b),(c)")]
        public void ReduceBrackets__Removes_only_unnecessary_parenthesis(string input, string exp)
        {
            // Act
            var result = input.ReduceBrackets();

            // Assert
            Assert.AreEqual(exp, result);
        }

        [TestMethod]
        [DataRow("A\r\n\t b", "ab")]
        [DataRow("(Ab)", "ab")]
        [DataRow("[Ab]", "ab")]
        [DataRow("A(B[C\rd\ne\tF G]H)I", "abcdefghi")]
        public void ScrubSQL__Removes_unwanted_chars(string input, string exp)
        {
            // Act
            var result = input.ScrubSQL();

            // Assert
            Assert.AreEqual(exp, result);
        }

        [TestMethod]
        [DataRow("abcd1234", @"^(a)...(\d)(\d)", true, 4)]
        [DataRow("abcd1234", @"^(a)(x)?(b)", true, 4)]
        [DataRow("abcd1234", @"^xabc", false, 1)]
        public void TryMatch__Returns_success_and_match(string input, string pattern, bool expSuccess, int expMatchCount)
        {
            // Act
            var result = input.TryMatch(pattern, out var m);

            // Assert
            Assert.AreEqual(expSuccess, result);
            Assert.AreEqual(expSuccess, m.Success);
            Assert.AreEqual(expMatchCount, m.Groups.Count);
        }
    }
}
