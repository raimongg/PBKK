using System.Windows;

namespace CampusApp
{
    /// <summary>
    /// Logika interaksi untuk MainWindow.xaml.
    /// Menginisialisasi komponen UI dan menetapkan DataContext ke StudentViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new StudentViewModel();
        }
    }
}
