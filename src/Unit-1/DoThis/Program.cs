using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program 
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // Добавляем для корректного вывода русских символов в консоль
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            
            // ициализируем MyActorSystem
            // ЗДЕСЬ НАДО ДОБАВИТЬ КОД
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            var consoleWriteActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));

            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriteActor)));
            PrintInstructions();

            consoleReaderActor.Tell("whatever");


            // Скажите актору, отвечающему за чтение из консоли приступить к работе.
            // ВАМ НАДО ДОБАВИТЬ КОД

            // Блокируем главный поток до тех пор, пока система акторов не отключится
            MyActorSystem.AwaitTermination();
        }

        private static void PrintInstructions()
        {
          
            Console.WriteLine("Напишите в консоли все что угодно!");
            Console.Write("Некоторые строки будут напечатаны");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" красным ");
            Console.ResetColor();
            Console.Write(" другие же будут");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" зелеными! ");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Напечатайте 'exit' чтобы завершить работу приложения.\n");
        }
    }
    #endregion
}
