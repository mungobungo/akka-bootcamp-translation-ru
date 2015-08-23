using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    ///  Актор, отвечающий за вывод сообщения в консоль.
    /// (пишет по одному сообщению за раз :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            var msg = message as string;
            // Убедимся, что мы что-то получили
            if (string.IsNullOrEmpty(msg))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Пожалуйста, введите что-нибудь.\n");
                Console.ResetColor();
                return;
            }

            // Если сообщение содержит четное число символов, напишем его красным, иначе зеленым
            var even = msg.Length % 2 == 0;
            var color = even ? ConsoleColor.Red : ConsoleColor.Green;
            var alert = even ? "В вашей строке четное число символов.\n" : "В вашей строке нечетное число символов.\n";
            Console.ForegroundColor = color;
            Console.WriteLine(alert);
            Console.ResetColor();

        }
    }
}
