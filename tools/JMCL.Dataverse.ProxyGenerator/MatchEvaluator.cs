using System.Collections.Generic;
using JMCL.Dataverse.ProxyGenerator.Extensions;

namespace JMCL.Dataverse.ProxyGenerator
{
    public class MatchEvaluator : IMatchEvaluator
    {
        private readonly List<string> MatchCandidates;

        public MatchEvaluator(IEnumerable<string> matchCandidates)
        {
            this.MatchCandidates = matchCandidates == null ? new List<string>() : new List<string>(matchCandidates);
        }

        /// <summary>
        /// Compares the passed in value against the evaluator candidates
        /// and returns a match score as follows:
        /// 
        /// 3 = Exact Match
        /// 2 = RegEx Pattern Match
        /// 1 = Wild Card Match
        /// 0 = No Match
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ScoreMatch(string value)
        {
            if (MatchCandidates.Contains(value))
                return 3;

            if (MatchCandidates.HasRegExMatch(value))
                return 2;

            if (MatchCandidates.Contains("*"))
                return 1;

            return 0;
        }
    }
}
