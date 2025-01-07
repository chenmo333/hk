using MachineVision.Extensions;
using MachineVision.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MachineVision.Services
{
    internal class NavigationMenuService : BindableBase, INavigationMenuService
    {
        // 构造函数，初始化菜单项集合
        public NavigationMenuService()
        {
            // 初始化导航菜单项集合
            Items = new ObservableCollection<NavigationItem>();
        }

        // 私有字段，存储菜单项集合
        private ObservableCollection<NavigationItem> items;

        // 公共属性，暴露菜单项集合
        public ObservableCollection<NavigationItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        // 初始化菜单项的方法
        public void InitMenus()
        {
            // 清空当前菜单项
            Items.Clear();

            // 添加主菜单项“全部”，并填充子菜单项
            Items.Add(new NavigationItem("", "All", "全部", "", new ObservableCollection<NavigationItem>()
            {
 //                new NavigationItem("","CameraParameters","相机参数","",new ObservableCollection<NavigationItem>()
 //{
 //     new NavigationItem("Camera Calibration","Camera Debugging","相机调试",""),

 //}),
    // 添加“模板匹配”菜单项及其子菜单
                new NavigationItem("","Cameraparameters","相机参数","",new ObservableCollection<NavigationItem>()
                {
                    // 添加子菜单项，形状匹配
                    new NavigationItem("Hikvision","Hik","海康威视","HikView"),
              
                }),

                // 添加“模板匹配”菜单项及其子菜单
                new NavigationItem("","TemplateMatch","模板匹配","",new ObservableCollection<NavigationItem>()
                {
                    // 添加子菜单项，形状匹配
                    new NavigationItem("ShapeOutline","ShapeMatch","形状匹配","ShapeView"),
                    // 添加子菜单项，相似性匹配
                    new NavigationItem("Clouds","NccMacth", "相似性匹配","NccView"),
                    // 添加子菜单项，形变匹配
                    new NavigationItem("ShapeOvalPlus","DeformationMatch","形变匹配","LocalDeformableView"),
                }),

                // 添加“比较测量”菜单项及其子菜单
                new NavigationItem("","Measure", "比较测量","",new ObservableCollection<NavigationItem>()
                {
                    // 添加子菜单项，卡尺找圆
                    new NavigationItem("Circle","Caliper","卡尺找圆","CircleMeasureView"),
                    // 注释掉的菜单项，颜色检测
                    //new NavigationItem("Palette","Color","颜色检测",""),
                    // 注释掉的菜单项，几何测量
                    //new NavigationItem("Ruler","GeometricMeasurement", "几何测量",""),
                }),

                // 添加“字符识别”菜单项及其子菜单
                new NavigationItem("","Character","字符识别","",new ObservableCollection<NavigationItem>()
                {
                    // 注释掉的菜单项，字符识别
                    //new NavigationItem("FormatColorText","Character", "字符识别",""),
                    // 添加子菜单项，一维码识别
                    new NavigationItem("Barcode","BarCode", "一维码识别","BarCodeView"),
                    // 添加子菜单项，二维码识别
                    new NavigationItem("Qrcode", "QrCode","二维码识别","QrCodeView"),
                }),

                // 添加“缺陷检测”菜单项及其子菜单
                new NavigationItem("","Defect","缺陷检测","",new ObservableCollection<NavigationItem>()
                {
                    // 添加子菜单项，缺陷检测
                    new NavigationItem("Crop", "Difference","缺陷检测","DefectView"),
                    // 注释掉的菜单项，形变模型
                    //new NavigationItem("CropRotate","Deformable", "形变模型",""),
                })
            }));

            // 添加系统设置菜单项
            Items.Add(new NavigationItem("", "Setting", "系统设置", "SettingView"));
        }

        // 刷新菜单项的方法，更新菜单项的名称
        public void RefreshMenus()
        {
            // 遍历菜单项集合
            foreach (var item in Items)
            {
                // 根据语言设置更新菜单项的名称
                item.Name = LanguageHelper.KeyValues[item.Key];

                // 如果该菜单项有子菜单项，则遍历子菜单项
                if (item.Items != null && item.Items.Count > 0)
                {
                    foreach (var subItem in item.Items)
                    {
                        // 更新子菜单项的名称
                        subItem.Name = LanguageHelper.KeyValues[subItem.Key];

                        // 如果子菜单项有更深层次的子菜单项，则继续遍历
                        if (subItem.Items != null && subItem.Items.Count > 0)
                        {
                            foreach (var other in subItem.Items)
                                // 更新更深层次的子菜单项名称
                                other.Name = LanguageHelper.KeyValues[other.Key];
                        }
                    }
                }
            }
        }
    }
}
