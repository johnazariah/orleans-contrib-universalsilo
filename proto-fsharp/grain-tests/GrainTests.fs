namespace Test.GeneratedProjectName

open FsCheck.Xunit
open Orleans.Contrib.UniversalSilo
open Orleans.TestingHost
open System
open System.Threading.Tasks
open GeneratedProjectName.Contract
open Xunit

/// <summary>
/// This is needed to group tests together into a fixture
/// </summary>
[<CollectionDefinition(nameof(ClusterCollection))>]
type public ClusterCollection() = class
    interface ICollectionFixture<ClusterFixture>
end

/// <summary>
/// These are the tests grouped by fixture
/// </summary>
[<Collection(nameof(ClusterCollection))>]
type public GrainTests(fixture : ClusterFixture) = class
    // create a single grain to test if you want here, or alternately create a grain in the test itself
    member val _calculatorGrain = fixture.Cluster.GrainFactory.GetGrain<ICalculatorGrain> <| Guid.NewGuid()

    /// <summary>
    /// This is a traditional unit test.
    ///
    /// Provide known inputs and check the actual result against a known expected value
    /// </summary>
    /// <returns></returns>
    [<Fact>]
    member __.KnownNumbersAreAddedCorrectly () =
        // this is an example of creating a grain within the test from using the TestCluster instance
        let adderGrain = fixture.Cluster.GrainFactory.GetGrain<ICalculatorGrain> <| Guid.NewGuid()
        let result = adderGrain.Add 1 2 |> Async.AwaitTask |> Async.RunSynchronously
        Assert.Equal(3, result)

    /// <summary>
    /// This is a property-based test.
    ///
    /// The test-runner will generate a large number of random values for
    /// <paramref name="l"/> and <paramref name="r"/> each time the test is run,
    /// and we will test a **property** of the addition operation.
    ///
    /// Mathematically, addition is the *only* operation to be
    ///     * associative
    ///     * commutative
    ///     * have an identity of zero
    /// so checking these three properties proves the addition operation is correct.
    ///
    /// In this test, we check that zero is the additive identity.
    ///
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns>`true` if the test should pass, `false` otherwise</returns>
    [<Property>]
    member this.AdditionIdentityIsZero (x : int) =
        let result = async {
            let! ``0 plus x`` = this._calculatorGrain.Add 0 x |> Async.AwaitTask
            let! ``x plus 0`` = this._calculatorGrain.Add x 0 |> Async.AwaitTask
            return (``0 plus x`` = ``x plus 0``) && (x = ``0 plus x``)
        }
        result |> Async.RunSynchronously

    /// <summary>
    /// This is a property-based test.
    ///
    /// The test-runner will generate a large number of random values for
    /// <paramref name="l"/> and <paramref name="r"/> each time the test is run,
    /// and we will test a **property** of the addition operation.
    ///
    /// Mathematically, addition is the *only* operation to be
    ///     * associative
    ///     * commutative
    ///     * have an identity of zero
    /// so checking these three properties proves the addition operation is correct.
    ///
    /// In this test, we check that the property of commutativity is satisfied
    ///
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns>`true` if the test should pass, `false` otherwise</returns>
    [<Property>]
    member this.AdditionIsCommutative(l : int, r : int) =
        let result = async {
            let! ``l plus r`` = this._calculatorGrain.Add l r |> Async.AwaitTask
            let! ``r plus l`` = this._calculatorGrain.Add r l |> Async.AwaitTask
            return ``l plus r`` = ``r plus l``
        }
        result |> Async.RunSynchronously

    /// <summary>
    /// This is a property-based test.
    ///
    /// The test-runner will generate a large number of random values for
    /// <paramref name="l"/> and <paramref name="r"/> each time the test is run,
    /// and we will test a **property** of the addition operation.
    ///
    /// Mathematically, addition is the *only* operation to be
    ///     * associative
    ///     * commutative
    ///     * have an identity of zero
    /// so checking these three properties proves the addition operation is correct.
    ///
    /// In this test, we check that the property of associativity is satisfied
    ///
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns>`true` if the test should pass, `false` otherwise</returns>
    [<Property>]
    member this.AdditionIsAssociative(x1 : int, x2 : int, x3 : int) =
        let result = async {
            let! ``x2 plus x3`` = this._calculatorGrain.Add x2 x3 |> Async.AwaitTask
            let! ``x1 plus (x2 plus x3)`` = this._calculatorGrain.Add x1 ``x2 plus x3`` |> Async.AwaitTask

            let! ``x1 plus x2`` = this._calculatorGrain.Add x1 x2 |> Async.AwaitTask
            let! ``(x1 plus x2) plus x3`` = this._calculatorGrain.Add ``x1 plus x2`` x3 |> Async.AwaitTask

            return ``x1 plus (x2 plus x3)`` = ``(x1 plus x2) plus x3``
        }
        result |> Async.RunSynchronously
end