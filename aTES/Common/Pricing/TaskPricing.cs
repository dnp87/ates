using System;

namespace Common.Pricing
{
    public class TaskPricing : ITaskPricing
    {
        private Random _random = new Random();
        private const int MinAssignAmount = 10;
        private const int MaxAssignAmount = 20;
        private const int MinCompletedAmount = 20;
        private const int MaxCompletedAmount = 40;

        public int GetAssignAmount()
        {
            return _random.Next(MinAssignAmount, MaxAssignAmount + 1);
        }

        public int GetCompletedAmount()
        {
            return _random.Next(MinCompletedAmount, MaxCompletedAmount + 1);
        }
    }
}
