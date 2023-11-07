using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils.Tasks
{
    public class TaskNotFoundException : TaskException
    {

        public TaskNotFoundException(string fullName)
            : base(fullName, "The specified task does not exists: " + fullName)
        {
        }
    }
}