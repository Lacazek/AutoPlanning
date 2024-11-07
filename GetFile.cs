using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using System.Reflection;
using System.Linq;

namespace Structure_optimisation
{

    internal class GetFile : INotifyPropertyChanged
    {
        private string _userFileChoice;
        private CreateVolume _createVolume;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<string> MessageChanged;
        private string _path;
        private string _prescriptionPath;
        private string _message;


        public GetFile(StructureSet ss, Course course, Image image)
        {
            _userFileChoice = string.Empty;
            _createVolume = new CreateVolume(ss, course, image);
            _path = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString(), @"File\Prescription");
            _createVolume.MessageChanged += VolumeMessageChanged;
        }

        public GetFile(UserInterfaceModel model)
        {
            _userFileChoice = string.Empty;
            _createVolume = new CreateVolume(model);
            _path = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString(), @"File");
            _prescriptionPath = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString(), @"File\Prescription");
            _createVolume.MessageChanged += VolumeMessageChanged;
        }

        internal void CreateUserFile(UserInterfaceModel model)
        {
            try
            {
                string directoryPath = Path.Combine(_path, @"Volume");
                string treatmentType = model.UserSelection[2].ToUpper();
                if (treatmentType == "ARCTHÉRAPIE" || treatmentType == "DYNAMIC ARC" || treatmentType == "STÉRÉOTAXIE")
                {
                    treatmentType = "VMAT"; // Standardiser en "VMAT" pour ces choix
                }
                string[] parts = {model.UserSelection[0].Split('_').LastOrDefault().ToUpper(),
                model.UserSelection[1].ToUpper(),
                treatmentType,
                model.UserSelection[3].ToUpper()
                };


                string CorrectStructureFile = Directory.GetFiles(directoryPath, "*.txt")
        .Select(file => Path.GetFileNameWithoutExtension(file)).FirstOrDefault(fileNameWithoutExtension =>
        {
            var segments = fileNameWithoutExtension.Split('_');

            return segments.Any(segment => segment.ToUpper().Equals(parts[0], StringComparison.OrdinalIgnoreCase)) &&
                   fileNameWithoutExtension.ToUpper().Contains(parts[1]) &&
                   fileNameWithoutExtension.ToUpper().Contains(parts[2]);
        });

                _userFileChoice = Path.Combine(directoryPath, CorrectStructureFile + ".txt"); ;
                Message = $"Fichier choisi : {_userFileChoice} \n";
                _createVolume.CreationVolume(_userFileChoice);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Message = $"Une erreur est survenue : {ex.Message} \n";
            }
        }

        #region Get and Set
        internal string UserFile
        {
            get { return _userFileChoice; }
            set
            {
                _userFileChoice = value;
                OnPropertyChanged(nameof(UserFile));
            }
        }
        internal string GetPath
        {
            get { return _path; }
        }
        internal string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnMessageChanged();
            }
        }
        internal string GetPrescription
        {
            get { return _prescriptionPath; }
        }
        internal CreateVolume GetVolume
        {
            get { return _createVolume; }
        }

        #region Update log file
        private void VolumeMessageChanged(object sender, string e)
        {
            Message = e;
        }
        protected virtual void OnMessageChanged()
        {
            MessageChanged?.Invoke(this, _message);
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_createVolume.Message));
        }
        #endregion
        #endregion
    }
}
