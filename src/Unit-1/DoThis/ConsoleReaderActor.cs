using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Актор, отвечающий за чтение ИЗ консоли. 
    /// Также заботится о вызове <see cref="ActorSystem.Shutdown"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        private IActorRef _consoleWriterActor;

        public ConsoleReaderActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var read = Console.ReadLine();
            if (!string.IsNullOrEmpty(read) && String.Equals(read, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                // Останавливает систему (получаем ссылку на систему акторов
                // через контекст текущего актора)
                Context.System.Shutdown();
                return;
            }

            _consoleWriterActor.Tell(read);
            Self.Tell("continue");
        }

    }
}