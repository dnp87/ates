using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Pricing
{
    /// <summary>
    /// Task pricing module interface.
    /// </summary>
    public interface ITaskPricing
    {
        /// <summary>
        /// Get assign fee reduced from parrot's account on task assign
        /// </summary>
        /// <returns></returns>
        int GetAssignAmount();

        /// <summary>
        /// Get completion amount added to parrot's account on task completion
        /// </summary>
        /// <returns></returns>
        int GetCompletedAmount();
    }
}
