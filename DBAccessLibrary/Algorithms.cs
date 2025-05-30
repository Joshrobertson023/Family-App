using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using DBAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace DBAccessLibrary
{
    public static class Algorithms
    {
        public static List<string> QuickSortUsernames(List<string> usernames)
        {
            return usernames;
        }
        
        public static List<RecoveryInfo> SortRecoveryInfoByName(List<RecoveryInfo> _recoveryInfo)
        {
            return _recoveryInfo;
        }
    }
}
