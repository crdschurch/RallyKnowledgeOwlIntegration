namespace RallyKnowledgeOwlIntegration
{
    public class RallyArtifact
    {
        public string Name { get; set; }
        public string FormattedID { get; set; }
        public string ScheduleState { get; set; }
        public string c_CrossroadsKanbanState { get; set; }
        public string Priority { get; set; }
        public string c_PriorityUS { get; set; }
        public RallyArtifactIteration Iteration { get; set; }

        public string Status { get; set; }
    }
}
