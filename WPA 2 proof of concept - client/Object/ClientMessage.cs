using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPA_2_proof_of_concept___client.Object
{
    public class ClientMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string iv { get; set; }
    }
}
