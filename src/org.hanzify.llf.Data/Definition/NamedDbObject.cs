
#region usings

using System;
using org.hanzify.llf.Data.Common;

#endregion

namespace org.hanzify.llf.Data.Definition
{
    [Serializable]
    public abstract class NamedDbObject
    {
        [DbKey(IsSystemGeneration=false), MaxLength(255)]
        public string Name = "";
    }
}