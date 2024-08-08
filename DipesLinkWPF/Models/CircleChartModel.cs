using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DipesLink.Models
{
    public class CircleChartModel : ObservableObject
    {
        public IEnumerable<ISeries> Series { get; set; }
        private GaugeItem _gaugeItem;
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                _gaugeItem.Value.Value = value;
                OnPropertyChanged();
            }
        }
        public CircleChartModel()
        {
            _gaugeItem = new GaugeItem(0, series =>
            {
                series.Fill = new SolidColorPaint(SKColor.Parse("#00C752"));//00C752
                series.MaxRadialColumnWidth = 10;
                series.DataLabelsSize = 20;
                series.DataLabelsFormatter = point => $"{point.PrimaryValue}%";
            });
            Series = GaugeGenerator.BuildSolidGauge(_gaugeItem);
            
        }
    }
}
