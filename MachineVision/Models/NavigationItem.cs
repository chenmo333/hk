using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineVision.Models
{
    /// <summary>
    /// 菜单功能项模型
    /// </summary>
    public class NavigationItem : BindableBase
    {
        /// <summary>
        /// 构造函数，用于初始化菜单项
        /// </summary>
        /// <param name="icon">菜单项图标</param>
        /// <param name="key">菜单项的唯一标识键</param>
        /// <param name="name">菜单项名称</param>
        /// <param name="pageName">菜单项对应的页面名称</param>
        /// <param name="items">菜单项的子项集合，默认为null</param>
        public NavigationItem(
            string icon,
            string key,
            string name,
            string pageName,
            ObservableCollection<NavigationItem> items = null)
        {
            Key = key;
            Icon = icon;
            Name = name;
            PageName = pageName;
            Items = items;
        }

        private string key; // 菜单项的唯一标识键
        private string name; // 菜单项的名称
        private string icon; // 菜单项的图标
        private string pageName; // 菜单项对应的页面名称
        private ObservableCollection<NavigationItem> items; // 子菜单项的集合

        /// <summary>
        /// 获取或设置菜单项的子项
        /// </summary>
        public ObservableCollection<NavigationItem> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); } // 设置时触发属性变化通知
        }

        /// <summary>
        /// 获取或设置菜单项的唯一标识键
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; RaisePropertyChanged(); } // 设置时触发属性变化通知
        }

        /// <summary>
        /// 获取或设置菜单项的图标
        /// </summary>
        public string Icon
        {
            get { return icon; }
            set { icon = value; RaisePropertyChanged(); } // 设置时触发属性变化通知
        }

        /// <summary>
        /// 获取或设置菜单项的名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); } // 设置时触发属性变化通知
        }

        /// <summary>
        /// 获取或设置菜单项导航到的页面名称
        /// </summary>
        public string PageName
        {
            get { return pageName; }
            set { pageName = value; RaisePropertyChanged(); } // 设置时触发属性变化通知
        }
    }
}
