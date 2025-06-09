namespace Infrastructure.Implementations.VehicleDetection;

public sealed class VehicleDetector(
    Net net,
    string[] classNames,
    ILogger<VehicleDetector> logger) : IVehicleDetector
{
    private static readonly string[] VehicleClasses = ["car", "truck", "bus"];

    public Result<List<Rect>> DetectVehicles(Mat frame)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(DetectVehicles), date);

        const int inputWidth = 416;
        const int inputHeight = 416;

        try
        {
            using Mat blob = CvDnn.BlobFromImage(
                frame,
                1 / 255.0,
                new(inputWidth, inputHeight),
                new(),
                true,
                false);
            net.SetInput(blob);

            string?[] outputNames = net.GetUnconnectedOutLayersNames() ??
                                    throw new Exception(Messages.GetUnconnectedOutLayersNamesFailed);

            Mat[] outputs = new Mat[outputNames.Length];
            try
            {
                for (int i = 0; i < outputs.Length; i++)
                    outputs[i] = new();

                net.Forward(outputs, outputNames!);

                List<Rect> rects = [];
                List<float> confidences = [];

                foreach (Mat output in outputs)
                {
                    for (int i = 0; i < output.Rows; i++)
                    {
                        float confidence = output.At<float>(i, 4);
                        if (confidence < 0.6) continue;

                        using Mat scores = output.Row(i).ColRange(5, output.Cols);
                        Cv2.MinMaxLoc(scores, out _, out _, out _, out Point maxLoc);
                        int classId = maxLoc.X;

                        if (classId < classNames.Length && VehicleClasses.Contains(classNames[classId]))
                        {
                            float centerX = output.At<float>(i, 0) * frame.Width;
                            float centerY = output.At<float>(i, 1) * frame.Height;
                            float width = output.At<float>(i, 2) * frame.Width;
                            float height = output.At<float>(i, 3) * frame.Height;

                            int left = Math.Max(0, (int)(centerX - width / 2));
                            int top = Math.Max(0, (int)(centerY - height / 2));
                            int boxWidth = Math.Min(frame.Cols - left, (int)width);
                            int boxHeight = Math.Min(frame.Rows - top, (int)height);

                            rects.Add(new Rect(left, top, boxWidth, boxHeight));
                            confidences.Add(confidence);
                        }
                    }
                }

                CvDnn.NMSBoxes(rects.ToArray(), confidences.ToArray(), 0.6f, 0.3f, out int[] indices);

                logger.OperationCompleted(nameof(DetectVehicles), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
                return Result<List<Rect>>.Success(indices.Select(index => rects[index]).ToList());
            }
            finally
            {
                foreach (Mat output in outputs)
                    output.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(DetectVehicles), ex.Message);
            logger.OperationCompleted(nameof(DetectVehicles), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            return Result<List<Rect>>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}