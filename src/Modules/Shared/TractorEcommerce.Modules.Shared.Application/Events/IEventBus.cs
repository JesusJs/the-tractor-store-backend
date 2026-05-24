using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Shared.Application.Events
{
    public interface IEventBus
    {
        Task PublishAsync<T>(string topic, string key, T message) where T : class;
    }
}
