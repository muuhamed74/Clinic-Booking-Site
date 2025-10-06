using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Abstraction
{
    public interface IMessageProvider
    {
        Task SendAsync(string toPhoneNumber, string message , int templateId);
    }
}
