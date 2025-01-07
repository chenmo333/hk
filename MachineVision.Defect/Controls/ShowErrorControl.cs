using HalconDotNet;
using MachineVision.Defect.Models;
using System.Windows.Controls;
using Prism.Ioc;
using Prism.Events;
using MachineVision.Defect.Events;
using System.Dynamic;
using MachineVision.Core.Extensions;

namespace MachineVision.Defect.Controls
{
    public class ShowErrorControl : System.Windows.Controls.Control
    {
        private HWindow hWindow;
        private HSmartWindowControlWPF hSmart;
        private HObject Image;
        private string Name;
        private TextBlock txtMsg;

        public void Display(RegionContextResult result)
        {
            var render = result.Render;

            Name = result.Name;

            if (render != null && hWindow != null)
            {
                hWindow.ClearWindow();

                txtMsg.Text = $"区域:{Name},亮缺陷:{render.Light.GetSumArea()}, 暗缺陷:{render.Dark.GetSumArea()}";

                Image = render.Image;
                //显示局部缺陷图像
                hWindow.DispObj(Image);

                //显示暗缺陷
                HOperatorSet.SetColor(hWindow, "red");
                hWindow.DispObj(render.Dark);

                //显示亮缺陷
                HOperatorSet.SetColor(hWindow, "green");
                hWindow.DispObj(render.Light);

                hWindow.SetPart(0, 0, -2, -2);
            }
        }

        public override void OnApplyTemplate()
        {
            hSmart = (HSmartWindowControlWPF)GetTemplateChild("PART_SMART");
            hSmart.Loaded += HSmart_Loaded;

            txtMsg = (TextBlock)GetTemplateChild("PART_MSG");

            ((MenuItem)GetTemplateChild("PART_TRAIN")).Click += (s, e) =>
            {
                //把当前这个图像追加到当前检测区域的训练集当中，并且刷新检测模型
                var aggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
                aggregator.GetEvent<ImageTrainEvent>().Publish(new ImageTrainInfo()
                {
                    Image = Image,
                    Name = Name,
                });
            };
            base.OnApplyTemplate();
        }

        private void HSmart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            hWindow = hSmart.HalconWindow;
        }
    }
}
