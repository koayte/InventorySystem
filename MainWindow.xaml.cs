using InventorySystem.SideBarNav;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SideBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = sidebar.SelectedItem as NavButton;
            navframe.Navigate(selected.Navlink);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int nWidth = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
            int nHieght = (int)System.Windows.SystemParameters.PrimaryScreenHeight;

            this.LayoutTransform = new ScaleTransform(nWidth / 1920, nHieght / 1080);
        }
    }


}
