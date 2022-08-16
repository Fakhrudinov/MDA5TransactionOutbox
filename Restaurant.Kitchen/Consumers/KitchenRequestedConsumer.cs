using System;
using System.Threading.Tasks;
using MassTransit;
using Restaurant.Kitchen.MassTransitDTO;
using Restaurant.Messages.Interfaces;

namespace Restaurant.Kitchen.Consumers
{
    internal class KitchenRequestedConsumer : IConsumer<ITableBooked>
    {
        private readonly Manager _manager;

        public KitchenRequestedConsumer(Manager manager)
        {
            _manager = manager;
        }

        public async Task Consume(ConsumeContext<ITableBooked> context)
        {
            var randomDelay = new Random().Next(100, 7000);
            Console.WriteLine($"Kitchen-KitchenRequestedConsumer=Проверим заказ #{context.Message.OrderId} на кухне, [id={context.Message.Dish.Id}] это займет {randomDelay}мс");
            await Task.Delay(randomDelay);

            var (confirmation, dish) = _manager.CheckKitchenReady(context.Message.OrderId, context.Message.Dish);
            
            if (confirmation)
            {
                Console.WriteLine($"Kitchen-KitchenRequestedConsumer=заказ #{context.Message.OrderId} = ok, Publish KitchenReady");
                await context.Publish<IKitchenReady>(new KitchenReady(context.Message.OrderId));
            }
            else
            {
                Console.WriteLine($"Kitchen-KitchenRequestedConsumer=заказ #{context.Message.OrderId} = failed, Publish KitchenAccident");
                await context.Publish<IKitchenReject>(new KitchenReject(context.Message.OrderId, dish!));
            }
        }
    }
}