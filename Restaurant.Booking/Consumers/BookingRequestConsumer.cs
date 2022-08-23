using MassTransit;
using Restaurant.Messages.Interfaces;
using System;
using System.Threading.Tasks;
using Restaurant.Booking.MassTransitDTO;
using Restaurant.Messages.CustomExceptions;

namespace Restaurant.Booking.Consumers
{
    public class BookingRequestConsumer : IConsumer<IBookingRequest>
    {
        private readonly Restaurant _restaurant;

        public BookingRequestConsumer(Restaurant restaurant)
        {
            _restaurant = restaurant;
        }

        public async Task Consume(ConsumeContext<IBookingRequest> context)
        {
            Console.WriteLine($"BookingRequestConsumer==[OrderId: {context.Message.OrderId}] Ищем свободный стол");

            // есть свободный стол?
            var result = await _restaurant.BookFreeTableAsync(1, context.Message.OrderId, context.CancellationToken);

            if (result == true)
            {
                Console.WriteLine($"BookingRequestConsumer==[OrderId: {context.Message.OrderId}] Столик найден");
                await context.Publish<ITableBooked>(
                    new TableBooked(
                        context.Message.OrderId,
                        context.Message.ClientId, 
                        context.Message.Dish));
            }
            else
            {
                Console.WriteLine($"BookingRequestConsumer== [OrderId: {context.Message.OrderId}] Ошибка бронирования!");

                //await context.Publish<IBookingReject>(new BookingReject(context.Message.OrderId));
                throw new BookingException($"Ошибка бронирования! для заказа {context.Message.OrderId}");
            }
        }
    }
}
