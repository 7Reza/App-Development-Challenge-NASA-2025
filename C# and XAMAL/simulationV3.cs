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
                    // Cancel any ongoing animation
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = new CancellationTokenSource();

                    LoadDataFromExcel(openFileDialog.FileName);
                    _currentIndex = 0; // Reset index for new data
                    _ = StartAnimationAsync(); // Start animation without awaiting
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}");
                }
            }
        }

        private void LoadDataFromExcel(string filePath)
        {
            _positions.Clear(); // Clear previous data

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

       
