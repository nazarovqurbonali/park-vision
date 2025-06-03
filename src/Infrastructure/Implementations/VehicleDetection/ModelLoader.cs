namespace Infrastructure.Implementations.VehicleDetection;

public sealed class ModelLoader(
    ILogger<ModelLoader> logger) : IModelLoader
{
    public Result<Net> LoadModel(string configPath, string weightsPath)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(LoadModel), date);

        if (!File.Exists(configPath) || !File.Exists(weightsPath))
        {
            logger.LogCritical(Messages.ModelLoaderLoadModelNotFound);
            logger.OperationCompleted(nameof(LoadModel), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<Net>.Failure(ResultPatternError.NotFound(Messages.ModelLoaderLoadModelNotFound));
        }

        try
        {
            Net? net = CvDnn.ReadNetFromDarknet(configPath, weightsPath);
            if (net is null)
                return Result<Net>.Failure(ResultPatternError.BadRequest(Messages.ModelLoaderLoadModelNotLoaded));

            logger.OperationCompleted(nameof(LoadModel), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<Net>.Success(net);
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(LoadModel), ex.Message);
            logger.OperationCompleted(nameof(LoadModel), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<Net>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }

    public Result<string[]> LoadClassNames(string namesPath)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(LoadClassNames), date);


        if (!File.Exists(namesPath))
        {
            logger.LogCritical(Messages.ModelLoaderLoadClassNamesNotFound);
            logger.OperationCompleted(nameof(LoadClassNames), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<string[]>.Failure(ResultPatternError.NotFound(Messages.ModelLoaderLoadClassNamesNotFound));
        }

        try
        {
            string[] classNames = File.ReadAllLines(namesPath);
            if (classNames.Length == 0)
                return Result<string[]>.Failure(
                    ResultPatternError.BadRequest(Messages.ModelLoaderLoadClassNamesNotLoaded));

            logger.OperationCompleted(nameof(LoadClassNames), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<string[]>.Success(classNames);
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(LoadClassNames), ex.Message);
            logger.OperationCompleted(nameof(LoadClassNames), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<string[]>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}