using System.Globalization; 
using System.Windows.Data;

namespace MachineVision.Defect.Converters
{
    public class KindToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (bool.TryParse(value.ToString(), out bool result))
                {
                    return result ? "green" : "red";
                }
            }
            return "green";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
