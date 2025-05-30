using OpenCvSharp.Dnn;

namespace Infrastructure.Implementation.VehicleDetection;

public sealed class YoloDetector : IDetector
{
    private readonly Net _net;
    private readonly string[] _classNames;
    private readonly string[] _vehicleClasses = { "car", "truck", "bus" };
    private const int InputSize = 416;
    private const float ConfidenceThreshold = 0.6f;
    private const float NmsThreshold = 0.3f;

    public YoloDetector(string configPath, string weightsPath, string namesPath)
    {
        _net = CvDnn.ReadNetFromDarknet(configPath, weightsPath) ?? throw new ArgumentNullException();
        _net.SetPreferableBackend(Backend.OPENCV);
        _net.SetPreferableTarget(Target.CPU);
        _classNames = File.ReadAllLines(namesPath);
    }

    public List<Rect> Detect(Mat frame)
    {
        // Преобразуем кадр в blob
        using var blob =
            CvDnn.BlobFromImage(frame, 1 / 255.0, new Size(InputSize, InputSize), new Scalar(), true, false);
        _net.SetInput(blob);

        // Получаем имена выходных слоёв
        var outputNames = _net.GetUnconnectedOutLayersNames();
        var outputs = outputNames.Select(_ => new Mat()).ToArray();

        _net.Forward(outputs, outputNames);

        var rects = new List<Rect>();
        var confidences = new List<float>();

        // Проходим по каждому выходному слою
        foreach (var output in outputs)
        {
            int rows = output.Rows;
            int cols = output.Cols;

            for (int i = 0; i < rows; i++)
            {
                float confidence = output.At<float>(i, 4);
                if (confidence < ConfidenceThreshold)
                    continue;

                using var scores = output.Row(i).ColRange(5, cols);
                Cv2.MinMaxLoc(scores, out _, out double maxVal, out _, out Point classPoint);
                int classId = classPoint.X;
                if (classId >= _classNames.Length)
                    continue;

                string label = _classNames[classId];
                if (!_vehicleClasses.Contains(label))
                    continue;

                float centerX = output.At<float>(i, 0) * frame.Width;
                float centerY = output.At<float>(i, 1) * frame.Height;
                float width = output.At<float>(i, 2) * frame.Width;
                float height = output.At<float>(i, 3) * frame.Height;

                int left = Math.Max(0, (int)(centerX - width / 2));
                int top = Math.Max(0, (int)(centerY - height / 2));
                int w = Math.Min(frame.Cols - left, (int)width);
                int h = Math.Min(frame.Rows - top, (int)height);

                rects.Add(new Rect(left, top, w, h));
                confidences.Add(confidence);
            }

            output.Dispose();
        }

        // Применяем NMS (Non-Maximum Suppression)
        CvDnn.NMSBoxes(rects.ToArray(), confidences.ToArray(), 0.6f, 0.3f, out int[] indices);
        var result = indices.Select(i => rects[i]).ToList();
        return result;
    }
}