// Only 2 workers on purpose: the site is bot-protected, and more parallel traffic from one IP risks throttling.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]
