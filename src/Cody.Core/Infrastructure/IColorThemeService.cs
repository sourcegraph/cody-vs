﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Infrastructure
{
    public interface IColorThemeService
    {
        bool IsDarkTheme();

        IReadOnlyDictionary<string, string> GetThemedColors();
    }
}
