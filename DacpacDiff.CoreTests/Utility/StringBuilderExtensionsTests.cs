using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace DacpacDiff.Core.Utility.Tests
{
    [TestClass]
    public class StringBuilderExtensionsTests
    {
        [TestMethod]
        public void AppendIf__True__Value_is_appended()
        {
            // Arrange
            var sb = new StringBuilder("start-");

            var isResolved = false;
            string getString()
            {
                isResolved = true;
                return "append";
            }

            // Act
            var res = sb.AppendIf(getString, true);

            // Assert
            Assert.AreSame(sb, res);
            Assert.IsTrue(isResolved);
            Assert.AreEqual("start-append", sb.ToString());
        }

        [TestMethod]
        public void AppendIf__True_Null__Value_ignored()
        {
            // Arrange
            var sb = new StringBuilder("start-");

            // Act
            var res = sb.AppendIf(() => null, true);

            // Assert
            Assert.AreSame(sb, res);
            Assert.AreEqual("start-", sb.ToString());
        }

        [TestMethod]
        public void AppendIf__False__Value_ignored()
        {
            // Arrange
            var sb = new StringBuilder("start-");
            
            var isResolved = false;
            string getString()
            {
                isResolved = true;
                return "append";
            }

            // Act
            var res = sb.AppendIf(getString, false);

            // Assert
            Assert.AreSame(sb, res);
            Assert.IsFalse(isResolved);
            Assert.AreEqual("start-", sb.ToString());
        }

        [TestMethod]
        [DataRow("", "{NL}")]
        [DataRow("str", "str{NL}")]
        public void EnsureLine__No_newline__Appends_newline(string input, string exp)
        {
            // Arrange
            var sb = new StringBuilder(input.Replace("{NL}", StringBuilderExtensions.NL));

            // Act
            var res = sb.EnsureLine();

            // Assert
            Assert.AreSame(sb, res);
            Assert.AreEqual(exp.Replace("{NL}", StringBuilderExtensions.NL), sb.ToString());
        }

        [TestMethod]
        [DataRow("{NL}")]
        [DataRow("str{NL}")]
        [DataRow("str{NL}{NL}")]
        public void EnsureLine__Ends_with_newline__No_change(string input)
        {
            // Arrange
            var exp = input;

            var sb = new StringBuilder(input.Replace("{NL}", StringBuilderExtensions.NL));

            // Act
            var res = sb.EnsureLine();

            // Assert
            Assert.AreSame(sb, res);
            Assert.AreEqual(exp.Replace("{NL}", StringBuilderExtensions.NL), sb.ToString());
        }

        [TestMethod]
        [DataRow(3, "\n", "", "{NL}{NL}{NL}")]
        [DataRow(3, "\n", "str", "str{NL}{NL}{NL}")]
        [DataRow(3, "\n", "str{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\n", "str{NL}{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\n", "str{NL}{NL}{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\n", "str{NL}{NL}{NL}{NL}", "str{NL}{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "", "{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "str", "str{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "str{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "str{NL}{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "str{NL}{NL}{NL}", "str{NL}{NL}{NL}")]
        [DataRow(3, "\r\n", "str{NL}{NL}{NL}{NL}", "str{NL}{NL}{NL}{NL}")]
        public void EnsureLine__Can_specify_EOL_type_and_count(int count, string nl, string input, string exp)
        {
            // Arrange
            var sb = new StringBuilder(input.Replace("{NL}", nl));

            // Act
            var res = sb.EnsureLine(count, nl);

            // Assert
            Assert.AreSame(sb, res);
            Assert.AreEqual(exp.Replace("{NL}", nl), sb.ToString());
        }
    }
}