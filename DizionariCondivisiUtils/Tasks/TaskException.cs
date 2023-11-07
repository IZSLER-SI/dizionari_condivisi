using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils.Tasks
{
    public class TaskException : Exception
    {
        public string FullName { get; private set; }

        public TaskException(string fullName, string description)
            : base(description)
        {
            FullName = fullName;
        }

        public TaskException(string fullName)
            : base("Exception in task: " + fullName)
        {
            FullName = fullName;
        }
    }
}