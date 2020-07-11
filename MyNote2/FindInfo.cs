using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyNote2
{
    public class FindInfo
    {
        public string filePath { get; set; }
        public int index;
        public FindInfo(string filePath, int idx)
        {
            this.filePath = filePath;
            this.index = idx;
        }
    }
}
