using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

namespace CurrencyConverter.Services
{
    public class ThreadingService : IThreadingService
    {
        private readonly Dispatcher _dispatcher;

        public ThreadingService([NotNull] Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void OnUiThread(Action action)
        {
            if (action == null)
                return;

            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }
    }
}
