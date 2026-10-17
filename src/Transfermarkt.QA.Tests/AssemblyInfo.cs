// Fixtures run in parallel (each test still gets its own isolated BrowserContext).
// Kept at 2 workers on purpose: the target site is bot-protected and aggressive
// parallelism from one IP increases the chance of throttling / challenges.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]
