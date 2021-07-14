using System;
using Microsoft.UI.Xaml;
using WinRT;
using System.Threading;
using Windows.ApplicationModel;
using Microsoft.UI.Dispatching;

namespace MultiMessenger
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ComWrappersSupport.InitializeComWrappers();
            var instance = AppInstance.FindOrRegisterInstanceForKey("SingleInstanceStartup");
            if (instance.IsCurrentInstance)
            {
                Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });
            }
            else
            {
                instance.RedirectActivationTo();
            }
        }
    }
}
