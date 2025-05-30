namespace Infrastructure.Implementations.VehicleDetection;

public class VideoProcessor(
    IVideoSource videoSource,
    IDetector detector,
    ITracker tracker,
    ISaver saver
)
{
    public void ProcessVideo()
    {
        var openResult = videoSource.Open();
        if (!openResult.IsSuccess || !openResult.Value)
        {
            Console.WriteLine("Не удалось открыть видеоисточник: " + openResult.Error.Message);
            return;
        }

        int frameCount = 0;
        while (true)
        {
            var readResult = videoSource.ReadFrame(out Mat frame);
            if (!readResult.IsSuccess)
            {
                Console.WriteLine("Ошибка чтения кадра: " + readResult.Error.Message);
                break;
            }

            if (!readResult.Value) // Кадры закончились
            {
                Console.WriteLine("Конец видео.");
                break;
            }

            using (frame) // Автоматически освобождаем кадр после использования
            {
                // Шаг 3: Детекция объектов
                Result<List<Rect>> detectResult = detector.Detect(frame);
                if (detectResult.IsSuccess)
                {
                    List<Rect> detections = detectResult.Value!;

                    // Шаг 4: Обновление трекера
                    var updateResult = tracker.Update(detections, frame, frameCount);
                    if (updateResult.IsSuccess)
                    {
                        // Шаг 5: Сохранение объектов (пример: сохраняем первый объект)
                        if (detections.Count > 0)
                        {
                            var saveResult = saver.Save(frameCount, detections[0], frame, frameCount);
                            if (!saveResult.IsSuccess)
                            {
                                Console.WriteLine("Ошибка сохранения: " + saveResult.Error.Message);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка обновления трекера: " + updateResult.Error.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Ошибка детекции: " + detectResult.Error.Message);
                }

                // Шаг 6: Очистка устаревших объектов
                var cleanupResult = tracker.Cleanup(frameCount);
                if (!cleanupResult.IsSuccess)
                {
                    Console.WriteLine("Ошибка очистки трекера: " + cleanupResult.Error.Message);
                }

                frameCount++;
            }
        }

        // Шаг 7: Освобождение ресурсов
        BaseResult releaseResult = videoSource.Release();
        if (!releaseResult.IsSuccess)
        {
            Console.WriteLine("Ошибка при освобождении видеоисточника: " + releaseResult.Error.Message);
        }
    }
}