using Orleans;
using System.Threading.Tasks;
using Template.Contract;

namespace Template.Implementation
{
    public class CalculatorGrain : Grain, ICalculatorGrain
    {
        public Task<int> Add(int l, int r) =>
            Task.FromResult(l + r);
    }
}
