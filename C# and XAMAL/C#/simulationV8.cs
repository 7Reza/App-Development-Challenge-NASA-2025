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
        private List<double> timeData = new List<double>();

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
        private double animationSpeed = 0.1; // Speed of interpolation (0-1)


        public MainWindow()
        {
            InitializeComponent();

            // Initialize the 3D scene with all visual elements
            InitializeScene();

            // Use WPF's built-in rendering event for smooth animations
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Set up a timer for position updates
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(.8); // Update every 100ms (10fps)
            timer.Tick += Timer_Tick;

            // Set up event handlers for the controls
            speedSlider.ValueChanged += SpeedSlider_ValueChanged;
            cameraDistanceSlider.ValueChanged += CameraDistanceSlider_ValueChanged;
            cameraAngleXSlider.ValueChanged += CameraAngleXSlider_ValueChanged;
            cameraAngleYSlider.ValueChanged += CameraAngleYSlider_ValueChanged;
            showOrbits.Checked += ShowOrbits_Checked;
            showOrbits.Unchecked += ShowOrbits_Unchecked;
            resetButton.Click += ResetButton_Click;

            chkFollowRocket.Checked += chkFollowRocket_Checked;
            chkFollowRocket.Unchecked += chkFollowRocket_Unchecked;

      
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            animationSpeed = e.NewValue / 10.0; // Adjust the speed factor as needed
        }

        private void CameraDistanceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewPort.CameraController.CameraPosition = new Point3D(viewPort.CameraController.CameraPosition.X, viewPort.CameraController.CameraPosition.Y, e.NewValue);
        }


        private void CameraAngleXSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewPort.CameraController.CameraRotationMode = CameraRotationMode.Turntable;
            viewPort.CameraController.CameraUpDirection = new Vector3D(1, 0, 0);
            viewPort.CameraController.CameraLookDirection = new Vector3D(0, 0, -1);
            viewPort.CameraController.CameraPosition = new Point3D(viewPort.CameraController.CameraPosition.X, viewPort.CameraController.CameraPosition.Y, e.NewValue);
        }

        private void CameraAngleYSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            viewPort.CameraController.CameraRotationMode = CameraRotationMode.Turntable;
            viewPort.CameraController.CameraUpDirection = new Vector3D(0, 1, 0);
            viewPort.CameraController.CameraLookDirection = new Vector3D(0, 0, -1);
            viewPort.CameraController.CameraPosition = new Point3D(viewPort.CameraController.CameraPosition.X, e.NewValue, viewPort.CameraController.CameraPosition.Z);
        }


        private void ShowOrbits_Checked(object sender, RoutedEventArgs e)
        {
            // Show orbits logic here
        }

        private void ShowOrbits_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide orbits logic here
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset camera and animation settings
            speedSlider.Value = 1;
            cameraDistanceSlider.Value = 20;
            cameraAngleXSlider.Value = 0;
            cameraAngleYSlider.Value = 0;
            showOrbits.IsChecked = true;
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
            staticSphereMeshBuilder.AddSphere(new Point3D(0, 0, 0), 6.738); // Center at origin, radius 0.5

            GeometryModel3D staticSphereModel = new GeometryModel3D();
            staticSphereModel.Geometry = staticSphereMeshBuilder.ToMesh();
            staticSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            staticSphereModel.BackMaterial = staticSphereModel.Material; // Ensure back faces are visible
            modelGroup.Children.Add(staticSphereModel);

            /* ROCKET SPHERE (BLUE) */
            MeshBuilder dynamicRocketSphereMeshBuilder = new MeshBuilder();
            dynamicRocketSphereMeshBuilder.AddSphere(new Point3D(2, 0, 0), 0.5); // Initial position at (2,0,0), radius 0.3

            dynamicRocketSphereModel = new GeometryModel3D();
            dynamicRocketSphereModel.Geometry = dynamicRocketSphereMeshBuilder.ToMesh();
            dynamicRocketSphereModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            dynamicRocketSphereModel.BackMaterial = dynamicRocketSphereModel.Material;
            modelGroup.Children.Add(dynamicRocketSphereModel);

            /* MOON SPHERE (GRAY) */
            MeshBuilder dynamicMoonSphereMeshBuilder = new MeshBuilder();
            dynamicMoonSphereMeshBuilder.AddSphere(new Point3D(-30, 20, 12), 1.737); // Initial position at (1,0,0), radius 0.2

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
                timeData.Clear(); // Clear time data as well

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
                        /* TIME DATA (Column A) */
                        if (double.TryParse(worksheet.Cells[row, 1].Text, out double time))
                        {
                            timeData.Add(time);
                        }

                        /* ROCKET POSITION DATA (Columns B, C, D) */
                        if (double.TryParse(worksheet.Cells[row, 2].Text, out double rocketX) &&
                            double.TryParse(worksheet.Cells[row, 3].Text, out double rocketY) &&
                            double.TryParse(worksheet.Cells[row, 4].Text, out double rocketZ))
                        {
                            rocketPositionData.Add(new Point3D(rocketX, rocketY, rocketZ));
                        }

                        /* MOON POSITION DATA (Columns I, J, K) */
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
                MessageBox.Show($"Successfully loaded {rocketPositionData.Count} rocket positions, {moonPositionData.Count} moon positions, and {timeData.Count} time points.",
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
            if (rocketPositionData.Count > 0 && moonPositionData.Count > 0 && timeData.Count > 0)
            {
                // Set up interpolation from current to next position
                currentRocketPosition = targetRocketPosition;
                targetRocketPosition = rocketPositionData[currentPositionIndex];

                currentMoonPosition = targetMoonPosition;
                targetMoonPosition = moonPositionData[currentPositionIndex];

                // Reset animation progress for new movement
                animationProgress = 0;

                // Move to next position (with wrap-around at end)
                // Use the smallest count to ensure we don't exceed any list bounds
                currentPositionIndex = (currentPositionIndex + 1) % timeData.Count;
            }
        }

        // Remove the duplicate FollowRocket method



        // Modify the CompositionTarget_Rendering method to include camera following
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isAnimating && rocketPositionData.Count > 0 && moonPositionData.Count > 0 && timeData.Count > 0)
            {
                // Existing interpolation code...
                animationProgress += animationSpeed;
                if (animationProgress > 1) animationProgress = 1;

                Point3D interpolatedRocketPosition = InterpolatePositions(
                    currentRocketPosition, targetRocketPosition, animationProgress);

                Point3D interpolatedMoonPosition = InterpolatePositions(
                    currentMoonPosition, targetMoonPosition, animationProgress);

                // Update the 3D models' positions
                UpdateRocketSpherePosition(interpolatedRocketPosition);
                UpdateMoonSpherePosition(interpolatedMoonPosition);

                // Add camera following functionality
                FollowRocket(interpolatedRocketPosition);

                // Existing UI update code...
                Dispatcher.Invoke(() =>
                {
                    txtTime.Text = timeData[currentPositionIndex].ToString("F2");
                    txtRocketX.Text = interpolatedRocketPosition.X.ToString("F2");
                    txtRocketY.Text = interpolatedRocketPosition.Y.ToString("F2");
                    txtRocketZ.Text = interpolatedRocketPosition.Z.ToString("F2");
                    txtMoonX.Text = interpolatedMoonPosition.X.ToString("F2");
                    txtMoonY.Text = interpolatedMoonPosition.Y.ToString("F2");
                    txtMoonZ.Text = interpolatedMoonPosition.Z.ToString("F2");
                });

                // Force the viewport to refresh
                viewPort.InvalidateVisual();
            }
        }

        // Optionally, add a checkbox to toggle camera following
        private bool isCameraFollowing = true;

        private void chkFollowRocket_Checked(object sender, RoutedEventArgs e)
        {
            isCameraFollowing = true;
        }

        private void chkFollowRocket_Unchecked(object sender, RoutedEventArgs e)
        {
            isCameraFollowing = false;
        }

        // Modify the FollowRocket method to respect the checkbox
        private void FollowRocket(Point3D rocketPosition)
        {
            if (isCameraFollowing)
            {
                // Approach 1: Directly set camera position relative to rocket
                Vector3D lookDirection = new Vector3D(0, 0, -1);
                Vector3D upDirection = new Vector3D(0, 1, 0);

                // Calculate camera position slightly behind and above the rocket
                Point3D cameraPosition = new Point3D(
                    rocketPosition.X,
                    rocketPosition.Y + 10,  // Offset camera above the rocket
                    rocketPosition.Z + 20   // Offset camera behind the rocket
                );

                // Update camera properties
                viewPort.CameraController.CameraPosition = cameraPosition;
                viewPort.CameraController.CameraLookDirection = lookDirection;
                viewPort.CameraController.CameraUpDirection = upDirection;
            }
        }

        // Optional: Add a method to adjust camera offset
        private double cameraVerticalOffset = 10;
        private double cameraDepthOffset = 20;

        private void UpdateCameraOffset(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // You can add a slider to dynamically adjust camera offset
            cameraVerticalOffset = e.NewValue;
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
