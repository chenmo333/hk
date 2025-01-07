using HalconDotNet;
using Microsoft.VisualBasic.ApplicationServices;
using Prism.Mvvm; 

namespace MachineVision.Defect.Models.UI
{
    public class ImageFile : BindableBase
    {
        private string fileName, filePath;
       
    
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; RaisePropertyChanged(); }
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; RaisePropertyChanged(); }
        }


        //加载图片
        public HObject GetImage()
        {
            var image = new HImage();
            image.ReadImage(FilePath);//// 读取图像
            return image;
        }
    }
}
