using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyConverter.Model;
using CurrencyConverter.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CurrencyConverter.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Private classes

        private enum ConvertDirection
        {
            None,
            Direct,
            Reverse
        }

        #endregion

        #region Const

        private const string DOLLAR_CODE = "USD";
        private const string RUBLE_CODE = "RUB";

        #endregion

        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly IThreadingService _threadingService;

        private ICollection<Currency> _allCurrencies;

        private IDictionary<string, Valute> _ratesByCurrency;
        private IDictionary<string, Valute> _ratesByCurrencyOnDate;

        private readonly Currency _rubCurrency = new Currency(RUBLE_CODE, "Рубль");
        private readonly Valute _rubValute = new Valute { CharCode = RUBLE_CODE, VunitRate = "1" };

        private ConvertDirection _convertDirection = ConvertDirection.None;

        #endregion

        #region Constructor

        public MainViewModel(ICurrencyService currencyservice, IThreadingService threadingService)
        {
            _currencyService = currencyservice ?? throw new ArgumentNullException(nameof(currencyservice));
            _threadingService = threadingService ?? throw new ArgumentNullException(nameof(threadingService));

            FilterCommand = new RelayCommand<string>(FilterExecute);
            LoadInfoOnDateCommand = new AsyncRelayCommand<DateTime>(LoadInfoOnDateExecute);

            // В полноценном приложении должно вызываться при событии навигации на страницу.
            Task.Run(UpdateCurrenciesCurrent);
        }

        #endregion

        #region Properties

        public ICommand FilterCommand { get; }

        public ICommand LoadInfoOnDateCommand { get; }

        private ObservableCollection<Currency> _currencies = new ObservableCollection<Currency>();
        public ObservableCollection<Currency> Currencies
        {
            get => _currencies;
            private set => SetProperty(ref _currencies, value);
        }

        private Currency _selectedCurrency;
        public Currency SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                SetProperty(ref _selectedCurrency, value);
                UpdateCurrencyInfoOnDate();
            }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                CurrencyInfoText = null;
                _ratesByCurrencyOnDate = null;
            }
        }

        private string _currencyInfoText;
        public string CurrencyInfoText
        {
            get => _currencyInfoText;
            set => SetProperty(ref _currencyInfoText, value);
        }

        public ICollection<Currency> AllCurrencies => new[] { _rubCurrency }.Union(_allCurrencies).ToArray();

        private string _currencyFromCode;
        public string CurrencyFromCode
        {
            get => _currencyFromCode;
            set
            {
                SetProperty(ref _currencyFromCode, value);
                TryConvertDirect();
            }
        }

        private string _currencyToCode;
        public string CurrencyToCode
        {
            get => _currencyToCode;
            set
            {
                SetProperty(ref _currencyToCode, value);
                TryConvertDirect();
            }
        }

        private decimal _valueFrom;
        public decimal ValueFrom
        {
            get => _valueFrom;
            set
            {
                SetProperty(ref _valueFrom, value);
                TryConvertDirect();
            }
        }

        private decimal _valueTo;
        public decimal ValueTo
        {
            get => _valueTo;
            set
            {
                SetProperty(ref _valueTo, value);
                TryConvertReverse();
            }
        }

        #endregion

        #region Private Methods

        private void FilterExecute(string value)
        {
            _threadingService.OnUiThread(() => { FilterCurrencies(value); });
        }

        private async Task LoadInfoOnDateExecute(DateTime date)
        {
            _ratesByCurrencyOnDate = null;

            var currencies = await _currencyService.GetCurrenciesAsync(date).ConfigureAwait(false);
            if (!currencies.IsSuccessStatusCode)
            {
                // обработка ошибки
                return;
            }

            _ratesByCurrencyOnDate = currencies.Content.Valutes.ToDictionary(v => v.CharCode, v => v);

            UpdateCurrencyInfoOnDate();
        }

        private async Task UpdateCurrenciesCurrent()
        {
            var response = await _currencyService.GetCurrenciesAsync(DateTime.Today).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                // обработка ошибки
                return;
            }

            var currencies = response.Content;

            _allCurrencies = currencies.Valutes?
                .Select(c => new Currency(c.CharCode, c.Name))
                .OrderBy(x => x.Code)
                .ToList();
            FilterExecute(string.Empty);

            _ratesByCurrency = currencies.Valutes.Union(new[] {_rubValute}).ToDictionary(v => v.CharCode, v => v);

            OnPropertyChanged(nameof(AllCurrencies));
        }     

        private void FilterCurrencies(string str)
        {
            Currencies.Clear();
            foreach (var currency in _allCurrencies)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    Currencies.Add(currency);
                    continue;
                }

                if (currency.Code.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    Currencies.Add(currency);
                    continue;
                }

                if (currency.Name.Contains(str, StringComparison.OrdinalIgnoreCase))
                {
                    Currencies.Add(currency);
                    continue;
                }
            }
        }

        private void UpdateCurrencyInfoOnDate()
        {
            _threadingService.OnUiThread(() => CurrencyInfoText = string.Empty);

            if (_selectedCurrency == null || _ratesByCurrency == null || _ratesByCurrencyOnDate == null)
                return;

            // Считаем что ЦБ отдает корректные данные и всегда есть есть курс доллара.
            var dollarRateCurrent = _ratesByCurrency[DOLLAR_CODE];
            var dollarRateOnDate = _ratesByCurrencyOnDate[DOLLAR_CODE];

            var rateCurrent = _ratesByCurrency[_selectedCurrency.Code];
            var rateOnDate = _ratesByCurrencyOnDate[_selectedCurrency.Code];
            var delta = rateCurrent.Rate - rateOnDate.Rate;

            var info = new CurrencyInfo
            {
                CurrentRate = rateCurrent.Rate,
                RateOnDate = rateOnDate.Rate,
                CurrentRateToDollar = decimal.Round(rateCurrent.Rate / dollarRateCurrent.Rate, 4),
                RateOnDateToDollar = decimal.Round(rateOnDate.Rate / dollarRateOnDate.Rate, 4),
                DeltaToDollar = decimal.Round((rateOnDate.Rate / dollarRateOnDate.Rate) - (rateCurrent.Rate / dollarRateCurrent.Rate), 4)
            };

            const string tab = "    ";
            var sb = new StringBuilder();
            sb.AppendLine("₽:");
            sb.AppendLine($"{tab}Текущий: {info.CurrentRate}");
            sb.AppendLine($"{tab}На дату: {info.RateOnDate}");
            sb.AppendLine($"{tab}Δ: {info.Delta}");
            sb.AppendLine();
            sb.AppendLine("$:");
            sb.AppendLine($"{tab}Текущий: {info.CurrentRateToDollar}");
            sb.AppendLine($"{tab}На дату: {info.RateOnDateToDollar}");
            sb.AppendLine($"{tab}Δ: {info.DeltaToDollar}");

            _threadingService.OnUiThread(() => CurrencyInfoText = sb.ToString());
        }

        private void TryConvertDirect()
        {
            if (_convertDirection != ConvertDirection.None)
                return;

            _convertDirection = ConvertDirection.Direct;
            ValueTo = Convert(CurrencyFromCode, CurrencyToCode, ValueFrom);
            _convertDirection = ConvertDirection.None;
        }


        private void TryConvertReverse()
        {
            if (_convertDirection != ConvertDirection.None)
                return;

            _convertDirection = ConvertDirection.Reverse;
            ValueFrom = Convert(CurrencyToCode, CurrencyFromCode, ValueTo);
            _convertDirection = ConvertDirection.None;
        }

        private decimal Convert(string currencyFrom, string currencyTo, decimal value)
        {
            try
            {
                if (_ratesByCurrency.TryGetValue(currencyFrom, out var currencyRateFrom)
                    && _ratesByCurrency.TryGetValue(currencyTo, out var currencyRateTo))
                {
                    return decimal.Round(value * currencyRateFrom.Rate / currencyRateTo.Rate, 4);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return 0;
        }

        #endregion
    }
}