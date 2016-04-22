using System.Collections.Generic;

namespace RallyKnowledgeOwlIntegration.Models
{
    public class RallyArtifactsByState
    {
        public List<RallyArtifact> Backlog { get; set; }
        public List<RallyArtifact> PreviousIterations { get; set; }
        public List<RallyArtifact> CurrentIteration { get; set; }
    }
}