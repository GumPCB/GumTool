using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GumCut
{
    public static class FFmpegArguments
    {
        public static string FastCut(in InputData data, bool hide_banner)
        {
            // %FFMPEG_PATH% -hide_banner -loglevel warning -i %IN_FILE% -ss 00:10:00.000 -to 00:20:00.000 -movflags +faststart -c copy -y %OUT_FILE%
            string arguments = new($"-i \"{data.LoadVideo}\" ");

            if (hide_banner)
            {
                arguments = "-hide_banner -loglevel warning " + arguments;
            }

            arguments += SS_TO(data);

            if (data.Streaming)
                arguments += "-movflags +faststart ";

            // "-y %OUT_FILE%"  // -y 이름이 같으면 덮어쓰기
            arguments += $"-c copy -y \"{data.SaveVideo}\"";

            return arguments;
        }

        public static string Edit(in InputData data, bool hide_banner)
        {
            string arguments = new($"-i \"{data.LoadVideo}\" ");

            if (hide_banner)
            {
                arguments = "-hide_banner -loglevel warning " + arguments;
            }

            arguments += SS_TO(data);

            arguments += CRF(data);

            arguments += VF(data);

            if(data.Streaming)
                arguments += "-movflags +faststart ";

            arguments += Encoder(data, arguments.Contains("-vf", StringComparison.Ordinal) == false);

            arguments += $"-y \"{data.SaveVideo}\"";

            return arguments;
        }

        public static string Image(in InputData data, bool hide_banner)
        {
            string arguments = new($"-i \"{data.LoadVideo}\" ");

            if (hide_banner)
            {
                arguments = "-hide_banner -loglevel warning " + arguments;
            }

            arguments += SS_TO(data);

            arguments += VF(data);

            if (data.ImageFormat == 2 && data.Qscale != 0)
            {
                arguments += "-qscale:v " + data.Qscale + " ";
            }

            arguments += $"-y \"{ImageSaveFileName(data)}\"";

            return arguments;
        }

        private static string SS_TO(in InputData data)
        {
            string time = string.Empty;

            // "-ss 00:10:00.000 "
            if (data.Start.IsZero == false)
            {
                time += $"-ss {data.Start} ";
            }
            // "-to 00:20:00.000 "
            if (data.End.IsZero == false)
            {
                time += $"-to {data.End} ";
            }

            return time;
        }

        private static string CRF(in InputData data)
        {
            string bitrate = string.Empty;

            if (data.Bitrate != 0)
            {
                bitrate = "-b:v " + data.Bitrate + "M ";
            }

            if (data.BitrateMax != 0)
            {
                bitrate += "-maxrate " + data.BitrateMax + "M ";
            }

            if (data.Bufsize != 0)
            {
                bitrate += "-bufsize " + data.Bufsize + "M ";
            }

            if (data.CRF > -1.0)
            {
                bitrate += "-crf " + data.CRF + " ";
            }

            if (data.QP > -1)
            {
                bitrate += "-qp " + data.QP + " ";
            }

            if (data.SelectedPreset != 0 && data.Presets.Count > data.SelectedPreset)
            {
                bitrate += "-preset " + data.Presets[data.SelectedPreset] + " ";
            }

            if (data.SelectedTune != 0 && data.Tunes.Count > data.SelectedTune)
            {
                bitrate += "-tune " + data.Tunes[data.SelectedTune] + " ";
            }

            return bitrate;
        }

        private static string VF(in InputData data)
        {
            string vf = string.Empty;

            // 90, -90 회전
            if (data.Rotation == 90)
            {
                vf += "transpose=1";
            }
            else if (data.Rotation == 270)
            {
                vf += "transpose=2";
            }

            // 플립과 180 회전
            bool hflip = data.HFlip;
            bool vflip = data.VFlip;
            if (data.Rotation == 180)
            {
                hflip = !hflip;
                vflip = !vflip;
            }
            if (hflip == true)
            {
                vf += (vf.Length == 0 ? "" : ",") + "hflip";
            }
            if (vflip == true)
            {
                vf += (vf.Length == 0 ? "" : ",") + "vflip";
            }

            // fps 변경
            if (data.Fps > 0.0)
            {
                vf += $"{(vf.Length == 0 ? "" : ",")}fps={data.Fps}";
            }

            // 사이즈 변경
            if (data.Scale.IsZero == false)
            {
                vf += $"{(vf.Length == 0 ? "" : ",")}scale={data.Scale.ToStringNotZero}";
            }

            // 크롭
            // ffmpeg.exe -i 2_cut.mp4 -filter:v "crop=iw/2:ih:0:0:keep_aspect=1" -c:a copy -y crop_1.mp4
            // 가로크기 절반, 세로는 전체로, 0,0 부터 자름, keep_aspect=1 은 잘라진 크기와 관계없이 원본 비율대로 늘려서 보여줌
            if (data.CropStart.IsZero == false || data.CropSize.IsZero == false)
            {
                vf += $"{(vf.Length == 0 ? "" : ",")}crop={data.CropSize.ToStringNotZero}:{data.CropStart}";
            }

            if (vf.Length != 0)
            {
                vf = $"-vf \"{vf}\" ";
            }

            return vf;
        }

        private static string Encoder(in InputData data, bool canVideoCopy)
        {
            string encoder = string.Empty;
            
            if (data.SelectedVideoEncoder == 0 && data.SelectedAudioEncoder == 0 && canVideoCopy)
            {
                return "-c copy ";
            }

            if (data.SelectedVideoEncoder != 0 && data.VideoEncoders.Count > data.SelectedVideoEncoder)
            {
                encoder = $"-c:v {data.VideoEncoders[data.SelectedVideoEncoder]} ";
            }
            else if (canVideoCopy)
            {
                encoder = "-c:v copy ";
            }

            if (data.SelectedAudioEncoder != 0 && data.AudioEncoders.Count > data.SelectedAudioEncoder)
            {
                encoder += $"-c:a {data.AudioEncoders[data.SelectedAudioEncoder]} ";
            }
            else
            {
                encoder += "-c:a copy ";
            }

            return encoder;
        }

        private static string ImageSaveFileName(in InputData data)
        {
            string filename = string.Empty;
            if (data.SaveVideo.Contains("_cut."))
            {
                string[] split = data.SaveVideo.Split("_cut.", StringSplitOptions.RemoveEmptyEntries);
                filename = split[0];
            }
            else if (data.SaveVideo.Contains('.'))
            {
                string[] split = data.SaveVideo.Split(Path.GetExtension(data.SaveVideo), StringSplitOptions.RemoveEmptyEntries);
                filename = split[0];
            }
            else
            {
                filename = data.SaveVideo;
            }

            switch (data.ImageFormat)
            {
                case 0:
                    filename += ".gif";
                    break;
                case 1:
                    filename += $"_%0{data.SaveZeroCount}d.png";
                    break;
                case 2:
                    filename += $"_%0{data.SaveZeroCount}d.jpg";
                    break;
                default:
                    break;
            }

            return filename;
        }
    }

}
// 영상파일 합치기
// https://stackoverflow.com/questions/43578882/ffmpeg-concat-makes-video-longer
// ffmpeg.exe -f concat -safe 0 -i sss.txt -movflags +faststart -c copy -y Sum.mp4
// sss.txt => file 'Z:\bbb\3_cut.mp4'
// 이어붙이기는 하지만 영상 넘어가는 부분이 끊김

// 썸네일 추출
// ffmpeg.exe -i 111.mp4 -ss 0:9:00.0 -to 0:10:0.0 -vf "yadif=0:-1:0,fps=5" -qscale:v 2 -y "Z:\thumb\111_%05d.jpg"
// 9분에서 10분까지 초당 5장씩 jpg로 출력
// ffmpeg.exe -i 2.mp4 -vf "fps=30,scale=640:-1" -y 2.gif
// 초당 30프레임, 가로 640, 세로는 비율에 맞춰서 gif 로 변환
// ffmpeg.exe -i 2.mp4 -vf fps=5 -y 2_%05d.png
// 초당 5장씩 png로 출력
// <TextBlock FontFamily="Segoe Fluent Icons" Text="&#xE96D; &#xE96E; &#xE96F; &#xE970; &#xE91B; &#xF4A9;"/>
// <TextBlock FontFamily="Segoe Fluent Icons" Text="&#xF4A9;"/>