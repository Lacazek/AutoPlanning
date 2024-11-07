using System;
using System.Collections.Generic;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Opti_Struct;


namespace Structure_optimisation
{

    internal class UserInterfaceModel : INotifyPropertyChanged
    {
        private ScriptContext _context;
        private List<string> _userSelection;


        private string _userChoice;
        private string _rename;
		private readonly string _fisherMan;
		private GetFile _file;
        private StreamWriter _logFile;

        private Beams _beams;
        private Dosimetry _dosimetry;
        private CreateVolume _createVolume;

        public event PropertyChangedEventHandler PropertyChanged;

        public UserInterfaceModel(ScriptContext context)
        {
            _userChoice = string.Empty;
            _rename = string.Empty;
            _context = context;

            _beams = new Beams();
            _dosimetry = new Dosimetry();

            _userSelection = new List<string>();
            _file = new GetFile(this);
            _fisherMan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString(), "fisherMan4.png");

            FileInfo _fileinfo = new FileInfo(@"LogFile.txt");
            if (_fileinfo.Exists && _fileinfo.Length > 500 * 1000)
                _fileinfo.Delete();
            _logFile = new StreamWriter(@"LogFile.txt", true);

            _file.MessageChanged += MessageChanged;
            Message = $"\n**********************************";
            Message = $"Debut de programme : {DateTime.Now}";
            Message = $"Ordinateur utilisé : {Environment.MachineName}";
            Message = $"OS : {Environment.OSVersion}";
            Message = $"Domaine windows : {Environment.UserDomainName}";
            Message = $"Dossier de travail : {Environment.SystemDirectory}";
            Message = $"Taille du jeu de travail : {Environment.WorkingSet}";
            Message = $"User : {Environment.UserName}\n";
            Message = $"Fichier ouvert\n";
        }

        internal void LaunchPlanning()
        {
            // 1 Création des contours --> getFile-> CreateVolume
            _file.CreateUserFile(this);
            // 2 Création des faisceaux --> Beams
            _beams.CreateBeams(this);
            //3 Paramétrages de la dosi --> Dosimetry
            _dosimetry.LaunchDosimetry(this);
        }

        #region get and set
        internal ScriptContext GetContext
        {
            get { return _context; }
        }
        internal string UserChoice
        {
            get { return _userChoice; }
        }
        internal string GetPrescription
        {
            get { return _file.GetPrescription; }
        }
        internal string Rename
        {
            get { return _rename; }
        }
        internal string Fisherman
        {
            get { return _fisherMan; }
        }
        internal GetFile File
        {
            get { return _file; }
        }
        internal string UserFile
        {
            get { return _file.UserFile; }
            set { _file.UserFile = value; }
        }
        internal string UserPath
        {
            get { return _file.GetPath; }
        }
        internal List<string> UserSelection
        {
            get { return _userSelection; }
            set { _userSelection.AddRange(value);
                _logFile.WriteLine($"Prescription : {_userSelection[0]}\nCôté : {_userSelection[1]}\nTechnique : {_userSelection[2]}\nMachine : {_userSelection[3]}\n");}
        }
        #endregion

        #region update message
        internal string Message
        {
            get { return _file.Message; }
            set
            {
                _logFile.WriteLine(value);
                _logFile.Flush();
                OnPropertyChanged(nameof(_file.Message));
            }
        }
        internal void IsOpened(bool test)
        {
            if (test == true)
            {
                _logFile.WriteLine($"Fichier Log fermé");
                _logFile.WriteLine($"Fin du programme : {DateTime.Now}");
                _logFile.WriteLine($"***************************Script terminé***************************");
                _logFile.Close();
                }
        }

        private void MessageChanged(object sender, string e)
        {
            _logFile.WriteLine(_file.Message);
            _logFile.Flush();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

