using System;

namespace CurrencyConverter.Services
{
    public interface IThreadingService
    {
        void OnUiThread(Action action);
    }
}
