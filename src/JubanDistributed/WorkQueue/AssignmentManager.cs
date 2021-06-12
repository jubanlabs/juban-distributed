using System.Collections.Concurrent;

namespace Jubanlabs.JubanDistributed.WorkQueue
{
    public class AssignmentManager
    {
        private static readonly object initLocker = new object();
        private static readonly ConcurrentDictionary<string, Assignment> assignmentList = new ConcurrentDictionary<string, Assignment>();

        public static Assignment GetAssignment(string channelName)
        {
            lock (initLocker)
            {
                if (!assignmentList.ContainsKey(channelName))
                {
                    assignmentList[channelName]=new Assignment(channelName);
                }
                return assignmentList[channelName];
            }
        }
    }
}