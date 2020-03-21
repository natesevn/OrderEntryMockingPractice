using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OrderEntryMockingPracticeTests
{
    [TestClass]
    public class OrderServiceTests
    {
        [TestMethod]
        public void TestMethodRunsProperly()
        {
            var testMsg = "hello";
            Assert.AreEqual("hello", testMsg);
        }
    }
}
