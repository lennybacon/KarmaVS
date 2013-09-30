using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace devcoach.Tools
{
    public static class Extensions
    {
        public static string GetProjectTypeGuids(
         this Project proj,
         Func<Type, object> getService)
        {

            string projectTypeGuids = null;
            IVsHierarchy hierarchy;
            var result = 0;

            var service = getService(typeof(IVsSolution));
            var solution = (IVsSolution)service;

            result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

            if (result == 0)
            {
                var aggregatableProject = (IVsAggregatableProject)hierarchy;
                aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
            }

            return projectTypeGuids.ToUpperInvariant();

        }
    }
}
