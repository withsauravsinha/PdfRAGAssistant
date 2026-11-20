namespace PdfKnowledgeAssistant.Models
{
    public class EmployeeLeave
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string? Category { get; set; }

        public int CasualLeaveTaken { get; set; }
        public int SickLeaveTaken { get; set; }
        public int EarnedLeaveTaken { get; set; }
    }
    public static class EmployeeMemory
    {
        public static List<EmployeeLeave> Employees = new()
    {
        new EmployeeLeave
        {
            EmployeeId = "EMP001",
            EmployeeName = "Rahul",
            Category = "T2",
            CasualLeaveTaken = 4,
            SickLeaveTaken = 2,
            EarnedLeaveTaken = 5
        },
        new EmployeeLeave
        {
            EmployeeId = "EMP002",
            EmployeeName = "Priya",
            Category = null,
            CasualLeaveTaken = 3,
            SickLeaveTaken = 1,
            EarnedLeaveTaken = 2
        },
        new EmployeeLeave
        {
            EmployeeId = "EMP003",
            EmployeeName = "Amit",
            Category = "T4",
            CasualLeaveTaken = 6,
            SickLeaveTaken = 4,
            EarnedLeaveTaken = 8
        }
    };
    }
}
