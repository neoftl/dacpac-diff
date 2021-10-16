using DacpacDiff.Core.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DacpacDiff.Mssql.Tests
{
    [TestClass]
    public class MssqlFormatProviderTests
    {
        private MssqlFormatProvider _factory = new();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IDictionary<Type, Func<ISqlFormattable, ISqlFormatter>> _sqlFormatters;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [ExcludeFromCodeCoverage]
        public class TestSql : ISqlFormattable
        {
            public string? Title { get; } = string.Empty;
            public string Name { get; } = string.Empty;
        }

        [TestInitialize]
        public void Init()
        {
            _factory = new MssqlFormatProvider();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
            _sqlFormatters = (IDictionary<Type, Func<ISqlFormattable, ISqlFormatter>>)typeof(MssqlFormatProvider).GetField("_sqlFormatters", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_factory);
            _sqlFormatters.Clear();
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [TestMethod]
        public void GetSqlFileBuilder()
        {
            // Act
            var res = _factory.GetSqlFileBuilder();

            // Assert
            Assert.IsInstanceOfType(res, typeof(MssqlFileBuilder));
            Assert.AreEqual("mssql", _factory.FormatName);
        }

        [TestMethod()]
        public void GetSqlFormatter__Fails_if_object_type_not_mapped()
        {
            // Arrange
            var sql = new TestSql();

            // Act
            var ex = Assert.ThrowsException<NotImplementedException>(() => _factory.GetSqlFormatter(sql));

            // Assert
            StringAssert.StartsWith(ex.Message, "No SQL formatter registered for type: ");
        }

        [TestMethod()]
        public void GetSqlFormatter__Returns_formatter_for_type()
        {
            // Arrange
            var sql = new TestSql();

            var formatterMock = new Mock<ISqlFormatter>();

            object? calledWith = null;
            _sqlFormatters[typeof(TestSql)] = (o) =>
            {
                calledWith = o;
                return formatterMock.Object;
            };

            // Act
            var res = _factory.GetSqlFormatter(sql);

            // Assert
            Assert.AreSame(formatterMock.Object, res);
            Assert.AreSame(sql, calledWith);
        }
    }
}