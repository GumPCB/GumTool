using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GumCut
{
    public static class FFmpegArguments
    {
        public static string FastCut(in InputData data)
        {
            // %FFMPEG_PATH% -hide_banner -loglevel warning -i %IN_FILE% -ss 00:10:00.000 -to 00:20:00.000 -movflags +faststart -c copy -y %OUT_FILE%
            string arguments = new($"-hide_banner -loglevel warning -i \"{data.InputVideo}\" ");

            // "-ss 00:10:00.000 "
            if (data.Start.Hour != 0 || data.Start.Minute != 0 || data.Start.Seconds != 0 || data.Start.Milliseconds != 0)
            {
                arguments += $"-ss {data.Start.Hour}:{data.Start.Minute}:{data.Start.Seconds}.{data.Start.Milliseconds} ";
            }
            // "-to 00:20:00.000 "
            if (data.End.Hour != 0 || data.End.Minute != 0 || data.End.Seconds != 0 || data.End.Milliseconds != 0)
            {
                arguments += $"-to {data.End.Hour}:{data.End.Minute}:{data.End.Seconds}.{data.End.Milliseconds} ";
            }

            arguments += "-movflags +faststart -c copy ";

            // "-y %OUT_FILE%"  // -y 이름이 같으면 덮어쓰기
            arguments += $"-y \"{data.OutputVideo}\"";

            return arguments;
        }

        private static string VF(in InputData data)
        {
            string vf = string.Empty;

            switch (data.Rotation) // 회전은 재인코딩 필요
            {
                case 90:    // "-vf transpose=1 "
                    vf += "-vf transpose=1 ";
                    break;
                case 180:   // "-vf "vflip,hflip" "
                    vf += "-vf \"vflip,hflip\" ";
                    break;
                case 270:   // "-vf transpose=2 "
                    vf += "-vf transpose=2 ";
                    break;
                default:
                    break;
            }

            return vf;
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