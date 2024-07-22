namespace DipesLink.ViewModels
{
    public class ViewModelSharedEvents
    {
        public static event EventHandler? OnMainListBoxMenu;
        public static void OnMainListBoxMenuChanged(int menuIndex)
        {
            OnMainListBoxMenu?.Invoke(menuIndex, EventArgs.Empty);
        }


        public static event EventHandler<int>? OnJobDetailChange; // event station detail usercontrol change
        public static void OnJobDetailChanged(int currentJob)
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
