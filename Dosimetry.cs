using Structure_optimisation;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Opti_Struct
{
    internal class Dosimetry
    {
        public Dosimetry()
        {
        }

        internal void LaunchDosimetry(UserInterfaceModel model)
        {
            Structure CalculationVolume = model.GetContext.StructureSet.Structures.First();
            CalculationVolume.Margin(model.GetContext.Image.XRes);
            model.GetContext.StructureSet.RemoveStructure(CalculationVolume);

            if (model.UserSelection[3].ToUpper().Contains("HALCYON"))
            {
                ConstructMyOptimizer(model);
                model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonVolumeDose, "AAA_18.0.1");
                model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonOptimization, "PO_18.0.0");
                //model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonVolumeDose, "PHOTONS_AAA_18.0.1");
                //model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonOptimization, "PO_18.0.1");

            }
            else
            {
                ConstructMyOptimizer(model);
                model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonVolumeDose, "PHOTONS_AAA_15.6.06 V2");
                model.GetContext.PlanSetup.SetCalculationModel(CalculationType.PhotonOptimization, "PO_15.6.06");
            }
            if (model.UserSelection[2].ToUpper().Equals("IMRT"))
            {
                //model.GetContext.ExternalPlanSetup.Optimize(500, OptimizationOption.ContinueOptimizationWithPlanDoseAsIntermediateDose, "SX2 MLC");
                model.GetContext.ExternalPlanSetup.Optimize(2);
                model.GetContext.ExternalPlanSetup.CalculateLeafMotionsAndDose();
            }
            else
            {
                model.GetContext.ExternalPlanSetup.OptimizeVMAT();
                model.GetContext.ExternalPlanSetup.CalculateDose();
            }
        }

        // Opti OK sur CE et CTV dosi, voir les autres
        internal void ConstructMyOptimizer(UserInterfaceModel model)
        {
            switch (model.UserSelection[2])
            {

                case "IMRT":
                    switch (model.UserSelection[0].Split('_').LastOrDefault().ToUpper())
                    {
                        case "SEIN":

                            // Paramètres faisceaux
                            foreach (var beam in model.GetContext.PlanSetup.Beams)
                            {
                                if (!beam.IsSetupField)
                                    model.GetContext.PlanSetup.OptimizationSetup.AddBeamSpecificParameter(beam, 200, 200, false);
                            }

                            // NTO
                            model.GetContext.PlanSetup.OptimizationSetup.AddNormalTissueObjective(300, 3, 100, 30, 0.1);

                            // Cible
                            // CTV dosi
                            try
                            {

                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CTV_DOSI")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 1.02, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 2, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CTV_DOSI")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 50, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 50, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 99, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 100, 400);
                            }
                            catch { }

                            // PTV dosi
                            try
                            {
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("PTV_DOSI")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 1.02, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 2, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("PTV_DOSI")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 50, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("PTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 50, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("PTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 99, 400);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("PTV_DOSI")).First(), OptimizationObjectiveOperator.Lower, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 100, 400);
                            }
                            catch { }

                            // OAR
                            // Sein contro
                            try
                            {
                                model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("SEIN CONTRO)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(1, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 170);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("SEIN CONTRO")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(2, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 170);
                            }
                            catch { }

                            // CE
                            try
                            {
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("CONTOUR EXTERNE")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 1.02, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 1000);
                            }
                            catch { }

                            // Ring sein D
                            try
                            {
                                model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("RING SEIN)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(35, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 150);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("RING SEIN")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.95, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 180);
                                model.GetContext.PlanSetup.OptimizationSetup.AddEUDObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("RING SEIN")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.9, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 40, 180);
                            }
                            catch { }

                            // CE - (PTV +1cm)
                            try
                            {
                                model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("CE -(PTV + 1CM)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(25, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 150);
                                model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("CE- (PTV + 1CM)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.9, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 180);
                                model.GetContext.PlanSetup.OptimizationSetup.AddEUDObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Contains("CE -(PTV + 1CM)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.72, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 40, 180);
                            }
                            catch { }

                            switch (model.UserSelection[1].ToUpper())
                            {
                                case "DROIT":

                                    // Coeur
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("COEUR)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(0.5, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 120);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("COEUR)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(3, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 120);
                                    }
                                    catch { }

                                    // Poumon D
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(3, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 120);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.95, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.90, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 2, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.7, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 5, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.40, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 10, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.2, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 25, 200);
                                    }
                                    catch { }

                                    // Poumon G
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(1, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 120);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(2, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 120);
                                    }
                                    catch { }
                                    break;

                                case "GAUCHE":

                                    // Coeur
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("COEUR)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(1, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 150);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("COEUR)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(20, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 220);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddEUDObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("COEUR)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(20, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 40, 200);
                                    }
                                    catch { }

                                    try
                                    {
                                        // Poumon G
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(3, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 120);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.95, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.90, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 2, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.7, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 5, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.40, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 10, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_G")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(model.GetContext.PlanSetup.TotalDose.Dose * 0.2, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 25, 200);
                                    }
                                    catch { }

                                    // Poumon D
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(1, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 120);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("POUMON_D")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(2, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 120);
                                    }
                                    catch { }

                                    // IVA
                                    try
                                    {
                                        model.GetContext.PlanSetup.OptimizationSetup.AddMeanDoseObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("IVA)")).First(), new VMS.TPS.Common.Model.Types.DoseValue(1, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 150);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddPointObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("IVA)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(5, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 0, 200);
                                        model.GetContext.PlanSetup.OptimizationSetup.AddEUDObjective(model.GetContext.StructureSet.Structures.Where(x => x.Id.ToUpper().Equals("IVA)")).First(), OptimizationObjectiveOperator.Upper, new VMS.TPS.Common.Model.Types.DoseValue(5, VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy), 40, 200);
                                    }
                                    catch { }
                                    break;
                            }
                            break;

                        default:
                            MessageBox.Show("Erreur lors de la construction de l'optimiseur");
                            break;
                    }
                    break;

                case "ARCTHERAPIE":

                    switch (model.UserSelection[0].Split('_').LastOrDefault().ToUpper() + " " + model.UserSelection[1].ToUpper())
                    {
                        case "PROSTATE":

                            break;


                        case "":

                            model.GetContext.PlanSetup.OptimizationSetup.AddNormalTissueObjective(300, 3, 100, 30, 0.1);
                            break;
                    }
                    break;

                case "3D":
                    switch (model.UserSelection[0].Split('_').LastOrDefault().ToUpper() + " " + model.UserSelection[1].ToUpper())
                    {
                        case "PROSTATE":

                            break;


                        case "":

                            model.GetContext.PlanSetup.OptimizationSetup.AddNormalTissueObjective(300, 3, 100, 30, 0.1);
                            break;
                    }
                    break;

                case "STEREOTAXIE":
                    switch (model.UserSelection[0].Split('_').LastOrDefault().ToUpper() + " " + model.UserSelection[1].ToUpper())
                    {
                        case "PROSTATE":

                            break;


                        case "":

                            model.GetContext.PlanSetup.OptimizationSetup.AddNormalTissueObjective(300, 3, 100, 30, 0.1);
                            break;
                    }
                    break;
                case "DYNAMIC ARC":

                    switch (model.UserSelection[0].Split('_').LastOrDefault().ToUpper() + " " + model.UserSelection[1].ToUpper())
                    {
                        case "PROSTATE":

                            break;


                        case "":

                            model.GetContext.PlanSetup.OptimizationSetup.AddNormalTissueObjective(300, 3, 100, 30, 0.1);
                            break;
                    }
                    break;
            }

        }
    }
}
