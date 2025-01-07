using HalconDotNet;
using MachineVision.Core.Extensions;
using MachineVision.Defect.Extensions;
using MachineVision.Defect.Models;
using Prism.Mvvm;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;

namespace MachineVision.Defect.Services
{
    /*
     *  检测服务功能
     * 
     * 1. 根据输入的图像源，以及当前项目和需要待检测的区域数据
     * 2. 执行检测, 并且输出结果
     * 3. 针对检测产生的对象进行资源管理。 非托管资源释放
     * 4. 统一参数检测的结果数据
     */

    public class InspectionService : BindableBase
    {
        public InspectionService(TargetService target)
        {
            this.target = target;
            HistoryDefects = new ObservableCollection<HistoryDefectResult>();
        }

        private readonly TargetService target;
        private ObservableCollection<HistoryDefectResult> historyDefects;

        /// <summary>
        /// 检测历史数据记录
        /// </summary>
        public ObservableCollection<HistoryDefectResult> HistoryDefects
        {
            get { return historyDefects; }
            set { historyDefects = value; RaisePropertyChanged(); }
        }

        public InspectionResult ExecuteAsync(HObject ImageSource, ProjectModel Model, ObservableCollection<InspecRegionModel> RegionList)
        {
            InspectionResult result = new InspectionResult();
            result.ContextResults = new List<RegionContextResult>();
            //1.先查找基准点位置
            var refer = target.GetRefer(ImageSource, Model);

            if (refer)
            {
                result.TimeSpan = SetTimeHelper.SetTimer(() =>
                 {
                     //并行处理
                     Parallel.ForEach(RegionList, Item =>
                     {
                         //根据基准点计算出预取的图像
                         var checkImage = Item.GetInspecImage(ImageSource, Model.ReferSetting.Row, Model.ReferSetting.Column);
                         //执行检测服务算法
                         var ItemResult = Item.Context.Run(checkImage, Item);

                         if (!ItemResult.IsSuccess)
                             result.ContextResults.Add(ItemResult);
                     });

                     if (result.ContextResults.Count > 0)
                     {
                         result.IsSuccess = false;
                         result.Message = $"存在: {result.ContextResults.Count} 处缺陷";
                     }
                     else
                         result.IsSuccess = true;
                 });
            }
            else
            {
                result.IsSuccess = false;
                result.Message = "未匹配图像基准";
            }
            AddpenHistoryResult(result);

            return result;
        }

        private int Index = 1;

        private void AddpenHistoryResult(InspectionResult result)
        {
            if (HistoryDefects.Count == 39)
                Index = 1;

            var defectResult = new HistoryDefectResult()
            { 
                Time = result.TimeSpan,
            };
            defectResult.SetResult(result.IsSuccess, Index);
            HistoryDefects.Add(defectResult);
            Index++;
        }
    }
}
