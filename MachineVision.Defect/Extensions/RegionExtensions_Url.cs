using MachineVision.Defect.Models;
using System.IO;

namespace MachineVision.Defect.Extensions
{
    public static partial class RegionExtensions
    {
        public static string BasrUrl = AppDomain.CurrentDomain.BaseDirectory + "Products\\";

        /// <summary>
        /// 检测区域文件夹地址
        /// </summary>
        /// <param name="Region"></param>
        /// <returns></returns>
        public static string GetRegionUrl(this InspecRegionModel Region)
        {
            string url = $"{BasrUrl}{Region.ProjectName}\\Regions\\{Region.Name}\\";

            if (!Directory.Exists(url))
                Directory.CreateDirectory(url);

            return url;
        }

        /// <summary>
        /// 检测区域训练文件夹地址
        /// </summary>
        /// <param name="Region"></param>
        /// <returns></returns>
        public static string GetRegionTrainUrl(this InspecRegionModel Region)
        {
            string url = $"{BasrUrl}{Region.ProjectName}\\Regions\\{Region.Name}\\Trains\\";

            if (!Directory.Exists(url))
                Directory.CreateDirectory(url);

            return url;
        }
    }
}
