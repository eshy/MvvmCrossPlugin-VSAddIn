using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using $ext_safeprojectname$.Core;

namespace $safeprojectname$
{
    public class Plugin : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterSingleton<IMvx$ext_safeprojectname$>(new Mvx$ext_safeprojectname$());
        }
    }
}