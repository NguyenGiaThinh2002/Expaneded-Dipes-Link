using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.Views.Extension
{
    public class TimeBaseExecution
    {
        private DateTime lastExecutionTime;
        private readonly TimeSpan minimumInterval;
        public TimeBaseExecution(TimeSpan interval)
        {
            lastExecutionTime = DateTime.MinValue;
            minimumInterval = interval;
        }
        public void ExecuteActionIfAllowed(Action action) // Accept the interval level
        {
            DateTime now = DateTime.Now;
            if (now - lastExecutionTime >= minimumInterval)
            {
                action();
                lastExecutionTime = now;
            }
            else
            {
                // Do nothing
            }
        }
    }
}
