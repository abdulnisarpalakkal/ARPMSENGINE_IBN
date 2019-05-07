using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.user
{
    interface UserAuthenticationService
    {
        bool ValidateUser(string userName,string password);
    }
}
