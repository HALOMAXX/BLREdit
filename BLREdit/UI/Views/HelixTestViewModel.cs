using HelixToolkit.Wpf.SharpDX;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLREdit.UI.Views
{
    public class HelixTestViewModel : INotifyPropertyChanged
    {
        public EffectsManager EffectsManager { get; }
        public Camera Camera { get; }
        public Geometry3D CubeMesh { get; }
        public Material Red { get; }

        public HelixTestViewModel()
        { 
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera();
            var builder = new MeshBuilder();
            builder.AddCube();
            CubeMesh = builder.ToMesh();
            Red = PhongMaterials.Red;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        protected bool Set<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(backingField, value))
            {
                return false;
            }
            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
