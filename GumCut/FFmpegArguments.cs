using System.IO;

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

            arguments += MAP(data);

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

            arguments += MAP(data);

            string edit = CRF(data) + VF(data);
            arguments += edit;

            if (data.Streaming)
                arguments += "-movflags +faststart ";

            arguments += Encoder(data, edit.Length == 0);

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

            arguments += $"-an -y \"{ImageSaveFileName(data)}\"";

            return arguments;
        }

        public static string Subtitle(in InputData data, bool hide_banner)
        {
            // %FFMPEG_PATH% -hide_banner -loglevel warning -i %IN_FILE% -map %STREAM_NUM% -c:s copy -y %OUT_FILE%
            string arguments = new($"-i \"{data.LoadVideo}\" ");

            if (hide_banner)
            {
                arguments = "-hide_banner -loglevel warning " + arguments;
            }

            arguments += SS_TO(data);

            if (data.SelectedSubtitle >= 0 && data.Subtitles.Count > data.SelectedSubtitle)
            {
                string[] splitSubtitle = data.Subtitles[data.SelectedSubtitle].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (splitSubtitle.Length > 0)
                {
                    arguments += $"-map {splitSubtitle[0]} ";
                }
                else
                {
                    arguments += "-map 0:s ";
                }
            }

            arguments += SubtitleEncoderAndFileName(data);

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

        private static string MAP(in InputData data)
        {
            string map = string.Empty;

            bool allSelectStream = true;
            foreach (var stream in data.Streams)
            {
                if (!stream.Checked)
                {
                    allSelectStream = false;
                    continue;
                }

                string[] line = stream.Line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (line.Length > 0)
                {
                    if (line[0].StartsWith("0:"))
                    {
                        map += $"-map {line[0]} ";
                    }
                    else
                    {
                        map += $"-map 0:{line[0]} ";
                    }
                }
            }

            if (allSelectStream || map.Length == 0)
                map = "-map 0 ";

            return map;
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

            if (data.SelectedProfile != 0 && data.Profiles.Count > data.SelectedProfile)
            {
                bitrate += "-profile:v " + data.Profiles[data.SelectedProfile] + " ";
            }

            if (data.SelectedLevel != 0 && data.Levels.Count > data.SelectedLevel)
            {
                bitrate += "-level:v " + data.Levels[data.SelectedLevel] + " ";
            }

            if (data.Qscale != 0)
            {
                bitrate += "-qscale:v " + data.Qscale + " ";
            }
            if (data.QscaleAudio != 0)
            {
                bitrate += "-qscale:a " + data.QscaleAudio + " ";
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
            else if (data.QscaleAudio == 0)
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

        private static string SubtitleEncoderAndFileName(in InputData data)
        {
            // $"-c:s copy -y \""
            string result = string.Empty;

            if (data.SelectedSubtitleEncoder > 0 && data.SubtitleEncoders.Count > data.SelectedSubtitleEncoder)
            {
                result = $"-c:s {data.SubtitleEncoders[data.SelectedSubtitleEncoder]} -vn -an -y \"";
            }
            else
            {
                result = $"-c:s copy -vn -an -y \"";
            }

            // 폴더경로와 파일명
            if (data.SaveVideo.Contains('.'))
            {
                string[] split = data.SaveVideo.Split(Path.GetExtension(data.SaveVideo), StringSplitOptions.RemoveEmptyEntries);
                result += split[0];
            }
            else
            {
                result += data.SaveVideo;
            }

            // 확장자
            string ext = string.Empty;
            if (data.SelectedSubtitleEncoder > 0 && data.SubtitleEncoders.Count > data.SelectedSubtitleEncoder)
            {
                foreach (string subtitleExt in data.SubtitleExtensions)
                {
                    if (subtitleExt.Contains(data.SubtitleEncoders[data.SelectedSubtitleEncoder], StringComparison.Ordinal))
                    {
                        ext = $"{subtitleExt.Substring(subtitleExt.IndexOf('=') + 1)}";
                        break;
                    }
                }
            }
            else if (data.SelectedSubtitle >= 0 && data.Subtitles.Count != 0 && data.Subtitles.Count > data.SelectedSubtitle)
            {
                string[] splitSubtitle = data.Subtitles[data.SelectedSubtitle].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (string split in splitSubtitle)
                {
                    foreach (string subtitleExt in data.SubtitleExtensions)
                    {
                        if (subtitleExt.Contains(split, StringComparison.Ordinal))
                        {
                            ext = $"{subtitleExt.Substring(subtitleExt.IndexOf('=') + 1)}";
                            break;
                        }
                    }
                    if (ext.Length != 0)
                        break;
                }
            }

            if (ext.Length == 0)
            {
                ext = "mkv";
            }

            result += $".{ext}\"";
            return result;
        }
    }

}
// 영상파일 합치기
// https://stackoverflow.com/questions/43578882/ffmpeg-concat-makes-video-longer
// ffmpeg.exe -f concat -safe 0 -i sss.txt -movflags +faststart -c copy -y Sum.mp4
// sss.txt => file 'Z:\bbb\3_cut.mp4'

// ffmpeg.exe -hide_banner -h encoder=libx265
// ffmpeg.exe -hide_banner -h encoder=hevc_nvenc
// ffmpeg.exe -hide_banner -filters
// ffmpeg.exe -hide_banner -h filter=yadif
// ffmpeg.exe -hide_banner -hwaccels
// ffmpeg.exe -hide_banner -codecs