using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetDispatcher;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageDispatcherTests
{
    [TestClass]
    public class MessageDispatcherTests
    {
        int countBar;

        [TestInitialize]
        public void TestInitialize()
        {
            countBar = 0;
        }

        /// <summary>
        /// Test registering and deregistering of handlers.
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            MessageDispatcher<string, string>.Clear("foo");

            // Register a handler for "foo" that accepts messages of type string
            MessageDispatcher<string, string>.RegisterHandler("foo", FooHandler1);

            // Register another handler for "foo" that accepts messages of type string
            MessageDispatcher<string, string>.RegisterHandler("foo", FooHandler2);

            // Register another handler for "foo" that accepts messages of type int
            MessageDispatcher<string, int>.RegisterHandler("foo", FooHandler3);

            // Dispatch a message of type string
            MessageDispatcher<string, string>.Dispatch("foo", "bar");
            Assert.AreEqual(2, countBar);

            // Dispatch an int
            MessageDispatcher<string, int>.Dispatch("foo", 1);

            // Dispatch another message of type string
            MessageDispatcher<string, string>.DeregisterHandler("foo", FooHandler1);
            MessageDispatcher<string, string>.Dispatch("foo", "bar");
            Assert.AreEqual(3, countBar);
        }

        private void FooHandler1(string message)
        {
            Assert.AreEqual("bar", message);
            countBar++;
        }

        private void FooHandler2(string message)
        {
            Assert.AreEqual("bar", message);
            countBar++;
        }

        private void FooHandler3(int i)
        {
            Assert.AreEqual(1, i);
        }

        /// <summary>
        /// Test multiple message producers and consumers.
        /// </summary>
        [TestMethod]
        public void TestMethod2()
        {
            MessageDispatcher<string, string>.Clear("foo");

            int NUM = 1000;
            List<Task> producerTasks = new List<Task>();
            int[] consumerStates = new int[NUM];

            // Create tasks
            for (int i = 0; i < NUM; i++)
            {
                Task task = new Task(delegate ()
                {
                    MessageDispatcher<string, string>.Dispatch("foo", "bar");
                });
                producerTasks.Add(task);
            }

            // Create listener tasks
            for (int i = 0; i < NUM; i++)
            {
                int[] state = new int[] { i }; // lexical closure
                MessageDispatcher<string, string>.RegisterHandler("foo", delegate (string message)
                {
                    Interlocked.Increment(ref consumerStates[state[0]]);
                });
            }

            // Start tasks
            foreach (Task task in producerTasks)
            {
                task.Start();
            }

            // Wait for tasks
            Task.WaitAll(producerTasks.ToArray());

            // Verify results
            for (int i = 0; i < NUM; i++)
            {
                Assert.IsTrue(NUM == consumerStates[i]);
            }
        }

        /// <summary>
        /// Test handlers that produce exceptions.
        /// </summary>
        [TestMethod]
        public void TestMethod3()
        {
            MessageDispatcher<string, string>.Clear("foo");

            // Register a handler for "foo" that accepts messages of type string
            MessageDispatcher<string, string>.RegisterHandler("foo", FooHandler4);

            // Register a handler for "foo" that accepts messages of type string
            MessageDispatcher<string, string>.RegisterHandler("foo", FooHandler5);

            // Dispatch a message of type string
            MessageDispatcher<string, string>.Dispatch("foo", "bar");

            Assert.AreEqual(2, countBar);
        }

        private void FooHandler4(string message)
        {
            Assert.AreEqual("bar", message);
            countBar++;
            throw new Exception();
        }

        private void FooHandler5(string message)
        {
            Assert.AreEqual("bar", message);
            countBar++;
            throw new Exception();
        }
    }
}
