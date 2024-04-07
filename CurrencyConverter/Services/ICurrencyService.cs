using CurrencyConverter.Model;
using Refit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CurrencyConverter.Services
{
    public interface ICurrencyService
    {
        [Get("/scripts/XML_daily.asp?date_req={date}")]
        Task<IApiResponse<ValCurs>> GetCurrenciesAsync([Query(Format = "dd/MM/yyyy")] DateTime date, CancellationToken cancellationToken = default);
    }
}
