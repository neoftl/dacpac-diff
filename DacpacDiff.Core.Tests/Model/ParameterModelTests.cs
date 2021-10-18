using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Model.Tests
{
    [TestClass]
    public class ParameterModelTests
    {
        [TestMethod]
        public void Equals__Null__False()
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();

            var p1 = new ParameterModel(parentMock.Object, "Param");

            // Act
            var res = p1.Equals(null);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Full_match_True(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsTrue(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Diff_parent_name__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var lParentMock = new Mock<IParameterisedModuleModel>();
            lParentMock.SetupGet(m => m.FullName).Returns("ParentL");

            var rParentMock = new Mock<IParameterisedModuleModel>();
            rParentMock.SetupGet(m => m.FullName).Returns("ParentR");

            var p1 = new ParameterModel(lParentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(rParentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        public void Equals__Diff_name__False()
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "ParamL")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "ParamR")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Diff_type__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "LType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "RType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, "DefValue")]
        [DataRow("DefValue", null)]
        [DataRow("DefValueL", "DefValueR")]
        public void Equals__Diff_default__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Diff_order__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 4,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Diff_IsReadOnly__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = false,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("def VALUE[]", "DEFvalue()")]
        public void Equals__Diff_IsOutput__False(string? lDefValue, string? rDefValue)
        {
            // Arrange
            var parentMock = new Mock<IParameterisedModuleModel>();
            parentMock.SetupGet(m => m.FullName).Returns("Parent");

            var p1 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = lDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            };

            var p2 = new ParameterModel(parentMock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = rDefValue,
                Order = 5,
                IsReadOnly = true,
                IsOutput = false
            };

            // Act
            var res = p1.Equals(p2);

            // Assert
            Assert.IsFalse(res);
        }
        
        [TestMethod]
        public void GetHashCode__Changes_for_each_field_change()
        {
            // Arrange
            var hashCodes = new List<int>();
            
            var parent1Mock = new Mock<IParameterisedModuleModel>();
            parent1Mock.SetupGet(m => m.FullName).Returns("Parent");

            var parent2Mock = new Mock<IParameterisedModuleModel>();
            parent2Mock.SetupGet(m => m.FullName).Returns("Parent2");

            // Act
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());

            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param") // Duplicate
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());

            hashCodes.Add(new ParameterModel(parent2Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param2")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType2",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = null,
                Order = 5,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 6,
                IsReadOnly = true,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = false,
                IsOutput = true
            }.GetHashCode());
            
            hashCodes.Add(new ParameterModel(parent1Mock.Object, "Param")
            {
                Type = "PType",
                DefaultValue = "DefValue",
                Order = 5,
                IsReadOnly = true,
                IsOutput = false
            }.GetHashCode());

            // Assert
            Assert.AreEqual(hashCodes.Count, hashCodes.Distinct().Count() + 1);
        }
    }
}