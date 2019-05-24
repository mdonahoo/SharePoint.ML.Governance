using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharePoint.ML.Governance
{
    public class GovernancePrediction : GovernanceData
    {
        [ColumnName("PredictedGovernance"),LoadColumn(0)]
        public bool PredictedGovernance;


        [ColumnName("Probability"), LoadColumn(0)]
        public float Probability { get; set; }

        [ColumnName("Score"), LoadColumn(0)]
        public float Score { get; set; }
    }
}
