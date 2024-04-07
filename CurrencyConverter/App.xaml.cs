using CommunityToolkit.Mvvm.DependencyInjection;
using CurrencyConverter.Services;
using CurrencyConverter.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CurrencyConverter
{
    public partial class App : Application
    {
        public App()
        {
            Task.Run(() => ConfigureIoc());
        }

        private void ConfigureIoc()
        {
            // Для работы xml десериализатора refit
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<IThreadingService>(new ThreadingService(this.Dispatcher))
                    .AddSingleton(
                        RestService.For<ICurrencyService>(
                            "https://www.cbr.ru",
                            new RefitSettings { ContentSerializer = new XmlContentSerializer()}))
                    .AddTransient<MainViewModel>()
                    .BuildServiceProvider()
            );

            // Здесь для конфигурации DEBUG хорошо бы добавить проверку на то,
            // что все зарегистрированные сервисы корректно резолвятся.
        }
    }
}
