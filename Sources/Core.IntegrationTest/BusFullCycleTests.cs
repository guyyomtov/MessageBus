﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusFullCycleTests
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        
        [TestMethod]
        public void Bus_Dialog_WithOrderedDelivery_AllSubscriptionTypes()
        {
            using (IBus entityA = new Bus(), entityB = new Bus())
            {
                Data messageA = new Person
                    {
                        Id = 5
                    };
                
                Data messageB = new Car
                    {
                        Number = "39847239847"
                    };

                List<Data> received = new List<Data>();

                ManualResetEvent ev1 = new ManualResetEvent(false), ev2 = new ManualResetEvent(false);

                using (ISubscriber subscriberA = entityA.CreateSubscriber(), subscriberB = entityB.CreateSubscriber())
                {
                    subscriberA.SubscribeHierarchy<Data>(received.Add);

                    subscriberA.Subscribe(typeof(OK), data => ev1.Set());

                    subscriberB.Subscribe<OK>(data => ev2.Set());

                    using (IPublisher publisher = entityB.CreatePublisher())
                    {
                        publisher.Send(messageA);

                        publisher.Send(messageB);

                        publisher.Send(new OK());
                    }

                    using (IPublisher publisher = entityA.CreatePublisher())
                    {
                        publisher.Send(new OK());
                    }

                    bool waitOne = ev1.WaitOne(TimeSpan.FromSeconds(20)) && ev2.WaitOne(TimeSpan.FromSeconds(20));

                    waitOne.Should().BeTrue("Message not received");

                    received.Should().ContainInOrder(messageA, messageB);
                }
            }
        }

    }

}
