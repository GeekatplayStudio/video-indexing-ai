using System.Windows;

namespace FootageSearch.App
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
