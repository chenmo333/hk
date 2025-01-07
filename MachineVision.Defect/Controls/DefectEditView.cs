using HalconDotNet;
using MachineVision.Core.Extensions;
using MachineVision.Defect.Models;
using MachineVision.Shared.Controls;
using MachineVision.Shared.Extensions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MachineVision.Defect.Controls
{
    public class DefectEditView : System.Windows.Controls.Control
    {
        #region 缺陷检测结果

        public InspectionResult Result
        {
            get { return (InspectionResult)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(InspectionResult), typeof(DefectEditView), new PropertyMetadata(DefectResultCallBack));

        public static void DefectResultCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DefectEditView view)
            {
                view.ClearDrawingObjects();
                view.DisplayDefectResult();
            }
        }

        /// <summary>
        /// 显示图像缺陷检测结果
        /// </summary>
        private void DisplayDefectResult()
        {
            if (Result != null)
            {
                if (Result.IsSuccess)
                    txtMsg.Text = "OK, 耗时:" + Result.TimeSpan + " ms"; 
                else
                    txtMsg.Text = "NG, 耗时:" + Result.TimeSpan + " ms," + Result.Message;

                if (Result.ContextResults != null)
                {
                    foreach (var context in Result.ContextResults)
                    {
                        //显示实际的检测位置
                        var location = context.Location;
                        HOperatorSet.GenRectangle1(out HObject rectangle, location.Y1, location.X1, location.Y2, location.X2);
                        HOperatorSet.GenContourRegionXld(rectangle, out HObject contour, "border");
                        rectangle.Dispose();
                        HOperatorSet.SetColor(hWindow, "blue");
                        HOperatorSet.DispObj(contour, hWindow);
                        contour.Dispose();

                        if (context.Render == null) continue;

                        //显示亮缺陷
                        HOperatorSet.SetColor(hWindow, "green");
                        if (context.Render.Light != null)
                            hWindow.DispObj(context.Render.Light.Move(location.Y1, location.X1).GetRegionContour());

                        //显示暗缺陷
                        HOperatorSet.SetColor(hWindow, "red");
                        if (context.Render.Dark != null)
                            hWindow.DispObj(context.Render.Dark.Move(location.Y1, location.X1).GetRegionContour());
                    }
                }
            }
        }

        #endregion

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(DefectEditView), new PropertyMetadata(""));

        private HSmartWindowControlWPF hSmart;
        private HWindow hWindow;

        public HObject Image
        {
            get { return (HObject)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(HObject), typeof(DefectEditView), new PropertyMetadata(IamgeChangedCallBack));

        public HWindow HWindow
        {
            get { return (HWindow)GetValue(HWindowProperty); }
            set { SetValue(HWindowProperty, value); }
        }

        public static readonly DependencyProperty HWindowProperty =
            DependencyProperty.Register("HWindow", typeof(HWindow), typeof(DefectEditView), new PropertyMetadata(null));

        /// <summary>
        /// 绘制的形状集合
        /// </summary> 
        public ObservableCollection<HDrawingObjectInfo> DrawingObjectInfos
        {
            get { return (ObservableCollection<HDrawingObjectInfo>)GetValue(DrawingObjectInfosProperty); }
            set { SetValue(DrawingObjectInfosProperty, value); }
        }

        public static readonly DependencyProperty DrawingObjectInfosProperty =
            DependencyProperty.Register("DrawingObjectInfos", typeof(ObservableCollection<HDrawingObjectInfo>), typeof(DefectEditView));

        public ProjectModel Model
        {
            get { return (ProjectModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(ProjectModel), typeof(DefectEditView), new PropertyMetadata(ModelChangedCallBack));

        public InspecRegionModel SelectedRegion
        {
            get { return (InspecRegionModel)GetValue(SelectedRegionProperty); }
            set { SetValue(SelectedRegionProperty, value); }
        }

        public static readonly DependencyProperty SelectedRegionProperty =
            DependencyProperty.Register("SelectedRegion", typeof(InspecRegionModel), typeof(DefectEditView), new PropertyMetadata(SelectedRegionChangedCallBack));

        public bool IsModelEditMode
        {
            get { return (bool)GetValue(IsModelEditModeProperty); }
            set { SetValue(IsModelEditModeProperty, value); }
        }

        public static readonly DependencyProperty IsModelEditModeProperty =
            DependencyProperty.Register("IsModelEditMode", typeof(bool), typeof(DefectEditView), new PropertyMetadata(IsModelEditModeChangedCallBack));

        public ICommand UpdateModelParamCommand
        {
            get { return (ICommand)GetValue(UpdateModelParamCommandProperty); }
            set { SetValue(UpdateModelParamCommandProperty, value); }
        }

        public static readonly DependencyProperty UpdateModelParamCommandProperty =
            DependencyProperty.Register("UpdateModelParamCommand", typeof(ICommand), typeof(DefectEditView));

        public ICommand UpdateRegionCommand
        {
            get { return (ICommand)GetValue(UpdateRegionCommandProperty); }
            set { SetValue(UpdateRegionCommandProperty, value); }
        }

        public static readonly DependencyProperty UpdateRegionCommandProperty =
            DependencyProperty.Register("UpdateRegionCommand", typeof(ICommand), typeof(DefectEditView));

        public static void SelectedRegionChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DefectEditView view && e.NewValue != null)
            {
                view.txtMsg.Text = $"当前选中检测区域:{view.SelectedRegion.Name}";

                view.ClearDrawingObjects();

                var setting = view.SelectedRegion.MatchSetting;

                if (setting.Y1 != 0 && setting.X1 != 0 && setting.Y2 != 0 && setting.X2 != 0)
                    view.AttachDrawingObjectToWindow("red", setting.Y1, setting.X1, setting.Y2, setting.X2);

                view.Menu_Refer.Visibility = Visibility.Collapsed;
                view.Menu_Update.Visibility = Visibility.Collapsed;
                view.Menu_Region.Visibility = Visibility.Visible;
                view.Menu_UpdateRegion.Visibility = Visibility.Visible;
            }
        }

        public static void IsModelEditModeChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModelChangedCallBack(d, e);
        }

        public static void ModelChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DefectEditView view)
            {
                if (view.Image == null || view.Model == null) return;

                view.ClearDrawingObjects();

                view.Menu_Region.Visibility = Visibility.Collapsed;
                view.Menu_UpdateRegion.Visibility = Visibility.Collapsed;

                view.RefreshProjectParameter();
            }
        }

        #region 项目相关参数

        /// <summary>
        /// 刷新项目参数
        /// </summary>
        public void RefreshProjectParameter()
        {
            var refer = Model.ReferSetting; //项目的参考点数据
            if (refer != null)
            {
                if (refer.Y1 != 0 && refer.X1 != 0 && refer.Y2 != 0 && refer.X2 != 0)
                    AttachDrawingObjectToWindow("green", refer.Y1, refer.X1, refer.Y2, refer.X2);
            }
        }

        /// <summary>
        /// 附加绘制对象至窗口中
        /// </summary>
        /// <param name="color"></param>
        /// <param name="tuples"></param>
        public void AttachDrawingObjectToWindow(string color, params HTuple[] tuples)
        {
            //根据参考点保存的参数创建一个矩形
            var drawingObj = HDrawingObject.CreateDrawingObject(HDrawingObject.HDrawingObjectType.RECTANGLE1, tuples);
            if (drawingObj == null) return;
            if (tuples[0] == tuples[2] && tuples[1] == tuples[3]) return;

            drawingObj.SetDrawingObjectParams("color", color);

            //缓存这个矩形参数
            var drawingObjectInfo = new HDrawingObjectInfo()
            {
                Color = color,
                HDrawingObject = drawingObj,
                HTuples = tuples
            };

            //绘制对象发生移动或者尺寸发生变化时的事件通知
            drawingObj.OnDrag(OnDragDrawingObject);
            drawingObj.OnResize(OnResizeDrawingObject);

            DrawingObjectInfos.Add(drawingObjectInfo);
            //将这个矩形显示到界面上
            hWindow.AttachDrawingObjectToWindow(drawingObj);
        }

        private void OnDragDrawingObject(HDrawingObject obj, HWindow hwin, string type)
        {
            RefreshDrawingObject(obj);
        }

        private void OnResizeDrawingObject(HDrawingObject obj, HWindow hwin, string type)
        {
            RefreshDrawingObject(obj);
        }

        /// <summary>
        /// 刷新绘制对象的参数信息
        /// </summary>
        /// <param name="obj"></param>
        private void RefreshDrawingObject(HDrawingObject obj)
        {
            if (obj == null) return;

            var hv_type = obj.GetDrawingObjectParams("type");
            var htuples = obj.GetTuples(hv_type);

            var objInfo = DrawingObjectInfos.FirstOrDefault(t => t.HDrawingObject != null && t.HDrawingObject.ID == obj.ID);

            if (objInfo != null)
            {
                objInfo.HTuples = htuples;
            }
        }

        #endregion

        public static void IamgeChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DefectEditView view && e.NewValue != null)
            {
                view.Display((HObject)e.NewValue);
            }
        }

        private void Display(HObject hObject)
        {
            hWindow.DispObj(hObject);
            hWindow.SetPart(0, 0, -2, -2);
        }

        /// <summary>
        /// 清空当前窗口中的绘制对象
        /// </summary>
        private void ClearDrawingObjects()
        {
            if (DrawingObjectInfos != null && DrawingObjectInfos.Count > 0)
            {
                for (int i = DrawingObjectInfos.Count - 1; i >= 0; i--)
                {
                    var item = DrawingObjectInfos[i];
                    item.HDrawingObject?.Dispose();
                    DrawingObjectInfos.Remove(item);
                }
            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <returns></returns>
        private async void DrawRectangle1(string color)
        {
            HObject drawObj;
            HOperatorSet.GenEmptyObj(out drawObj);
            HOperatorSet.SetColor(hWindow, color);

            hSmart.HZoomContent = HSmartWindowControlWPF.ZoomContent.Off;
            var hTuples = new HTuple[4];
            await Task.Run(() =>
            {
                HOperatorSet.DrawRectangle1(hWindow, out hTuples[0], out hTuples[1], out hTuples[2], out hTuples[3]);
                drawObj = hTuples.GenRectangle();
            });

            if (drawObj != null)
                AttachDrawingObjectToWindow(color, hTuples);

            hSmart.HZoomContent = HSmartWindowControlWPF.ZoomContent.WheelForwardZoomsIn;
        }

        private void HSmart_Loaded(object sender, RoutedEventArgs e)
        {
            hWindow = this.hSmart.HalconWindow;
            HWindow = hWindow;
        }

        private MenuItem Menu_Refer, Menu_Update, Menu_Region, Menu_UpdateRegion;
        private TextBlock txtMsg;

        public override void OnApplyTemplate()
        {
            txtMsg = (TextBlock)GetTemplateChild("PART_MSG");

            if (GetTemplateChild("PART_SMART") is HSmartWindowControlWPF hSmart)
            {
                this.hSmart = hSmart;
                this.hSmart.Loaded += HSmart_Loaded;
            }

            //绘制参考点
            Menu_Refer = (MenuItem)GetTemplateChild("PART_Refer");
            Menu_Refer.Click += (s, e) =>
            {
                DrawRectangle1("green");
            };

            //更新参考点
            Menu_Update = (MenuItem)GetTemplateChild("PART_Update");
            Menu_Update.Click += (s, e) =>
            {
                UpdateModelParamCommand?.Execute(this);
            };

            //绘制检测区域
            Menu_Region = (MenuItem)GetTemplateChild("PART_Region");
            Menu_Region.Click += (s, e) =>
            {
                DrawRectangle1("red");
            };

            //更新检测区域
            Menu_UpdateRegion = (MenuItem)GetTemplateChild("PART_UpdateRegion");
            Menu_UpdateRegion.Click += (s, e) =>
            {
                UpdateRegionCommand?.Execute(this);
            };

            var Menu_Clear = (MenuItem)GetTemplateChild("PART_Clear");
            Menu_Clear.Click += (s, e) =>
            {
                this.ClearDrawingObjects();
            };
        }
    }
}
