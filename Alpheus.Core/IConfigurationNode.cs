﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpheus
{
    public interface IConfigurationNode
    {
        AString Name { get; set; }
        bool IsTerminal { get; }
    }
}
