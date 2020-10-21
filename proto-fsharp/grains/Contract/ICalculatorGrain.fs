namespace GeneratedProjectName.Contract

open Orleans
open System.Threading.Tasks

type public ICalculatorGrain = interface
    inherit IGrainWithGuidKey
    /// <summary>
    /// Adds two integers
    /// </summary>
    /// <param name="l">Integer to Add</param>
    /// <param name="r">Integer to Add</param>
    /// <returns>Sum of <see param="l"/> and <see param="r"/></returns>
    abstract Add : int -> int -> Task<int>
end
