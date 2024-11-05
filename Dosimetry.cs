using Structure_optimisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace Opti_Struct
{
    internal class Dosimetry
    {
        public Dosimetry()
        {
        }

        internal void LaunchDosimetry(UserInterfaceModel model)
        {

            if (model.UserSelection[2].Contains("HALCYON"))
            {
                ConstructMyOptimizer(model);
                //model.GetContext.PlanSetup.SetCalculationOption("PHOTONS_AAA_18.0.1",,);

            }
            else
            {
                //model.GetContext.PlanSetup.SetCalculationOption("PHOTONS_AAA_15.6.06 V2",,);

            }
        }


        internal void ConstructMyOptimizer(UserInterfaceModel model)
        {
            switch (model.UserSelection[0].ToUpper())
            {
                case "SEIN":
                    foreach (var beam in model.GetContext.PlanSetup.Beams)
                    {
                        model.GetContext.PlanSetup.OptimizationSetup.AddBeamSpecificParameter(beam, 200, 200, false);
                        //model.GetContext.PlanSetup.OptimizationSetup.AddEUDObjective(;
                    }
                    break;

                case "PROSTATE":
                    //ghgh
                    break;

                case "":
                    break;

                default:
                    MessageBox.Show("Erreur lors de la construction de l'optimiseur");
                    break;
            }


        }
    }
}
