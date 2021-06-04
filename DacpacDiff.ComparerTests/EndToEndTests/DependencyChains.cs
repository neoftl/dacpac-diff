using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DacpacDiff.Comparer.Tests.EndToEndTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class DependencyChains
    {
        [TestMethod]
        public void Function_alter_requires_all_dependents_to_be_dropped_and_recreated()
        {

        }
    }
}
