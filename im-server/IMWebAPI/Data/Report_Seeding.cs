﻿using IMWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMWebAPI.Data
{
    public class Report_Seeding
    {
        public static void SeedReports(IM_API_Context context)
        {
            // Look for any existing reports.
            if (context.Reports.Any())
            {
                return;   // DB has been seeded
            }

            var reports = new Report[]
            {
                new Report
                {
                    ReportName = "Students Grouped by Total Observation Severity",
                    Query = "SELECT studentName, SUM(severity) AS severity FROM Observations GROUP BY studentName"
                }
            };

            context.Reports.AddRange(reports);
            context.SaveChanges();
        }
    }
}
