using HalconDotNet;
using MachineVision.Defect.Models; 

namespace MachineVision.Defect.Extensions
{
    public static class RectangleLocationExtensions
    {
        /// <summary>
        /// 根据2点坐标生成一个矩形对象
        /// </summary>
        /// <param name="rl"></param>
        /// <returns></returns>
        public static HObject GenRectangle1(this RectangleLocation rl)
        {
            HOperatorSet.GenRectangle1(out HObject rect, rl.Y1, rl.X1, rl.Y2, rl.X2);
            return rect;
        }
    }
}
