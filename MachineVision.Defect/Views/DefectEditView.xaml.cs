using MachineVision.Defect.Models.UI;
using MachineVision.Defect.ViewModels;
using System.IO;
using System.Windows.Controls;

namespace MachineVision.Defect.Views
{
    public partial class DefectEditView : System.Windows.Controls.UserControl
    {
        public DefectEditView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is DefectEditViewModel vm)
            {
                if (ListBox.SelectedItem is ImageFile file)
                {
                    if (File.Exists(file.FilePath))
                        vm.Image = file.GetImage();
                }
            }
        }

        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.DataContext is DefectEditViewModel vm)
            {
                if (ListBox.SelectedItem is ImageFile file)
                {
                    if (File.Exists(file.FilePath))
                    {
                        vm.Image = file.GetImage();
                        vm.Run();
                    }
                }
            }
        }
    }
}
