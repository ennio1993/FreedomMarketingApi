using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreedomMarketingApi.Helpers
{
    public class DataManagement
    {
        public string CreateReferenceCode()
        {
            Random random = new Random();
            string numbers = random.Next(0, 99999).ToString("D5"); 
            string referenceCode = "F" + numbers + "M";
            return referenceCode;
        }
    }
}
