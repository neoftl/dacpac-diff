using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
namespace DacpacDiff.Core.Utility.Tests
{
    [TestClass]
    public class CollectionExtensionsTests
    {
        [TestMethod]
        [DataRow(0, null, null, null, 0, DisplayName = "no values")]
        [DataRow(1, null, null, null, 0, DisplayName = "null")]
        [DataRow(3, 1d, 2, true, -1085276117, DisplayName = "different types")]
        [DataRow(3, 1d, 2, false, -1085276134, DisplayName = "diff result on any change")]
        // Note: strings change each run
        public void CalculateHashCode__Generates_code_for_inputs(int count, object a, object b, object c, int expected)
        {
            // Arrange
            var values = new object[] { a, b, c }.Take(count).ToArray();

            // Act
            var res = values.CalculateHashCode();

            // Assert
            Assert.AreEqual(expected, res);
        }

        [TestMethod]
        public void CalculateHashCode__Not_limited_in_count()
        {
            // Arrange
            var values = Enumerable.Range(1, CollectionExtensions.PRIMES.Length * CollectionExtensions.PRIMES.Length)
                .Select(i => (object)(i * i)).ToArray();

            // Act
            var res = values.CalculateHashCode();

            // Assert
            Assert.AreEqual(212598626, res);
        }

        [TestMethod]
        public void Get__Null_dict__Null()
        {
            // Act
            var res = ((IDictionary<string, object>?)null).Get("key");

            // Assert
            Assert.IsNull(res);
        }

        [TestMethod]
        public void Get__Missing_key__Nul()
        {
            // Arrange
            var dict = new Dictionary<string, object>();

            // Act
            var res = dict.Get("key");

            // Assert
            Assert.IsNull(res);
        }

        [TestMethod]
        public void Get__Has_key__Value()
        {
            // Arrange
            var obj = new object();

            var dict = new Dictionary<string, object>
            {
                ["key"] = obj
            };

            // Act
            var res = dict.Get("key");

            // Assert
            Assert.AreSame(obj, res);
        }
        
        [TestMethod]
        public void Merge_1__Null_dict__Null()
        {
            // Act
            var res = ((IDictionary<string, object>?)null).Merge(Array.Empty<KeyValuePair<string, object>>());

            // Assert
            Assert.IsNull(res);
        }
        
        [TestMethod]
        public void Merge_1__Null_arg__Noop()
        {
            // Arrange
            var dict = new Dictionary<string, object>
            {
                ["key1"] = "value1"
            };

            // Act
            IDictionary<string, object>? arg = null;
            var res = dict.Merge(arg);

            // Assert
            Assert.AreSame(dict, res);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("value1", res["key1"]);
        }
        
        [TestMethod]
        public void Merge_1__Combines_items_from_other_array()
        {
            // Arrange
            var arr = new Dictionary<string, string>
            {
                ["key1"] = "value1b",
                ["key3"] = "value3"
            };

            var dict = new Dictionary<string, object>
            {
                ["key1"] = "value1a",
                ["key2"] = "value2"
            };

            // Act
            var res = dict.Merge(arr, e => e.Key, e => e.Value);

            // Assert
            Assert.AreSame(dict, res);
            Assert.AreEqual(3, res.Count);
            Assert.AreEqual("value1b", res["key1"]); // Overwrite
            Assert.AreEqual("value2", res["key2"]); // Ignore
            Assert.AreEqual("value3", res["key3"]); // Add
        }

        [TestMethod]
        public void Merge_2__Null_dict__Null()
        {
            // Act
            var res = ((IDictionary<string, object>?)null).Merge(Array.Empty<object>(), (o) => throw new NotImplementedException(), (o) => throw new NotImplementedException());

            // Assert
            Assert.IsNull(res);
        }

        [TestMethod]
        public void Merge_2__Null_arg__Noop()
        {
            // Arrange
            var dict = new Dictionary<string, object>
            {
                ["key1"] = "value1"
            };

            // Act
            var res = dict.Merge((IDictionary<string, object>?)null, e => e.Key, e => e.Value);

            // Assert
            Assert.AreSame(dict, res);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("value1", res["key1"]);
        }

        [TestMethod]
        public void Merge_2__Combines_items_from_other_array()
        {
            // Arrange
            var arr = new[]
            {
                new KeyValuePair<string, string>("key1", "value1b"),
                new KeyValuePair<string, string>("key3", "value3")
            };

            var dict = new Dictionary<string, object>
            {
                ["key1"] = "value1a",
                ["key2"] = "value2"
            };

            // Act
            var res = dict.Merge(arr, e => e.Key, e => e.Value);

            // Assert
            Assert.AreSame(dict, res);
            Assert.AreEqual(3, res.Count);
            Assert.AreEqual("value1b", res["key1"]); // Overwrite
            Assert.AreEqual("value2", res["key2"]); // Ignore
            Assert.AreEqual("value3", res["key3"]); // Add
        }

        [TestMethod]
        public void NotNull__Keeps_only_not_null_items()
        {
            // Arrange
            var arr = new object?[]
            {
                1,
                "str",
                null,
                4L,
                5M,
                DateTime.UtcNow,
                default(bool?)
            };

            // Act
            var res = arr.NotNull().ToArray();

            // Assert
            Assert.AreEqual(7, arr.Length);
            Assert.AreEqual(5, res.Length);
        }

        [TestMethod]
        public void TryGetValue__Null_coll__False()
        {
            // Act
            var res = ((object[]?)null).TryGetValue(e => true, out var value);

            // Assert
            Assert.IsFalse(res);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TryGetValue__No_predicate_matches__False()
        {
            // Arrange
            var arr = new int[]
            {
                1
            };

            // Act
            var res = arr.TryGetValue(e => false, out var value);

            // Assert
            Assert.IsFalse(res);
            Assert.AreEqual(default, value);
        }

        [TestMethod]
        public void TryGetValue__Returns_true_with_first_value_that_matches_predicate()
        {
            // Arrange
            var arr = new int[]
            {
                1,
                2,
                3
            };

            // Act
            var res = arr.TryGetValue(e => e == 2, out var value);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual(2, value);
        }
    }
}