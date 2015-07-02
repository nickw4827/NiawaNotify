using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.IpcControllerTestClient.UnitTests
{
    public class ComplexMmfReadWriteUnitTests
    {
        private const int TEST_BUFFER_LENGTH = 32000;

        //utility bus
        Niawa.Utilities.UtilsServiceBus _utilsBus = null;

        [SetUp]
        public void Setup()
        {
            _utilsBus = new Niawa.Utilities.UtilsServiceBus();
        }

        [TearDown]
        public void TearDown()
        {
            _utilsBus = null;
        }


        [Test]
        public void Test_NIC_ComplexMmf_Write1Message()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
        
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);
            Niawa.IpcController.MmfReader reader = new IpcController.MmfReader(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            writer.StartWriting();
            reader.StartListening();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero before writing message");

            //write some data
            Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer(DateTime.Now, TEST_IPC_TYPE, "test data");
            writer.WriteData(msg);

            System.Threading.Thread.Sleep(500);

            //assertion
            Assert.AreEqual(1, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be 1 after writing message");

            //retrieve message
            //while (reader.CountMessagesInBuffer() == 0)
            //{
            //    System.Threading.Thread.Sleep(100);
            //}

            Niawa.IpcController.NiawaMmfContainer msgNew = reader.GetNextMessageFromBuffer();

            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero after reading message");

            //assertions
            Assert.AreEqual(msg.IpcData, msgNew.IpcData, "Message received from buffer doesn't have the same IpcData");
            Assert.AreEqual(msg.IpcType, msgNew.IpcType, "Message received from buffer doesn't have the same IpcType");
            Assert.AreEqual(msg.RefreshedDate, msgNew.RefreshedDate, "Message received from buffer doesn't have the same RefreshedDate");
            Assert.AreEqual(msg.SerialID, msgNew.SerialID, "Message received from buffer doesn't have the same SerialID");
            Assert.AreEqual(msg.ToString(), msgNew.ToString(), "Message received from buffer doesn't have the same ToString value");

            reader.StopListening(true);
            writer.StopWriting(true);

        }

        [Test]
        public void Test_NIC_ComplexMmf_Write10Messages_DelayedRead_DataLoss()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();
            
            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);
            Niawa.IpcController.MmfReader reader = new IpcController.MmfReader(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            SortedList<int, Niawa.IpcController.NiawaMmfContainer> sourceMessages = new SortedList<int, Niawa.IpcController.NiawaMmfContainer>();
            SortedList<int, Niawa.IpcController.NiawaMmfContainer> readMessages = new SortedList<int, Niawa.IpcController.NiawaMmfContainer>();
            
            writer.StartWriting();
            System.Threading.Thread.Sleep(100);

            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero before writing message");

            //write some data
            int ix = 0;
            while (ix < 10)
            {
                ix++;

                System.Diagnostics.Debug.WriteLine("Writing message " + ix);

                Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer(DateTime.Now, TEST_IPC_TYPE, "test data " + ix);
                sourceMessages.Add(ix, msg);
                
                writer.WriteData(msg);
            }

            //wait for messages to be written
            int ixWriteTimeout = 75;
            while (writer.CountMessagesInSendQueue() != 0 && ixWriteTimeout > 0)
            {
                ixWriteTimeout--;
                System.Diagnostics.Debug.WriteLine("Writing messages " + writer.CountMessagesInSendQueue() + " remaining timeout " + ixWriteTimeout);
                
                System.Threading.Thread.Sleep(100);
            }


            reader.StartListening();

            //wait for messages
            int ixTimeout = 75;
            int ixCountMessages = 0;
            while ((ixCountMessages = reader.CountMessagesInBuffer()) != 1 && ixTimeout > 0)
            {
                ixTimeout--;
                System.Diagnostics.Debug.WriteLine("Waiting for messages currently " + ixCountMessages + " timeout " + ixTimeout);

                System.Threading.Thread.Sleep(100);
                
            }

            //assertion
            Assert.AreEqual(1, reader.CountMessagesInBuffer(), "Count of messages in buffer did not match expectations after writing messages");

            //get messages
            int ixRead = 0;
            while (reader.CountMessagesInBuffer() != 0)
            {
                ixRead++;

                System.Diagnostics.Debug.WriteLine("Reading message " + ixRead);
                
                Niawa.IpcController.NiawaMmfContainer msgNew = reader.GetNextMessageFromBuffer();
                readMessages.Add(ixRead, msgNew); 
            }

            //compare read and written messages
            foreach (KeyValuePair<int, Niawa.IpcController.NiawaMmfContainer> kvp in sourceMessages)
            {
                //skip first 9 messages
                if (kvp.Key > 9)
                {

                    //get source messages
                    if (readMessages.ContainsKey(kvp.Key - 9))
                    {
                        Niawa.IpcController.NiawaMmfContainer msg = kvp.Value;
                        Niawa.IpcController.NiawaMmfContainer msgNew = readMessages[kvp.Key - 9];

                        //assertions
                        Assert.AreEqual(msg.IpcData, msgNew.IpcData, "Message [" + kvp.Key + "] received from buffer doesn't have the same IpcData");
                        Assert.AreEqual(msg.IpcType, msgNew.IpcType, "Message [" + kvp.Key + "] received from buffer doesn't have the same IpcType");
                        Assert.AreEqual(msg.RefreshedDate, msgNew.RefreshedDate, "Message [" + kvp.Key + "] received from buffer doesn't have the same RefreshedDate");
                        Assert.AreEqual(msg.SerialID, msgNew.SerialID, "Message [" + kvp.Key + "] received from buffer doesn't have the same SerialID");
                        Assert.AreEqual(msg.ToString(), msgNew.ToString(), "Message [" + kvp.Key + "] received from buffer doesn't have the same ToString value");

                    }
                    else
                    {
                        Assert.Fail("Received messages array does not include source message " + kvp.Key);

                    }
                }
                else
                {
                    //skip - was overwritten
                }

            }

            reader.StopListening(true);
            writer.StopWriting(true);

        }

        
        [Test]
        public void Test_NIC_ComplexMmf_Write150Messages_ConcurrentRead()
        {
            string TEST_IPC_TYPE = "TEST_IPC_TYPE_" + Guid.NewGuid().ToString();

            Niawa.IpcController.MmfWriter writer = new IpcController.MmfWriter(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);
            Niawa.IpcController.MmfReader reader = new IpcController.MmfReader(TEST_IPC_TYPE, TEST_BUFFER_LENGTH, _utilsBus, false);

            SortedList<int, Niawa.IpcController.NiawaMmfContainer> sourceMessages = new SortedList<int, Niawa.IpcController.NiawaMmfContainer>();
            SortedList<int, Niawa.IpcController.NiawaMmfContainer> readMessages = new SortedList<int, Niawa.IpcController.NiawaMmfContainer>();

            writer.StartWriting();
            reader.StartListening();
            System.Threading.Thread.Sleep(100);
            
            //assertion
            Assert.AreEqual(0, reader.CountMessagesInBuffer(), "Count of messages in buffer expected to be zero before writing message");

            //write some data
            int ix = 0;
            int sleepOffset = 1000;
            while (ix < 150)
            {
                ix++;

                System.Diagnostics.Debug.WriteLine("Writing message " + ix);

                Niawa.IpcController.NiawaMmfContainer msg = new IpcController.NiawaMmfContainer(DateTime.Now, TEST_IPC_TYPE, "test data " + ix);
                sourceMessages.Add(ix, msg);

                writer.WriteData(msg);
                
                //no buffer, so sensitive to losing data.  start up slowly
                System.Threading.Thread.Sleep(50 + sleepOffset);
                if (sleepOffset > 0) sleepOffset = sleepOffset - 100;
                if (sleepOffset < 0) sleepOffset = 0;

            }

  
            //wait for messages
            int ixTimeout = 200;
            int ixCountMessages = 0;
            while ((ixCountMessages = reader.CountMessagesInBuffer()) < 50 && ixTimeout > 0)
            {
                ixTimeout--;
                System.Diagnostics.Debug.WriteLine("Waiting for messages currently " + ixCountMessages + " timeout " + ixTimeout);

                System.Threading.Thread.Sleep(100);

            }

            //get messages
            int ixRead = 0;
            int ixReadTimeout = 400;

            while ((reader.CountMessagesInBuffer() != 0 || ixRead != 150) && ixReadTimeout > 0)
            {
                ixReadTimeout--;

                if (reader.CountMessagesInBuffer() != 0)
                {
                    ixRead++;
                    System.Diagnostics.Debug.WriteLine("Reading message " + ixRead);

                    Niawa.IpcController.NiawaMmfContainer msgNew = reader.GetNextMessageFromBuffer();
                    readMessages.Add(ixRead, msgNew);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Waiting for read to complete " + ixRead + " timeout " + ixReadTimeout);
                    System.Threading.Thread.Sleep(100);
                }

            }

            //compare read and written messages
            foreach (KeyValuePair<int, Niawa.IpcController.NiawaMmfContainer> kvp in sourceMessages)
            {
                
                //get source messages
                if (readMessages.ContainsKey(kvp.Key))
                {
                    Niawa.IpcController.NiawaMmfContainer msg = kvp.Value;
                    Niawa.IpcController.NiawaMmfContainer msgNew = readMessages[kvp.Key];

                    //assertions
                    Assert.AreEqual(msg.IpcData, msgNew.IpcData, "Message [" + kvp.Key + "] received from buffer doesn't have the same IpcData");
                    Assert.AreEqual(msg.IpcType, msgNew.IpcType, "Message [" + kvp.Key + "] received from buffer doesn't have the same IpcType");
                    Assert.AreEqual(msg.RefreshedDate, msgNew.RefreshedDate, "Message [" + kvp.Key + "] received from buffer doesn't have the same RefreshedDate");
                    Assert.AreEqual(msg.SerialID, msgNew.SerialID, "Message [" + kvp.Key + "] received from buffer doesn't have the same SerialID");
                    Assert.AreEqual(msg.ToString(), msgNew.ToString(), "Message [" + kvp.Key + "] received from buffer doesn't have the same ToString value");

                }
                else
                {
                    Assert.Fail("Received messages array does not include source message " + kvp.Key);

                }

            }

            //assertion
            Assert.AreEqual(150, readMessages.Count, "Count of messages in read queue did not match expectations after writing messages");

            reader.StopListening(true);
            writer.StopWriting(true);
        }


    }
}
