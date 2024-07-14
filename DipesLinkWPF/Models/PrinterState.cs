using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;

namespace DipesLink.Models
{
    public class PrinterState : ObservableObject
    {
        private string? state;
        private string? name;

        public string? Name { get => name; set { name = value; OnPropertyChanged(); } }
        public string? State { get => state; set { state = value; OnPropertyChanged(); } }
    }
}
