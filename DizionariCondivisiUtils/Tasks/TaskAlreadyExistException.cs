using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils.Tasks
{
    public class TaskAlreadyExistException : TaskException
    {

        public TaskAlreadyExistException(string fullName)
            : base(fullName, "The specified task already exists: " + fullName)
        {
        }
    }
}