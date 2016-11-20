﻿using System;
using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Tests
{
	[TestFixture]
	public class ServiceBusTest
	{
		[Test]
		public void Should_be_able_to_handle_expired_message()
		{
			var handlerInvoker = new FakeMessageHandlerInvoker();
			var fakeQueue = new FakeQueue(2);

		    var configuration = new Mock<IServiceBusConfiguration>();

		    configuration.Setup(m => m.Inbox).Returns(new InboxQueueConfiguration
		    {
		        WorkQueue = fakeQueue,
		        ErrorQueue = fakeQueue,
		        ThreadCount = 1
		    });

		    configuration.Setup(m => m.HasInbox).Returns(true);

            var container = new DefaultComponentContainer();

            container.Register<IMessageHandlerInvoker>(handlerInvoker);

            new DefaultConfigurator().RegisterComponents(container);

            using (var bus = new ServiceBus(configuration.Object, new DefaultPipelineFactory(container), new NullSubscriptionService()))
            {
				bus.Start();

				var timeout = DateTime.Now.AddMilliseconds(500);

				while (fakeQueue.MessageCount < 2 && DateTime.Now < timeout)
				{
					Thread.Sleep(5);
				}
			}

			Assert.AreEqual(1, handlerInvoker.GetInvokeCount("SimpleCommand"), "FakeHandlerInvoker was not invoked exactly once.");
			Assert.AreEqual(2, fakeQueue.MessageCount, "FakeQueue was not invoked exactly twice.");
		}
	}
}