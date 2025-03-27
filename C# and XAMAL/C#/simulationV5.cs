using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;  // For 3D visualization components
using Microsoft.Win32;   // For file dialogs
using OfficeOpenXml;     // For Excel file handling
using System.Collections.Generic;
using System.Windows.Threading;  // For timer functionality

namespace SpherePositionTracker
{
    public partial class MainWindow : Window
    {
        // File path for the Excel data
        private string excelFilePath;

        // 3D models for visualization
        private GeometryModel3D dynamicRocketSphereModel;
        private GeometryModel3D dynamicMoonSphereModel;

        // Data storage for positions
        private List<Point3D> rocketPositionData = new List<Point3D>();
        private List<Point3D> moonPositionData = new List<Point3D>();

        // Animation control variables
        private int currentPositionIndex = 0;
        private DispatcherTimer timer;
        private bool isAnimating = false;

        // Current and target positions for smooth animation
        private Point3D currentRocketPosition;
        private Point3D targetRocketPosition;
        private Point3D currentMoonPosition;
        private Point3D targetMoonPosition;
        private double animationProgress = 0;
        private const double AnimationSpeed = 0.1; // Speed of interpolation (0-1)

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the 3D scene with all visual elements
            InitializeScene();

            // Use WPF's built-in rendering event for smooth animations
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Set up a timer for position updates
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // Update every 100ms (10fps)
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Initializes the 3D scene with all visual elements
        /// </summary>
        private void InitializeScene()
        {
            // Create a container for all 3D models
            Model3DGroup modelGroup = new Model3DGroup();

            /* CENTRAL REFERENCE SPHERE (RED) */
            MeshBuilder staticSphereMeshBuilder = new MeshBuilder();
            staticSphereMeshBuilder.AddSphere(new Point3D(0, 0, 0), 0.5); // Center at origin, radius 0.5

            GeometryModel3D staticSphereModel = new GeometryModel3D();
            staticSphereModel.Geometry = staticSphereMeshBuilder.ToMesh();
            staticSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            staticSphereModel.BackMaterial = staticSphereModel.Material; // Ensure back faces are visible
            modelGroup.Children.Add(staticSphereModel);

            /* ROCKET SPHERE (BLUE) */
            MeshBuilder dynamicRocketSphereMeshBuilder = new MeshBuilder();
            dynamicRocketSphereMeshBuilder.AddSphere(new Point3D(2, 0, 0), 0.3); // Initial position at (2,0,0), radius 0.3

            dynamicRocketSphereModel = new GeometryModel3D();
            dynamicRocketSphereModel.Geometry = dynamicRocketSphereMeshBuilder.ToMesh();
            dynamicRocketSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            dynamicRocketSphereModel.BackMaterial = dynamicRocketSphereModel.Material;
            modelGroup.Children.Add(dynamicRocketSphereModel);

            /* MOON SPHERE (GRAY) */
            MeshBuilder dynamicMoonSphereMeshBuilder = new MeshBuilder();
            dynamicMoonSphereMeshBuilder.AddSphere(new Point3D(1, 0, 0), 0.2); // Initial position at (1,0,0), radius 0.2

            dynamicMoonSphereModel = new GeometryModel3D();
            dynamicMoonSphereModel.Geometry = dynamicMoonSphereMeshBuilder.ToMesh();
            dynamicMoonSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            dynamicMoonSphereModel.BackMaterial = dynamicMoonSphereModel.Material;
            modelGroup.Children.Add(dynamicMoonSphereModel);

            // Add all models to the viewport
            ModelVisual3D visual = new ModelVisual3D();
            visual.Content = modelGroup;
            sphereContainer.Children.Add(visual);

            // Add coordinate axes for reference (X,Y,Z arrows)
            sphereContainer.Children.Add(new CoordinateSystemVisual3D { ArrowLengths = 1 });
        }

        /// <summary>
        /// Handles the Browse button click to select an Excel file
        /// </summary>
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

