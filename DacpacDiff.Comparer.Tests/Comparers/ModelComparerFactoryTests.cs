using DacpacDiff.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DacpacDiff.Comparer.Comparers.Tests
{
    [TestClass]
    public class ModelComparerFactoryTests
    {
        private ModelComparerFactory _factory = new();
        private Dictionary<Type, Func<IModelComparer>> _modelComparers = new();
        
        public class TestModel : IModel
        {
            public string FullName { get; } = string.Empty;
            public string Name { get; } = string.Empty;
        }

        [TestInitialize]
        public void Init()
        {
            _factory = new ModelComparerFactory();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
            _modelComparers = (Dictionary<Type, Func<IModelComparer>>)typeof(ModelComparerFactory).GetField("_modelComparers", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_factory);
            _modelComparers.Clear();
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            _ = new TestModel().Name;
        }

        [TestMethod]
        public void GetComparer__Fails_if_model_type_not_mapped()
        {
            // Arrange
            var comparerMock = new Mock<IModelComparer>();

            // Act
            var ex = Assert.ThrowsException<NotImplementedException>(() => _factory.GetComparer<TestModel>());

            // Assert
            StringAssert.StartsWith(ex.Message, "Unknown model type to compare: ");
        }

        [TestMethod]
        public void GetComparer__Fails_if_mapped_result_is_invalid()
        {
            // Arrange
            var comparerMock = new Mock<IModelComparer>();
            _modelComparers[typeof(TestModel)] = () => comparerMock.Object;

            // Act
            var ex = Assert.ThrowsException<InvalidCastException>(() => _factory.GetComparer<TestModel>());

            // Assert
            StringAssert.Contains(ex.Message, "to type 'DacpacDiff.Comparer.Comparers.IModelComparer`1[DacpacDiff.Comparer.Comparers.Tests.ModelComparerFactoryTests+TestModel]'");
        }

        [TestMethod]
        public void GetComparer__Returns_mapped_result()
        {
            // Arrange
            var comparerMock = new Mock<IModelComparer<TestModel>>();
            _modelComparers[typeof(TestModel)] = () => comparerMock.Object;

            // Act
            var res = _factory.GetComparer<TestModel>();

            // Assert
            Assert.AreSame(comparerMock.Object, res);
        }
    }
}