using OpenCvSharp.Dnn;

namespace Infrastructure.Implementations.VehicleDetection;

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
            if (!File.Exists(configPath) || !File.Exists(weightsPath) || !File.Exists(namesPath))
                throw new FileNotFoundException("Model files or coco.names not found.");

            _net = CvDnn.ReadNetFromDarknet(configPath, weightsPath) ?? throw new InvalidOperationException("Failed to load model.");
            _net.SetPreferableBackend(Backend.OPENCV);
            _net.SetPreferableTarget(Target.CPU);
            _classNames = File.ReadAllLines(namesPath);
        }

        public Result<List<Rect>> Detect(Mat frame)
        {
            try
            {
                using var blob = CvDnn.BlobFromImage(frame, 1 / 255.0, new Size(InputSize, InputSize), new Scalar(), true, false);
                _net.SetInput(blob);

                string[] outputNames = _net.GetUnconnectedOutLayersNames()!;
                Mat[] outputs = outputNames.Select(_ => new Mat()).ToArray();
                _net.Forward(outputs, outputNames);

                List<Rect> rects = [];
                List<float> confidences = [];

                foreach (var output in outputs)
                {
                    for (int i = 0; i < output.Rows; i++)
                    {
                        float confidence = output.At<float>(i, 4);
                        if (confidence < ConfidenceThreshold)
                            continue;

                        using var scores = output.Row(i).ColRange(5, output.Cols);
                        Cv2.MinMaxLoc(scores, out _, out _, out _, out Point classPoint);
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

                CvDnn.NMSBoxes(rects.ToArray(), confidences.ToArray(), ConfidenceThreshold, NmsThreshold, out int[] indices);
                var result = indices.Select(i => rects[i]).ToList();
                return Result<List<Rect>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<Rect>>.Failure(ResultPatternError.InternalServerError($"Detection error: {ex.Message}"));
            }
        }
    }