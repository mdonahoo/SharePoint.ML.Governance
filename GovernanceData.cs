using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.ML.Data;

namespace SharePoint.ML.Governance
{
    public class GovernanceData
    {
        [Column("0"),ColumnName("Tenant"),LoadColumn(0)]
        public string Tenant;
        [Column("1"), ColumnName("Track"), LoadColumn(1)]
        public string Track;
        [Column("2"), ColumnName("Age"), LoadColumn(2)]
        public float Age;
        [Column("3"), ColumnName("Security"), LoadColumn(3)]
        public float Security;
        [Column("4"), ColumnName("Activity"), LoadColumn(4)]
        public float Activity;
        [Column("5"), ColumnName("Protection"), LoadColumn(5)]
        public float Protection;
        [Column("6"), ColumnName("Usage"), LoadColumn(6)]
        public float Usage;
        [Column("7"), ColumnName("Change"), LoadColumn(7)]
        public float Change;
        [Column("8"), ColumnName("GoodBad"), LoadColumn(8)]
        public bool GoodBad;
        [Column("9"), ColumnName("Description"), LoadColumn(9)]
        public string Description;

        //Tenant,Track,Age,Security,Activity,Protection,Usage,Change,GoodBad,Description
    }
}
