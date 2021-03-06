﻿using System;

namespace FlexiGlob
{
    public class GlobFormatException : FormatException
    {
        public string GlobPattern { get; }
        public int ErrorPosition { get; }

        public GlobFormatException(string message, string globPattern, int errorPosition) : base(message)
        {
            GlobPattern = globPattern;
            ErrorPosition = errorPosition;
        }
    }
}
