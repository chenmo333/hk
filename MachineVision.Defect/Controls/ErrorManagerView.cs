using MachineVision.Defect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MachineVision.Defect.Controls
{
    public class ErrorManagerView : System.Windows.Controls.Control
    {
        public InspectionResult Result
        {
            get { return (InspectionResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(InspectionResult), typeof(ErrorManagerView), new PropertyMetadata(ResultChangedCallBack));

        private ShowErrorControl[] Errors = new ShowErrorControl[10];

        public static void ResultChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorManagerView view)
                view.Display();
        }

        private void Display()
        {
            var count = Result.ContextResults.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 9) break;
                Errors[i].Display(Result.ContextResults[i]);
            }
        }

        public override void OnApplyTemplate()
        {
            Errors[0] = (ShowErrorControl)GetTemplateChild("PART1");
            Errors[1] = (ShowErrorControl)GetTemplateChild("PART2");
            Errors[2] = (ShowErrorControl)GetTemplateChild("PART3");
            Errors[3] = (ShowErrorControl)GetTemplateChild("PART4");
            Errors[4] = (ShowErrorControl)GetTemplateChild("PART5");
            Errors[5] = (ShowErrorControl)GetTemplateChild("PART6");
            Errors[6] = (ShowErrorControl)GetTemplateChild("PART7");
            Errors[7] = (ShowErrorControl)GetTemplateChild("PART8");
            Errors[8] = (ShowErrorControl)GetTemplateChild("PART9");
            Errors[9] = (ShowErrorControl)GetTemplateChild("PART10");
            base.OnApplyTemplate();
        }
    }
}
