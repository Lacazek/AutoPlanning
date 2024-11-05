using Opti_Struct;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace Structure_optimisation
{
    internal class CreateVolume
    {
        private string name;
        private readonly char[] filterTags;
        private StructureSet _ss;
        private Course _course;
        private Image _image;
        private StreamReader srf;
        private readonly Dictionary<char, Action<Structure, Structure>> _structure;
        private string line;
        private string _message;
        public event EventHandler<string> MessageChanged;
        private int verbose;
        private Struct_dictionnary _dictionnary;

        #region Constructeurs
        #region Constructeur 1
        public CreateVolume(StructureSet ss, Course course, Image image)
        {
            name = string.Empty;
            _ss = ss;
            _course = course;
            _image = image;
            verbose = 1;
            filterTags = new char[] {
                '|', // totalite
                '&', // intersection
                '#', // soustraction  
                '^', // union
                '!', // Marge
                '(', // parenthese ouvre
                ')', // parenthese fermee
                '/', // Suppression structure
                ',' // Marges asymétriques
				//'!' // inversion
				};
            _dictionnary = new Struct_dictionnary(_ss);
            _structure = new Dictionary<char, Action<Structure, Structure>>()
{
    { filterTags[0], (arg1, arg2) => arg1.SegmentVolume = arg1.Or(arg2)},  // Or est la totalité
	{ filterTags[1], (arg1, arg2) => arg1.SegmentVolume = arg1.And(arg2)}, // And est  l'intersection
	{ filterTags[2], (arg1, arg2) => arg1.SegmentVolume = arg1.Sub(arg2)}, // sub est la soustraction et conserve que Arg1
	{ filterTags[3], (arg1, arg2) => arg1.SegmentVolume = arg1.Xor(arg2)}, // Xor est l'union
};
            _dictionnary.MessageChanged += DictionnaryMessageChanged;
        }
        #endregion
        #region Constructeur 2
        public CreateVolume(UserInterfaceModel model)
        {
            name = string.Empty;
            _ss = model.GetContext.StructureSet;
            _course = model.GetContext.Course;
            _image = model.GetContext.Image;
            verbose = 1;
            filterTags = new char[] {
                '|', // totalite
                '&', // intersection
                '#', // soustraction  
                '^', // union
                '!', // Marge
                '(', // parenthese ouvre
                ')', // parenthese fermee
                '/', // Suppression structure
                ',' // Marges asymétriques
				//'!' // inversion
				};
            _dictionnary = new Struct_dictionnary(_ss);
            _structure = new Dictionary<char, Action<Structure, Structure>>()
{
    { filterTags[0], (arg1, arg2) => arg1.SegmentVolume = arg1.Or(arg2)},  // Or est la totalité
	{ filterTags[1], (arg1, arg2) => arg1.SegmentVolume = arg1.And(arg2)}, // And est  l'intersection
	{ filterTags[2], (arg1, arg2) => arg1.SegmentVolume = arg1.Sub(arg2)}, // sub est la soustraction et conserve que Arg1
	{ filterTags[3], (arg1, arg2) => arg1.SegmentVolume = arg1.Xor(arg2)}, // Xor est l'union
};
            _dictionnary.MessageChanged += DictionnaryMessageChanged;
        }
        #endregion
        #endregion
        private void _dictionnary_MessageChanged(object sender, string e)
        {
            throw new NotImplementedException();
        }

        internal void CreationVolume(string _userFileChoice)
        {
            #region Body creation
            Structure BODY = _ss.Structures.Where(x => x.DicomType.ToUpper().Equals("EXTERNAL")).SingleOrDefault();

            SearchBodyParameters _bodyParameters = _ss.GetDefaultSearchBodyParameters();
            _bodyParameters.KeepLargestParts = true;
            _bodyParameters.FillAllCavities = true;
            _bodyParameters.Smoothing = true;
            _bodyParameters.SmoothingLevel = 2;

            if (_userFileChoice.ToLower().Contains("sein"))
            {
                _ss.RemoveStructure(BODY);
                _bodyParameters.LowerHUThreshold = -700;
                BODY = _ss.CreateAndSearchBody(_bodyParameters);
                Message = $"Modification du contour externe ; seuil défini à : {_bodyParameters.LowerHUThreshold.ToString()} UH\n";
            }
            else if (_userFileChoice.ToLower().Contains("orl"))
            {
                _ss.RemoveStructure(BODY);
                _bodyParameters.LowerHUThreshold = -350;
                BODY = _ss.CreateAndSearchBody(_bodyParameters);
                Message = $"Modification du contour externe ; seuil défini à : {_bodyParameters.LowerHUThreshold.ToString()} UH\n";
            }
            Message = $"Aucune modification du contour externe\n";
            BODY.Color = Color.FromRgb(255, 0, 255);
            BODY.Id = "Contour externe";
            #endregion

            srf = new StreamReader(System.IO.Path.GetFullPath(_userFileChoice));
            List<string> nSplit = new List<string>();
            List<char> indexeur = new List<char>();
            List<string> _operation = new List<string>();
            line = "Start";
            Random col = new Random();

            #region Read file
            while ((line = srf.ReadLine()) != null)
            {
                name = line.Split(':')[0].Trim();
                line = line.Split(':')[1].Trim();
                byte color = (byte)col.Next(60, 256);
                nSplit.Clear();
                indexeur.Clear();
                _operation.Clear();
                Structure myStruct, _StructureInter, _StructureInter1, _StructureInter2;
                Message = $"Travail sur la structure {name}";

                try
                {
                    #region Verbose
                    if (name.ToLower().Contains("verbose"))
                    {
                        verbose = int.Parse(line);
                        Message = $"Niveau de verbose {verbose.ToString()}";
                        continue;
                    }
                    #endregion

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    #region Table
                    if (name.ToLower().Equals("table") && !_ss.Structures.Any(x => x.Id.Trim().ToLower() == "couchinterior" || x.Id.Trim().ToLower() == "couchsurface"))
                    {
                        Message = $"Travail sur la structure {name} en cours ....\n";
                        try
                        {
                            IReadOnlyList<Structure> couchStructureList = _ss.Structures.ToList();
                            bool ImageResized = false;
                            string error = "Erreur dans la création de la table";
                            double interiorHu = -1000, surfaceHu = -300;

                            if (_course.PlanSetups.Any(x => x.Beams.Any(y => y.TreatmentUnit.Id.ToLower().Contains("halcyon"))))
                            {
                                _ss.AddCouchStructures("RDS_Couch_Top", PatientOrientation.NoOrientation, RailPosition.In, RailPosition.In, null, null, null, out couchStructureList, out ImageResized, out error);
                                Message = $"Ajout de la structure Table Halcyon";
                                return;
                            }
                            else if (line.ToLower().Equals(("fine")))
                            {
                                _ss.AddCouchStructures("Exact_IGRT_Couch_Top_thin", PatientOrientation.NoOrientation, RailPosition.In, RailPosition.In, surfaceHu, interiorHu, null, out couchStructureList, out ImageResized, out error);
                                Message = $"Ajout de la structure Table Truebeam fine";
                            }
                            else if (line.ToLower().Equals("moyenne"))
                            {
                                _ss.AddCouchStructures("Exact_IGRT_Couch_Top_medium", PatientOrientation.NoOrientation, RailPosition.In, RailPosition.In, surfaceHu, interiorHu, null, out couchStructureList, out ImageResized, out error);
                                Message = $"Ajout de la structure Table Truebeam medium";
                            }
                            else if (line.ToLower().Equals("epaisse"))
                            {
                                _ss.AddCouchStructures("Exact_IGRT_Couch_Top_thick", PatientOrientation.NoOrientation, RailPosition.In, RailPosition.In, surfaceHu, interiorHu, null, out couchStructureList, out ImageResized, out error);
                                Message = $"Ajout de la structure Table Truebeam epaisse";
                            }

                            VVector Isocenter = _course.PlanSetups.SelectMany(ps => ps.Beams).Where(b => b.IsocenterPosition != null).Select(b => b.IsocenterPosition).FirstOrDefault();
                            VVector Couchposition = new VVector();
                            Couchposition.x = _image.Origin.x - Isocenter.x + 0;
                            Couchposition.y = _image.Origin.y - Isocenter.y + 0;
                            Couchposition.z = _image.Origin.z - Isocenter.z + 100; // DSA = 100 cm

                            foreach (var item in _ss.Structures)
                            {
                                if (item.Id.ToLower().Contains("couch"))
                                {
                                    item.CenterPoint.Update(VVector.Component.X, Couchposition.x);
                                    item.CenterPoint.Update(VVector.Component.Y, Couchposition.y);
                                    item.CenterPoint.Update(VVector.Component.Z, Couchposition.z);
                                }
                            }
                            //MessageBox.Show("resized?: " + ImageResized + "\nerror?:" + error);
                        }
                        catch(Exception ex) 
                        {
                            Message = $"Erreur dans la manipulation de la table de traitement\n {ex.Message}";
                        }

                        Message = $"********** Ajout de la table de traitement **********";
                        Message = $"************ Positionnement non vérifié *************";
                        Message = $"*****************************************************";
                    }
                    #endregion

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    #region Operation
                    foreach (char key in filterTags)
                    {
                        if (line.Contains(key))
                        {
                            #region Margin
                            if (key == filterTags[4]) // cas x+marge
                            {
                                string[] V = line.Split(filterTags[4]);

                                if (!_ss.Structures.Any(x => x.Id.ToLower().Trim().Equals(V[0].ToLower().Trim()))) // modif
                                {
                                    Message = $"Mauvaise nomenclature sur la structure {V[0]}";
                                    V[0] = _dictionnary.SearchStruct(V[0]);
                                }

                                Structure Struct1 = _ss.Structures.FirstOrDefault(x => x.Id.Equals(V[0].Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? _ss.Structures.FirstOrDefault(x => Regex.IsMatch(x.Id, string.Join(@".*", V[0].Trim().Select(c => Regex.Escape(c.ToString() + @"\s*"))), RegexOptions.IgnoreCase));

                                if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(name.ToLower())))
                                    if (Regex.IsMatch(name, @"\b(?:.*(?:ITV|PTV|GTV|CTV).*\b)", RegexOptions.IgnoreCase))
                                    {
                                        myStruct = _ss.AddStructure("PTV", name);
                                        myStruct.Color = Color.FromRgb(255, 0, 0);
                                    }
                                    else if (name.ToLower().Equals("externe +7") || name.ToLower().Equals("externe -7"))
                                    {
                                        myStruct = _ss.AddStructure("Control", name);
                                        myStruct.Color = Color.FromRgb(0, 255, 0);
                                    }
                                    else
                                    {
                                        myStruct = _ss.AddStructure("Control", name);
                                        myStruct.Color = Color.FromRgb(color, color, color);
                                    }
                                else if (name.ToLower() == Struct1.Id.ToLower())
                                    myStruct = Struct1;
                                else
                                    myStruct = _ss.Structures.Where(x => x.Id.ToLower().Equals(name.ToLower())).SingleOrDefault();

                                // Asymetric margin
                                if (line.Contains(filterTags[8]))
                                {
                                    float[] V_f = V[1].Split(filterTags[8]).Select(float.Parse).ToArray();

                                    AxisAlignedMargins _margin;
                                    if (V_f.Any(x => x < 0))
                                    {
                                        // dans l'ordre : x1, x2, y1, y2, z1 et z2
                                        // ordre dans la fonction :  droite, avant, bas, gauche, arrière, haut
                                        // ici droite puis gauche, arrière puis avant, bas puis haut
                                        _margin = new AxisAlignedMargins(StructureMarginGeometry.Inner, Math.Abs(V_f[0]), Math.Abs(V_f[3]), Math.Abs(V_f[4]), Math.Abs(V_f[1]), Math.Abs(V_f[2]), Math.Abs(V_f[5]));
                                    }
                                    else
                                    {
                                        // dans l'ordre : x1, x2, y1, y2, z1 et z2
                                        // ordre dans la fonction :  droite, avant, bas, gauche, arrière, haut
                                        // ici droite puis gauche, arrière puis avant, bas puis haut
                                        _margin = new AxisAlignedMargins(StructureMarginGeometry.Outer, V_f[0], V_f[3], V_f[4], V_f[1], V_f[2], V_f[5]);
                                    }
                                    myStruct.SegmentVolume = Struct1.SegmentVolume.AsymmetricMargin(_margin);
                                    Message = $"********** Operation simple **********";
                                    Message = $"Marge asymétriques de {V_f[0]} mm à droite,\n{V_f[3]} mm à gauche,\n{V_f[4]} mm en arrière,\n{V_f[1]} mm en avant,\n{V_f[2]} mm en bas,\n{V_f[5]} mm haut sur la structure : {Struct1.Id}";
                                    Message = $"Structure {name} créée";
                                }
                                else
                                {
                                    myStruct.SegmentVolume = Struct1.Margin(float.Parse(V[1]));
                                    Message = $"********** Operation simple **********";
                                    Message = $"Marge symétriques de {V[1]} mm sur la structure : {Struct1.Id}";
                                    Message = $"Structure {name} créée";
                                }
                                if (!name.ToLower().Contains("externe"))
                                    myStruct.SegmentVolume = myStruct.And(BODY);
                                continue;
                            }
                            #endregion

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region All others operations
                            else if (key != filterTags[4] && key != filterTags[5] && key != filterTags[6] && key != filterTags[7] && key != filterTags[8]) // toutes les autres opérations
                            {
                                #region multiple structures
                                string[] _parts = line.Split(filterTags, StringSplitOptions.RemoveEmptyEntries);

                                if (filterTags.Any(c => line.Count(ch => ch == c) > 1)) // plusieurs structures
                                {
                                    foreach (string part in _parts)
                                    {
                                        string part_corr = part; // modif

                                        if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(part.ToLower())))  // modif
                                        {
                                            Message = $"Mauvaise nomenclature sur la structure {part}";
                                            part_corr = _dictionnary.SearchStruct(part); // modif
                                            if (part_corr != part)
                                                Message = $"Modification du nom de la structure {part} par {part_corr}";
                                        }
                                        nSplit.Add(part_corr.Trim()); // modif
                                    }

                                    foreach (char c in line)
                                    {
                                        if (Array.IndexOf(filterTags, c) != -1)
                                        {
                                            indexeur.Add(c);
                                        }
                                    }

                                    if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(name.ToLower())))
                                        if (Regex.IsMatch(name, @"\b(?:.*(?:ITV|PTV|GTV|CTV).*\b)", RegexOptions.IgnoreCase))
                                        {
                                            _StructureInter = _ss.AddStructure("PTV", name);
                                            _StructureInter.Color = Color.FromRgb(255, 0, 0);
                                        }
                                        else
                                        {
                                            _StructureInter = _ss.AddStructure("Control", name);
                                            _StructureInter.Color = Color.FromRgb(color, color, color);
                                        }
                                    else
                                        _StructureInter = _ss.Structures.Where(x => x.Id.ToLower().Equals(name.ToLower())).SingleOrDefault();

                                    if (name == nSplit[0])
                                        _StructureInter1 = _StructureInter;
                                    else
                                        _StructureInter1 = _ss.Structures.Where(x => x.Id.ToLower().Equals(nSplit[0].Trim().ToLower())).SingleOrDefault();

                                    _StructureInter2 = _ss.Structures.Where(x => x.Id.ToLower().Equals(nSplit[1].Trim().ToLower())).SingleOrDefault();
                                    _operation.Add(nSplit[0] + " " + indexeur[0] + " " + nSplit[1]);

                                    foreach (var key2 in _structure.Keys)
                                    {
                                        if (_operation[0].Contains(key2))
                                        {
                                            _StructureInter.SegmentVolume = _StructureInter1.Margin(0.00);
                                            _structure[key2](_StructureInter, _StructureInter2);
                                        }
                                    }

                                    nSplit[0] = name.ToLower();

                                    for (int i = 2; i < nSplit.Count; i++)
                                    {
                                        _operation.Add(nSplit[0] + " " + indexeur[i - 1] + " " + nSplit[i]);
                                        foreach (var key2 in _structure.Keys)
                                        {
                                            if (_operation[i - 1].Contains(key2))
                                            {
                                                _StructureInter1 = _ss.Structures.Where(x => x.Id.ToLower().Equals(nSplit[i].ToLower())).SingleOrDefault();
                                                _structure[key2](_StructureInter, _StructureInter1);
                                            }
                                        }
                                    }
                                    _StructureInter.SegmentVolume = _StructureInter.And(BODY);
                                    Message = $"********** Operation multiple **********";
                                    Message = $"Operation sur les structures : {_StructureInter1.Id} et {_StructureInter2.Id}";
                                    Message = $"Structure {name} créée";
                                    continue;
                                }
                                #endregion

                                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                                #region 2 structures
                                else // 2 structures
                                {
                                    string[] V = line.Split(key);

                                    if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(V[0].ToLower())))  // modif
                                    {
                                        Message = $"Mauvaise nomenclature sur la structure {V[0]}";
                                        V[0] = _dictionnary.SearchStruct(V[0]);
                                    }

                                    if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(V[1].ToLower())))  // modif
                                    {
                                        Message = $"Mauvaise nomenclature sur la structure {V[1]}";
                                        V[1] = _dictionnary.SearchStruct(V[1]);
                                    }

                                    Structure Struct1 = _ss.Structures.Where(x => x.Id.ToLower().Equals(V[0].Trim().ToLower())).SingleOrDefault();
                                    Structure Struct2 = _ss.Structures.Where(x => x.Id.ToLower().Equals(V[1].Trim().ToLower())).SingleOrDefault();
                                    if (!_ss.Structures.Any(x => x.Id.ToLower().Equals(name.ToLower())))
                                        if (Regex.IsMatch(name, @"\b(?:.*(?:ITV|PTV|GTV|CTV).*\b)", RegexOptions.IgnoreCase))
                                        {
                                            myStruct = _ss.AddStructure("PTV", name);
                                            myStruct.Color = Color.FromRgb(255, 0, 0);
                                        }
                                        else
                                        {
                                            myStruct = _ss.AddStructure("Control", name);
                                            myStruct.Color = Color.FromRgb(color, color, color);
                                        }
                                    else if (name.ToLower() == Struct1.Id.ToLower())
                                        myStruct = Struct1;
                                    else
                                        myStruct = _ss.Structures.Where(x => x.Id.ToLower().Equals(name.ToLower())).SingleOrDefault();
                                    myStruct.SegmentVolume = Struct1.Margin(0.00);
                                    _structure[key](myStruct, Struct2);
                                    myStruct.SegmentVolume = myStruct.And(BODY);
                                    Message = $"********** Operation simple **********";
                                    Message = $"Operation complexe {key}";
                                    Message = $"Operation sur les structures : {Struct1.Id} et {Struct2.Id}";
                                    Message = $"Structure {name} créée";
                                }
                                continue;
                                #endregion
                            }
                            #endregion

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Remove structure
                            else if (key == filterTags[7])
                            {
                                //_ss.RemoveStructure(_ss.Structures.First(x => x.Id.ToLower().Contains("test")));
                                _ss.RemoveStructure(_ss.Structures.FirstOrDefault(x => Regex.IsMatch(x.Id, @"\bt\s*e\s*s\s*t*[_\s]*(?:[1-9]|10)?.*?(?!\bintestin\b)", RegexOptions.IgnoreCase)));
                                Message = $"********** Operation simple **********";
                                Message = $"Suppression de structure";
                                Message = $"Structure {name} supprimée";
                                continue;
                            }
                            #endregion
                        }

                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        #region Create empty structure
                        else if (line.Length == 0) // Créé une structure vide
                        {
                            if (Regex.IsMatch(name, @"\b(?:.*(?:ITV|PTV|GTV|CTV).*\b)", RegexOptions.IgnoreCase))
                            {
                                myStruct = _ss.AddStructure("PTV", name);
                                myStruct.Color = Color.FromRgb(255, 0, 0);
                            }
                            else
                            {
                                myStruct = _ss.AddStructure("Control", name);
                                myStruct.Color = Color.FromRgb(color, color, color);
                            }
                            Message = $"********** Operation simple **********";
                            Message = $"Création de structure";
                            Message = $"Structure {name} créée";
                            continue;
                        }
                        #endregion

                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    if (verbose >= 1)
                        MessageBox.Show($"Une erreur est survenue sur la structure {name} : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    Message = $"Une erreur est survenue sur la structure {name} : " + ex.Message;
                }
                Message = "\n";
            }
            #endregion

            if (verbose >= 0)
                MessageBox.Show("Les structures ont été créées.\nMerci de les vérifier !", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            srf.Close();
        }

        #region Message
        internal string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnMessageChanged();
            }
        }
        private void DictionnaryMessageChanged(object sender, string e)
        {
            Message = e;
        }
        protected virtual void OnMessageChanged()
        {
            MessageChanged?.Invoke(this, _message);
        }
        #endregion
    }
}