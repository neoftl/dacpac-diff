using DacpacDiff.Core.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DacpacDiff.Mssql.Tests
{
    [TestClass]
    public class MssqlFormatProviderTests
    {
        private MssqlFormatProvider _factory = new();
        private Dictionary<Type, Func<ISqlFormattable, ISqlFormatter>> _sqlFormatters = new();

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
            _sqlFormatters = (Dictionary<Type, Func<ISqlFormattable, ISqlFormatter>>)typeof(MssqlFormatProvider).GetField("_sqlFormatters", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_factory);
            _sqlFormatters.Clear();
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            _ = new MssqlFormatProvider().FormatName;
            _ = new TestSql().Title;
            _ = new TestSql().Name;
        }

        [TestMethod]
        public void GetSqlFileBuilder()
        {
            // Arrange
            var prov = new MssqlFormatProvider();

            // Act
            var res = prov.GetSqlFileBuilder();

            // Assert
            Assert.IsInstanceOfType(res, typeof(MssqlFileBuilder));
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