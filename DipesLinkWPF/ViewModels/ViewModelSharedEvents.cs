using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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

        public static event EventHandler<int>? OnChangeJob;
        public static void OnChangeJobHandler(string buttonName, int currentStation)
        {
            OnChangeJob?.Invoke(buttonName, currentStation);
        }

        public static event EventHandler? OnListBoxMenuSelectionChange;
        public static void OnListBoxMenuSelectionChangeHandler(int index)
        {
            OnListBoxMenuSelectionChange?.Invoke(index, EventArgs.Empty);
        }

        public static event EventHandler? OnDataTableLoading;
        public static void OnDataTableLoadingHandler()
        {
            OnDataTableLoading?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler<bool>? OnEnableUIChange;
        public static void OnEnableUIChangeHandler(int index, bool isEnable)
        {
            OnEnableUIChange?.Invoke(index, isEnable);
        }

        public static event EventHandler? OnMoveToJobDetail;
        public static void OnMoveToJobDetailHandler(int index)
        {
            OnMoveToJobDetail?.Invoke(index,EventArgs.Empty);
        }
    }
}
