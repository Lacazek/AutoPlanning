using Structure_optimisation;
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Text.RegularExpressions;

namespace Opti_Struct
{
    internal class Beams
    {
        private VVector _isocenter;
        private double _gantryAngle = 10;
        private double _collimatorAngle = 10;
        public Beams()
        {
            _isocenter = new VVector();
        }

        internal void CreateBeams(UserInterfaceModel model)
        {
            try
            {
                AddPrescription(model);
                model.GetContext.PlanSetup.AddReferencePoint(true, null, "AutoPoint");
                model.GetContext.PlanSetup.ReferencePoints.First(x => x.Id.Equals("AutoPoint")).TotalDoseLimit = model.GetContext.PlanSetup.TotalDose;
                model.GetContext.PlanSetup.ReferencePoints.First(x => x.Id.Equals("AutoPoint")).DailyDoseLimit = model.GetContext.PlanSetup.DosePerFraction;
                model.GetContext.PlanSetup.ReferencePoints.First(x => x.Id.Equals("AutoPoint")).SessionDoseLimit = model.GetContext.PlanSetup.DosePerFraction;

                if (model.UserSelection[2].Contains("IMRT"))
                {
                    ExternalBeamMachineParameters BeamParameters = new ExternalBeamMachineParameters
                       (model.UserSelection[3].Contains(':') ? model.UserSelection[3].Split(':')[0] : model.UserSelection[3], "6X-FFF", 740, "STATIC", "FFF");
                    _isocenter = model.GetContext.StructureSet.Structures.First(x => new[] { "ctv sein", "ctvsein" }.Any(keyword => x.Id.ToLower().Trim().Contains(keyword))).CenterPoint;

                    ImagingBeamSetupParameters ImageParameters = new ImagingBeamSetupParameters(ImagingSetup.kVCBCT, 140, 140, 140, 140, 280, 280);

                    SetBeamAngles(model);
                    SetCollimatorAngles(model);

                    model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle,  _isocenter);
                    model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle + 10 , _isocenter);
                    model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle - 10, _isocenter);
               
                    if (model.UserSelection[1].Contains("Droit"))
                    {
                        model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle + 180, _isocenter);
                        model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle + 190, _isocenter);
                    }
                    else
                    {
                        model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle - 180, _isocenter);
                        model.GetContext.ExternalPlanSetup.AddFixedSequenceBeam(BeamParameters, _collimatorAngle, _gantryAngle - 190, _isocenter);
                    }
                    model.GetContext.ExternalPlanSetup.AddImagingSetup(BeamParameters, ImageParameters, model.GetContext.StructureSet.Structures.First(st => st.Id.Equals(model.GetContext.PlanSetup.TargetVolumeID)));

                    foreach (var (b, index) in model.GetContext.PlanSetup.Beams.Select((b, index) => (b, index)))
                    {
                        if (!b.IsSetupField)
                        b.Id = index < 3 ? "TGI " + (11 + index): "TGE " + (11 +  index-3);
                    }
                }
                else
                {
                    //_isocenter = model.GetContext.StructureSet.Structures.ToList().First(x => x.Id.ToLower().Trim().Contains("ctvsein")).CenterPoint;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur dans la création des faisceaux\n {ex.Message}");
            }
        }

        internal void SetBeamAngles(UserInterfaceModel model)
        {
            int[] angle_degree = new int[61];

            if (model.UserSelection[1].Contains("Droit"))
            {
                for (int i = 0; i < 61; i++)
                {
                    angle_degree[i] = 90 + i;
                }
            }
            else
            {
                for (int i = 0; i < 61; i++)
                {
                    angle_degree[i] = 270 + i;
                }
            }

             _gantryAngle = model.GetContext.StructureSet.Structures.ToList().First(x => x.Id.ToLower().Trim().Contains("ctvsein")).CenterPoint
            model.GetContext.Image.
                double OptimisedCollimatorAngle

            model.GetContext.PlanSetup.Beams.First().CollimatorAngleToUser(OptimisedGantryAngle);
            model.GetContext.PlanSetup.Beams.First().CollimatorAngleToUser(OptimisedCollimatorAngle);

        }

        internal void SetCollimatorAngles(UserInterfaceModel model)
        {
            //model.GetContext.Image.
        }

        internal void AddPrescription( UserInterfaceModel model)
        {
            using (StreamReader SelectedPrescription = new StreamReader(Path.Combine(model.GetPrescription, model.UserSelection[0] + ".txt")))
            {
                string firstLine = SelectedPrescription.ReadLine();
                string secondLine = SelectedPrescription.ReadLine();
                string thirdLine = SelectedPrescription.ReadLine();

                model.GetContext.PlanSetup.SetPrescription(
                    int.Parse(Regex.Match(secondLine.Split(':')[1], @"\d+").Value),
                    new DoseValue(double.Parse(Regex.Match(firstLine.Split(':')[1], @"\d+\.?\d*").Value), DoseValue.DoseUnit.Gy),
                    double.Parse(Regex.Match(thirdLine.Split(':')[1], @"\d+").Value)/100);
            }          
        }
    }
}
