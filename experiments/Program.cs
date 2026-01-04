using ProceduralMaze.Autoload;
using ProceduralMaze.Experiments;

// Create service container and dependencies
var services = new ServiceContainer();
var outputWriter = new OutputWriter();
var runner = new ExperimentRunner(services, outputWriter);

// Run experiments
runner.Run();
