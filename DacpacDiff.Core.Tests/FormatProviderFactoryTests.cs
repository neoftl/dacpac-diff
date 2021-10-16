using DacpacDiff.Core.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DacpacDiff.Core.Tests
{
    [TestClass]
    public class FormatProviderFactoryTests
    {
        private FormatProviderFactory _factory = new();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IDictionary<string, Func<IFormatProvider>> _formatProviders;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [ExcludeFromCodeCoverage]
        public class TestFormatProvider : IFormatProvider
        {
            public string FormatName { get; } = nameof(TestFormatProvider);
            public ISqlFileBuilder GetSqlFileBuilder() => throw new NotImplementedException();
            public ISqlFormatter GetSqlFormatter(ISqlFormattable sqlObj) => throw new NotImplementedException();
        }

        [TestInitialize]
        public void Init()
        {
            _factory = new FormatProviderFactory();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
            _formatProviders = (IDictionary<string, Func<IFormatProvider>>)typeof(FormatProviderFactory).GetField("_formatProviders", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_factory);
            _formatProviders.Clear();
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [TestMethod]
        public void GetFormat__Unknown_format__Fail()
        {
            // Act
            var ex = Assert.ThrowsException<NotImplementedException>(() => _factory.GetFormat("format"));

            // Assert
            StringAssert.StartsWith(ex.Message, "Unregistered format: format");
        }

        [TestMethod]
        public void GetFormat__Gets_known_format()
        {
            // Arrange
            var provMock = new Mock<IFormatProvider>();

            _formatProviders["format"] = () => provMock.Object;

            // Act
            var res = _factory.GetFormat("format");

            // Assert
            Assert.AreSame(provMock.Object, res);
        }

        [TestMethod]
        public void GetFormat__Nothing_registered__Finds_item_in_assembly()
        {
            // Act
            var res = _factory.GetFormat(nameof(TestFormatProvider));

            // Assert
            Assert.IsInstanceOfType(res, typeof(TestFormatProvider));
        }
    }
}