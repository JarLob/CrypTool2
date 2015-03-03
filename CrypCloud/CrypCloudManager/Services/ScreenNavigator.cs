using System.Collections.Generic;
using System.Linq;
using CrypCloud.Manager.ViewModels;

namespace CrypCloud.Manager.Services
{
    public class ScreenNavigator
    {
        private readonly Dictionary<ScreenPaths, ScreenViewModel> screens = new Dictionary<ScreenPaths, ScreenViewModel>();

        public void AddScreenWithPath(ScreenViewModel screenModel, ScreenPaths path)
        {
            screens.Add(path, screenModel);
            screenModel.Navigator = this;
        }

        public void ShowScreenWithPath(ScreenPaths path)
        {
            if ( ! screens.ContainsKey(path))
            {
                return;
            }

            foreach (var screenViewModel in screens)
            {
                screenViewModel.Value.IsActive = false;
            }

            screens.All(it => it.Value.IsActive = false);
            screens[path].IsActive = true;
        }

 
    }
}
