using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using static DipesLink.Views.Enums.ViewEnums;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink.Views.Models
{
    public partial class JobSystemSettings :ObservableObject
    {
        [ObservableProperty]
        public int _Id;

        [ObservableProperty]
        private string? _Name;

        [ObservableProperty]
        private ProcessCheckeType _CompleteCondition = ProcessCheckeType.TotalChecked;

        [ObservableProperty]
        private bool _OutputSignal;

        [ObservableProperty]
        private bool _ImageExport;

        [ObservableProperty]
        private string? _ImageExportPath;
    }
}
