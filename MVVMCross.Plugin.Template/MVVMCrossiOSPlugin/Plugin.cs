using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using $ext_safeprojectname$.Core;

namespace $safeprojectname$
{
    public class Plugin : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterSingleton<IMvx$ext_pluginname$>(new Mvx$ext_pluginname$());
        }
    }
}