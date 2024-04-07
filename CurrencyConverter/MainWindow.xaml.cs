using CommunityToolkit.Mvvm.DependencyInjection;
using CurrencyConverter.ViewModels;
using System.Windows;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // в проекте одна view model, поэтому без заморочек и использования ViewModelLocator
            DataContext = Ioc.Default.GetService<MainViewModel>();
        }
    }
}
