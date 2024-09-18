namespace CheckMediaFiles;

using Emgu.CV;
using Emgu.CV.CvEnum;
using MimeTypes;
using NAudio.Wave;
using System;

class Program
{
    static void Main(string[] args)
    {
        string programName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;
        File.Delete(programName + ".log");
        using MultiWriter logger = new(File.AppendText(programName + ".log"), Console.Out);

        logger.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 程式啟動");

        string[] filePaths = Directory.GetFiles(args[0], "*", SearchOption.AllDirectories);
        foreach (string filePath in filePaths)
        {
            // GIF其實是影片 https://answers.opencv.org/question/226582/cv2imread-fail-to-open-gif-image/?answer=226583#post-id-226583
            string fileSubType = MimeTypeMap.GetMimeType(filePath).Split("/")[1];
            string fileMimeType = fileSubType == "gif" ? "video" : MimeTypeMap.GetMimeType(filePath).Split("/")[0];
            switch (fileMimeType)
            {
                case "image":
                    string checkImageFileResult = CheckImageFile(filePath);
                    logger.WriteLine(checkImageFileResult);
                    break;
                case "video":
                    string checkVideoFileResult = CheckVideoFile(filePath);
                    logger.WriteLine(checkVideoFileResult);
                    break;
                case "audio":
                    string checkAudioFileResult = CheckAudioFile(filePath);
                    logger.WriteLine(checkAudioFileResult);
                    break;
                default:
                    break;
            }
        }

        logger.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 程式結束");
    }

    static string CheckImageFile(string imagePath)
    {
        try
        {
            using Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color);
            if (image.IsEmpty)
            {
                return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {imagePath} 無法開啟";
            }

            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} success: {imagePath} 可正常開啟";
        }
        catch (Exception ex)
        {
            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {imagePath} 開啟時發生錯誤: {ex.Message}";
        }
    }

    static string CheckVideoFile(string videoPath)
    {
        try
        {
            using VideoCapture capture = new(videoPath);
            if (!capture.IsOpened)
            {
                return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {videoPath} 無法開啟";
            }

            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} success: {videoPath} 可正常開啟";
        }
        catch (Exception ex)
        {
            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {videoPath} 開啟時發生錯誤: {ex.Message}";
        }
    }

    static string CheckAudioFile(string audioPath)
    {
        try
        {
            using AudioFileReader reader = new(audioPath);
            if (!reader.CanRead)
            {
                return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {audioPath} 無法開啟";
            }

            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} success: {audioPath} 可正常開啟";
        }
        catch (Exception ex)
        {
            return $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} failed: {audioPath} 開啟時發生錯誤: {ex.Message}";
        }
    }
}

public class MultiWriter : TextWriter
{
    private readonly TextWriter[] writers;

    public MultiWriter(params TextWriter[] writers)
    {
        this.writers = writers;

        foreach (TextWriter writer in writers)
        {
            if (writer is StreamWriter)
            {
                ((StreamWriter)writer).AutoFlush = true;
            }
        }
    }

    public override void WriteLine(string value)
    {
        foreach (TextWriter writer in writers)
        {
            writer.WriteLine(value);
        }
    }

    public override System.Text.Encoding Encoding
    {
        get { return System.Text.Encoding.UTF8; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var writer in writers)
            {
                writer.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
