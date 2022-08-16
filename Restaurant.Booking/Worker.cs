using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Restaurant.Booking.MassTransitDTO;
using Restaurant.Messages;
using Restaurant.Messages.Interfaces;

namespace Restaurant.Booking
{
    public class Worker : BackgroundService
    {
        private readonly IBus _bus;
        private readonly Restaurant _restaurant;
        private readonly Random _random = new Random();

        public Worker(IBus bus, Restaurant restaurant)
        {
            _bus = bus;
            _restaurant = restaurant;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("\r\n\tВыберите действие:\r\n" +
                    "\t\t0 Распечатать список столов и их состояние\r\n" +
                    "\t\t1 Забронировать столик (без опозданий) асинхронно с нормальным блюдом\r\n" +
                    "\t\t2 Забронировать столик (без опозданий) aсинхронно с отказным блюдом\r\n" +
                    "\t\t3 Забронировать столик (точно опоздает) асинхронно с нормальным блюдом\r\n" +
                    "\t\t4 Забронировать столик (random) aсинхронно с нормальным блюдом\r\n" +
                    "\t\t5 Снять бронь\r\n"
                    );

                string userInput = Console.ReadLine();

                if (int.TryParse(userInput, out int choise) && (choise < 0 || choise > 5))
                {
                    Console.WriteLine("\tВнимание, некорректный ввод! допускается только целые числа  0  1  2  3  4  5");
                    continue;
                }

                var orderId = NewId.NextGuid();
                var clientId = Guid.NewGuid();
                var bookingArrivalTime = _random.Next(7, 15); // время прибытия указанное гостем при бронировании
                var actualArrivalTime = _random.Next(7, 15); // фактическое время прибытия гостя

                if (choise == 0) // print list
                {
                    _restaurant.PrintTablesStatus();
                    continue;
                }
                else if (choise == 1)//correct client and correct order
                {
                    Console.WriteLine($"Worker=Заказ #{orderId}, для клиента #{clientId}");

                    await _bus.Publish(
                        (IBookingRequest)new BookingRequested(
                            orderId, 
                            clientId, 
                            new Dish { Id = _random.Next(0, 2) },
                            16,
                            5
                            ),
                        stoppingToken);
                }
                else if (choise == 2)//correct client and bad order
                {
                    Console.WriteLine($"Worker=Заказ #{orderId}, для клиента #{clientId}");

                    await _bus.Publish(
                        (IBookingRequest)new BookingRequested(
                            orderId, 
                            clientId, 
                            new Dish { Id = 3},
                            16,
                            5
                            ),
                        stoppingToken);
                }
                else if (choise == 3)//bad client and correct order
                {
                    Console.WriteLine($"Worker=Заказ #{orderId}, для клиента #{clientId}");

                    await _bus.Publish(
                        (IBookingRequest)new BookingRequested(
                            orderId,
                            clientId,
                            new Dish { Id = _random.Next(0, 2) },
                            5,
                            16
                            ),
                        stoppingToken);
                }
                else if (choise == 4)//random client and correct order
                {
                    Console.WriteLine($"Worker=Заказ #{orderId}, для клиента #{clientId}");

                    await _bus.Publish(
                        (IBookingRequest)new BookingRequested(
                            orderId,
                            clientId,
                            new Dish { Id = _random.Next(0, 2) },
                            bookingArrivalTime,
                            actualArrivalTime
                            ),
                        stoppingToken);
                }
                else//remowe order manually
                {
                    bool tableNumberNotInputed = true;
                    while (tableNumberNotInputed)
                    {
                        Console.WriteLine("\tВыберите номер стола, которому отменяем бронь");

                        string userInputTableNumber = Console.ReadLine();

                        if (int.TryParse(userInputTableNumber, out int tableNumber))
                        {
                            if (tableNumber < 1 || // минимум
                                tableNumber > 10) //максимум
                            {
                                Console.WriteLine($"Tакого столика не существует. Вводите целое число, с 1 по {10} включительно");
                                continue;
                            }

                            tableNumberNotInputed = false; // ввод номера стола завершен

                            await _restaurant.DeleteBookingForTableAsync(tableNumber);
                        }
                        else
                        {
                            Console.WriteLine("Допустимы только целые числа!");
                        }
                    }
                }
            }
        }
    }
}