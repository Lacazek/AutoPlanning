using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using Structure_optimisation;
using System.Windows;

// This line is necessary to "write" in database
[assembly: ESAPIScript(IsWriteable = true)]
[assembly: AssemblyVersion("2.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {
            UserInterface _interface = new UserInterface(context);
            try
            {
                context.Patient.BeginModifications();
                _interface.ShowDialog();
                _interface.IsOpened(true);
                MessageBox.Show($"AutoPlanning terminé, vérifiez la dosimétrie", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            }
            catch
            {
                _interface.IsOpened(true);
                MessageBox.Show($"Une erreur est survenue", "Erreur", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }
    }
}

