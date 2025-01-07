using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineVision.Shared.Services.Session
{
    public abstract class HostDialogViewModel : BindableBase, IHostDialogAware
    {
        public string Title { get; set; }

        public string IdentifierName { get; set; }

        public DelegateCommand SaveCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        private bool isBusy;
        public bool IsNotBusy => !IsBusy;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNotBusy));
            }
        }

        public virtual async Task SetBusyAsync(Func<Task> func, string loadingMessage = null)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await func();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public HostDialogViewModel()
        {
            SaveCommand = new DelegateCommand(async () => await Save());
            CancelCommand = new DelegateCommand(Cancel);
        }

        public virtual void Cancel()
        {
            if (DialogHost.IsDialogOpen(IdentifierName))
                DialogHost.Close(IdentifierName, new DialogResult(ButtonResult.No));
        }

        public virtual async Task Save()
        {
            if (DialogHost.IsDialogOpen(IdentifierName))
                DialogHost.Close(IdentifierName, new DialogResult(ButtonResult.OK));

            await Task.CompletedTask;
        }

        protected virtual void Save(object value)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Value", value);

            if (DialogHost.IsDialogOpen(IdentifierName))
                DialogHost.Close(IdentifierName, new DialogResult(ButtonResult.OK, param));
        }

        protected virtual void Save(DialogParameters param)
        {
            if (DialogHost.IsDialogOpen(IdentifierName))
                DialogHost.Close(IdentifierName, new DialogResult(ButtonResult.OK, param));
        }

        public abstract void OnDialogOpened(IDialogParameters parameters);
    }
}
