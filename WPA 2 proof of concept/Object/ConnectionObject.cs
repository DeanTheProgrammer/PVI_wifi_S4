using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPA_2_proof_of_concept.Object
{
    public class ConnectionObject
    {
        public ConnectionObject() 
        {
            PasswordCorrect = false;
        }
        public byte[] Private_key { get; set; }
        public byte[] public_Key { get; set; }
        public string MAC_Adress { get; set; }
        public string Id { get; set; }
        public bool PasswordCorrect { get; set; }
    }
}
