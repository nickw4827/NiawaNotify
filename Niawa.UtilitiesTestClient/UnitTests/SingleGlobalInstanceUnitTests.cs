using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Niawa.UtilitiesTestClient.UnitTests
{
    [TestFixture]
    public class SingleGlobalInstanceUnitTests
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
        public void Test_NU_SGI_SingleGlobalInstance_NoWait()
        {
            //check for unhandled exceptions
            bool exceptionWasThrown = false;
            UnhandledExceptionEventHandler unhandledExceptionHandler = (s, e) =>
            {
                if (!exceptionWasThrown)
                { exceptionWasThrown = true; }
            };
            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;

            bool objectiveReached1 = false;
            bool objectiveReached2 = false;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime1 = DateTime.MinValue;
            DateTime endTime2 = DateTime.MinValue;

            startTime = DateTime.Now;

            //thread 1 - single global instance
            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    using (new Niawa.Utilities.SingleGlobalInstance(100, "SingleGlobalInstanceUnitTest1.1"))
                    {
                        System.Threading.Thread.Sleep(50);

                        endTime1 = DateTime.Now;
                        objectiveReached1 = true;
                        
                    }
                }
                );

            //thread 2 - single global instance
            System.Threading.Thread t2 = new System.Threading.Thread
                (delegate()
                {
                    using (new Niawa.Utilities.SingleGlobalInstance(100, "SingleGlobalInstanceUnitTest1.2"))
                    {
                        System.Threading.Thread.Sleep(50);
                        
                        endTime2 = DateTime.Now;
                        objectiveReached2 = true;
                    }
                }
                );

            //start threads
            t1.Start();
            System.Threading.Thread.Sleep(100);
            t2.Start();
            System.Threading.Thread.Sleep(100);

            //wait for threads to finish
            t1.Join(1000);
            t2.Join(1000);

            //unhandled exceptions handler
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;

            TimeSpan ts1 = endTime1 - startTime;
            TimeSpan ts2 = endTime2 - startTime;

            //assertions
            Assert.True(objectiveReached1, "Thread 1 did not complete");
            Assert.True(objectiveReached2, "Thread 2 did not complete");
            Assert.GreaterOrEqual(ts1.TotalMilliseconds, 50, "Thread 1 ran less than 50 ms");
            Assert.LessOrEqual(ts2.TotalMilliseconds, 200, "Thread 2 ran more than 200 ms");
            Assert.Greater(endTime2, endTime1, "Thread 2 finished before Thread 1");
            Assert.IsFalse(exceptionWasThrown, "There was at least one unhandled exception");


        }

        [Test]
        public void Test_NU_SGI_SingleGlobalInstance_WaitAndReceive()
        {
            //check for unhandled exceptions
            bool exceptionWasThrown = false;
            UnhandledExceptionEventHandler unhandledExceptionHandler = (s, e) =>
            {
                if (!exceptionWasThrown)
                { exceptionWasThrown = true; }
            };
            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;

            bool objectiveReached1 = false;
            bool objectiveReached2 = false;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime1 = DateTime.MinValue;
            DateTime endTime2 = DateTime.MinValue;

            startTime = DateTime.Now;

            //thread 1 - single global instance
            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    using (new Niawa.Utilities.SingleGlobalInstance(200, "SingleGlobalInstanceUnitTest2"))
                    {
                        System.Threading.Thread.Sleep(100);

                        endTime1 = DateTime.Now;
                        objectiveReached1 = true;
                    }
                }
                );

            //thread 2 - single global instance
            System.Threading.Thread t2 = new System.Threading.Thread
                (delegate()
                {
                    using (new Niawa.Utilities.SingleGlobalInstance(400, "SingleGlobalInstanceUnitTest2"))
                    {
                        System.Threading.Thread.Sleep(100);
                        
                        endTime2 = DateTime.Now;
                        objectiveReached2 = true;
                    }
                }
                );

            //start threads
            t1.Start();
            System.Threading.Thread.Sleep(100);
            t2.Start();
            System.Threading.Thread.Sleep(100);
            
            //wait for threads to finish
            t1.Join(1000);
            t2.Join(1000);

            TimeSpan ts1 = endTime1 - startTime;
            TimeSpan ts2 = endTime2 - startTime;

            //unhandled exceptions handler
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;

            //assertions
            Assert.IsTrue(objectiveReached1, "Thread 1 did not complete");
            Assert.IsTrue(objectiveReached2, "Thread 2 did not complete");
            Assert.GreaterOrEqual(ts1.TotalMilliseconds, 50, "Thread 1 finished in less than 50 ms");
            Assert.GreaterOrEqual(ts2.TotalMilliseconds, 50, "Thread 2 finished in less than 50 ms");
            Assert.Greater(endTime2, endTime1, "Thread 2 finished before Thread 1");
            Assert.IsFalse(exceptionWasThrown, "There was at least one unhandled exception");


        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void Test_NU_SGI_SingleGlobalInstance_TimeoutWaiting()
        {
            //check for unhandled exceptions
            bool exceptionWasThrown = false;
            UnhandledExceptionEventHandler unhandledExceptionHandler = (s, e) =>
            {
                if (!exceptionWasThrown)
                { exceptionWasThrown = true; }
            };
            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;

            bool objectiveReached1 = false;
            bool objectiveReached2 = false;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime1 = DateTime.MinValue;
            DateTime endTime2 = DateTime.MinValue;

            startTime = DateTime.Now;

            //thread 1 - single global instance
            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    using (new Niawa.Utilities.SingleGlobalInstance(100, "SingleGlobalInstanceUnitTest2"))
                    {
                        System.Threading.Thread.Sleep(300);

                        endTime1 = DateTime.Now;
                        objectiveReached1 = true;
                    }
                }
                );

          

            //start threads
            t1.Start();
            System.Threading.Thread.Sleep(100);

            //lock timeout expected
            using (new Niawa.Utilities.SingleGlobalInstance(100, "SingleGlobalInstanceUnitTest2"))
            {
                System.Threading.Thread.Sleep(10);

                endTime2 = DateTime.Now;
                objectiveReached2 = true;
            }

            //wait for threads to finish
            t1.Join(1000);

            //unhandled exceptions handler
            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;

            TimeSpan ts1 = endTime1 - startTime;
            TimeSpan ts2 = endTime2 - startTime;

            //assertions
            //Assert.True(objectiveReached1);
            //Assert.False(objectiveReached2);
            Assert.IsFalse(exceptionWasThrown, "There was at least one unhandled exception");

        }

    }
}
