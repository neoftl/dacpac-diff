using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DacpacDiff.Comparer.Tests.TestHelpers
{
        [ExcludeFromCodeCoverage]
    public static class AssertExtensions
    {

        public static void DoesNotContain<T>(this Assert _, IEnumerable<T> collection, Func<T, bool> predicate)
        {
            var arr = collection.ToArray();

            if (arr.Any(predicate))
            {
                var itemIndex = arr.TakeWhile(e => !predicate(e)).Count();
                Assert.Fail("Found matching item in collection at index " + itemIndex);
            }
        }
        public static void ItemAppearsBefore<T>(this Assert _, IEnumerable<T> collection, Func<T, bool> predicateForFirstItem, Func<T, bool> predicateForOtherItems)
        {
            var arr = collection.ToArray();
            var firstItem = arr.SingleOrDefault(predicateForFirstItem);
            if (firstItem == null)
            {
                Assert.Fail("Did not find item 1 in collection");
                return;
            }

            Assert.That.ItemAppearsBefore<T>(arr, firstItem, predicateForOtherItems);
        }
        public static void ItemAppearsBefore<T>(this Assert _, IEnumerable<T> collection, T firstItem, Func<T, bool> predicateForOtherItems)
        {
            var arr = collection.ToArray();
            var otherItems = collection.Where(predicateForOtherItems).ToArray();
            if (otherItems == null)
            {
                Assert.Fail("Did not match other items in collection");
                return;
            }

            Assert.That.ItemAppearsBefore<T>(arr, firstItem, otherItems);
        }
        public static void ItemAppearsBefore<T>(this Assert _, IEnumerable<T> collection, Func<T, bool> predicateForFirstItem, params T[] otherItems)
        {
            var arr = collection.ToArray();
            var firstItem = arr.SingleOrDefault(predicateForFirstItem);
            if (firstItem == null)
            {
                Assert.Fail("Did not find item 1 in collection");
                return;
            }

            Assert.That.ItemAppearsBefore<T>(arr, firstItem, otherItems);
        }
        public static void ItemAppearsBefore<T>(this Assert _, IEnumerable<T> collection, T firstItem, params T[] otherItems)
        {
            var arr = collection.ToArray();
            var firstItemIndex = Array.IndexOf(arr, firstItem);
            var otherItemFirstIndex = otherItems.Select(e => Array.IndexOf(arr, e)).Min();
            Assert.IsTrue(firstItemIndex < otherItemFirstIndex, "Item 1 ({0}) appeared at index {1}; first of other items ({2}) appeared at index {3}", firstItem, firstItemIndex, otherItems.First(), otherItemFirstIndex);
        }

        public static void AreEqual<T>(this CollectionAssert _, IEnumerable<T> collection, params Func<T, bool>[] predicates)
        {
            Assert.AreEqual(predicates.Length, collection.Count());
            CollectionAssert.AreEqual(Enumerable.Range(1, predicates.Length).Select(i => true).ToArray(),
                collection.Zip(predicates).Select(e => e.Second(e.First)).ToArray());
        }
    }
}
