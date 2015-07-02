using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    [TestFixture]
    public class NiawaMmfBufferHeaderUnitTests
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
        public void Test_NIC_NMBH_ToByteArray()
        {
            //create test header
            Niawa.IpcController.NiawaMmfBufferHeader header = new IpcController.NiawaMmfBufferHeader();
            SortedList<int, KeyValuePair<string, DateTime>> entries = new SortedList<int, KeyValuePair<string, DateTime>>();
            DateTime dt = new DateTime(2001, 1, 1);
            DateTime dt2 = new DateTime(2014,12,1);
            KeyValuePair<string, DateTime> kvp = new KeyValuePair<string, DateTime>("test", dt);
            entries.Add(1, kvp);
            header.Entries = entries;
            header.LatestUpdateDate = dt2;
            header.LatestEntryID = 2;

            //set to byte array and back to object
            Byte[] bytes = header.ToByteArray();

            Niawa.IpcController.NiawaMmfBufferHeader headerNew = new IpcController.NiawaMmfBufferHeader(bytes);

            //assertions
            Assert.AreEqual(header.Entries, headerNew.Entries, "Header Entries doesn't match expectation");
            Assert.AreEqual(header.LatestEntryID, headerNew.LatestEntryID, "Header Latest Entry ID doesn't match expectation");
            Assert.AreEqual(header.LatestUpdateDate, headerNew.LatestUpdateDate, "Header Latest Update Date doesn't match expectation");

        }

        [Test]
        public void Test_NIC_NMBH_Prop_Entries()
        {
            //create test header
            Niawa.IpcController.NiawaMmfBufferHeader header = new IpcController.NiawaMmfBufferHeader();
            SortedList<int, KeyValuePair<string, DateTime>> entries = new SortedList<int, KeyValuePair<string, DateTime>>();
            DateTime dt = new DateTime(2001, 1, 1);
            KeyValuePair<string, DateTime> kvp = new KeyValuePair<string, DateTime>("test", dt);
            entries.Add(1, kvp);
            header.Entries = entries;

            //assertion
            Assert.AreEqual(entries, header.Entries, "Header Entries property doesn't match expectation");
            
        }

        [Test]
        public void Test_NIC_NMBH_Prop_LatestUpdateDate()
        {
            //create test header
            Niawa.IpcController.NiawaMmfBufferHeader header = new IpcController.NiawaMmfBufferHeader();
            DateTime dt2 = new DateTime(2014, 12, 1);
            header.LatestUpdateDate = dt2;

            //assertion
            Assert.AreEqual(dt2, header.LatestUpdateDate, "Header LatestUpdateDate property doesn't match expectation");
        }

        [Test]
        public void Test_NIC_NMBH_Prop_LatestEntryID()
        {
            //create test header
            Niawa.IpcController.NiawaMmfBufferHeader header = new IpcController.NiawaMmfBufferHeader();
            header.LatestEntryID = 2;

            //assertion
            Assert.AreEqual(2, header.LatestEntryID, "Header LatestEntryID doesn't match expectation");
        }

    }
}
