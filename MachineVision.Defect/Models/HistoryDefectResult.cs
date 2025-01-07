using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineVision.Defect.Models
{
    /// <summary>
    /// 历史缺陷记录
    /// </summary>
    public class HistoryDefectResult : BindableBase
    {
        private bool result;
        private int index;
        private double time;

        /// <summary>
        /// 检测结果
        /// </summary>
        public bool Result
        {
            get { return result; }
            set { result = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 管位号
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 检测耗时
        /// </summary>
        public double Time
        {
            get { return time; }
            set { time = value; RaisePropertyChanged(); }
        }

        public void SetResult(bool result, int Index)
        {
            this.Result = result;
            this.Index = Index;
        }
    }
}
