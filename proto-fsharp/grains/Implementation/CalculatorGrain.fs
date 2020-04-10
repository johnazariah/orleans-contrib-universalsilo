namespace Template.Implementation

open Orleans
open System.Threading.Tasks
open Template.Contract

type public CalculatorGrain() = class
    inherit Grain()
    interface ICalculatorGrain with
        member this.Add l r =
            Task.FromResult (l + r)
end