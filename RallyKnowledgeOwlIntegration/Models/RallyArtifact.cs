using System;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class RallyArtifact
    {
        public string Name { get; set; }
        public string FormattedId { get; set; }
        public string ScheduleState { get; set; }
        public string KanbanState { get; set; }
        public string Priority { get; set; }
        public string c_PriorityUS { get; set; }
        public string IterationName { get; set; }

        // Calculated fields
        public string Status { get; set; }
        public DateTime? TargetDate { get; set; }
    }
}
