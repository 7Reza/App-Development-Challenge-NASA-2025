using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace SolarSystem3D
{
    public partial class MainWindow : Window
    {
        private PerspectiveCamera _camera;
       /// <summary>
       /// private double _orbitSpeed;
       /// </summary>
        ///private double _cameraDistance;
        ///private double _cameraAngleX;
        ///private double _cameraAngleY;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _camera = viewport3D.Camera as PerspectiveCamera;
            if (_camera == null)
            {
                MessageBox.Show("Error: Camera could not be initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ///InitializeControls();
        }

        ///private void InitializeControls()
        ///{
   ////         if (speedSlider != null)
   ///             speedSlider.ValueChanged += (s, e) => _orbitSpeed = speedSlider.Value;
   ///
      ///      if (cameraDistanceSlider != null)
         ///       cameraDistanceSlider.ValueChanged += (s, e) => _cameraDistance = cameraDistanceSlider.Value;

///            if (cameraAngleXSlider != null)
   ///             cameraAngleXSlider.ValueChanged += (s, e) => _cameraAngleX = cameraAngleXSlider.Value;

      ///      if (cameraAngleYSlider != null)
         ///       cameraAngleYSlider.ValueChanged += (s, e) => _cameraAngleY = cameraAngleYSlider.Value;

           /// if (resetButton != null)
              ///  resetButton.Click += (s, e) => ResetSettings();
        ///}

///        private void ResetSettings()
      ///  {
   ///         _orbitSpeed = 1.0;
      ///      _cameraDistance = 10.0;
         ///   _cameraAngleX = 0;
           /// _cameraAngleY = 0;

///            if (speedSlider != null) speedSlider.Value = _orbitSpeed;
   ///         if (cameraDistanceSlider != null) cameraDistanceSlider.Value = _cameraDistance;
      ///      if (cameraAngleXSlider != null) cameraAngleXSlider.Value = _cameraAngleX;
         ///   if (cameraAngleYSlider != null) cameraAngleYSlider.Value = _cameraAngleY;
    ///    }
    }
}
