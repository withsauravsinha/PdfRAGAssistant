using PdfKnowledgeAssistant.Models;

namespace PdfKnowledgeAssistant.Services.Employee
{
    public class EmployeeLeaveService
    {
        public EmployeeLeave? GetEmployee(string employeeId)
        {
            return EmployeeMemory.Employees.FirstOrDefault(
                x => x.EmployeeId.Equals(
                    employeeId,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public bool UpdateCategory(
            string employeeId,
            string category)
        {
            var employee = GetEmployee(employeeId);

            if (employee == null)
                return false;

            employee.Category = category;

            return true;
        }
    }
}
