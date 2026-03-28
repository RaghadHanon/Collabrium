using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ICourseAssignmentsRepository
    {
        Task AddAsync(CourseAssignment assignment, CancellationToken ct = default);

        Task<CourseAssignment?> GetByIdAsync(
            string tenantId,
            string assignmentId,
            CancellationToken ct = default);

        Task<CourseAssignment?> GetByCourseAndUserAsync(
            string tenantId,
            string courseId,
            string assignedToUserId,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string tenantId,
            string courseId,
            string assignedToUserId,
            CancellationToken ct = default);

        Task UpdateAsync(CourseAssignment assignment, CancellationToken ct = default);

        Task<bool> RemoveAsync(
            string tenantId,
            string assignmentId,
            CancellationToken ct = default);
    }
}