                // Load position data from the selected Excel file
                ReadPositionDataFromExcel();
            }
        }

        /// <summary>
        /// Reads position data from the selected Excel file
        /// </summary>
        private void ReadPositionDataFromExcel()
        {
            try
            {
                // Clear any existing data
                rocketPositionData.Clear();
                moonPositionData.Clear();

                // Required for EPPlus license compliance
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
                {
                    // Access the first worksheet in the Excel file
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    // Process each row (starting from row 2 to skip headers)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        /* ROCKET POSITION DATA (Columns B, C, D) */
                        // Column B (2) = PxR (X coordinate)
                        // Column C (3) = PyR (Y coordinate)
                        // Column D (4) = PzR (Z coordinate)
                        if (double.TryParse(worksheet.Cells[row, 2].Text, out double rocketX) &&
                            double.TryParse(worksheet.Cells[row, 3].Text, out double rocketY) &&
                            double.TryParse(worksheet.Cells[row, 4].Text, out double rocketZ))
                        {
                            rocketPositionData.Add(new Point3D(rocketX, rocketY, rocketZ));
                        }

                        /* MOON POSITION DATA (Columns I, J, K) */
                        // Column I (9) = RxM (X coordinate)
                        // Column J (10) = RyM (Y coordinate)
                        // Column K (11) = RzM (Z coordinate)
                        if (double.TryParse(worksheet.Cells[row, 9].Text, out double moonX) &&
                            double.TryParse(worksheet.Cells[row, 10].Text, out double moonY) &&
                            double.TryParse(worksheet.Cells[row, 11].Text, out double moonZ))
                        {
                            moonPositionData.Add(new Point3D(moonX, moonY, moonZ));
                        }
                    }
                }

                // Reset animation to start from the beginning
                currentPositionIndex = 0;

                // Show success message with loaded data counts
                MessageBox.Show($"Successfully loaded {rocketPositionData.Count} rocket positions and {moonPositionData.Count} moon positions.",
                    "Data Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Excel file: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles Start/Stop animation button click
        /// </summary>
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (rocketPositionData.Count > 0 && moonPositionData.Count > 0)
            {
                if (isAnimating)
                {
                    // Stop the animation
                    isAnimating = false;
                    timer.Stop();
                    btnUpdate.Content = "Start Animation";
                }
                else
                {
                    // Start the animation
                    isAnimating = true;

                    // Initialize positions for smooth animation
                    currentRocketPosition = rocketPositionData[currentPositionIndex];
                    targetRocketPosition = rocketPositionData[currentPositionIndex];
                    currentMoonPosition = moonPositionData[currentPositionIndex];
                    targetMoonPosition = moonPositionData[currentPositionIndex];

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

        /// <summary>
        /// Timer tick event handler - advances to next position in sequence
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (rocketPositionData.Count > 0 && moonPositionData.Count > 0)
            {
                // Set up interpolation from current to next position
                currentRocketPosition = targetRocketPosition;
                targetRocketPosition = rocketPositionData[currentPositionIndex];

                currentMoonPosition = targetMoonPosition;
                targetMoonPosition = moonPositionData[currentPositionIndex];

                // Reset animation progress for new movement
                animationProgress = 0;

                // Move to next position (with wrap-around at end)
                currentPositionIndex = (currentPositionIndex + 1) % rocketPositionData.Count;
            }
        }

        /// <summary>
        /// Rendering event handler for smooth animation between positions
        /// </summary>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isAnimating && rocketPositionData.Count > 0 && moonPositionData.Count > 0)
            {
                // Update animation progress (clamped to 1)
                animationProgress += AnimationSpeed;
                if (animationProgress > 1) animationProgress = 1;

                // Calculate intermediate positions for smooth movement
                Point3D interpolatedRocketPosition = InterpolatePositions(
                    currentRocketPosition, targetRocketPosition, animationProgress);

                Point3D interpolatedMoonPosition = InterpolatePositions(
                    currentMoonPosition, targetMoonPosition, animationProgress);

                // Update the 3D models' positions
                UpdateRocketSpherePosition(interpolatedRocketPosition);
                UpdateMoonSpherePosition(interpolatedMoonPosition);

                // Force the viewport to refresh
                viewPort.InvalidateVisual();
            }
        }

        /// <summary>
        /// Linear interpolation between two 3D points
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">Target position</param>
        /// <param name="progress">Interpolation progress (0-1)</param>
        /// <returns>Interpolated position</returns>
        private Point3D InterpolatePositions(Point3D start, Point3D end, double progress)
        {
            return new Point3D(
                start.X + (end.X - start.X) * progress,
                start.Y + (end.Y - start.Y) * progress,
                start.Z + (end.Z - start.Z) * progress
            );
        }

        /// <summary>
        /// Updates the rocket sphere's position in the 3D view
        /// </summary>
        private void UpdateRocketSpherePosition(Point3D newPosition)
        {
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new TranslateTransform3D(
                newPosition.X, newPosition.Y, newPosition.Z));

            dynamicRocketSphereModel.Transform = transformGroup;
        }

        /// <summary>
        /// Updates the moon sphere's position in the 3D view
        /// </summary>
        private void UpdateMoonSpherePosition(Point3D newPosition)
        {
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new TranslateTransform3D(
                newPosition.X, newPosition.Y, newPosition.Z));

            dynamicMoonSphereModel.Transform = transformGroup;
        }
    }
}
