using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class InlineSortedListCreatorUnitTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void Test_NU_ISLC_CreateStrStr()
        {
            string testKey = "TestKey";
            string testValue = "TestValue";

            SortedList<string, string> list = Niawa.Utilities.InlineSortedListCreator.CreateStrStr(testKey, testValue);

            Assert.IsNotNull(list, "List is null");
            Assert.IsNotEmpty(list, "List is empty");

            Assert.True(list.ContainsKey(testKey), "List does not contain key");
            Assert.AreEqual(testValue, list[testKey], "List does not contain value");

        }

    }
}
