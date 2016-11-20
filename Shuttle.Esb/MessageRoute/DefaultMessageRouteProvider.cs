﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb
{
	public sealed class DefaultMessageRouteProvider : IMessageRouteProvider
	{
		private readonly IMessageRouteCollection _messageRoutes = new MessageRouteCollection();

		public IEnumerable<string> GetRouteUris(string messageType)
		{
			var uri =
				_messageRoutes.FindAll(messageType).Select(messageRoute => messageRoute.Queue.Uri.ToString()).FirstOrDefault();

			return
				string.IsNullOrEmpty(uri)
					? new List<string>()
					: new List<string> {uri};
		}

	    public DefaultMessageRouteProvider(IQueueManager queueManager)
	    {
            Guard.AgainstNull(queueManager, "queueManager");

            if (ServiceBusConfiguration.ServiceBusSection == null ||
                ServiceBusConfiguration.ServiceBusSection.MessageRoutes == null)
            {
                return;
            }

            var specificationFactory = new MessageRouteSpecificationFactory();

            foreach (MessageRouteElement mapElement in ServiceBusConfiguration.ServiceBusSection.MessageRoutes)
            {
                var messageRoute = Find(mapElement.Uri);

                if (messageRoute == null)
                {
                    messageRoute = new MessageRoute(queueManager.GetQueue(mapElement.Uri));

                    Add(messageRoute);
                }

                foreach (SpecificationElement specificationElement in mapElement)
                {
                    messageRoute.AddSpecification(specificationFactory.Create(specificationElement.Name, specificationElement.Value));
                }
            }
        }

        public void Add(IMessageRoute messageRoute)
		{
			Guard.AgainstNull(messageRoute, "messageRoute");

			var existing = _messageRoutes.Find(messageRoute.Queue.Uri);

			if (existing == null)
			{
				_messageRoutes.Add(messageRoute);
			}
			else
			{
				foreach (var specification in messageRoute.Specifications)
				{
					existing.AddSpecification(specification);
				}
			}
		}

		public IMessageRoute Find(string uri)
		{
			return _messageRoutes.Find(uri);
		}

		public bool Any()
		{
			return _messageRoutes.Any();
		}

	    public IEnumerable<IMessageRoute> MessageRoutes {
	        get { return new ReadOnlyCollection<IMessageRoute>(new List<IMessageRoute>(_messageRoutes));} 
	    }
	}
}