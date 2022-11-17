using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LightServer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LSEditor : Page
    {
        public LSEditor()
        {
            this.InitializeComponent();
        }

        private void WebView2_Loaded(object sender, RoutedEventArgs e)
        {
            var host = App.Current.As<App>().host;
            string address = host.Urls.First();
            int port = int.Parse(address.Split(':').Last());
            WebView2.Source = new Uri("http://localhost:" + port);
        }
    }
}
