using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using ExcelDataReader;
using HelixToolkit.Wpf;
using Microsoft.Win32;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private SphereVisual3D _sphere;
        private List<PositionData> _positions = new();
        private int _currentIndex = 0;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            LoadSphere();
            LoadDataWithFileDialog();
        }

        private void LoadSphere()
        {
            _sphere = new SphereVisual3D
            {
                Center = new Point3D(0, 0, 0),
                Radius = 1,
                Fill = System.Windows.Media.Brushes.Blue
            };
            Viewport.Children.Add(_sphere);
        }

        private void LoadDataWithFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Title = "Select Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LoadDataFromExcel(openFileDialog.FileName);
                    StartAnimationAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}");
                }
            }
        }

        private void LoadDataFromExcel(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Selected file not found");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            
            var result = reader.AsDataSet();
            var table = result.Tables[0];

            for (int row = 1; row < table.Rows.Count; row++)
            {
                var cells = table.Rows[row].ItemArray;
                
                if (cells.Length < 4) continue;

                if (TryParseCell(cells[0], out double t) &&
                    TryParseCell(cells[1], out double x) &&
                    TryParseCell(cells[2], out double y) &&
                    TryParseCell(cells[3], out double z))
                {
                    _positions.Add(new PositionData(t, x, y, z));
                }
            }

            if (_positions.Count == 0)
                throw new InvalidDataException("No valid position data found");
        }

        private bool TryParseCell(object cellValue, out double result)
        {
            result = 0;
            return cellValue != null && 
                   double.TryParse(cellValue.ToString(), out result);
        }

        private async Task StartAnimationAsync()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                while (_currentIndex < _positions.Count && !_cts.IsCancellationRequested)
                {
                    UpdateSpherePosition();
                    
                    if (_currentIndex > 0)
                    {
                        double timeDiff = _positions[_currentIndex].Time - 
                                        _positions[_currentIndex - 1].Time;
                        await Task.Delay(TimeSpan.FromSeconds(timeDiff), _cts.Token);
                    }
                    else
                    {
                        await Task.Delay(100, _cts.Token);
                    }

                    _currentIndex++;
                }
            }
            catch (TaskCanceledException) { /* Normal cancellation */ }
            catch (Exception ex)
            {
                MessageBox.Show($"Animation error: {ex.Message}");
            }
        }

        private void UpdateSpherePosition()
        {
            var currentPos = _positions[_currentIndex];
            _sphere.Center = new Point3D(currentPos.X, currentPos.Y, currentPos.Z);
        }

        protected override void OnClosed(EventArgs e)
        {
            _cts?.Cancel();
            Viewport.Children.Clear();
            _positions.Clear();
            base.OnClosed(e);
        }

        // Add this to XAML: <Button Content="Cancel" Click="CancelButton_Click"/>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }
    }

    public class PositionData
    {
        public double Time { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public PositionData(double time, double x, double y, double z)
        {
            Time = time;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
