using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class UtilsServiceBusUnitTests
    {
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
        public void Test_NU_USB_InitializeSerialId()
        {
            int serialIDRoot = 123;

            //get serial ID
            Niawa.Utilities.SerialId id = _utilsBus.InitializeSerialId(serialIDRoot);

            //assertions
            Assert.AreEqual(serialIDRoot, id.IdRoot, "SerialIDRoot is not equal");

        }

        [Test]
        public void Test_NU_USB_InitializeSerialId_TwoThreads()
        {
            int serialIDRoot = 123;
            
            Niawa.Utilities.SerialId id1 = null;
            Niawa.Utilities.SerialId id2 = null;

            //thread 1 
            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    //get serial ID
                    id1 = _utilsBus.InitializeSerialId(serialIDRoot);

                }
                );

            //thread 2 
            System.Threading.Thread t2 = new System.Threading.Thread
                (delegate()
                {
                    //get serial ID
                    id2 = _utilsBus.InitializeSerialId(serialIDRoot);
    
                }
                );

            //start threads
            t1.Start();
            t2.Start();

            //wait for threads to finish
            t1.Join(100);
            t2.Join(100);

            //assertions
            Assert.AreEqual(serialIDRoot, id1.IdRoot, "SerialIDRoot is not equal on ID1");
            Assert.AreEqual(serialIDRoot, id2.IdRoot, "SerialIDRoot is not equal on ID2");

            Assert.AreNotEqual(id1.IdSession, id2.IdSession, "IDSession is equal on ID1 and ID2");

        }

        [Test]
        public void Test_NU_USB_SetValueToRegistry()
        {
            string category1 = "cat1";
            string value1 = "value1";
            
            //add value
            _utilsBus.SetValueToRegistry(category1, value1);

            Assert.True(_utilsBus.RegistryContainsValue(category1, value1), "Registry does not contain value added");

        }

        [Test]
        public void Test_NU_USB_SetValueToRegistry_TwoValuesTwoCategories()
        {
            string category1 = "cat1";
            string category2 = "cat2";
            string value1 = "value1";
            string value2 = "value2";

            //add values
            _utilsBus.SetValueToRegistry(category1, value1);
            _utilsBus.SetValueToRegistry(category1, value2);
            _utilsBus.SetValueToRegistry(category2, value1);
            
            Assert.True(_utilsBus.RegistryContainsValue(category1, value1), "Registry does not contain c1v1 added");
            Assert.True(_utilsBus.RegistryContainsValue(category1, value2), "Registry does not contain c1v2 added");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value1), "Registry does not contain c2v1 added");

        }

        [Test]
        public void Test_NU_USB_RegistryContainsValue()
        {
            string category1 = "cat1";
            string category2 = "cat2";
            string value1 = "value1";
            string value2 = "value2";

            //add value
            _utilsBus.SetValueToRegistry(category1, value1);

            Assert.True(_utilsBus.RegistryContainsValue(category1, value1), "Registry does not contain c1v1 added");
            Assert.False(_utilsBus.RegistryContainsValue(category1, value2), "Registry should not contain c1v2");
            Assert.False(_utilsBus.RegistryContainsValue(category2, value1), "Registry should not contain c2v1");

        }

        [Test]
        public void Test_NU_USB_RemoveValueFromRegistry()
        {
            string category1 = "cat1";
            string category2 = "cat2";
            string category3 = "cat3";
            string value1 = "value1";
            string value3 = "value3";

            //add values
            _utilsBus.SetValueToRegistry(category1, value1);
            _utilsBus.SetValueToRegistry(category2, value1);
            _utilsBus.SetValueToRegistry(category3, value3);

            Assert.True(_utilsBus.RegistryContainsValue(category1, value1), "Registry does not contain c1v1 added");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value1), "Registry does not contain c2v1 added");
            Assert.True(_utilsBus.RegistryContainsValue(category3, value3), "Registry does not contain c3v3 added");

            //remove value
            _utilsBus.RemoveValueFromRegistry(category1, value1);

            Assert.False(_utilsBus.RegistryContainsValue(category1, value1), "Registry should not contain c1v1");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value1), "Registry does not contain c2v1 added");
            Assert.True(_utilsBus.RegistryContainsValue(category3, value3), "Registry does not contain c3v3 added");

        }

        [Test]
        public void Test_NU_USB_RemoveAllValuesFromRegistry()
        {
            string category1 = "cat1";
            string category2 = "cat2";
            string value1 = "value1";
            string value2 = "value2";

            //add values
            _utilsBus.SetValueToRegistry(category1, value1);
            _utilsBus.SetValueToRegistry(category1, value2);
            _utilsBus.SetValueToRegistry(category2, value1);
            _utilsBus.SetValueToRegistry(category2, value2);
            
            Assert.True(_utilsBus.RegistryContainsValue(category1, value1), "Registry does not contain c1v1");
            Assert.True(_utilsBus.RegistryContainsValue(category1, value2), "Registry does not contain c1v2");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value1), "Registry does not contain c2v1");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value2), "Registry does not contain c2v2");

            //remove all values
            _utilsBus.RemoveAllValuesFromRegistry(category1);

            Assert.False(_utilsBus.RegistryContainsValue(category1, value1), "Registry should not contain c1v1");
            Assert.False(_utilsBus.RegistryContainsValue(category1, value2), "Registry should not contain c1v2");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value1), "Registry does not contain c2v1");
            Assert.True(_utilsBus.RegistryContainsValue(category2, value2), "Registry does not contain c2v2");

        }

        [Test]
        public void Test_NU_USB_AcquireLock_Succeeded()
        {
            try
            {
                int timeout = 100;
                bool succeeded = false;

                //get lock
                succeeded = _utilsBus.AcquireLock("Test_NU_USB_AcquireLock_Succeeded", timeout);

                Assert.True(succeeded, "Failed to acquire lock");

            }
            finally
            {
                //release lock
                if (_utilsBus.IsLocked) _utilsBus.ReleaseLock();
            }
        }

        [Test]
        public void Test_NU_USB_AcquireLock_TwoInOrderSucceeded()
        {
            try
            {
                int timeout = 100;
                bool lock1Succeeded = false;
                bool lock1Released = false;
                bool lock2Succeeded = false;

                //get lock
                lock1Succeeded = _utilsBus.AcquireLock("Test_NU_USB_AcquireLock_TwoInOrderSucceeded", timeout);

                Assert.True(lock1Succeeded, "Failed to acquire lock1");

                //release lock
                lock1Released = _utilsBus.ReleaseLock();

                Assert.True(lock1Released, "Could not release lock1");

                //get lock
                lock2Succeeded = _utilsBus.AcquireLock("Test_NU_USB_AcquireLock_TwoInOrderSucceeded", timeout);

                Assert.True(lock2Succeeded, "Failed to acquire lock2");

            }
            finally
            {
                //release lock
                if (_utilsBus.IsLocked) _utilsBus.ReleaseLock();
            }
        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void Test_NU_USB_AcquireLock_TimedOut()
        {
            try
            {
                int timeout = 100;
                bool lock1Succeeded = false;
                bool lock2Succeeded = false;

                //get lock
                lock1Succeeded = _utilsBus.AcquireLock("Test_NU_USB_AcquireLock_TimedOut", timeout);

                Assert.True(lock1Succeeded, "Failed to acquire lock1");

                //get another lock without releasing the first
                lock2Succeeded = _utilsBus.AcquireLock("Test_NU_USB_AcquireLock_TimedOut", timeout);

            }
            finally
            {
                if (_utilsBus.IsLocked) _utilsBus.ReleaseLock();
            }

        }

        [Test]
        public void Test_NU_USB_ReleaseLock()
        {
            try
            {
                int timeout = 100;
                bool succeeded = false;
                bool released = false;

                //get lock
                succeeded = _utilsBus.AcquireLock("Test_NU_USB_ReleaseLock", timeout);

                //release lock
                released = _utilsBus.ReleaseLock();

                Assert.True(succeeded, "Failed to acquire lock");
                Assert.True(released, "Failed to release lock");

            }
            finally
            {
                //release lock
                if(_utilsBus.IsLocked) _utilsBus.ReleaseLock();
            }
        }


    }
}
