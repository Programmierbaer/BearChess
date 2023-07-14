using System;
using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChessBase
{

    public struct FieldChanges
    {
        public string[] RemovedFields;
        public string[] AddedFields;
    }

    public static class FieldChangeHelper
    {
        public static FieldChanges GetFieldChanges(string firstLine, string secondLine)
        {
            FieldChanges fieldChanges = new FieldChanges();
            List<string> changes = new List<string>();
            HashSet<string> firstArray = new HashSet<string>(firstLine.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            HashSet<string> secondArray = new HashSet<string>(secondLine.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            foreach (var first in firstArray)
            {
                if (!secondArray.Contains(first))
                {
                    changes.Add(first);
                }
            }
            fieldChanges.RemovedFields = changes.ToArray();
            changes.Clear();
            foreach (var second in secondArray)
            {
                if (!firstArray.Contains(second))
                {
                    changes.Add(second);
                }
            }
            fieldChanges.AddedFields = changes.ToArray();
            return fieldChanges;
        }
    }
}