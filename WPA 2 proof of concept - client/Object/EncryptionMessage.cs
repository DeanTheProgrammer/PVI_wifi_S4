using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPA_2_proof_of_concept___client.Object
{
    public class EncryptionMessage
    {
        public byte[] message { get; set; }
        public byte[] iv { get; set; }
    }
}
