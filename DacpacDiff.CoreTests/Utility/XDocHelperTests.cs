using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;

namespace DacpacDiff.Core.Utility.Tests
{
    [TestClass]
    public class XDocHelperTests
    {
        private const string XML = @"<root>
    <child attr1=""A"" attr2=""C"">
        <value>V1</value>
        <value>V2</value>
    </child>
    <child attr1=""B"" attr2=""C"">
        <value>V3</value>
        <value>V4</value>
    </child>
    <child attr2=""D"" attr3=""E"">
        <value>V5</value>
        <value>V6</value>
    </child>
</root>";

        [TestMethod]
        public void Find__Named_attributes()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", "attr1", "attr2");

            // Assert
            Assert.AreEqual(2, res.Length);
        }
        
        [TestMethod]
        public void Find__Specific_value__Find_none()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr1", "X"));

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Find__Specific_value__Find_one()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr1", "A"));

            // Assert
            Assert.AreEqual(1, res.Length);
        }
        
        [TestMethod]
        public void Find__Specific_value__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr2", "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }

        [TestMethod]
        public void Find__Specific_values_Multiple_checks__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr2", "C"), ("attr2", "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }

        [TestMethod]
        public void Find__Specific_value_Deep_elements_Check_shallow__Find_none()
        {
            // Arrange
            var doc = XDocument.Parse($"<parent>{XML}</parent>");

            // Act
            var res = doc.Root.Find("child", ("attr2", "C"));

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Find__Specific_value_Deep_elements_Check_deep__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse($"<parent>{XML}</parent>");

            // Act
            var res = doc.Root.Find(true, "child", ("attr2", "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }

        [TestMethod]
        public void Find__Many_roots_Specific_value__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse(XML);
            var roots = new [] { doc.Root, doc.Root };

            // Act
            var res = roots.Find("child", ("attr2", "C"));

            // Assert
            Assert.AreEqual(4, res.Length);
        }
        
        [TestMethod]
        public void Find__Dynamic_value__Find_none()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr1", v => v == "X"));

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Find__Dynamic_value__Find_one()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr1", v => v == "A"));

            // Assert
            Assert.AreEqual(1, res.Length);
        }
        
        [TestMethod]
        public void Find__Dynamic_value__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr2", v => v == "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }

        [TestMethod]
        public void Find__Dynamic_value_Multiple_checks__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse(XML);

            // Act
            var res = doc.Root.Find("child", ("attr1", v => v == "A" || v == "B"), ("attr2", v => v == "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }

        [TestMethod]
        public void Find__Dynamic_value_Deep_elements_Check_shallow__Find_none()
        {
            // Arrange
            var doc = XDocument.Parse($"<parent>{XML}</parent>");

            // Act
            var res = doc.Root.Find("child", ("attr2", v => v == "C"));

            // Assert
            Assert.AreEqual(0, res.Length);
        }

        [TestMethod]
        public void Find__Dynamic_value_Deep_elements_Check_deep__Find_many()
        {
            // Arrange
            var doc = XDocument.Parse($"<parent>{XML}</parent>");

            // Act
            var res = doc.Root.Find(true, "child", ("attr2", v => v == "C"));

            // Assert
            Assert.AreEqual(2, res.Length);
        }
    }
}
