using System.Globalization; 
using System.Windows.Data;

namespace MachineVision.Defect.Converters
{
    public class IStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && int.TryParse(value.ToString(), out int result))
            {
                if (result == 0) return "#000000";

                if (result == 1) return "#ff33ff";
                else
                    return "#0099ff";
            }
            return "#000000";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
