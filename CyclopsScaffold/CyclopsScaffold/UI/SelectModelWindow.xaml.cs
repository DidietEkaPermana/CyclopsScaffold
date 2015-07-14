using System;
using System.Collections.Generic;
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

namespace CyclopsScaffold.UI
{
    /// <summary>
    /// Interaction logic for SelectModelWindow.xaml
    /// </summary>
    public partial class SelectModelWindow : Window
    {
        public SelectModelWindow(CustomViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            ((CustomViewModel)DataContext).SelectedView = "Plain";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (ModelType item in trModelTree.Items)
            {
                if (item.IsChecked)
                    ((CustomViewModel)DataContext).SelectedModelType.Add(item);
            }

            this.DialogResult = true;
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	trModelTree.ItemsSource = ((CustomViewModel)DataContext).ModelTypes;
        }
    }
}
