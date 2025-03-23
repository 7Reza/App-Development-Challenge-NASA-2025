using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SpherePositionTracker
{
    public partial class MainWindow : Window
    {
        private string excelFilePath;
        private GeometryModel3D dynamicSphereModel;
        private List<Point3D> positionData = new List<Point3D>();
        private int currentPositionIndex = 0;
        private DispatcherTimer timer;
        private bool isAnimating = false;
        private Point3D currentPosition;
        private Point3D targetPosition;
        private double animationProgress = 0;
        private double animationSpeed = 0.1; // Now a variable, default matches slider

        public MainWindow()
        {
            InitializeComponent();

            // Setup 3D viewport
            InitializeScene();

            // Use CompositionTarget.Rendering for smoother animations
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Initialize timer for position updates
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // Update 10 times per second
            timer.Tick += Timer_Tick;
        }

        private void InitializeScene()
        {
            // Create a model group to hold all 3D objects
            Model3DGroup modelGroup = new Model3DGroup();

            // Create central static sphere (red)
            MeshBuilder staticSphereMeshBuilder = new MeshBuilder();
            staticSphereMeshBuilder.AddSphere(new Point3D(0, 0, 0), 0.5);
            GeometryModel3D staticSphereModel = new GeometryModel3D();
            staticSphereModel.Geometry = staticSphereMeshBuilder.ToMesh();
            staticSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            staticSphereModel.BackMaterial = staticSphereModel.Material;
            modelGroup.Children.Add(staticSphereModel);

            // Create dynamic sphere (blue)
            MeshBuilder dynamicSphereMeshBuilder = new MeshBuilder();
            dynamicSphereMeshBuilder.AddSphere(new Point3D(2, 0, 0), 0.3);
            dynamicSphereModel = new GeometryModel3D();
            dynamicSphereModel.Geometry = dynamicSphereMeshBuilder.ToMesh();
            dynamicSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            dynamicSphereModel.BackMaterial = dynamicSphereModel.Material;
            modelGroup.Children.Add(dynamicSphereModel);

            // Add to viewport
            ModelVisual3D visual = new ModelVisual3D();
            visual.Content = modelGroup;
            sphereContainer.Children.Add(visual);

            // Add a coordinate system
            sphereContainer.Children.Add(new CoordinateSystemVisual3D { ArrowLengths = 1 });
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select Excel File with Position Data"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                excelFilePath = openFileDialog.FileName;
                txtFilePath.Text = Path.GetFileName(excelFilePath);
                btnUpdate.IsEnabled = true;

                // Read position data from Excel
                ReadPositionDataFromExcel();
            }
        }

        private void ReadPositionDataFromExcel()
        {
            try
            {
                positionData.Clear();

                // Set the EPPlus license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // First worksheet
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Assuming first row is header
                    {
                        if (double.TryParse(worksheet.Cells[row, 1].Text, out double x) &&
                            double.TryParse(worksheet.Cells[row, 2].Text, out double y) &&
                            double.TryParse(worksheet.Cells[row, 3].Text, out double z))
                        {
                            positionData.Add(new Point3D(x, y, z));
                        }
                    }
                }

                currentPositionIndex = 0;

                MessageBox.Show($"Successfully loaded {positionData.Count} position data points.",
                    "Data Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Excel file: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (positionData.Count > 0)
            {
                if (isAnimating)
                {
                    isAnimating = false;
                    timer.Stop();
                    btnUpdate.Content = "Start Animation";
                }
                else
                {
                    isAnimating = true;
                    currentPosition = positionData[currentPositionIndex];
                    targetPosition = positionData[currentPositionIndex];
                    timer.Start();
                    btnUpdate.Content = "Stop Animation";
                }
            }
            else
            {
                MessageBox.Show("No position data available.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (positionData.Count > 0)
            {
                currentPosition = targetPosition;
                targetPosition = positionData[currentPositionIndex];
                animationProgress = 0;

                currentPositionIndex = (currentPositionIndex + 1) % positionData.Count;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isAnimating && positionData.Count > 0)
            {
                // Use the variable animationSpeed instead of the constant
                animationProgress += animationSpeed;
                if (animationProgress > 1) animationProgress = 1;

                Point3D interpolatedPosition = InterpolatePositions(currentPosition, targetPosition, animationProgress);
                UpdateDynamicSpherePosition(interpolatedPosition);

                viewPort.InvalidateVisual();
            }
        }

        private Point3D InterpolatePositions(Point3D start, Point3D end, double progress)
        {
            return new Point3D(
                start.X + (end.X - start.X) * progress,
                start.Y + (end.Y - start.Y) * progress,
                start.Z + (end.Z - start.Z) * progress
            );
        }

        private void UpdateDynamicSpherePosition(Point3D newPosition)
        {
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new TranslateTransform3D(newPosition.X, newPosition.Y, newPosition.Z));
            dynamicSphereModel.Transform = transformGroup;
        }

        // New method to handle slider changes
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            animationSpeed = speedSlider.Value; // Update animationSpeed when slider moves
        }
    }
}
