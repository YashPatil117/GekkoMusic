using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekkoMusic.ViewModels
{
    
        public static class PathPlayer
        {
            public static string Data =>
                Path.Combine(FileSystem.AppDataDirectory, "Data");

            public static string Downloads =>
                Path.Combine(Data, "Downloads");

            public static string TempMusic =>
                Path.Combine(Data, "TempMusic");

            static PathPlayer()
            {
                Directory.CreateDirectory(Data);
                Directory.CreateDirectory(Downloads);
                Directory.CreateDirectory(TempMusic);
            }
        }
    

}
