using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler
{
    public class GRBHelper
    {
        GRBModel model;
        public GRBHelper(GRBModel model)
        {
            this.model = model;
        }
        public void ComputeIIS()
        {
            model.ComputeIIS();
            // Print the names of all of the constraints in the IIS set.
            foreach (var c in model.GetConstrs())
            {
                if (c.Get(GRB.IntAttr.IISConstr) > 0)
                {
                    Console.WriteLine(c.Get(GRB.StringAttr.ConstrName));
                }
            }

            // Print the names of all of the variables in the IIS set.
            foreach (var v in model.GetVars())
            {
                if (v.Get(GRB.IntAttr.IISLB) > 0 || v.Get(GRB.IntAttr.IISUB) > 0)
                {
                    Console.WriteLine(v.Get(GRB.StringAttr.VarName));
                }
            }
        }

        public void TuneParameters()
        {
            // Set the parameter for tuning tool
            model.Parameters.TuneResults = 10;
            model.Parameters.TuneTimeLimit = 55800;

            // Tune the model
            model.Tune();

            // Get the number of tuning results
            int resultcount = model.TuneResultCount;

            if (resultcount > 0)
            {
                for (int i = 0; i < resultcount; i++)
                {
                    // Load the tuned parameters into the model's environment
                    model.GetTuneResult(i);

                    // Write the tuned parameters to a file
                    model.Write(@"..\..\Logs\tune_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + "__" + i + ".prm");
                }

                model.Optimize();
            }
        }
    }
}
