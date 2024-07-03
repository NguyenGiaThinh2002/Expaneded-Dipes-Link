using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.ViewModels
{
    public class ViewModelSharedEvents
    {
        public static event EventHandler? MainListBoxMenuChange;
        public static void OnMainListBoxMenuChange()
        {
            MainListBoxMenuChange?.Invoke(null, EventArgs.Empty);
        }


        public static event EventHandler<int>? OnJobDetailChange; // event station detail usercontrol change
        public static void OnJobDetailChangeHandler(int currentJob)
        {
            OnJobDetailChange?.Invoke(null, currentJob);
        }

        public static event EventHandler<int>? OnRestartStation;
        public static void OnRestartStationHandler(int currentStation)
        {
            OnRestartStation?.Invoke(null, currentStation);
        }

        public static event EventHandler? OnListBoxMenuSelectionChange;
        public static void OnListBoxMenuSelectionChangeHandler(int index)
        {
            OnListBoxMenuSelectionChange?.Invoke(index, EventArgs.Empty);
        }

        public static event EventHandler? OnLoadingTableStatusChange;
        public static void OnLoadingTableStatusChangeHandler()
        {
            OnLoadingTableStatusChange?.Invoke(null, EventArgs.Empty);
        }

    }
}
