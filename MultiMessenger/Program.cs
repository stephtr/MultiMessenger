using System;
using Microsoft.UI.Xaml;
using WinRT;
using Microsoft.System;
using System.Threading;
using Windows.ApplicationModel;

namespace MultiMessenger
{
    public static class Program
    {
        [STAThreadAttribute]
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
