using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineVision.Shared.Services.Session
{
    /// <summary>
    /// 对话主机服务接口
    /// </summary>
    public interface IHostDialogService : IDialogService
    {
        Task<IDialogResult> ShowDialogAsync(
            string name,
            IDialogParameters parameters = null,
            string IdentifierName = "Root");
    }
}
